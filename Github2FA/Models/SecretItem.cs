using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Models
{
    public class SecretItem : INotifyPropertyChanged, IEquatable<SecretItem>
    {

        private bool _isBeingEdited;
        public bool IsBeingEdited
        {
            get => _isBeingEdited;
            set
            {
                _isBeingEdited = value;
                OnPropertyChanged(); // If implementing INotifyPropertyChanged
            }
        }

        private string _key;
        public string Key
        {
            get => _key;
            set { _key = value; OnPropertyChanged(); }
        }

        private string _value;
        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }
        public SecretItem(string k, string v)
        {
            Key = k;
            Value = v;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public bool Equals(SecretItem? other)
        {
            return other is not null &&
                   Key == other.Key &&
                   Value == other.Value;
        }
    }

}
