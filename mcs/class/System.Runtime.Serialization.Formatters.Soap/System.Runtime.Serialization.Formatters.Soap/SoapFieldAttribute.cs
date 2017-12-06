using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata
{
    /// <summary>Customizes SOAP generation and processing for a field. This class cannot be inherited.</summary>
    [AttributeUsage(AttributeTargets.Field), ComVisible(true)]
    public sealed class SoapFieldAttribute : SoapAttribute
    {
        [Flags]
        [Serializable]
        private enum ExplicitlySet
        {
            None = 0,
            XmlElementName = 1
        }

        private SoapFieldAttribute.ExplicitlySet _explicitlySet;

        private string _xmlElementName;

        private int _order;

        /// <summary>Gets or sets the XML element name of the field contained in the <see cref="T:System.Runtime.Remoting.Metadata.SoapFieldAttribute" /> attribute.</summary>
        /// <returns>The XML element name of the field contained in this attribute.</returns>
        public string XmlElementName
        {
            get
            {
                if (this._xmlElementName == null && this.ReflectInfo != null)
                {
                    this._xmlElementName = ((FieldInfo)this.ReflectInfo).Name;
                }
                return this._xmlElementName;
            }
            set
            {
                this._xmlElementName = value;
                this._explicitlySet |= SoapFieldAttribute.ExplicitlySet.XmlElementName;
            }
        }

        /// <summary>You should not use this property; it is not used by the .NET Framework remoting infrastructure.</summary>
        /// <returns>A <see cref="T:System.Int32" />.</returns>
        public int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                this._order = value;
            }
        }

        /// <summary>Returns a value indicating whether the current attribute contains interop XML element values.</summary>
        /// <returns>true if the current attribute contains interop XML element values; otherwise, false.</returns>
        public bool IsInteropXmlElement()
        {
            return (this._explicitlySet & SoapFieldAttribute.ExplicitlySet.XmlElementName) > SoapFieldAttribute.ExplicitlySet.None;
        }
    }
}
