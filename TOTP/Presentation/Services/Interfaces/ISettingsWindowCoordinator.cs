using System;
using System.Windows;
using TOTP.ViewModels.Interfaces;

namespace TOTP.Presentation.Services.Interfaces;

public interface ISettingsWindowCoordinator : IDisposable
{
    void Attach(Window owner, IMainViewModel viewModel, Action bringOwnerToFront);
}
