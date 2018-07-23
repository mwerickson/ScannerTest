using System;
using System.Collections.ObjectModel;
using System.Net;
using FreshMvvm;
using PropertyChanged;
using ScannerTest.Abstractions;
using Xamarin.Forms;

namespace ScannerTest.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class ScanningPageModel : FreshBasePageModel
    {
        public ScanningPageModel()
        {
            // subscribe to scanned message
            MessagingCenter.Subscribe<App, string>(this, "Barcode", (sender, arg) => {

                // Add the barcode to a list (first position)
                Barcodes.Insert(0, new ScannedItem{Barcode = arg});
            });
        }

        public ObservableCollection<ScannedItem> Barcodes { get; set; }


        #region COMMANDS

        #endregion



        // OVERRIDES
        public override void Init(object initData)
        {
            Barcodes = new ObservableCollection<ScannedItem>();

            // connect to this device
            DependencyService.Get<IBluetooth>().Start(initData as string, 0, true);

        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
        }

    }

    public class ScannedItem
    {
        public string Barcode { get; set; }
    }
}