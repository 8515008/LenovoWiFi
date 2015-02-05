using System;
using System.Runtime.InteropServices;
using Lenovo.WiFi.Client.Native;

namespace Lenovo.WiFi.Client
{
    public class DeskbandWarningDialogHook : IDisposable
    {
        private const string HookLibrary = "LenovoWiFiDeskbandHook.dll";
        private bool _disposed;

        private readonly ModuleSafeHandle _module;

        public DeskbandWarningDialogHook()
        {
            _module = NativeMethods.LoadLibrary(HookLibrary);
        }

        ~DeskbandWarningDialogHook() 
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _module.Dispose();

            _disposed = true;
        }

        public void StartHook()
        {
            var functionPtr = NativeMethods.GetProcAddress(_module, "StartHook");
            var startHook = (Action)Marshal.GetDelegateForFunctionPointer(functionPtr, typeof(Action));
            startHook();
        }

        public void StopHook()
        {
            var functionPtr = NativeMethods.GetProcAddress(_module, "StopHook");
            var stopHook = (Action)Marshal.GetDelegateForFunctionPointer(functionPtr, typeof(Action));
            stopHook();
        }
    }
}
