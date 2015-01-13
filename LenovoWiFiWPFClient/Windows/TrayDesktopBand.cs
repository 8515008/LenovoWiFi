using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lenovo.WiFi.Client.Windows
{
    [ComImport, Guid("E6442437-6C68-4f52-94DD-2CFED267EFB9")]
    class TrayDesktopBand
    {
    }

    [ComImport, Guid("6D67E846-5B9C-4db8-9CBC-DDE12F4254F1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ITrayDeskBand
    {
        void ShowDeskBand(Guid clsid);

        void HideDeskBand(Guid clsid);

        bool IsDeskBandShown(Guid clsid);

        void DeskBandRegistrationChanged();
    }
}
