using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TOTP.Validation;

namespace TOTP.Models;

public class SecretItem : INotifyPropertyChanged, IEquatable<SecretItem>, IEditableObject, IDataErrorInfo
{
    private Dictionary<string, object>? _storedValues;

    [JsonConstructor]
    public SecretItem(string platform, string secret)
    {
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        Secret = secret ?? throw new ArgumentNullException(nameof(secret));
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

            switch (columnName)
            {
                case nameof(Platform):
                    string? error = SecretValidator.ValidatePlatform(Platform);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        errors.Add(error);
                    }
                    break;

                case nameof(Secret):
                    error = SecretValidator.ValidateSecret(Secret);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        errors.Add(error);
                    }
                    break;
            }

            return string.Join(" ", errors); // Or use newline: string.Join("\n", errors)
        }
    }


    #endregion

    #region IEquatable & Overrides

    public bool Equals(SecretItem? other)
    {
        return other is not null &&
               Platform == other.Platform &&
               Secret == other.Secret;
    }

    public override bool Equals(object? obj) => Equals(obj as SecretItem);

    public override int GetHashCode() => HashCode.Combine(Platform, Secret);

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

    #region Properties

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

    private string _platform;

    public string Platform
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

    private string _secret = string.Empty;

    public string Secret
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

    public string? Account { get; set; }

    #endregion
}
