using Avalonia.Controls;
using Avalonia;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TOTP.Avalonia.Desktop.Controls;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Platform;

namespace TOTP.Avalonia.Desktop;

public partial class MainWindow : Window
{
    private const double MaximumScreenHeightRatio = 0.75;
    private const double DefaultPageHeight = 520;
    private static readonly TimeSpan HeightAnimationDuration = TimeSpan.FromMilliseconds(220);
    private bool _initialized;
    private bool _fitScheduled;
    private MainWindowViewModel? _observedViewModel;
    private AccountListViewModel? _observedAccountList;
    private CancellationTokenSource? _heightAnimationLifetime;

    public AvaloniaClipboardAccessor? ClipboardAccessor { get; init; }
    public AvaloniaStorageProviderAccessor? StorageProviderAccessor { get; init; }
    public AvaloniaWindowCoordinator? WindowCoordinator { get; init; }

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        WindowCoordinator?.RegisterMainWindow(this);
        if (Clipboard is not null)
            ClipboardAccessor?.Set(Clipboard);
        StorageProviderAccessor?.Set(StorageProvider);
        ApplyScreenHeightLimit();
        if (DataContext is MainWindowViewModel viewModel)
        {
            ObserveViewModel(viewModel);
            if (!_initialized)
            {
                _initialized = true;
                viewModel.InitializeCommand.Execute(null);
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        ObserveViewModel(null);
        _heightAnimationLifetime?.Cancel();
        WindowCoordinator?.UnregisterMainWindow(this);
        base.OnClosed(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.PrepareForShutdown();
        base.OnClosing(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty
            && change.NewValue is WindowState.Minimized
            && DataContext is MainWindowViewModel viewModel)
        {
            _ = viewModel.HandleWindowMinimizedAsync();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel settingsViewModel
            && settingsViewModel.IsSettingsVisible
            && e.Key == Key.Escape)
        {
            settingsViewModel.CloseSettingsCommand.Execute(null);
            e.Handled = true;
            base.OnKeyDown(e);
            return;
        }

        if (e.Key == Key.Escape
            && DataContext is MainWindowViewModel accountViewModel
            && accountViewModel.IsAccountListVisible
            && accountViewModel.AccountList.IsEditorVisible)
        {
            accountViewModel.AccountList.CancelEditCommand.Execute(null);
            e.Handled = true;
            base.OnKeyDown(e);
            return;
        }

        if (e.Key == Key.F
            && e.KeyModifiers.HasFlag(KeyModifiers.Control)
            && DataContext is MainWindowViewModel viewModel
            && viewModel.IsShellVisible)
        {
            viewModel.ToggleSearchCommand.Execute(null);
            FocusAccountSearch();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape
            && DataContext is MainWindowViewModel searchViewModel
            && searchViewModel.IsSearchVisible)
        {
            searchViewModel.ClearSearchCommand.Execute(null);
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    private void FocusAccountSearch(object? sender, RoutedEventArgs e) =>
        FocusAccountSearch();

    private void FocusAccountSearch()
    {
        if (DataContext is not MainWindowViewModel viewModel
            || !viewModel.IsShellVisible
            || viewModel.IsSettingsVisible)
            return;
        Dispatcher.UIThread.Post(
            () =>
            {
                if (!viewModel.IsSearchVisible) return;
                this.GetVisualDescendants()
                    .OfType<TextBox>()
                    .FirstOrDefault(control => control.Name == "AccountSearchBox")
                    ?.Focus();
            },
            DispatcherPriority.Input);
    }

    private void AccountEditorFlyoutPropertyChanged(
        object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Visual.IsVisibleProperty
            || e.NewValue is not true
            || sender is not Border flyout
            || flyout.DataContext is not AccountListViewModel accountList
            || accountList.IsEditingExistingAccount)
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () =>
            {
                if (!flyout.IsVisible
                    || flyout.DataContext is not AccountListViewModel current
                    || current.IsEditingExistingAccount)
                {
                    return;
                }

                flyout.GetVisualDescendants()
                    .OfType<TextBox>()
                    .FirstOrDefault(control => control.Name == "AccountIssuerBox")
                    ?.Focus();
            },
            DispatcherPriority.Input);
    }

    private void ObserveViewModel(MainWindowViewModel? viewModel)
    {
        if (ReferenceEquals(_observedViewModel, viewModel)) return;
        if (_observedViewModel is not null)
            _observedViewModel.PropertyChanged -= MainViewModelPropertyChanged;
        if (_observedAccountList is not null)
            _observedAccountList.PropertyChanged -= AccountListPropertyChanged;

        _observedViewModel = viewModel;
        _observedAccountList = viewModel?.AccountList;

        if (_observedViewModel is not null)
            _observedViewModel.PropertyChanged += MainViewModelPropertyChanged;
        if (_observedAccountList is not null)
            _observedAccountList.PropertyChanged += AccountListPropertyChanged;
    }

    private void MainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainWindowViewModel.IsAccountListVisible)) return;
        if (_observedViewModel is { IsAccountListVisible: true })
            ScheduleAccountPageFit();
        else
            StartHeightAnimation(Math.Min(MaxHeight, DefaultPageHeight));
    }

    private void AccountListPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AccountListViewModel.Accounts)
            or nameof(AccountListViewModel.SelectedAccount)
            or nameof(AccountListViewModel.HasSelectedAccount)
            or nameof(AccountListViewModel.HasNoAccounts)
            or nameof(AccountListViewModel.HasNoSearchResults))
        {
            ScheduleAccountPageFit();
        }
    }

    private void ScheduleAccountPageFit()
    {
        if (_fitScheduled) return;
        _fitScheduled = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _fitScheduled = false;
                FitAccountPageHeight();
            },
            DispatcherPriority.Loaded);
    }

    private void FitAccountPageHeight()
    {
        if (_observedViewModel is not { IsAccountListVisible: true }) return;
        ApplyScreenHeightLimit();
        UpdateLayout();

        var descendants = this.GetVisualDescendants().ToArray();
        var accountScroller = descendants
            .OfType<ScrollViewer>()
            .FirstOrDefault(control => control.Name == "AccountPageScrollViewer");
        var accountList = descendants
            .OfType<ContextPreservingAccountListBox>()
            .FirstOrDefault(control => control.Name == "AccountsListBox");
        var accountContent = descendants
            .OfType<StackPanel>()
            .FirstOrDefault(control => control.Name == "AccountPageContent");
        if (accountScroller is null || accountList is null || accountContent is null) return;

        var accountCount = _observedAccountList?.Accounts.Count ?? 0;
        var realizedRowHeight = accountList.GetVisualDescendants()
            .OfType<ListBoxItem>()
            .Select(item => item.Bounds.Height)
            .FirstOrDefault(height => height > 0);
        var rowHeight = realizedRowHeight > 0 ? realizedRowHeight : 42;
        var naturalListHeight = accountCount == 0 ? 0 : (accountCount * rowHeight) + 2;
        accountList.MaxHeight = naturalListHeight;
        var availableWidth = Math.Max(0, accountScroller.Bounds.Width);
        accountContent.Measure(new Size(availableWidth, double.PositiveInfinity));

        var windowChromeHeight = Math.Max(0, Bounds.Height - accountScroller.Bounds.Height);
        var fixedPageHeight = Math.Max(
            0,
            accountContent.DesiredSize.Height - accountList.DesiredSize.Height);
        var availableListHeight = Math.Max(
            0,
            MaxHeight - windowChromeHeight - fixedPageHeight - 2);
        accountList.MaxHeight = Math.Min(naturalListHeight, availableListHeight);
        accountContent.Measure(new Size(availableWidth, double.PositiveInfinity));

        var targetHeight = Math.Clamp(
            windowChromeHeight + accountContent.DesiredSize.Height + 2,
            MinHeight,
            MaxHeight);
        StartHeightAnimation(targetHeight);
    }

    private void ApplyScreenHeightLimit()
    {
        var screen = Screens.ScreenFromWindow(this);
        if (screen is null) return;
        var scale = screen.Scaling > 0 ? screen.Scaling : 1;
        var screenHeightLimit = (screen.WorkingArea.Height / scale) * MaximumScreenHeightRatio;
        if (MinHeight > screenHeightLimit) MinHeight = screenHeightLimit;
        MaxHeight = screenHeightLimit;
        if (Height > MaxHeight) Height = MaxHeight;
    }

    private void StartHeightAnimation(double targetHeight)
    {
        targetHeight = Math.Clamp(targetHeight, MinHeight, MaxHeight);
        _heightAnimationLifetime?.Cancel();
        var lifetime = new CancellationTokenSource();
        _heightAnimationLifetime = lifetime;
        _ = AnimateHeightAsync(targetHeight, lifetime);
    }

    private async Task AnimateHeightAsync(
        double targetHeight,
        CancellationTokenSource lifetime)
    {
        var startHeight = Math.Max(MinHeight, Bounds.Height > 0 ? Bounds.Height : Height);
        var startPosition = Position;
        var screen = Screens.ScreenFromWindow(this);
        var screenScale = screen?.Scaling is > 0 ? screen.Scaling : 1;
        var targetY = screen is null
            ? startPosition.Y
            : screen.WorkingArea.Y + (int)Math.Round(
                (screen.WorkingArea.Height - (targetHeight * screenScale)) / 2);
        if (Math.Abs(startHeight - targetHeight) < 0.5)
        {
            Height = targetHeight;
            CompleteHeightAnimation(lifetime);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            while (stopwatch.Elapsed < HeightAnimationDuration)
            {
                lifetime.Token.ThrowIfCancellationRequested();
                var progress = Math.Clamp(
                    stopwatch.Elapsed.TotalMilliseconds / HeightAnimationDuration.TotalMilliseconds,
                    0,
                    1);
                var eased = 1 - Math.Pow(1 - progress, 3);
                Height = startHeight + ((targetHeight - startHeight) * eased);
                Position = new PixelPoint(
                    startPosition.X,
                    (int)Math.Round(startPosition.Y + ((targetY - startPosition.Y) * eased)));
                await Task.Delay(16, lifetime.Token);
            }

            Height = targetHeight;
            Position = new PixelPoint(startPosition.X, targetY);
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
        }
        finally
        {
            CompleteHeightAnimation(lifetime);
        }
    }

    private void CompleteHeightAnimation(CancellationTokenSource lifetime)
    {
        if (ReferenceEquals(_heightAnimationLifetime, lifetime))
            _heightAnimationLifetime = null;
        lifetime.Dispose();
    }

}
