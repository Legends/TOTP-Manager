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

    private int remainingSeconds;
    public int RemainingSeconds
    {
        get => remainingSeconds;
        set
        {
            remainingSeconds = value;
            OnPropertyChanged();
        }
    }

    private int remainingPercent = 10;
    public int TotpRemainingPercent
    {
        get => remainingPercent;
        set
        {
            remainingPercent = value;
            OnPropertyChanged();
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

        Debug.WriteLine("End Edit Called");
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
                    error = SecretValidator.ValidateSecret(Secret);
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
