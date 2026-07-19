using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using TOTP.Avalonia.Desktop;
using TOTP.Avalonia.Shared.Controls;
using TOTP.Avalonia.Shared.Styles;
using TOTP.Avalonia.Desktop.Dialogs;
using TOTP.Avalonia.Desktop.Localization;
using TOTP.Avalonia.Desktop.Controls;
using TOTP.Avalonia.Desktop.Presentation;

namespace TOTP.Tests.Avalonia.Headless;

public sealed class MainWindowSmokeTests
{
    [AvaloniaFact]
    public void AccountList_RightClickOpensContextWithoutChangingSelection()
    {
        var first = new AccountListItemViewModel(Guid.NewGuid(), "First", "selected");
        var second = new AccountListItemViewModel(Guid.NewGuid(), "Second", "context");
        var menu = new ContextMenu { ItemsSource = new[] { new MenuItem { Header = "Edit" } } };
        var list = new ContextPreservingAccountListBox
        {
            Width = 200,
            Height = 100,
            ItemsSource = new[] { first, second },
            SelectedItem = first,
            ContextMenu = menu
        };
        var window = new Window { Width = 240, Height = 140, Content = list };

        try
        {
            window.Show();
            list.ApplyTemplate();
            window.UpdateLayout();
            var secondContainer = Assert.Single(
                list.GetVisualDescendants().OfType<ListBoxItem>(),
                item => ReferenceEquals(item.DataContext, second));
            var clickPoint = secondContainer.TranslatePoint(new Point(8, 8), window);
            Assert.NotNull(clickPoint);

            window.MouseDown(clickPoint.Value, MouseButton.Right, RawInputModifiers.None);
            window.MouseUp(clickPoint.Value, MouseButton.Right, RawInputModifiers.None);

            Assert.Same(first, list.SelectedItem);
            Assert.Same(second, list.ContextAccount);
            Assert.True(menu.IsOpen);
            menu.Close();
            Assert.Null(list.ContextAccount);
        }
        finally
        {
            menu.Close();
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task AccountList_ProgrammaticSelectionScrollsImportedRowIntoView()
    {
        var accounts = Enumerable.Range(0, 50)
            .Select(index => new AccountListItemViewModel(
                Guid.NewGuid(),
                $"Issuer {index:00}",
                $"account-{index:00}"))
            .ToArray();
        var list = new ContextPreservingAccountListBox
        {
            Width = 220,
            Height = 120,
            ItemsSource = accounts
        };
        var window = new Window { Width = 260, Height = 160, Content = list };

        try
        {
            window.Show();
            list.ApplyTemplate();
            window.UpdateLayout();

            list.SelectedItem = accounts[^1];
            await Task.Delay(80);
            window.UpdateLayout();

            Assert.Contains(
                list.GetVisualDescendants().OfType<ListBoxItem>(),
                item => ReferenceEquals(item.DataContext, accounts[^1]));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AccountRowHighlightContainer_PadsContentInsideHighlight()
    {
        var text = new TextBlock { Text = "new account" };
        var highlight = new Border { Child = text };
        highlight.Classes.Add("account-row-container");
        highlight.Classes.Add("recently-added");
        var window = new Window { Content = highlight };

        try
        {
            window.Show();
            window.UpdateLayout();

            Assert.Equal(new Thickness(8, 5), highlight.Padding);
            Assert.True(text.Bounds.X >= 8);
            Assert.True(text.Bounds.Y >= 5);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task AccountEditorFlyout_OpenClassSlidesFromRight()
    {
        var transform = new TranslateTransform();
        var flyout = new Border
        {
            Width = 320,
            Height = 300,
            RenderTransform = transform
        };
        flyout.Classes.Add("flyout");
        flyout.Classes.Add("open");
        var window = new Window { Content = flyout };

        try
        {
            window.Show();
            await Task.Delay(30);
            var openingOffset = transform.X;
            await Task.Delay(220);

            Assert.True(openingOffset > 0);
            Assert.Equal(0, transform.X, precision: 2);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task AccountMessageToast_OpenClassFliesInWithoutTakingContentSpace()
    {
        var transform = new TranslateTransform();
        var toast = new Border
        {
            Width = 240,
            Height = 44,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top,
            IsHitTestVisible = false,
            RenderTransform = transform
        };
        toast.Classes.Add("account-toast");
        toast.Classes.Add("open");
        var content = new Border { Height = 300 };
        var host = new Grid { Children = { content, toast } };
        var window = new Window { Content = host };

        try
        {
            window.Show();
            await Task.Delay(30);
            var openingOffset = transform.Y;
            await Task.Delay(220);

            Assert.True(openingOffset < 0);
            Assert.Equal(0, transform.Y, precision: 2);
            Assert.Equal(300, content.Bounds.Height);
            Assert.False(toast.IsHitTestVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolbarIconStyle_LeavesFullHeightForSearchSymbol()
    {
        var symbol = new SymbolIcon
        {
            Kind = SymbolIconKind.Search,
            IconSize = 16
        };
        var button = new Button { Content = symbol };
        button.Classes.Add("icon");
        var window = new Window { Content = button };

        try
        {
            window.Show();
            button.ApplyTemplate();
            symbol.ApplyTemplate();
            window.UpdateLayout();

            Assert.Equal(16, symbol.Bounds.Width);
            Assert.Equal(16, symbol.Bounds.Height);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LanguageFlagBinding_DecodesAndProducesImageSource()
    {
        using var flags = new AvaloniaLanguageFlagProvider();
        var localization = new AvaloniaLocalizationService(
            new ResourceDictionary(),
            new AvaloniaStringCatalog(),
            flags);
        var images = localization.SupportedLanguages.Select(option =>
        {
            var image = new Image { DataContext = option };
            image.Bind(Image.SourceProperty, new Binding(nameof(LanguageOption.Icon)));
            return image;
        }).ToArray();
        var window = new Window
        {
            Content = new StackPanel { Children = { images[0], images[1] } }
        };

        try
        {
            window.Show();

            Assert.All(images, image =>
            {
                Assert.NotNull(image.Source);
                Assert.True(image.Source.Size.Width > 0);
                Assert.True(image.Source.Size.Height > 0);
            });
            Assert.NotSame(images[0].Source, images[1].Source);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CameraScannerDialog_LoadsRealXaml()
    {
        var window = new CameraScannerDialogWindow();

        try
        {
            window.Show();

            Assert.Single(window.GetVisualDescendants().OfType<Image>());
            Assert.Single(window.GetVisualDescendants().OfType<ProgressBar>());
            Assert.Single(window.GetVisualDescendants().OfType<Button>());
            Assert.Equal(560, window.Width);
            Assert.Equal(420, window.Height);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void QrPreviewDialog_EscapeClosesWindow()
    {
        var window = new QrPreviewDialogWindow();

        window.Show();
        window.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Escape
        });

        Assert.False(window.IsVisible);
    }

    [AvaloniaFact]
    public void MainWindow_LoadsRealXamlAndEssentialSurfaces()
    {
        var window = new MainWindow();

        try
        {
            window.Show();

            Assert.NotNull(window.Icon);
            Assert.Single(window.GetVisualDescendants().OfType<BusyOverlay>());
            Assert.True(window.GetVisualDescendants().OfType<Button>().Count() >= 5);
            Assert.True(window.GetVisualDescendants().OfType<Border>().Count() >= 5);
            Assert.NotEmpty(window.GetVisualDescendants().OfType<ScrollViewer>());
            Assert.Single(window.GetVisualDescendants().OfType<TabControl>());
            Assert.Equal(4, window.GetVisualDescendants().OfType<TabItem>().Count());
            var settingsTabs = window.GetVisualDescendants()
                .OfType<TabControl>()
                .Single(tabControl => tabControl.Classes.Contains("settings-tabs"));
            Assert.All(
                settingsTabs.GetVisualDescendants().OfType<TabItem>(),
                tabItem =>
                {
                    Assert.Equal(11, tabItem.FontSize);
                    Assert.Equal(FontWeight.SemiBold, tabItem.FontWeight);
                });
            Assert.True(AssetLoader.Exists(new Uri(
                "avares://TOTP.UI.Avalonia.Desktop/Assets/flags/en.png")));
            Assert.True(AssetLoader.Exists(new Uri(
                "avares://TOTP.UI.Avalonia.Desktop/Assets/flags/de.png")));
            Assert.Contains(
                window.GetVisualDescendants().OfType<ComboBox>(),
                combo => combo.Width == 64
                    && combo.HorizontalContentAlignment == global::Avalonia.Layout.HorizontalAlignment.Center);
            Assert.Equal(320, window.Width);
            Assert.Equal(520, window.Height);
            Assert.Equal(300, window.MinWidth);
            Assert.Equal(200, window.MinHeight);
            var screen = window.Screens.ScreenFromWindow(window);
            Assert.NotNull(screen);
            Assert.InRange(
                window.MaxHeight,
                0,
                (screen.WorkingArea.Height / screen.Scaling) * 0.75);
            Assert.Equal(WindowStartupLocation.CenterScreen, window.WindowStartupLocation);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DarkVariant_UsesEstablishedWpfVisualIdentity()
    {
        var application = Assert.IsType<App>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Dark;
        var window = new MainWindow();

        try
        {
            window.Show();

            Assert.True(window.TryFindResource(
                "BrushWindowBackground",
                ThemeVariant.Dark,
                out var background));
            Assert.Equal(
                Color.Parse("#0C1C33"),
                Assert.IsType<SolidColorBrush>(background).Color);
            Assert.True(window.TryFindResource(
                "BrushAccent",
                ThemeVariant.Dark,
                out var accent));
            Assert.Equal(
                Color.Parse("#7D7FF4"),
                Assert.IsType<SolidColorBrush>(accent).Color);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = ThemeVariant.Dark;
        }
    }

    [AvaloniaFact]
    public void HighContrastVariant_ResolvesDedicatedSemanticPalette()
    {
        var application = Assert.IsType<App>(Application.Current);
        application.RequestedThemeVariant = AvaloniaThemeVariants.HighContrast;
        var window = new MainWindow();

        try
        {
            window.Show();

            Assert.True(window.TryFindResource(
                "BrushWindowBackground",
                AvaloniaThemeVariants.HighContrast,
                out var background));
            Assert.Equal(Colors.Black, Assert.IsType<SolidColorBrush>(background).Color);
            Assert.True(window.TryFindResource(
                "BrushFocus",
                AvaloniaThemeVariants.HighContrast,
                out var focus));
            Assert.Equal(Color.Parse("#00FFFF"), Assert.IsType<SolidColorBrush>(focus).Color);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = ThemeVariant.Dark;
        }
    }
}
