using System;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata
{
    /// <summary>Provides default functionality for all SOAP attributes.</summary>
    [ComVisible(true)]
    public class SoapAttribute : Attribute
    {
        /// <summary>The XML namespace to which the target of the current SOAP attribute is serialized.</summary>
        protected string ProtXmlNamespace;

        private bool _bUseAttribute;

        private bool _bEmbedded;

        /// <summary>A reflection object used by attribute classes derived from the <see cref="T:System.Runtime.Remoting.Metadata.SoapAttribute" /> class to set XML serialization information.</summary>
        protected object ReflectInfo;

        /// <summary>Gets or sets the XML namespace name.</summary>
        /// <returns>The XML namespace name under which the target of the current attribute is serialized.</returns>
        public virtual string XmlNamespace
        {
            get
            {
                return this.ProtXmlNamespace;
            }
            set
            {
                this.ProtXmlNamespace = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the target of the current attribute will be serialized as an XML attribute instead of an XML field.</summary>
        /// <returns>true if the target object of the current attribute must be serialized as an XML attribute; false if the target object must be serialized as a subelement.</returns>
        public virtual bool UseAttribute
        {
            get
            {
                return this._bUseAttribute;
            }
            set
            {
                this._bUseAttribute = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the type must be nested during SOAP serialization.</summary>
        /// <returns>true if the target object must be nested during SOAP serialization; otherwise, false.</returns>
        public virtual bool Embedded
        {
            get
            {
                return this._bEmbedded;
            }
            set
            {
                this._bEmbedded = value;
            }
        }

        internal void SetReflectInfo(object info)
        {
            this.ReflectInfo = info;
        }
    }
}
