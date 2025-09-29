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


    [JsonConstructor]
    public SecretItemViewModel(string platform, string secret, string? account = null)
    {
        //_platform = platform;
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
                case nameof(Platform):
                    error = SecretValidator.ValidatePlatform(Platform);
                    if (error != ValidationError.None)
                    {
                        errors.Add(ValidationMessageMapper.ToMessage(error));
                    }
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

    public bool Equals(SecretItemViewModel? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return other is not null && string.Equals(Platform, other.Platform, StringComparison.Ordinal) &&
               string.Equals(Secret, other.Secret, StringComparison.Ordinal) &&
               string.Equals(Account, other.Account, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is SecretItemViewModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Platform is null ? 0 : StringComparer.Ordinal.GetHashCode(Platform),
            Secret is null ? 0 : StringComparer.Ordinal.GetHashCode(Secret),
            Account is null ? 0 : StringComparer.Ordinal.GetHashCode(Account)
        );
    }

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

}
