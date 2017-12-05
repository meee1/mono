using Microsoft.Win32;
using System;
using System.Security;
using System.Security.Permissions;

using Microsoft.Win32.SafeHandles;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Runtime.InteropServices
{
    /// <summary>Replaces the standard common language runtime (CLR) free-threaded marshaler with the standard OLE STA marshaler.</summary>
    [ComVisible(true)]
    public class StandardOleMarshalObject : MarshalByRefObject
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall), SuppressUnmanagedCodeSecurity]
        private delegate int GetMarshalSizeMax_Delegate(IntPtr _this, ref Guid riid, IntPtr pv, int dwDestContext,
            IntPtr pvDestContext, int mshlflags, out int pSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall), SuppressUnmanagedCodeSecurity]
        private delegate int MarshalInterface_Delegate(IntPtr _this, IntPtr pStm, ref Guid riid, IntPtr pv,
            int dwDestContext, IntPtr pvDestContext, int mshlflags);

        private static readonly Guid CLSID_StdMarshal = new Guid("00000017-0000-0000-c000-000000000046");

        /// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.StandardOleMarshalObject" /> class. </summary>
        protected StandardOleMarshalObject()
        {
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private IntPtr GetStdMarshaler(ref Guid riid, int dwDestContext, int mshlflags)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this);
            if (iUnknownForObject != IntPtr.Zero)
            {
                try
                {
                   // if (UnsafeNativeMethods.CoGetStandardMarshal(ref riid, iUnknownForObject, dwDestContext,
                     //       IntPtr.Zero, mshlflags, out zero) == 0)
                    {
                        return zero;
                    }
                }
                finally
                {
                    Marshal.Release(iUnknownForObject);
                }
            }

            throw new InvalidOperationException();
        }
    }
}