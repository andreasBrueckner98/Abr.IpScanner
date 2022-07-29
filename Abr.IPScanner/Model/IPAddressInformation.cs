using System.ComponentModel;

namespace Abr.IPScanner.Model
{
    public class IPAddressInformation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _ipAddress;
        private bool? _reachable;
        private string _resolvedName;

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }

        public bool? Reachable
        {
            get => _reachable;
            set
            {
                _reachable = value;
                OnPropertyChanged(nameof(Reachable));
            }
        }

        public string ResolvedName
        {
            get => _resolvedName;
            set
            {
                _resolvedName = value;
                OnPropertyChanged(nameof(ResolvedName));
            }
        }

    }
}