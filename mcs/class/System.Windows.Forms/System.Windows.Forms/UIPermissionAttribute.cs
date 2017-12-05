using System;
using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
    /// <summary>Allows security actions for <see cref="T:System.Security.Permissions.UIPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false), ComVisible(true)]
    [Serializable]
    public sealed class UIPermissionAttribute : CodeAccessSecurityAttribute
    {
        private UIPermissionWindow m_windowFlag;

        private UIPermissionClipboard m_clipboardFlag;

        /// <summary>Gets or sets the type of access to the window resources that is permitted.</summary>
        /// <returns>One of the <see cref="T:System.Security.Permissions.UIPermissionWindow" /> values.</returns>
        public UIPermissionWindow Window
        {
            get
            {
                return this.m_windowFlag;
            }
            set
            {
                this.m_windowFlag = value;
            }
        }

        /// <summary>Gets or sets the type of access to the clipboard that is permitted.</summary>
        /// <returns>One of the <see cref="T:System.Security.Permissions.UIPermissionClipboard" /> values.</returns>
        public UIPermissionClipboard Clipboard
        {
            get
            {
                return this.m_clipboardFlag;
            }
            set
            {
                this.m_clipboardFlag = value;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.UIPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
        /// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
        public UIPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        /// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.UIPermission" />.</summary>
        /// <returns>A <see cref="T:System.Security.Permissions.UIPermission" /> that corresponds to this attribute.</returns>
        public override IPermission CreatePermission()
        {
            if (true)
            {
                return new UIPermission(PermissionState.Unrestricted);
            }
            return new UIPermission(this.m_windowFlag, this.m_clipboardFlag);
        }
    }
}
