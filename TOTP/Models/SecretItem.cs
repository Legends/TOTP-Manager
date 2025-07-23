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
        set { _platform = value; OnPropertyChanged(); }
    }

    private string _secret = string.Empty;
    public string Secret
    {
        get => _secret;
        set { _secret = value; OnPropertyChanged(); }
    }


    public string? Account { get; set; }

    #endregion

    [JsonConstructor]
    public SecretItem(string platform, string secret)
    {
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        Secret = secret ?? throw new ArgumentNullException(nameof(secret));

    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public bool Equals(SecretItem? other)
    {
        return other is not null &&
               Platform == other.Platform &&
               Secret == other.Secret;
    }

    protected Dictionary<string, object> BackUp()
    {
        var dict = new Dictionary<string, object>();
        var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;

        foreach (var pDescriptor in itemProperties)
        {

            if (pDescriptor.CanWrite)
                dict.Add(pDescriptor.Name, pDescriptor.GetValue(this)!);
        }
        return dict;
    }

    private Dictionary<string, object>? storedValues;

    public void BeginEdit()
    {
        this.storedValues = this.BackUp();
    }

    public void CancelEdit()
    {

        if (this.storedValues == null)
            return;

        foreach (var item in this.storedValues)
        {
            var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;
            var pDesc = itemProperties.FirstOrDefault(p => p.Name == item.Key);

            pDesc?.SetValue(this, item.Value);
        }
    }

    public void EndEdit()
    {

        if (this.storedValues != null)
        {
            this.storedValues.Clear();
            this.storedValues = null;
        }
        Debug.WriteLine("End Edit Called");
    }


    public override bool Equals(object? obj) => Equals(obj as SecretItem);

    public override int GetHashCode() => HashCode.Combine(Platform, Secret);

}
