using System.Collections.ObjectModel;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public interface ITokensCollectionContext
{
    ObservableCollection<OtpViewModel> AllOtps { get; }
}
