using System.ComponentModel;
using System.Diagnostics;

namespace HostedNetworkManager
{
    class Utilities
    {
        [DebuggerStepThrough]
        internal static void ThrowOnError(int error)
        {
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
        }

        [DebuggerStepThrough]
        internal static void ThrowOnError(uint error)
        {
            ThrowOnError((int)error);
        }
    }
}
