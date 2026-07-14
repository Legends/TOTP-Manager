using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using TOTP.Presentation.Services.Interfaces;
using TOTP.ViewModels.Interfaces;
using TOTP.Views;

namespace TOTP.Presentation.Services;

public sealed class SettingsWindowCoordinator : ISettingsWindowCoordinator
{
    private Window? _owner;
    private IMainViewModel? _viewModel;
    private Action? _bringOwnerToFront;
    private SettingsWindow? _window;
    private bool _preloadQueued;
    private bool _preloaded;
    private bool _allowClose;
    private bool _handlingClose;

    public void Attach(Window owner, IMainViewModel viewModel, Action bringOwnerToFront)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(bringOwnerToFront);

        if (_viewModel != null)
            throw new InvalidOperationException("The settings window coordinator is already attached.");

        _owner = owner;
        _viewModel = viewModel;
        _bringOwnerToFront = bringOwnerToFront;
        _viewModel.PropertyChanged += ViewModelPropertyChanged;
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_owner == null || _viewModel == null ||
            (e.PropertyName != nameof(IMainViewModel.IsSettingsViewOpen) &&
             e.PropertyName != nameof(IMainViewModel.SettingsVm)))
        {
            return;
        }

        _owner.Dispatcher.Invoke(SyncWindow);
        if (e.PropertyName == nameof(IMainViewModel.SettingsVm))
            QueuePreload();
    }

    private void SyncWindow()
    {
        if (_viewModel?.IsSettingsViewOpen == true && _viewModel.SettingsVm != null)
        {
            EnsureCreated();
            if (_window == null)
                return;

            if (!_window.IsVisible)
                _window.Show();

            _window.Activate();
            _window.Focus();
        }
        else if (_window?.IsVisible == true)
        {
            _window.Hide();
        }
    }

    private void QueuePreload()
    {
        if (_owner == null || _viewModel?.SettingsVm == null || _preloaded || _preloadQueued)
            return;

        _preloadQueued = true;
        _owner.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
        {
            _preloadQueued = false;
            if (_preloaded || _viewModel?.SettingsVm == null)
                return;

            EnsureCreated();
            _window?.ApplyTemplate();
            _window?.UpdateLayout();
            _preloaded = true;
        }));
    }

    private void EnsureCreated()
    {
        if (_owner == null || _viewModel?.SettingsVm == null)
            return;

        if (_window == null)
        {
            _window = new SettingsWindow
            {
                Owner = _owner,
                DataContext = _viewModel.SettingsVm
            };
            _window.Closing += WindowClosing;
        }
        else if (!ReferenceEquals(_window.DataContext, _viewModel.SettingsVm))
        {
            _window.DataContext = _viewModel.SettingsVm;
        }
    }

    private void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (_allowClose)
            return;

        e.Cancel = true;
        if (_handlingClose)
            return;

        try
        {
            _handlingClose = true;
            _window?.Hide();
            if (_viewModel?.IsSettingsViewOpen == true)
                _viewModel.IsSettingsViewOpen = false;
            _bringOwnerToFront?.Invoke();
        }
        finally
        {
            _handlingClose = false;
        }
    }

    public void Dispose()
    {
        if (_viewModel != null)
            _viewModel.PropertyChanged -= ViewModelPropertyChanged;

        if (_window != null)
        {
            _allowClose = true;
            _window.Closing -= WindowClosing;
            _window.Close();
            _window = null;
        }

        _owner = null;
        _viewModel = null;
        _bringOwnerToFront = null;
    }
}
