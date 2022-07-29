using Abr.IPScanner.Helpers;
using Abr.IPScanner.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Abr.IPScanner.ViewModels
{
    internal class OverviewViewModel : ViewModelBase
    {
        private string _startIp;
        private string _endIp;
        private string _status;
        


        public string StartIp
        {
            get => _startIp;
            set
            {
                _startIp = value;
                OnPropertyChanged();
            }
        }

        public string EndIp
        {
            get => _endIp;
            set
            {
                _endIp = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }        

        public RelayCommand StartScanCommand
        {
            get => new RelayCommand(
                o => StartScan(),
                o => string.IsNullOrWhiteSpace(StartIp) == false && string.IsNullOrWhiteSpace(EndIp) == false &&
                IPAddress.TryParse(StartIp, out IPAddress _) && IPAddress.TryParse(EndIp, out IPAddress _));
        }

        public RelayCommand CloseCommand
        {
            get => new RelayCommand(o => Application.Current.Shutdown());
        }

        public RelayCommand BorderDoubleClickedCommand
        {
            get => new RelayCommand(o =>
            {
                if (Application.Current.MainWindow.WindowState == WindowState.Normal)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                }
                else if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
            });
        }

        public RelayCommand MinimizeCommand
        {
            get => new RelayCommand(o => Application.Current.MainWindow.WindowState = WindowState.Minimized);
        }

        private bool _started;
        public bool Started
        {
            get => _started;
            set
            {
                _started = value;
                OnPropertyChanged();
            }
        }

        private void StartScan()
        {
            var ipsAddressInfos = new List<Model.IPAddressInformation>();
            var ips = RangeFinder.GetIPRange(IPAddress.Parse(StartIp), IPAddress.Parse(EndIp));
            foreach (var ip in ips)
            {
                var newIp = new Model.IPAddressInformation { IpAddress = ip };
                ipsAddressInfos.Add(newIp);
            }

            ScannedIps = new ObservableCollection<Model.IPAddressInformation>(ipsAddressInfos);

            Task.Run(() =>
            {
                var tasks = new List<Task>();

                foreach (var ip in ScannedIps)
                {
                    tasks.Add(
                        Task.Run(() =>
                        {
                            bool reachable = false;
                            if (PingHost(ip))
                            {
                                ResolveName(ip);
                                reachable = true;
                            }

                            lock (StatusLock)
                            {
                                Dispatcher.CurrentDispatcher.Invoke(() =>
                                {
                                    if (reachable)
                                    {
                                        reachableCount++;
                                        ReachableOnes = $"Erreichbar: {reachableCount}";
                                    }

                                    _finishedOnes++;
                                    Status = $"Fortschritt: {(double)_finishedOnes / ScannedIps.Count * 100}% ({_finishedOnes}/{ScannedIps.Count})";                                        
                                
                                });
                            }
                        })
                    );
                };

                Task.WaitAll(tasks.ToArray());
                Dispatcher.CurrentDispatcher.Invoke(() => Status = $"Abgeschlossen.");
            });
        }

        private static readonly object StatusLock = new object();
        private static int _finishedOnes = 0;

        private int reachableCount = 0;

        private string _reachableOnes;
        public string ReachableOnes
        {
            get => _reachableOnes;
            set
            {
                _reachableOnes = value;
                OnPropertyChanged();
            }
        }

        public static bool PingHost(Model.IPAddressInformation nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress.IpAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            Application.Current.Dispatcher.Invoke(() => nameOrAddress.Reachable = pingable);
            return pingable;
        }

        private void ResolveName(Model.IPAddressInformation iPAddressInformation)
        {
            string hostName = "Unknown";

            try
            {
                IPHostEntry hostEntry;

                hostEntry = Dns.GetHostEntry(IPAddress.Parse(iPAddressInformation.IpAddress));
                hostName = hostEntry.HostName;
            }
            catch { }

            Application.Current.Dispatcher.Invoke(() => iPAddressInformation.ResolvedName = hostName);
        }

        private ObservableCollection<Model.IPAddressInformation> _scannedIps = new ObservableCollection<Model.IPAddressInformation>();

        public ObservableCollection<Model.IPAddressInformation> ScannedIps
        {
            get => _scannedIps;
            set
            {
                _scannedIps = value;
                OnPropertyChanged(nameof(ScannedIps));
            }
        }
    }
}
