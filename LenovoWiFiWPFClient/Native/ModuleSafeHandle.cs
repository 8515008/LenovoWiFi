using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Lenovo.WiFi.Client.Native
{
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    internal sealed class ModuleSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal ModuleSafeHandle()
            : base(true)
        {

        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return Convert.ToBoolean(NativeMethods.FreeLibrary(handle));
        }
    }
}
