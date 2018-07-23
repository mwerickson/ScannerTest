using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Acr.UserDialogs;
using FreshMvvm;
using Plugin.BluetoothLE;
using PropertyChanged;
using ScannerTest.Abstractions;
using ScannerTest.Models;
using Xamarin.Forms;

namespace ScannerTest.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class DeviceListPageModel : FreshBasePageModel
    {
        public DeviceListPageModel()
        {
        }

        public ObservableCollection<BluetoothDevice> Devices { get; set; }
        public bool HasDevices => Devices != null && Devices.Count > 0;
        public bool IsBusy { get; set; }


        private BluetoothDevice _selectedItem;

        public BluetoothDevice SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if (value == null) return;
                ItemSelectedCommand.Execute(_selectedItem);
                SelectedItem = null;
            }
        }


        #region COMMANDS

        public Command RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await GetBluetoothDevices();
                });
            }
        }

        public Command ItemSelectedCommand
        {
            get
            {
                return new Command<BluetoothDevice>(async (device) =>
                    {
                        await CoreMethods.PushPageModel<ScanningPageModel>(device.Name);
                    });
            }
        }

        #endregion

        private async Task<bool> GetBluetoothDevices()
        {
            try
            {
                IsBusy = true;

                //CrossBleAdapter.Current.Scan().Subscribe(scanResult =>
                //{
                //    if (scanResult?.Device != null)
                //    {
                //        Devices.Add(new BluetoothDevice { Name = scanResult.Device.Name });

                //        Console.WriteLine($"Bluetooth scanResult: {scanResult.Device.Name}");
                //    }
                //});



                var devices = DependencyService.Get<IBluetooth>().PairedDevices();

                // convert to view model
                foreach (var device in devices)
                {
                    Devices.Add(new BluetoothDevice { Name = device });
                }

                Devices = new ObservableCollection<BluetoothDevice>(Devices.ToList());

                // just in case, let the UI know the list has possibly changed
                //RaisePropertyChanged(nameof(Devices));
                IsBusy = false;
                return true;
            }
            catch (Exception e)
            {
                IsBusy = false;
                Console.WriteLine($"GetBluetoothDevices: Exception Caught -> {e.Message}");
                await UserDialogs.Instance.AlertAsync($"{e.Message}", "Exception", "Ok");
                return false;
            }
        }

        // OVERRIDES
        public override void Init(object initData)
        {
            Devices = new ObservableCollection<BluetoothDevice>();
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            // just in case there are any sessions still around
            DependencyService.Get<IBluetooth>().Cancel();   

            await GetBluetoothDevices();
        }

    }
}