using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Github2FA.ViewModels
{
    public class KeyValueDialogViewModel : INotifyPropertyChanged
    {
        private string? _key;
        private string? _value;

        public string? Key
        {
            get => _key;
            set { _key = value; OnPropertyChanged(); }
        }

        public string? Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
