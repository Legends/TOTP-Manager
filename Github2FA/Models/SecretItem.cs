using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Models
{
    public class SecretItem : INotifyPropertyChanged
    {
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
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
