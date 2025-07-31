using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TOTP.Models;

public class SecretItem : INotifyPropertyChanged, IEquatable<SecretItem>, IEditableObject
{
    private Dictionary<string, object>? _storedValues;

    [JsonConstructor]
    public SecretItem(string platform, string secret)
    {
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        Secret = secret ?? throw new ArgumentNullException(nameof(secret));
    }

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
        if (_storedValues != null)
        {
            _storedValues.Clear();
            _storedValues = null;
        }

        Debug.WriteLine("End Edit Called");
    }

    public bool Equals(SecretItem? other)
    {
        return other is not null &&
               Platform == other.Platform &&
               Secret == other.Secret;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    protected Dictionary<string, object> BackUp()
    {
        var dict = new Dictionary<string, object>();
        var itemProperties = GetType().GetTypeInfo().DeclaredProperties;

        foreach (var pDescriptor in itemProperties)
            if (pDescriptor.CanWrite)
                dict.Add(pDescriptor.Name, pDescriptor.GetValue(this)!);
        return dict;
    }


    public override bool Equals(object? obj)
    {
        return Equals(obj as SecretItem);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Platform, Secret);
    }

    #region ### PROPS and VARs

    private bool _isBeingEdited;

    [JsonIgnore]
    public bool IsBeingEdited
    {
        get => _isBeingEdited;
        set
        {
            _isBeingEdited = value;
            OnPropertyChanged(); // If implementing INotifyPropertyChanged
        }
    }

    private string _platform;

    public string Platform
    {
        get => _platform;
        set
        {
            _platform = value;
            OnPropertyChanged();
        }
    }

    private string _secret = string.Empty;

    public string Secret
    {
        get => _secret;
        set
        {
            _secret = value;
            OnPropertyChanged();
        }
    }


    public string? Account { get; set; }

    #endregion
}