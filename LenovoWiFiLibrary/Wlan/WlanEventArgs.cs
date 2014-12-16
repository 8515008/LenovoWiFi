using System;
using System.Net.NetworkInformation;

namespace Lenovo.WiFi.Wlan
{
    public class DeviceConnectedEventArgs : EventArgs
    {
        public DeviceConnectedEventArgs(byte[] macAddress, bool authenticated)
        {
            this.PhysicalAddress = new PhysicalAddress(macAddress);
            this.IsAuthenticated = authenticated;
        }

        public PhysicalAddress PhysicalAddress { get; private set; }
        public bool IsAuthenticated { get; private set; }
    }

    public class DeviceDisconnectedEventArgs : EventArgs
    {
        public DeviceDisconnectedEventArgs(byte[] macAddress)
        {
            this.PhysicalAddress = new PhysicalAddress(macAddress);
        }

        public PhysicalAddress PhysicalAddress { get; private set; }
    }
}
