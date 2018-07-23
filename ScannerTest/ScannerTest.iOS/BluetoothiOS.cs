using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using ExternalAccessory;
using Foundation;
using ScannerTest.Abstractions;
using ScannerTest.Extensions;
using ScannerTest.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothiOS))]
namespace ScannerTest.iOS
{
    public class BluetoothiOS : IBluetooth
    {
        private CancellationTokenSource _ct { get; set; }
        public byte StartByte { get; set; }
        public byte EndByte { get; set; }
        public string Protocol { get; set; } = "com.socketmobile.chs";
        public EAAccessory Accessory { get; set; }
        private EASession Session { get; set; }

        public void Start(string name, int sleepTime, bool readAsCharArray)
        {
            if (Session != null)
                throw new Exception("Session already started");

            // get session with the given device name
            EAAccessoryManager manager = EAAccessoryManager.SharedAccessoryManager;
            var allaccessorries = manager.ConnectedAccessories;
            foreach (var accessory in allaccessorries)
            {
                if (String.Equals(accessory.Name, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    Session = new EASession(accessory, Protocol);
                    Session.Accessory.Disconnected += Accessory_Disconnected;

                    Session.InputStream.Schedule(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
                    Session.InputStream.OnEvent += InputStream_OnEvent;
                    Session.InputStream.Open();
                }
            }
        }

        public void Cancel()
        {
            if (Session == null) return;

            Session.InputStream.Close();
            Session.InputStream.Unschedule(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
            Session.InputStream.Dispose();

            Session.Dispose();
            Session = null;
        }

        public List<string> PairedDevices()
        {
            //throw new NotImplementedException();
            var devices = new List<string>();

            EAAccessoryManager manager = EAAccessoryManager.SharedAccessoryManager;
            var allaccessorries = manager.ConnectedAccessories;

            return allaccessorries.Select(a => a.Name).ToList();
        }

        private void Accessory_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine($"Accessory Disconnected");
        }

        private void InputStream_OnEvent(object sender, NSStreamEventArgs e)
        {
            Console.WriteLine($"InputStream:{e.StreamEvent}");

            // something was scanned
            if (e.StreamEvent == NSStreamEvent.HasBytesAvailable)
            {
                if (!(sender is NSInputStream input)) return;
                byte[] buffer = new byte[1024];
                var bytesRead = input.Read(buffer, 1024);

                var dataString = GetDataString(buffer, bytesRead);
                Console.WriteLine($"BytesRead:{bytesRead}");
                Console.WriteLine($"Barcode:{dataString}");

                Xamarin.Forms.MessagingCenter.Send<App, string>((App)Xamarin.Forms.Application.Current, "Barcode", dataString);
            }
        }

        private string GetDataString(byte[] buffer, nint bytesRead)
        {
            if (bytesRead <= 0) return null;
            if (buffer.Length <= 0) return null;

            byte startByte = 1;
            byte endByte = 253;

            // find the start  1
            var startPos = buffer.GetFirstOccurance(startByte);
            if (startPos == -1) return null;  // not found

            // find the end byte
            var endPos = buffer.GetFirstOccurance(endByte);
            if (endPos == -1) return null; // not found
            if (endPos <= startPos) 
                throw new Exception("Delimiter characters are out of order");

            var length = endPos - startPos - 1;

            // get the sub-array
            var subArray = buffer.SubArray(startPos + 1, length);

            string s = System.Text.Encoding.UTF8.GetString(subArray, 0, subArray.Length);

            return s;
        }
    }

    public class MyOutputStreamDelegate : NSStreamDelegate
    {
    }
}