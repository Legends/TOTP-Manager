using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TOTP.Core.Enums;
using TOTP.Validation;

namespace TOTP.ViewModels;

public class OtpViewModel : INotifyPropertyChanged, IEquatable<OtpViewModel>, IEditableObject, IDataErrorInfo
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
            //OnPropertyChanged();
        }
    }
    
    private string? _issuer;

    /// <summary>
    /// Like Github, Microsoft, etc. This is the "platform" or "service" name
    /// </summary>
    public string? Issuer
    {
        get => _issuer;
        set
        {
            if (_issuer != value)
            {
                _issuer = value;
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

    private string? _accountName;
    /// <summary>
    /// like johne@doe.com, this is the "account name" or "username" associated with the platform.
    /// Optional, but often useful for disambiguation.
    /// </summary>
    public string? AccountName
    {
        get => _accountName;
        set
        {
            if (_accountName != value)
            {
                _accountName = value;
                OnPropertyChanged();
            }
        }
    }





    #endregion

    private Func<OtpViewModel, ValidationError>? _duplicateCheck;

    public void SetDuplicateCheck(Func<OtpViewModel, ValidationError> duplicateCheck)
        => _duplicateCheck = duplicateCheck;


    [JsonConstructor]
    public OtpViewModel(Guid id, string issuer, string secret, string? account = null)
    {
        ID = id;
        Issuer = issuer;
        Secret = secret;
        AccountName = account;
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

    public OtpViewModel? Copy()
    {
        return this.MemberwiseClone() as OtpViewModel;
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
                case nameof(Issuer): // TODO: Add duplicate check here!
                    error = UiValidation.ValidatePlatformName(Issuer);
                    if (error != ValidationError.None)
                    {
                        errors.Add(ValidationMessageMapper.ToMessage(error));
                    }

                    // cross-item duplicate check (injected)
                    // TODO: we have to wire new items with duplicateCheck handler ! soemthing wrong here
                    var isDuplicate = _duplicateCheck?.Invoke(this);
                    if (isDuplicate == ValidationError.PlatformAlreadyExists)
                        errors.Add(ValidationMessageMapper.ToMessage(isDuplicate.Value, this.Issuer));
                    break;

                case nameof(Secret):
                    error = UiValidation.ValidateSecretValue(Secret);
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

    public bool Equals(OtpViewModel? other) => other is not null && ID == other.ID;
    public override bool Equals(object? obj) => obj is OtpViewModel o && Equals(o);
    public override int GetHashCode() => ID.GetHashCode();

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        //if (name == nameof(IsBeingEdited))
        //    return;

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

    public void UpdateSelf(OtpViewModel changed)
    {
        this.Issuer = changed.Issuer;
        this.Secret = changed.Secret;
        this.AccountName = changed.AccountName;
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
        PlatformError = this[nameof(Issuer)];
        SecretError = this[nameof(Secret)];
        AccountError = this[nameof(AccountName)];
    }

    #endregion


}


/// <summary>
/// Compares SecretItemViewModel by value (Platform, Account, Secret),
/// case-insensitive, ignoring whitespace/padding in Secret.
/// </summary>
public sealed class OtpViewModelValueComparer : IEqualityComparer<OtpViewModel>
{
    public static readonly OtpViewModelValueComparer Default = new();

    public bool Equals(OtpViewModel? x, OtpViewModel? y)
    {
        return ReferenceEquals(x, y) || x is not null && y is not null && StringComparer.OrdinalIgnoreCase.Equals(Norm(x.Issuer), Norm(y.Issuer))
            && StringComparer.OrdinalIgnoreCase.Equals(Norm(x.AccountName), Norm(y.AccountName))
            && SecretsEqual(x.Secret, y.Secret);
    }

    public int GetHashCode(OtpViewModel obj)
    {
        var hc = new HashCode();

        hc.Add(Norm(obj.Issuer), StringComparer.OrdinalIgnoreCase);
        hc.Add(Norm(obj.AccountName), StringComparer.OrdinalIgnoreCase);
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
