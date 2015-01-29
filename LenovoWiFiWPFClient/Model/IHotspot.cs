namespace Lenovo.WiFi.Client.Model
{
    public interface IHotspot
    {
        string SSID { get; set; }

        string PresharedKey { get; set; }

        string Authentication { get; }

        bool IsStarted { get; set; }

        int ConnectedDeviceCount { get; }

        void Start();

        void Stop();

        void Restart();
    }
}
