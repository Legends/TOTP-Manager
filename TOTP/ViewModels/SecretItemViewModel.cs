using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TOTP.Core.Enums;
using TOTP.Core.Validation;
using TOTP.Validation;

namespace TOTP.ViewModels;

public class SecretItemViewModel : INotifyPropertyChanged, IEquatable<SecretItemViewModel>, IEditableObject, IDataErrorInfo
{

    #region Properties
    private Dictionary<string, object>? _storedValues;

    public Guid ID { get; set; }

    bool _IsHighlighted;
    public bool IsHighlighted
    {
        get => _IsHighlighted;
        set
        {
            if (_IsHighlighted != value)
            {
                _IsHighlighted = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _isBeingEdited;

    [JsonIgnore]
    public bool IsBeingEdited
    {
        get => _isBeingEdited;
        set
        {
            _isBeingEdited = value;
            OnPropertyChanged();
        }
    }



    #region TOTP Progress

    string _totpCode;
    public string TotpCode
    {
        get => _totpCode;
        set
        {
            if (_totpCode != value)
            {
                _totpCode = value;
                OnPropertyChanged();
            }
        }
    }

    private double _progress;
    public double Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
    }
    public int PeriodSeconds { get; } = 30;

    int _remainingSeconds;
    public int RemainingSeconds
    {
        get => _remainingSeconds;
        set
        {
            _remainingSeconds = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ElapsedSeconds));
        }
    }

    public int ElapsedSeconds => PeriodSeconds - RemainingSeconds;

    #endregion


    private string? _platform;

    public string? Platform
    {
        get => _platform;
        set
        {
            if (_platform != value)
            {
                _platform = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _secret = string.Empty;
    public string? Secret
    {
        get => _secret;
        set
        {
            if (_secret != value)
            {
                _secret = value;
                OnPropertyChanged();
            }
        }
    }

    public string? EditingSecret
    {
        get;
        set;
    }

    private string? _account;
    public string? Account
    {
        get => _account;
        set
        {
            if (_account != value)
            {
                _account = value;
                OnPropertyChanged();
            }
        }
    }





    #endregion

    private Func<SecretItemViewModel, ValidationError>? _duplicateCheck;

    public void SetDuplicateCheck(Func<SecretItemViewModel, ValidationError> duplicateCheck)
        => _duplicateCheck = duplicateCheck;


    [JsonConstructor]
    public SecretItemViewModel(Guid id, string platform, string secret, string? account = null)
    {
        ID = id;
        Platform = platform;
        Secret = secret;
        Account = account;
    }


    #region IEditableObject Implementation

    public void BeginEdit()
    {
        _storedValues = BackUp();
    }


    public void CancelEdit()
    {
        if (_storedValues == null)
            return;

        foreach (var item in _storedValues)
        {
            var itemProperties = GetType().GetTypeInfo().DeclaredProperties;
            var pDesc = itemProperties.FirstOrDefault(p => p.Name == item.Key);
            pDesc?.SetValue(this, item.Value);
        }
    }

    public void EndEdit()
    {
        _storedValues?.Clear();
        _storedValues = null;
        IsHighlighted = true; // Doesnt work: TODO: highlight the currently edited item, not the next one, event to mainviewmodel?
        Debug.WriteLine("SecretItemViewModel - EndEdit() Called");
    }

    public SecretItemViewModel? Copy()
    {
        return this.MemberwiseClone() as SecretItemViewModel;
    }

    #endregion

    #region IDataErrorInfo Implementation

    [JsonIgnore]
    public string Error => null!;

    [JsonIgnore]
    public string this[string columnName]
    {
        get
        {
            var errors = new List<string>();
            ValidationError error;
            switch (columnName)
            {
                case nameof(Platform): // TODO: Add duplicate check here!
                    error = SecretValidator.ValidatePlatform(Platform);
                    if (error != ValidationError.None)
                    {
                        errors.Add(ValidationMessageMapper.ToMessage(error));
                    }

                    // cross-item duplicate check (injected)
                    // TODO: we have to wire new items with duplicateCheck handler ! soemthing wrong here
                    var isDuplicate = _duplicateCheck?.Invoke(this);
                    if (isDuplicate == ValidationError.PlatformAlreadyExists)
                        errors.Add(ValidationMessageMapper.ToMessage(isDuplicate.Value, this.Platform));
                    break;

                case nameof(Secret):
                    error = SecretValidator.ValidateSecretValue(Secret);
                    if (error != ValidationError.None)
                    {
                        errors.Add(ValidationMessageMapper.ToMessage(error));
                    }
                    break;
            }

            return string.Join(" ", errors); // Or use newline: string.Join("\n", errors)
        }
    }


    #endregion

    #region IEquatable & Overrides

    public bool Equals(SecretItemViewModel? other) => other is not null && ID == other.ID;
    public override bool Equals(object? obj) => obj is SecretItemViewModel o && Equals(o);
    public override int GetHashCode() => ID.GetHashCode();

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion

    #region Backup Logic

    protected Dictionary<string, object> BackUp()
    {
        var dict = new Dictionary<string, object>();
        var itemProperties = GetType().GetTypeInfo().DeclaredProperties;

        foreach (var pDescriptor in itemProperties)
        {
            if (pDescriptor.CanWrite)
                dict.Add(pDescriptor.Name, pDescriptor.GetValue(this)!);
        }

        return dict;
    }

    #endregion

    public void UpdateSelf(SecretItemViewModel changed)
    {
        this.Platform = changed.Platform;
        this.Secret = changed.Secret;
        this.Account = changed.Account;
    }

    #region Inline Error Properties (for flyout binding)

    private string? _platformError;
    [JsonIgnore]
    public string? PlatformError
    {
        get => _platformError;
        private set
        {
            if (_platformError != value)
            {
                _platformError = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _secretError;
    [JsonIgnore]
    public string? SecretError
    {
        get => _secretError;
        private set
        {
            if (_secretError != value)
            {
                _secretError = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _accountError;
    [JsonIgnore]
    public string? AccountError
    {
        get => _accountError;
        private set
        {
            if (_accountError != value)
            {
                _accountError = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Re-runs IDataErrorInfo validation and updates inline error bindings.
    /// </summary>
    public void RefreshValidation()
    {
        PlatformError = this[nameof(Platform)];
        SecretError = this[nameof(Secret)];
        AccountError = this[nameof(Account)];
    }

    #endregion


}


/// <summary>
/// Compares SecretItemViewModel by value (Platform, Account, Secret),
/// case-insensitive, ignoring whitespace/padding in Secret.
/// </summary>
public sealed class SecretItemViewModelValueComparer : IEqualityComparer<SecretItemViewModel>
{
    public static readonly SecretItemViewModelValueComparer Default = new();

    public bool Equals(SecretItemViewModel? x, SecretItemViewModel? y)
    {
        return ReferenceEquals(x, y) || x is not null && y is not null && StringComparer.OrdinalIgnoreCase.Equals(Norm(x.Platform), Norm(y.Platform))
            && StringComparer.OrdinalIgnoreCase.Equals(Norm(x.Account), Norm(y.Account))
            && SecretsEqual(x.Secret, y.Secret);
    }

    public int GetHashCode(SecretItemViewModel obj)
    {
        var hc = new HashCode();

        hc.Add(Norm(obj.Platform), StringComparer.OrdinalIgnoreCase);
        hc.Add(Norm(obj.Account), StringComparer.OrdinalIgnoreCase);
        hc.Add(NormSecret(obj.Secret), StringComparer.OrdinalIgnoreCase);

        return hc.ToHashCode();
    }

    private static string Norm(string? s) => (s ?? string.Empty).Trim();

    // Normalize Base32-like secret: remove spaces/hyphens, trim '=', uppercase.
    private static string NormSecret(string? s) =>
        new string((s ?? "").Where(ch => !char.IsWhiteSpace(ch) && ch != '-').ToArray())
            .TrimEnd('=')
            .ToUpperInvariant();

    private static bool SecretsEqual(string? a, string? b)
    {
        var sa = NormSecret(a);
        var sb = NormSecret(b);

        return StringComparer.Ordinal.Equals(sa, sb);
    }
}
