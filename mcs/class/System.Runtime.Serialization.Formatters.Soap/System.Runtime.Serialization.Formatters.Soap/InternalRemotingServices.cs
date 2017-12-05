using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Serialization;
using System.Security;

namespace System.Runtime.Remoting
{
    /// <summary>Defines utility methods for use by the .NET Framework remoting infrastructure.</summary>
    [ComVisible(true), SecurityCritical]
    public class InternalRemotingServices
    {
        // System.Runtime.Remoting.InternalRemotingServices
        /// <summary>Gets an appropriate SOAP-related attribute for the specified class member or method parameter. </summary>
        /// <param name="reflectionObject">A class member or method parameter.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
        /// </PermissionSet>
        [SecurityCritical]
        public static SoapAttribute GetCachedSoapAttribute(object reflectionObject)
        {
            return null;
        }
    }
}
