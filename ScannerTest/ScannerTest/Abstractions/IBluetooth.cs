using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScannerTest.Abstractions
{
    public interface IBluetooth
    {
        void Start(string name, int sleepTime, bool readAsCharArray);
        void Cancel();
        List<string> PairedDevices();
    }
}