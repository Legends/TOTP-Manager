using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Models
{
    public class SecretItem : INotifyPropertyChanged, IEquatable<SecretItem>,IEditableObject
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

        protected Dictionary<string, object> BackUp()
        {
            var dict = new Dictionary<string, object>();
            var itemProperties = this.GetType().GetTypeInfo().DeclaredProperties;

            foreach (var pDescriptor in itemProperties)
            {

                if (pDescriptor.CanWrite)
                    dict.Add(pDescriptor.Name, pDescriptor.GetValue(this));
            }
            return dict;
        }

        private Dictionary<string, object> storedValues;

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

                if (pDesc != null)
                    pDesc.SetValue(this, item.Value);
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
    }

}
