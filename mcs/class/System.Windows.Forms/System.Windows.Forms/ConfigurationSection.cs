using System;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Xml;

namespace System.Configuration
{
    /// <summary>Represents a section within a configuration file.</summary>
    public abstract class ConfigurationSection : ConfigurationElement
    {
        private SectionInformation _section;

        /// <summary>Gets a <see cref="T:System.Configuration.SectionInformation" /> object that contains the non-customizable information and functionality of the <see cref="T:System.Configuration.ConfigurationSection" /> object. </summary>
        /// <returns>A <see cref="T:System.Configuration.SectionInformation" /> that contains the non-customizable information and functionality of the <see cref="T:System.Configuration.ConfigurationSection" />.</returns>
        public SectionInformation SectionInformation
        {
            get
            {
                return this._section;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationSection" /> class. </summary>
        protected ConfigurationSection()
        {
            this._section = new SectionInformation(this);
        }

        /// <summary>Returns a custom object when overridden in a derived class.</summary>
        /// <returns>The object representing the section.</returns>
        protected internal virtual object GetRuntimeObject()
        {
            return this;
        }

        /// <summary>Indicates whether this configuration element has been modified since it was last saved or loaded when implemented in a derived class.</summary>
        /// <returns>true if the element has been modified; otherwise, false. </returns>
        protected internal override bool IsModified()
        {
            return this.SectionInformation.IsModifiedFlags() || base.IsModified();
        }

        /// <summary>Resets the value of the <see cref="M:System.Configuration.ConfigurationElement.IsModified" /> method to false when implemented in a derived class.</summary>
        protected internal override void ResetModified()
        {
            this.SectionInformation.ResetModifiedFlags();
            base.ResetModified();
        }

        /// <summary>Reads XML from the configuration file.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> object, which reads from the configuration file. </param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">
        ///   <paramref name="reader" /> found no elements in the configuration file.</exception>
        protected internal virtual void DeserializeSection(XmlReader reader)
        {
            if (!reader.Read() || reader.NodeType != XmlNodeType.Element)
            {
                throw new ConfigurationErrorsException(SR.GetString("Config_base_expected_to_find_element"), reader);
            }
            this.DeserializeElement(reader, false);
        }

        /// <summary>Creates an XML string containing an unmerged view of the <see cref="T:System.Configuration.ConfigurationSection" /> object as a single section to write to a file.</summary>
        /// <returns>An XML string containing an unmerged view of the <see cref="T:System.Configuration.ConfigurationSection" /> object.</returns>
        /// <param name="parentElement">The <see cref="T:System.Configuration.ConfigurationElement" /> instance to use as the parent when performing the un-merge.</param>
        /// <param name="name">The name of the section to create.</param>
        /// <param name="saveMode">The <see cref="T:System.Configuration.ConfigurationSaveMode" /> instance to use when writing to a string.</param>
        protected internal virtual string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
        {
            if (base.CurrentConfiguration != null && base.CurrentConfiguration.TargetFramework != null && !this.ShouldSerializeSectionInTargetVersion(base.CurrentConfiguration.TargetFramework))
            {
                return string.Empty;
            }
            ConfigurationElement.ValidateElement(this, null, true);
            ConfigurationElement configurationElement = base.CreateElement(base.GetType());
            configurationElement.Unmerge(this, parentElement, saveMode);
            StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlTextWriter.Indentation = 4;
            xmlTextWriter.IndentChar = ' ';
            configurationElement.DataToWriteInternal = (saveMode != ConfigurationSaveMode.Minimal);
            if (base.CurrentConfiguration != null && base.CurrentConfiguration.TargetFramework != null)
            {
                this._configRecord.SectionsStack.Push(this);
            }
            configurationElement.SerializeToXmlElement(xmlTextWriter, name);
            if (base.CurrentConfiguration != null && base.CurrentConfiguration.TargetFramework != null)
            {
                this._configRecord.SectionsStack.Pop();
            }
            xmlTextWriter.Flush();
            return stringWriter.ToString();
        }

        /// <summary>Indicates whether the specified property should be serialized when the configuration object hierarchy is serialized for the specified target version of the .NET Framework.</summary>
        /// <returns>true if the <paramref name="property" /> should be serialized; otherwise, false.</returns>
        /// <param name="property">The <see cref="T:System.Configuration.ConfigurationProperty" /> object that is a candidate for serialization.</param>
        /// <param name="propertyName">The name of the <see cref="T:System.Configuration.ConfigurationProperty" /> object as it occurs in XML.</param>
        /// <param name="targetFramework">The target version of the .NET Framework.</param>
        /// <param name="parentConfigurationElement">The parent element of the property.</param>
        protected internal virtual bool ShouldSerializePropertyInTargetVersion(ConfigurationProperty property, string propertyName, FrameworkName targetFramework, ConfigurationElement parentConfigurationElement)
        {
            return true;
        }

        /// <summary>Indicates whether the specified element should be serialized when the configuration object hierarchy is serialized for the specified target version of the .NET Framework.</summary>
        /// <returns>true if the <paramref name="element" /> should be serialized; otherwise, false.</returns>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> object that is a candidate for serialization.</param>
        /// <param name="elementName">The name of the <see cref="T:System.Configuration.ConfigurationElement" /> object as it occurs in XML.</param>
        /// <param name="targetFramework">The target version of the .NET Framework.</param>
        protected internal virtual bool ShouldSerializeElementInTargetVersion(ConfigurationElement element, string elementName, FrameworkName targetFramework)
        {
            return true;
        }

        /// <summary>Indicates whether the current <see cref="T:System.Configuration.ConfigurationSection" /> instance should be serialized when the configuration object hierarchy is serialized for the specified target version of the .NET Framework.</summary>
        /// <returns>true if the current section should be serialized; otherwise, false.</returns>
        /// <param name="targetFramework">The target version of the .NET Framework.</param>
        protected internal virtual bool ShouldSerializeSectionInTargetVersion(FrameworkName targetFramework)
        {
            return true;
        }
    }
}
