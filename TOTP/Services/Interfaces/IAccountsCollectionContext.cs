using System.Collections.ObjectModel;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public interface IAccountsCollectionContext
{
    ObservableCollection<OtpViewModel> AllOtps { get; }
}
