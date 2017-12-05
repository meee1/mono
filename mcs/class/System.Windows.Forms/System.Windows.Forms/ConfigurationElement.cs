using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace System.Configuration
{
    /// <summary>Represents a configuration element within a configuration file.</summary>
    public abstract class ConfigurationElement
    {
        private const string LockAttributesKey = "lockAttributes";

        private const string LockAllAttributesExceptKey = "lockAllAttributesExcept";

        private const string LockElementsKey = "lockElements";

        private const string LockAll = "*";

        private const string LockAllElementsExceptKey = "lockAllElementsExcept";

        private const string LockItemKey = "lockItem";

        internal const string DefaultCollectionPropertyName = "";

        private static string[] s_lockAttributeNames = new string[]
        {
            "lockAttributes",
            "lockAllAttributesExcept",
            "lockElements",
            "lockAllElementsExcept",
            "lockItem"
        };

        private static Hashtable s_propertyBags = new Hashtable();

        private static volatile Dictionary<Type, ConfigurationValidatorBase> s_perTypeValidators;

        internal static readonly object s_nullPropertyValue = new object();

        private static ConfigurationElementProperty s_ElementProperty = new ConfigurationElementProperty(new DefaultValidator());

        private bool _bDataToWrite;

        private bool _bModified;

        private bool _bReadOnly;

        private bool _bElementPresent;

        private bool _bInited;

        internal ConfigurationLockCollection _lockedAttributesList;

        internal ConfigurationLockCollection _lockedAllExceptAttributesList;

        internal ConfigurationLockCollection _lockedElementsList;

        internal ConfigurationLockCollection _lockedAllExceptElementsList;

        private readonly ConfigurationValues _values;

        private string _elementTagName;

        private volatile ElementInformation _evaluationElement;

        private ConfigurationElementProperty _elementProperty = ConfigurationElement.s_ElementProperty;

        internal ConfigurationValueFlags _fItemLocked;

        internal ContextInformation _evalContext;

        internal BaseConfigurationRecord _configRecord;

        internal bool DataToWriteInternal
        {
            get
            {
                return this._bDataToWrite;
            }
            set
            {
                this._bDataToWrite = value;
            }
        }

        internal bool ElementPresent
        {
            get
            {
                return this._bElementPresent;
            }
            set
            {
                this._bElementPresent = value;
            }
        }

        internal string ElementTagName
        {
            get
            {
                return this._elementTagName;
            }
        }

        internal ConfigurationLockCollection LockedAttributesList
        {
            get
            {
                return this._lockedAttributesList;
            }
        }

        internal ConfigurationLockCollection LockedAllExceptAttributesList
        {
            get
            {
                return this._lockedAllExceptAttributesList;
            }
        }

        internal ConfigurationValueFlags ItemLocked
        {
            get
            {
                return this._fItemLocked;
            }
        }

        /// <summary>Gets the collection of locked attributes </summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationLockCollection" /> of locked attributes (properties) for the element.</returns>
        public ConfigurationLockCollection LockAttributes
        {
            get
            {
                if (this._lockedAttributesList == null)
                {
                    this._lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                }
                return this._lockedAttributesList;
            }
        }

        /// <summary>Gets the collection of locked attributes.</summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationLockCollection" /> of locked attributes (properties) for the element.</returns>
        public ConfigurationLockCollection LockAllAttributesExcept
        {
            get
            {
                if (this._lockedAllExceptAttributesList == null)
                {
                    this._lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, this._elementTagName);
                }
                return this._lockedAllExceptAttributesList;
            }
        }

        /// <summary>Gets the collection of locked elements.</summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationLockCollection" /> of locked elements.</returns>
        public ConfigurationLockCollection LockElements
        {
            get
            {
                if (this._lockedElementsList == null)
                {
                    this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                }
                return this._lockedElementsList;
            }
        }

        /// <summary>Gets the collection of locked elements.</summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationLockCollection" /> of locked elements.</returns>
        public ConfigurationLockCollection LockAllElementsExcept
        {
            get
            {
                if (this._lockedAllExceptElementsList == null)
                {
                    this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, this._elementTagName);
                }
                return this._lockedAllExceptElementsList;
            }
        }

        /// <summary>Gets or sets a value indicating whether the element is locked.</summary>
        /// <returns>true if the element is locked; otherwise, false. The default is false.</returns>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element has already been locked at a higher configuration level.</exception>
        public bool LockItem
        {
            get
            {
                return (this._fItemLocked & ConfigurationValueFlags.Locked) > ConfigurationValueFlags.Default;
            }
            set
            {
                if ((this._fItemLocked & ConfigurationValueFlags.Inherited) == ConfigurationValueFlags.Default)
                {
                    this._fItemLocked = (value ? ConfigurationValueFlags.Locked : ConfigurationValueFlags.Default);
                    this._fItemLocked |= ConfigurationValueFlags.Modified;
                    return;
                }
                throw new ConfigurationErrorsException(SR.GetString("Config_base_attribute_locked", new object[]
                {
                    "lockItem"
                }));
            }
        }

        /// <summary>Gets or sets a property or attribute of this configuration element.</summary>
        /// <returns>The specified property, attribute, or child element.</returns>
        /// <param name="prop">The property to access. </param>
        /// <exception cref="T:System.Configuration.ConfigurationException">
        ///   <paramref name="prop" /> is null or does not exist within the element.</exception>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">
        ///   <paramref name="prop" /> is read only or locked.</exception>
        protected internal object this[ConfigurationProperty prop]
        {
            get
            {
                object obj = this._values[prop.Name];
                if (obj == null)
                {
                    if (prop.IsConfigurationElementType)
                    {
                        object syncRoot = this._values.SyncRoot;
                        lock (syncRoot)
                        {
                            obj = this._values[prop.Name];
                            if (obj == null)
                            {
                                ConfigurationElement configurationElement = this.CreateElement(prop.Type);
                                if (this._bReadOnly)
                                {
                                    configurationElement.SetReadOnly();
                                }
                                if (typeof(ConfigurationElementCollection).IsAssignableFrom(prop.Type))
                                {
                                    ConfigurationElementCollection configurationElementCollection = configurationElement as ConfigurationElementCollection;
                                    if (prop.AddElementName != null)
                                    {
                                        configurationElementCollection.AddElementName = prop.AddElementName;
                                    }
                                    if (prop.RemoveElementName != null)
                                    {
                                        configurationElementCollection.RemoveElementName = prop.RemoveElementName;
                                    }
                                    if (prop.ClearElementName != null)
                                    {
                                        configurationElementCollection.ClearElementName = prop.ClearElementName;
                                    }
                                }
                                this._values.SetValue(prop.Name, configurationElement, ConfigurationValueFlags.Inherited, null);
                                obj = configurationElement;
                            }
                            goto IL_FF;
                        }
                    }
                    obj = prop.DefaultValue;
                }
                else if (obj == ConfigurationElement.s_nullPropertyValue)
                {
                    obj = null;
                }
                IL_FF:
                if (obj is InvalidPropValue)
                {
                    throw ((InvalidPropValue)obj).Error;
                }
                return obj;
            }
            set
            {
                this.SetPropertyValue(prop, value, false);
            }
        }

        /// <summary>Gets or sets a property, attribute, or child element of this configuration element.</summary>
        /// <returns>The specified property, attribute, or child element</returns>
        /// <param name="propertyName">The name of the <see cref="T:System.Configuration.ConfigurationProperty" /> to access.</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">
        ///   <paramref name="prop" /> is read-only or locked.</exception>
        protected internal object this[string propertyName]
        {
            get
            {
                ConfigurationProperty configurationProperty = this.Properties[propertyName];
                if (configurationProperty == null)
                {
                    configurationProperty = this.Properties[""];
                    if (configurationProperty.ProvidedName != propertyName)
                    {
                        return null;
                    }
                }
                return this[configurationProperty];
            }
            set
            {
                this.SetPropertyValue(this.Properties[propertyName], value, false);
            }
        }

        /// <summary>Gets the collection of properties.</summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection" /> of properties for the element.</returns>
        protected internal virtual ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection result = null;
                if (ConfigurationElement.PropertiesFromType(base.GetType(), out result))
                {
                    ConfigurationElement.ApplyInstanceAttributes(this);
                    ConfigurationElement.ApplyValidatorsRecursive(this);
                }
                return result;
            }
        }

        internal ConfigurationValues Values
        {
            get
            {
                return this._values;
            }
        }

        /// <summary>Gets an <see cref="T:System.Configuration.ElementInformation" /> object that contains the non-customizable information and functionality of the <see cref="T:System.Configuration.ConfigurationElement" /> object. </summary>
        /// <returns>An <see cref="T:System.Configuration.ElementInformation" /> that contains the non-customizable information and functionality of the <see cref="T:System.Configuration.ConfigurationElement" />.</returns>
        public ElementInformation ElementInformation
        {
            get
            {
                if (this._evaluationElement == null)
                {
                    this._evaluationElement = new ElementInformation(this);
                }
                return this._evaluationElement;
            }
        }

        /// <summary>Gets the <see cref="T:System.Configuration.ContextInformation" /> object for the <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
        /// <returns>The <see cref="T:System.Configuration.ContextInformation" /> for the <see cref="T:System.Configuration.ConfigurationElement" />.</returns>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The current element is not associated with a context.</exception>
        protected ContextInformation EvaluationContext
        {
            get
            {
                if (this._evalContext == null)
                {
                    if (this._configRecord == null)
                    {
                        throw new ConfigurationErrorsException(SR.GetString("Config_element_no_context"));
                    }
                    this._evalContext = new ContextInformation(this._configRecord);
                }
                return this._evalContext;
            }
        }

        /// <summary>Gets the <see cref="T:System.Configuration.ConfigurationElementProperty" /> object that represents the <see cref="T:System.Configuration.ConfigurationElement" /> object itself.</summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationElementProperty" /> that represents the <see cref="T:System.Configuration.ConfigurationElement" /> itself.</returns>
        protected internal virtual ConfigurationElementProperty ElementProperty
        {
            get
            {
                return this._elementProperty;
            }
        }

        protected bool HasContext
        {
            get
            {
                return this._configRecord != null;
            }
        }

        /// <summary>Gets a reference to the top-level <see cref="T:System.Configuration.Configuration" /> instance that represents the configuration hierarchy that the current <see cref="T:System.Configuration.ConfigurationElement" /> instance belongs to.</summary>
        /// <returns>The top-level <see cref="T:System.Configuration.Configuration" /> instance that the current <see cref="T:System.Configuration.ConfigurationElement" /> instance belongs to.</returns>
        public Configuration CurrentConfiguration
        {
            get
            {
                if (this._configRecord != null)
                {
                    return this._configRecord.CurrentConfiguration;
                }
                return null;
            }
        }

        internal ConfigurationElement CreateElement(Type type)
        {
            ConfigurationElement configurationElement = (ConfigurationElement)TypeUtil.CreateInstanceRestricted(base.GetType(), type);
            configurationElement.CallInit();
            return configurationElement;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationElement" /> class. </summary>
        protected ConfigurationElement()
        {
            this._values = new ConfigurationValues();
            ConfigurationElement.ApplyValidator(this);
        }

        /// <summary>Sets the <see cref="T:System.Configuration.ConfigurationElement" /> object to its initial state.</summary>
        protected internal virtual void Init()
        {
            this._bInited = true;
        }

        internal void CallInit()
        {
            if (!this._bInited)
            {
                this.Init();
                this._bInited = true;
            }
        }

        internal void MergeLocks(ConfigurationElement source)
        {
            if (source != null)
            {
                this._fItemLocked = (((source._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) ? (ConfigurationValueFlags.Inherited | source._fItemLocked) : this._fItemLocked);
                if (source._lockedAttributesList != null)
                {
                    if (this._lockedAttributesList == null)
                    {
                        this._lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                    }
                    foreach (string name in source._lockedAttributesList)
                    {
                        this._lockedAttributesList.Add(name, ConfigurationValueFlags.Inherited);
                    }
                }
                if (source._lockedAllExceptAttributesList != null)
                {
                    if (this._lockedAllExceptAttributesList == null)
                    {
                        this._lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, string.Empty, source._lockedAllExceptAttributesList);
                    }
                    StringCollection stringCollection = this.IntersectLockCollections(this._lockedAllExceptAttributesList, source._lockedAllExceptAttributesList);
                    this._lockedAllExceptAttributesList.ClearInternal(false);
                    foreach (string current in stringCollection)
                    {
                        this._lockedAllExceptAttributesList.Add(current, ConfigurationValueFlags.Default);
                    }
                }
                if (source._lockedElementsList != null)
                {
                    if (this._lockedElementsList == null)
                    {
                        this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                    }
                    ConfigurationElementCollection configurationElementCollection = null;
                    if (this.Properties.DefaultCollectionProperty != null)
                    {
                        configurationElementCollection = (this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection);
                        if (configurationElementCollection != null)
                        {
                            configurationElementCollection.internalElementTagName = source.ElementTagName;
                            if (configurationElementCollection._lockedElementsList == null)
                            {
                                configurationElementCollection._lockedElementsList = this._lockedElementsList;
                            }
                        }
                    }
                    foreach (string name2 in source._lockedElementsList)
                    {
                        this._lockedElementsList.Add(name2, ConfigurationValueFlags.Inherited);
                        if (configurationElementCollection != null)
                        {
                            configurationElementCollection._lockedElementsList.Add(name2, ConfigurationValueFlags.Inherited);
                        }
                    }
                }
                if (source._lockedAllExceptElementsList != null)
                {
                    if (this._lockedAllExceptElementsList == null || this._lockedAllExceptElementsList.Count == 0)
                    {
                        this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, source._elementTagName, source._lockedAllExceptElementsList);
                    }
                    StringCollection stringCollection2 = this.IntersectLockCollections(this._lockedAllExceptElementsList, source._lockedAllExceptElementsList);
                    if (this.Properties.DefaultCollectionProperty != null)
                    {
                        ConfigurationElementCollection configurationElementCollection2 = this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (configurationElementCollection2 != null && configurationElementCollection2._lockedAllExceptElementsList == null)
                        {
                            configurationElementCollection2._lockedAllExceptElementsList = this._lockedAllExceptElementsList;
                        }
                    }
                    this._lockedAllExceptElementsList.ClearInternal(false);
                    foreach (string current2 in stringCollection2)
                    {
                        if (!this._lockedAllExceptElementsList.Contains(current2) || current2 == this.ElementTagName)
                        {
                            this._lockedAllExceptElementsList.Add(current2, ConfigurationValueFlags.Default);
                        }
                    }
                    if (this._lockedAllExceptElementsList.HasParentElements)
                    {
                        foreach (ConfigurationProperty configurationProperty in this.Properties)
                        {
                            if (!this._lockedAllExceptElementsList.Contains(configurationProperty.Name) && configurationProperty.IsConfigurationElementType)
                            {
                                ((ConfigurationElement)this[configurationProperty]).SetLocked();
                            }
                        }
                    }
                }
            }
        }

        internal void HandleLockedAttributes(ConfigurationElement source)
        {
            if (source != null && (source._lockedAttributesList != null || source._lockedAllExceptAttributesList != null))
            {
                foreach (PropertyInformation propertyInformation in source.ElementInformation.Properties)
                {
                    if (((source._lockedAttributesList != null && (source._lockedAttributesList.Contains(propertyInformation.Name) || source._lockedAttributesList.Contains("*"))) || (source._lockedAllExceptAttributesList != null && !source._lockedAllExceptAttributesList.Contains(propertyInformation.Name))) && propertyInformation.Name != "lockAttributes" && propertyInformation.Name != "lockAllAttributesExcept")
                    {
                        if (this.ElementInformation.Properties[propertyInformation.Name] == null)
                        {
                            ConfigurationPropertyCollection properties = this.Properties;
                            ConfigurationProperty property = source.Properties[propertyInformation.Name];
                            properties.Add(property);
                            this._evaluationElement = null;
                            ConfigurationValueFlags valueFlags = ConfigurationValueFlags.Inherited | ConfigurationValueFlags.Locked;
                            this._values.SetValue(propertyInformation.Name, propertyInformation.Value, valueFlags, source.PropertyInfoInternal(propertyInformation.Name));
                        }
                        else
                        {
                            if (this.ElementInformation.Properties[propertyInformation.Name].ValueOrigin == PropertyValueOrigin.SetHere)
                            {
                                throw new ConfigurationErrorsException(SR.GetString("Config_base_attribute_locked", new object[]
                                {
                                    propertyInformation.Name
                                }));
                            }
                            this.ElementInformation.Properties[propertyInformation.Name].Value = propertyInformation.Value;
                        }
                    }
                }
            }
        }

        internal virtual void AssociateContext(BaseConfigurationRecord configRecord)
        {
            this._configRecord = configRecord;
            this.Values.AssociateContext(configRecord);
        }

        /// <summary>Indicates whether this configuration element has been modified since it was last saved or loaded, when implemented in a derived class.</summary>
        /// <returns>true if the element has been modified; otherwise, false. </returns>
        protected internal virtual bool IsModified()
        {
            if (this._bModified)
            {
                return true;
            }
            if (this._lockedAttributesList != null && this._lockedAttributesList.IsModified)
            {
                return true;
            }
            if (this._lockedAllExceptAttributesList != null && this._lockedAllExceptAttributesList.IsModified)
            {
                return true;
            }
            if (this._lockedElementsList != null && this._lockedElementsList.IsModified)
            {
                return true;
            }
            if (this._lockedAllExceptElementsList != null && this._lockedAllExceptElementsList.IsModified)
            {
                return true;
            }
            if ((this._fItemLocked & ConfigurationValueFlags.Modified) != ConfigurationValueFlags.Default)
            {
                return true;
            }
            foreach (ConfigurationElement configurationElement in this._values.ConfigurationElements)
            {
                if (configurationElement.IsModified())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Resets the value of the <see cref="M:System.Configuration.ConfigurationElement.IsModified" /> method to false when implemented in a derived class.</summary>
        protected internal virtual void ResetModified()
        {
            this._bModified = false;
            if (this._lockedAttributesList != null)
            {
                this._lockedAttributesList.ResetModified();
            }
            if (this._lockedAllExceptAttributesList != null)
            {
                this._lockedAllExceptAttributesList.ResetModified();
            }
            if (this._lockedElementsList != null)
            {
                this._lockedElementsList.ResetModified();
            }
            if (this._lockedAllExceptElementsList != null)
            {
                this._lockedAllExceptElementsList.ResetModified();
            }
            foreach (ConfigurationElement configurationElement in this._values.ConfigurationElements)
            {
                configurationElement.ResetModified();
            }
        }

        /// <summary>Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement" /> object is read-only.</summary>
        /// <returns>true if the <see cref="T:System.Configuration.ConfigurationElement" /> object is read-only; otherwise, false.</returns>
        public virtual bool IsReadOnly()
        {
            return this._bReadOnly;
        }

        /// <summary>Sets the <see cref="M:System.Configuration.ConfigurationElement.IsReadOnly" /> property for the <see cref="T:System.Configuration.ConfigurationElement" /> object and all subelements.</summary>
        protected internal virtual void SetReadOnly()
        {
            this._bReadOnly = true;
            foreach (ConfigurationElement configurationElement in this._values.ConfigurationElements)
            {
                configurationElement.SetReadOnly();
            }
        }

        internal void SetLocked()
        {
            this._fItemLocked = (ConfigurationValueFlags.Locked | ConfigurationValueFlags.XMLParentInherited);
            foreach (ConfigurationProperty prop in this.Properties)
            {
                ConfigurationElement configurationElement = this[prop] as ConfigurationElement;
                if (configurationElement != null)
                {
                    if (configurationElement.GetType() != base.GetType())
                    {
                        configurationElement.SetLocked();
                    }
                    ConfigurationElementCollection configurationElementCollection = this[prop] as ConfigurationElementCollection;
                    if (configurationElementCollection != null)
                    {
                        foreach (object current in configurationElementCollection)
                        {
                            ConfigurationElement configurationElement2 = current as ConfigurationElement;
                            if (configurationElement2 != null)
                            {
                                configurationElement2.SetLocked();
                            }
                        }
                    }
                }
            }
        }

        internal ArrayList GetErrorsList()
        {
            ArrayList arrayList = new ArrayList();
            this.ListErrors(arrayList);
            return arrayList;
        }

        internal ConfigurationErrorsException GetErrors()
        {
            ArrayList errorsList = this.GetErrorsList();
            if (errorsList.Count == 0)
            {
                return null;
            }
            return new ConfigurationErrorsException(errorsList);
        }

        /// <summary>Adds the invalid-property errors in this <see cref="T:System.Configuration.ConfigurationElement" /> object, and in all subelements, to the passed list.</summary>
        /// <param name="errorList">An object that implements the <see cref="T:System.Collections.IList" /> interface.</param>
        protected virtual void ListErrors(IList errorList)
        {
            foreach (InvalidPropValue invalidPropValue in this._values.InvalidValues)
            {
                errorList.Add(invalidPropValue.Error);
            }
            foreach (ConfigurationElement configurationElement in this._values.ConfigurationElements)
            {
                configurationElement.ListErrors(errorList);
                ConfigurationElementCollection configurationElementCollection = configurationElement as ConfigurationElementCollection;
                if (configurationElementCollection != null)
                {
                    foreach (ConfigurationElement configurationElement2 in configurationElementCollection)
                    {
                        configurationElement2.ListErrors(errorList);
                    }
                }
            }
        }

        /// <summary>Used to initialize a default set of values for the <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
        protected internal virtual void InitializeDefault()
        {
        }

        internal void CheckLockedElement(string elementName, XmlReader reader)
        {
            if (elementName != null && ((this._lockedElementsList != null && (this._lockedElementsList.DefinedInParent("*") || this._lockedElementsList.DefinedInParent(elementName))) || (this._lockedAllExceptElementsList != null && this._lockedAllExceptElementsList.Count != 0 && this._lockedAllExceptElementsList.HasParentElements && !this._lockedAllExceptElementsList.DefinedInParent(elementName)) || (this._fItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default))
            {
                throw new ConfigurationErrorsException(SR.GetString("Config_base_element_locked", new object[]
                {
                    elementName
                }), reader);
            }
        }

        internal void RemoveAllInheritedLocks()
        {
            if (this._lockedAttributesList != null)
            {
                this._lockedAttributesList.RemoveInheritedLocks();
            }
            if (this._lockedElementsList != null)
            {
                this._lockedElementsList.RemoveInheritedLocks();
            }
            if (this._lockedAllExceptAttributesList != null)
            {
                this._lockedAllExceptAttributesList.RemoveInheritedLocks();
            }
            if (this._lockedAllExceptElementsList != null)
            {
                this._lockedAllExceptElementsList.RemoveInheritedLocks();
            }
        }

        internal void ResetLockLists(ConfigurationElement parentElement)
        {
            this._lockedAttributesList = null;
            this._lockedAllExceptAttributesList = null;
            this._lockedElementsList = null;
            this._lockedAllExceptElementsList = null;
            if (parentElement != null)
            {
                this._fItemLocked = (((parentElement._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) ? (ConfigurationValueFlags.Inherited | parentElement._fItemLocked) : ConfigurationValueFlags.Default);
                if (parentElement._lockedAttributesList != null)
                {
                    this._lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                    foreach (string name in parentElement._lockedAttributesList)
                    {
                        this._lockedAttributesList.Add(name, ConfigurationValueFlags.Inherited);
                    }
                }
                if (parentElement._lockedAllExceptAttributesList != null)
                {
                    this._lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, string.Empty, parentElement._lockedAllExceptAttributesList);
                }
                if (parentElement._lockedElementsList != null)
                {
                    this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                    if (this.Properties.DefaultCollectionProperty != null)
                    {
                        ConfigurationElementCollection configurationElementCollection = this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (configurationElementCollection != null)
                        {
                            configurationElementCollection.internalElementTagName = parentElement.ElementTagName;
                            if (configurationElementCollection._lockedElementsList == null)
                            {
                                configurationElementCollection._lockedElementsList = this._lockedElementsList;
                            }
                        }
                    }
                    foreach (string name2 in parentElement._lockedElementsList)
                    {
                        this._lockedElementsList.Add(name2, ConfigurationValueFlags.Inherited);
                    }
                }
                if (parentElement._lockedAllExceptElementsList != null)
                {
                    this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, parentElement._elementTagName, parentElement._lockedAllExceptElementsList);
                    if (this.Properties.DefaultCollectionProperty != null)
                    {
                        ConfigurationElementCollection configurationElementCollection2 = this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (configurationElementCollection2 != null && configurationElementCollection2._lockedAllExceptElementsList == null)
                        {
                            configurationElementCollection2._lockedAllExceptElementsList = this._lockedAllExceptElementsList;
                        }
                    }
                }
            }
        }

        /// <summary>Resets the internal state of the <see cref="T:System.Configuration.ConfigurationElement" /> object, including the locks and the properties collections.</summary>
        /// <param name="parentElement">The parent node of the configuration element.</param>
        protected internal virtual void Reset(ConfigurationElement parentElement)
        {
            this.Values.Clear();
            this.ResetLockLists(parentElement);
            ConfigurationPropertyCollection properties = this.Properties;
            this._bElementPresent = false;
            if (parentElement == null)
            {
                this.InitializeDefault();
                return;
            }
            bool flag = false;
            ConfigurationPropertyCollection configurationPropertyCollection = null;
            for (int i = 0; i < parentElement.Values.Count; i++)
            {
                string key = parentElement.Values.GetKey(i);
                ConfigurationValue configValue = parentElement.Values.GetConfigValue(i);
                object obj = (configValue != null) ? configValue.Value : null;
                PropertySourceInfo sourceInfo = (configValue != null) ? configValue.SourceInfo : null;
                ConfigurationProperty configurationProperty = parentElement.Properties[key];
                if (configurationProperty != null && (configurationPropertyCollection == null || configurationPropertyCollection.Contains(configurationProperty.Name)))
                {
                    if (configurationProperty.IsConfigurationElementType)
                    {
                        flag = true;
                    }
                    else
                    {
                        ConfigurationValueFlags valueFlags = ConfigurationValueFlags.Inherited | (((this._lockedAttributesList != null && (this._lockedAttributesList.Contains(key) || this._lockedAttributesList.Contains("*"))) || (this._lockedAllExceptAttributesList != null && !this._lockedAllExceptAttributesList.Contains(key))) ? ConfigurationValueFlags.Locked : ConfigurationValueFlags.Default);
                        if (obj != ConfigurationElement.s_nullPropertyValue)
                        {
                            this._values.SetValue(key, obj, valueFlags, sourceInfo);
                        }
                        if (!properties.Contains(key))
                        {
                            properties.Add(configurationProperty);
                            this._values.SetValue(key, obj, valueFlags, sourceInfo);
                        }
                    }
                }
            }
            if (flag)
            {
                for (int j = 0; j < parentElement.Values.Count; j++)
                {
                    string key2 = parentElement.Values.GetKey(j);
                    object obj2 = parentElement.Values[j];
                    ConfigurationProperty configurationProperty2 = parentElement.Properties[key2];
                    if (configurationProperty2 != null && configurationProperty2.IsConfigurationElementType)
                    {
                        ConfigurationElement configurationElement = (ConfigurationElement)this[configurationProperty2];
                        configurationElement.Reset((ConfigurationElement)obj2);
                    }
                }
            }
        }

        /// <summary>Compares the current <see cref="T:System.Configuration.ConfigurationElement" /> instance to the specified object.</summary>
        /// <returns>true if the object to compare with is equal to the current <see cref="T:System.Configuration.ConfigurationElement" /> instance; otherwise, false. The default is false. </returns>
        /// <param name="compareTo">The object to compare with.</param>
        public override bool Equals(object compareTo)
        {
            ConfigurationElement configurationElement = compareTo as ConfigurationElement;
            if (configurationElement == null || compareTo.GetType() != base.GetType() || (configurationElement != null && configurationElement.Properties.Count != this.Properties.Count))
            {
                return false;
            }
            foreach (ConfigurationProperty configurationProperty in this.Properties)
            {
                if (!object.Equals(this.Values[configurationProperty.Name], configurationElement.Values[configurationProperty.Name]) && ((this.Values[configurationProperty.Name] != null && this.Values[configurationProperty.Name] != ConfigurationElement.s_nullPropertyValue) || !object.Equals(configurationElement.Values[configurationProperty.Name], configurationProperty.DefaultValue)) && ((configurationElement.Values[configurationProperty.Name] != null && configurationElement.Values[configurationProperty.Name] != ConfigurationElement.s_nullPropertyValue) || !object.Equals(this.Values[configurationProperty.Name], configurationProperty.DefaultValue)))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Gets a unique value representing the current <see cref="T:System.Configuration.ConfigurationElement" /> instance.</summary>
        /// <returns>A unique value representing the current <see cref="T:System.Configuration.ConfigurationElement" /> instance.</returns>
        public override int GetHashCode()
        {
            int num = 0;
            foreach (ConfigurationProperty prop in this.Properties)
            {
                object obj = this[prop];
                if (obj != null)
                {
                    num ^= this[prop].GetHashCode();
                }
            }
            return num;
        }

        private static void ApplyInstanceAttributes(object instance)
        {
            Type type = instance.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                ConfigurationPropertyAttribute configurationPropertyAttribute = Attribute.GetCustomAttribute(propertyInfo, typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;
                if (configurationPropertyAttribute != null)
                {
                    Type propertyType = propertyInfo.PropertyType;
                    if (typeof(ConfigurationElementCollection).IsAssignableFrom(propertyType))
                    {
                        ConfigurationCollectionAttribute configurationCollectionAttribute = Attribute.GetCustomAttribute(propertyInfo, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
                        if (configurationCollectionAttribute == null)
                        {
                            configurationCollectionAttribute = (Attribute.GetCustomAttribute(propertyType, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute);
                        }
                        ConfigurationElementCollection configurationElementCollection = propertyInfo.GetValue(instance, null) as ConfigurationElementCollection;
                        if (configurationElementCollection == null)
                        {
                            throw new ConfigurationErrorsException(SR.GetString("Config_element_null_instance", new object[]
                            {
                                propertyInfo.Name,
                                configurationPropertyAttribute.Name
                            }));
                        }
                        if (configurationCollectionAttribute != null)
                        {
                            if (configurationCollectionAttribute.AddItemName.IndexOf(',') == -1)
                            {
                                configurationElementCollection.AddElementName = configurationCollectionAttribute.AddItemName;
                            }
                            configurationElementCollection.RemoveElementName = configurationCollectionAttribute.RemoveItemName;
                            configurationElementCollection.ClearElementName = configurationCollectionAttribute.ClearItemsName;
                        }
                    }
                    else if (typeof(ConfigurationElement).IsAssignableFrom(propertyType))
                    {
                        object value = propertyInfo.GetValue(instance, null);
                        if (value == null)
                        {
                            throw new ConfigurationErrorsException(SR.GetString("Config_element_null_instance", new object[]
                            {
                                propertyInfo.Name,
                                configurationPropertyAttribute.Name
                            }));
                        }
                        ConfigurationElement.ApplyInstanceAttributes(value);
                    }
                }
            }
        }

        private static bool PropertiesFromType(Type type, out ConfigurationPropertyCollection result)
        {
            ConfigurationPropertyCollection configurationPropertyCollection = (ConfigurationPropertyCollection)ConfigurationElement.s_propertyBags[type];
            result = null;
            bool result2 = false;
            if (configurationPropertyCollection == null)
            {
                object syncRoot = ConfigurationElement.s_propertyBags.SyncRoot;
                lock (syncRoot)
                {
                    configurationPropertyCollection = (ConfigurationPropertyCollection)ConfigurationElement.s_propertyBags[type];
                    if (configurationPropertyCollection == null)
                    {
                        configurationPropertyCollection = ConfigurationElement.CreatePropertyBagFromType(type);
                        ConfigurationElement.s_propertyBags[type] = configurationPropertyCollection;
                        result2 = true;
                    }
                }
            }
            result = configurationPropertyCollection;
            return result2;
        }

        private static ConfigurationPropertyCollection CreatePropertyBagFromType(Type type)
        {
            if (typeof(ConfigurationElement).IsAssignableFrom(type))
            {
                ConfigurationValidatorAttribute configurationValidatorAttribute = Attribute.GetCustomAttribute(type, typeof(ConfigurationValidatorAttribute)) as ConfigurationValidatorAttribute;
                if (configurationValidatorAttribute != null)
                {
                    configurationValidatorAttribute.SetDeclaringType(type);
                    ConfigurationValidatorBase validatorInstance = configurationValidatorAttribute.ValidatorInstance;
                    if (validatorInstance != null)
                    {
                        ConfigurationElement.CachePerTypeValidator(type, validatorInstance);
                    }
                }
            }
            ConfigurationPropertyCollection configurationPropertyCollection = new ConfigurationPropertyCollection();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInformation = properties[i];
                ConfigurationProperty configurationProperty = ConfigurationElement.CreateConfigurationPropertyFromAttributes(propertyInformation);
                if (configurationProperty != null)
                {
                    configurationPropertyCollection.Add(configurationProperty);
                }
            }
            return configurationPropertyCollection;
        }

        private static ConfigurationProperty CreateConfigurationPropertyFromAttributes(PropertyInfo propertyInformation)
        {
            ConfigurationProperty configurationProperty = null;
            ConfigurationPropertyAttribute configurationPropertyAttribute = Attribute.GetCustomAttribute(propertyInformation, typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;
            if (configurationPropertyAttribute != null)
            {
                configurationProperty = new ConfigurationProperty(propertyInformation);
            }
            if (configurationProperty != null && typeof(ConfigurationElement).IsAssignableFrom(configurationProperty.Type))
            {
                ConfigurationPropertyCollection configurationPropertyCollection = null;
                ConfigurationElement.PropertiesFromType(configurationProperty.Type, out configurationPropertyCollection);
            }
            return configurationProperty;
        }

        private static void CachePerTypeValidator(Type type, ConfigurationValidatorBase validator)
        {
            if (ConfigurationElement.s_perTypeValidators == null)
            {
                ConfigurationElement.s_perTypeValidators = new Dictionary<Type, ConfigurationValidatorBase>();
            }
            if (!validator.CanValidate(type))
            {
                throw new ConfigurationErrorsException(SR.GetString("Validator_does_not_support_elem_type", new object[]
                {
                    type.Name
                }));
            }
            ConfigurationElement.s_perTypeValidators.Add(type, validator);
        }

        private static void ApplyValidatorsRecursive(ConfigurationElement root)
        {
            ConfigurationElement.ApplyValidator(root);
            foreach (ConfigurationElement root2 in root._values.ConfigurationElements)
            {
                ConfigurationElement.ApplyValidatorsRecursive(root2);
            }
        }

        private static void ApplyValidator(ConfigurationElement elem)
        {
            if (ConfigurationElement.s_perTypeValidators != null && ConfigurationElement.s_perTypeValidators.ContainsKey(elem.GetType()))
            {
                elem._elementProperty = new ConfigurationElementProperty(ConfigurationElement.s_perTypeValidators[elem.GetType()]);
            }
        }

        /// <summary>Sets a property to the specified value.</summary>
        /// <param name="prop">The element property to set. </param>
        /// <param name="value">The value to assign to the property.</param>
        /// <param name="ignoreLocks">true if the locks on the property should be ignored; otherwise, false.</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">Occurs if the element is read-only or <paramref name="ignoreLocks" /> is true but the locks cannot be ignored.</exception>
        protected void SetPropertyValue(ConfigurationProperty prop, object value, bool ignoreLocks)
        {
            if (this.IsReadOnly())
            {
                throw new ConfigurationErrorsException(SR.GetString("Config_base_read_only"));
            }
            if (!ignoreLocks && ((this._lockedAllExceptAttributesList != null && this._lockedAllExceptAttributesList.HasParentElements && !this._lockedAllExceptAttributesList.DefinedInParent(prop.Name)) || (this._lockedAttributesList != null && (this._lockedAttributesList.DefinedInParent(prop.Name) || this._lockedAttributesList.DefinedInParent("*"))) || ((this._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default && (this._fItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default)))
            {
                throw new ConfigurationErrorsException(SR.GetString("Config_base_attribute_locked", new object[]
                {
                    prop.Name
                }));
            }
            this._bModified = true;
            if (value != null)
            {
                prop.Validate(value);
            }
            this._values[prop.Name] = ((value != null) ? value : ConfigurationElement.s_nullPropertyValue);
        }

        internal PropertySourceInfo PropertyInfoInternal(string propertyName)
        {
            return this._values.GetSourceInfo(propertyName);
        }

        internal string PropertyFileName(string propertyName)
        {
            PropertySourceInfo propertySourceInfo = this.PropertyInfoInternal(propertyName);
            if (propertySourceInfo == null)
            {
                propertySourceInfo = this.PropertyInfoInternal(string.Empty);
            }
            if (propertySourceInfo == null)
            {
                return string.Empty;
            }
            return propertySourceInfo.FileName;
        }

        internal int PropertyLineNumber(string propertyName)
        {
            PropertySourceInfo propertySourceInfo = this.PropertyInfoInternal(propertyName);
            if (propertySourceInfo == null)
            {
                propertySourceInfo = this.PropertyInfoInternal(string.Empty);
            }
            if (propertySourceInfo == null)
            {
                return 0;
            }
            return propertySourceInfo.LineNumber;
        }

        internal virtual void Dump(TextWriter tw)
        {
            tw.WriteLine("Type: " + base.GetType().FullName);
            PropertyInfo[] properties = base.GetType().GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                tw.WriteLine("{0}: {1}", propertyInfo.Name, propertyInfo.GetValue(this, null));
            }
        }

        /// <summary>Modifies the <see cref="T:System.Configuration.ConfigurationElement" /> object to remove all values that should not be saved. </summary>
        /// <param name="sourceElement">A <see cref="T:System.Configuration.ConfigurationElement" /> at the current level containing a merged view of the properties.</param>
        /// <param name="parentElement">The parent <see cref="T:System.Configuration.ConfigurationElement" />, or null if this is the top level.</param>
        /// <param name="saveMode">A <see cref="T:System.Configuration.ConfigurationSaveMode" /> that determines which property values to include.</param>
        protected internal virtual void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement != null)
            {
                bool flag = false;
                this._lockedAllExceptAttributesList = sourceElement._lockedAllExceptAttributesList;
                this._lockedAllExceptElementsList = sourceElement._lockedAllExceptElementsList;
                this._fItemLocked = sourceElement._fItemLocked;
                this._lockedAttributesList = sourceElement._lockedAttributesList;
                this._lockedElementsList = sourceElement._lockedElementsList;
                this.AssociateContext(sourceElement._configRecord);
                if (parentElement != null)
                {
                    if (parentElement._lockedAttributesList != null)
                    {
                        this._lockedAttributesList = this.UnMergeLockList(sourceElement._lockedAttributesList, parentElement._lockedAttributesList, saveMode);
                    }
                    if (parentElement._lockedElementsList != null)
                    {
                        this._lockedElementsList = this.UnMergeLockList(sourceElement._lockedElementsList, parentElement._lockedElementsList, saveMode);
                    }
                    if (parentElement._lockedAllExceptAttributesList != null)
                    {
                        this._lockedAllExceptAttributesList = this.UnMergeLockList(sourceElement._lockedAllExceptAttributesList, parentElement._lockedAllExceptAttributesList, saveMode);
                    }
                    if (parentElement._lockedAllExceptElementsList != null)
                    {
                        this._lockedAllExceptElementsList = this.UnMergeLockList(sourceElement._lockedAllExceptElementsList, parentElement._lockedAllExceptElementsList, saveMode);
                    }
                }
                ConfigurationPropertyCollection properties = this.Properties;
                ConfigurationPropertyCollection configurationPropertyCollection = null;
                for (int i = 0; i < sourceElement.Values.Count; i++)
                {
                    string key = sourceElement.Values.GetKey(i);
                    object obj = sourceElement.Values[i];
                    ConfigurationProperty configurationProperty = sourceElement.Properties[key];
                    if (configurationProperty != null && (configurationPropertyCollection == null || configurationPropertyCollection.Contains(configurationProperty.Name)))
                    {
                        if (configurationProperty.IsConfigurationElementType)
                        {
                            flag = true;
                        }
                        else if (obj != ConfigurationElement.s_nullPropertyValue && !properties.Contains(key))
                        {
                            ConfigurationValueFlags valueFlags = sourceElement.Values.RetrieveFlags(key);
                            this._values.SetValue(key, obj, valueFlags, null);
                            properties.Add(configurationProperty);
                        }
                    }
                }
                foreach (ConfigurationProperty configurationProperty2 in this.Properties)
                {
                    if (configurationProperty2 != null && (configurationPropertyCollection == null || configurationPropertyCollection.Contains(configurationProperty2.Name)))
                    {
                        if (configurationProperty2.IsConfigurationElementType)
                        {
                            flag = true;
                        }
                        else
                        {
                            object obj2 = sourceElement.Values[configurationProperty2.Name];
                            if ((configurationProperty2.IsRequired || saveMode == ConfigurationSaveMode.Full) && (obj2 == null || obj2 == ConfigurationElement.s_nullPropertyValue) && configurationProperty2.DefaultValue != null)
                            {
                                obj2 = configurationProperty2.DefaultValue;
                            }
                            if (obj2 != null && obj2 != ConfigurationElement.s_nullPropertyValue)
                            {
                                object obj3 = null;
                                if (parentElement != null)
                                {
                                    obj3 = parentElement.Values[configurationProperty2.Name];
                                }
                                if (obj3 == null)
                                {
                                    obj3 = configurationProperty2.DefaultValue;
                                }
                                switch (saveMode)
                                {
                                    case ConfigurationSaveMode.Modified:
                                        {
                                            bool flag2 = sourceElement.Values.IsModified(configurationProperty2.Name);
                                            bool flag3 = sourceElement.Values.IsInherited(configurationProperty2.Name);
                                            if ((configurationProperty2.IsRequired | flag2) || !flag3 || ((parentElement == null & flag3) && !object.Equals(obj2, obj3)))
                                            {
                                                this._values[configurationProperty2.Name] = obj2;
                                            }
                                            break;
                                        }
                                    case ConfigurationSaveMode.Minimal:
                                        if (!object.Equals(obj2, obj3) || configurationProperty2.IsRequired)
                                        {
                                            this._values[configurationProperty2.Name] = obj2;
                                        }
                                        break;
                                    case ConfigurationSaveMode.Full:
                                        if (obj2 != null && obj2 != ConfigurationElement.s_nullPropertyValue)
                                        {
                                            this._values[configurationProperty2.Name] = obj2;
                                        }
                                        else
                                        {
                                            this._values[configurationProperty2.Name] = obj3;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                if (flag)
                {
                    foreach (ConfigurationProperty configurationProperty3 in this.Properties)
                    {
                        if (configurationProperty3.IsConfigurationElementType)
                        {
                            ConfigurationElement parentElement2 = (ConfigurationElement)((parentElement != null) ? parentElement[configurationProperty3] : null);
                            ConfigurationElement configurationElement = (ConfigurationElement)this[configurationProperty3];
                            if ((ConfigurationElement)sourceElement[configurationProperty3] != null)
                            {
                                configurationElement.Unmerge((ConfigurationElement)sourceElement[configurationProperty3], parentElement2, saveMode);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Writes the outer tags of this configuration element to the configuration file when implemented in a derived class.</summary>
        /// <returns>true if writing was successful; otherwise, false.</returns>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> that writes to the configuration file. </param>
        /// <param name="elementName">The name of the <see cref="T:System.Configuration.ConfigurationElement" /> to be written. </param>
        /// <exception cref="T:System.Exception">The element has multiple child elements. </exception>
        protected internal virtual bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            if (this._configRecord != null && this._configRecord.TargetFramework != null)
            {
                ConfigurationSection configurationSection = null;
                if (this._configRecord.SectionsStack.Count > 0)
                {
                    configurationSection = (this._configRecord.SectionsStack.Peek() as ConfigurationSection);
                }
                if (configurationSection != null && !configurationSection.ShouldSerializeElementInTargetVersion(this, elementName, this._configRecord.TargetFramework))
                {
                    return false;
                }
            }
            bool flag = this._bDataToWrite;
            if ((this._lockedElementsList != null && this._lockedElementsList.DefinedInParent(elementName)) || (this._lockedAllExceptElementsList != null && this._lockedAllExceptElementsList.HasParentElements && !this._lockedAllExceptElementsList.DefinedInParent(elementName)))
            {
                return flag;
            }
            if (this.SerializeElement(null, false))
            {
                if (writer != null)
                {
                    writer.WriteStartElement(elementName);
                }
                flag |= this.SerializeElement(writer, false);
                if (writer != null)
                {
                    writer.WriteEndElement();
                }
            }
            return flag;
        }

        /// <summary>Writes the contents of this configuration element to the configuration file when implemented in a derived class.</summary>
        /// <returns>true if any data was actually serialized; otherwise, false.</returns>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> that writes to the configuration file. </param>
        /// <param name="serializeCollectionKey">true to serialize only the collection key properties; otherwise, false. </param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The current attribute is locked at a higher configuration level.</exception>
        protected internal virtual bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            this.PreSerialize(writer);
            bool flag = this._bDataToWrite;
            bool flag2 = false;
            bool flag3 = false;
            ConfigurationPropertyCollection properties = this.Properties;
            ConfigurationPropertyCollection configurationPropertyCollection = null;
            for (int i = 0; i < this._values.Count; i++)
            {
                string key = this._values.GetKey(i);
                object obj = this._values[i];
                ConfigurationProperty configurationProperty = properties[key];
                if (configurationProperty != null && (configurationPropertyCollection == null || configurationPropertyCollection.Contains(configurationProperty.Name)))
                {
                    if (configurationProperty.IsVersionCheckRequired && this._configRecord != null && this._configRecord.TargetFramework != null)
                    {
                        ConfigurationSection configurationSection = null;
                        if (this._configRecord.SectionsStack.Count > 0)
                        {
                            configurationSection = (this._configRecord.SectionsStack.Peek() as ConfigurationSection);
                        }
                        if (configurationSection != null && !configurationSection.ShouldSerializePropertyInTargetVersion(configurationProperty, configurationProperty.Name, this._configRecord.TargetFramework, this))
                        {
                            goto IL_1F5;
                        }
                    }
                    if (configurationProperty.IsConfigurationElementType)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        if ((this._lockedAllExceptAttributesList != null && this._lockedAllExceptAttributesList.HasParentElements && !this._lockedAllExceptAttributesList.DefinedInParent(configurationProperty.Name)) || (this._lockedAttributesList != null && this._lockedAttributesList.DefinedInParent(configurationProperty.Name)))
                        {
                            if (configurationProperty.IsRequired)
                            {
                                throw new ConfigurationErrorsException(SR.GetString("Config_base_required_attribute_locked", new object[]
                                {
                                    configurationProperty.Name
                                }));
                            }
                            obj = ConfigurationElement.s_nullPropertyValue;
                        }
                        if (obj != ConfigurationElement.s_nullPropertyValue && (!serializeCollectionKey || configurationProperty.IsKey))
                        {
                            string text;
                            if (obj is InvalidPropValue)
                            {
                                text = ((InvalidPropValue)obj).Value;
                            }
                            else
                            {
                                configurationProperty.Validate(obj);
                                text = configurationProperty.ConvertToString(obj);
                            }
                            if (text != null && writer != null)
                            {
                                if (configurationProperty.IsTypeStringTransformationRequired)
                                {
                                    text = this.GetTransformedTypeString(text);
                                }
                                if (configurationProperty.IsAssemblyStringTransformationRequired)
                                {
                                    text = this.GetTransformedAssemblyString(text);
                                }
                                writer.WriteAttributeString(configurationProperty.Name, text);
                            }
                            flag = (flag || text != null);
                        }
                    }
                }
                IL_1F5:;
            }
            if (!serializeCollectionKey)
            {
                flag |= this.SerializeLockList(this._lockedAttributesList, "lockAttributes", writer);
                flag |= this.SerializeLockList(this._lockedAllExceptAttributesList, "lockAllAttributesExcept", writer);
                flag |= this.SerializeLockList(this._lockedElementsList, "lockElements", writer);
                flag |= this.SerializeLockList(this._lockedAllExceptElementsList, "lockAllElementsExcept", writer);
                if ((this._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default && (this._fItemLocked & ConfigurationValueFlags.Inherited) == ConfigurationValueFlags.Default && (this._fItemLocked & ConfigurationValueFlags.XMLParentInherited) == ConfigurationValueFlags.Default)
                {
                    flag = true;
                    if (writer != null)
                    {
                        writer.WriteAttributeString("lockItem", true.ToString().ToLower(CultureInfo.InvariantCulture));
                    }
                }
            }
            if (flag2)
            {
                for (int j = 0; j < this._values.Count; j++)
                {
                    string key2 = this._values.GetKey(j);
                    object obj2 = this._values[j];
                    ConfigurationProperty configurationProperty2 = properties[key2];
                    if ((!serializeCollectionKey || configurationProperty2.IsKey) && obj2 is ConfigurationElement && (this._lockedElementsList == null || !this._lockedElementsList.DefinedInParent(key2)) && (this._lockedAllExceptElementsList == null || !this._lockedAllExceptElementsList.HasParentElements || this._lockedAllExceptElementsList.DefinedInParent(key2)))
                    {
                        ConfigurationElement configurationElement = (ConfigurationElement)obj2;
                        if (configurationProperty2.Name != ConfigurationProperty.DefaultCollectionPropertyName)
                        {
                            flag |= configurationElement.SerializeToXmlElement(writer, configurationProperty2.Name);
                        }
                        else
                        {
                            if (flag3)
                            {
                                throw new ConfigurationErrorsException(SR.GetString("Config_base_element_cannot_have_multiple_child_elements", new object[]
                                {
                                    configurationProperty2.Name
                                }));
                            }
                            configurationElement._lockedAttributesList = null;
                            configurationElement._lockedAllExceptAttributesList = null;
                            configurationElement._lockedElementsList = null;
                            configurationElement._lockedAllExceptElementsList = null;
                            flag |= configurationElement.SerializeElement(writer, false);
                            flag3 = true;
                        }
                    }
                }
            }
            return flag;
        }

        private bool SerializeLockList(ConfigurationLockCollection list, string elementKey, XmlWriter writer)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (list != null)
            {
                foreach (string text in list)
                {
                    if (!list.DefinedInParent(text))
                    {
                        if (stringBuilder.Length != 0)
                        {
                            stringBuilder.Append(',');
                        }
                        stringBuilder.Append(text);
                    }
                }
            }
            if (writer != null && stringBuilder.Length != 0)
            {
                writer.WriteAttributeString(elementKey, stringBuilder.ToString());
            }
            return stringBuilder.Length != 0;
        }

        internal void ReportInvalidLock(string attribToLockTrim, ConfigurationLockCollectionType lockedType, ConfigurationValue value, string collectionProperties)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(collectionProperties) && (lockedType == ConfigurationLockCollectionType.LockedElements || lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList))
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.Append(',');
                }
                stringBuilder.Append(collectionProperties);
            }
            foreach (object current in this.Properties)
            {
                ConfigurationProperty configurationProperty = (ConfigurationProperty)current;
                if (configurationProperty.Name != "lockAttributes" && configurationProperty.Name != "lockAllAttributesExcept" && configurationProperty.Name != "lockElements" && configurationProperty.Name != "lockAllElementsExcept")
                {
                    if (lockedType == ConfigurationLockCollectionType.LockedElements || lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList)
                    {
                        if (typeof(ConfigurationElement).IsAssignableFrom(configurationProperty.Type))
                        {
                            if (stringBuilder.Length != 0)
                            {
                                stringBuilder.Append(", ");
                            }
                            stringBuilder.Append("'");
                            stringBuilder.Append(configurationProperty.Name);
                            stringBuilder.Append("'");
                        }
                    }
                    else if (!typeof(ConfigurationElement).IsAssignableFrom(configurationProperty.Type))
                    {
                        if (stringBuilder.Length != 0)
                        {
                            stringBuilder.Append(", ");
                        }
                        stringBuilder.Append("'");
                        stringBuilder.Append(configurationProperty.Name);
                        stringBuilder.Append("'");
                    }
                }
            }
            string @string;
            if (lockedType == ConfigurationLockCollectionType.LockedElements || lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList)
            {
                if (value != null)
                {
                    @string = SR.GetString("Config_base_invalid_element_to_lock");
                }
                else
                {
                    @string = SR.GetString("Config_base_invalid_element_to_lock_by_add");
                }
            }
            else if (value != null)
            {
                @string = SR.GetString("Config_base_invalid_attribute_to_lock");
            }
            else
            {
                @string = SR.GetString("Config_base_invalid_attribute_to_lock_by_add");
            }
            if (value != null)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, @string, new object[]
                {
                    attribToLockTrim,
                    stringBuilder.ToString()
                }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
            }
            throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, @string, new object[]
            {
                attribToLockTrim,
                stringBuilder.ToString()
            }));
        }

        private ConfigurationLockCollection ParseLockedAttributes(ConfigurationValue value, ConfigurationLockCollectionType lockType)
        {
            ConfigurationLockCollection configurationLockCollection = new ConfigurationLockCollection(this, lockType);
            string text = (string)value.Value;
            if (string.IsNullOrEmpty(text))
            {
                if (lockType == ConfigurationLockCollectionType.LockedAttributes)
                {
                    throw new ConfigurationErrorsException(SR.GetString("Empty_attribute", new object[]
                    {
                        "lockAttributes"
                    }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                }
                if (lockType == ConfigurationLockCollectionType.LockedElements)
                {
                    throw new ConfigurationErrorsException(SR.GetString("Empty_attribute", new object[]
                    {
                        "lockElements"
                    }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                }
                if (lockType == ConfigurationLockCollectionType.LockedExceptionList)
                {
                    throw new ConfigurationErrorsException(SR.GetString("Config_empty_lock_attributes_except", new object[]
                    {
                        "lockAllAttributesExcept",
                        "lockAttributes"
                    }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                }
                if (lockType == ConfigurationLockCollectionType.LockedElementsExceptionList)
                {
                    throw new ConfigurationErrorsException(SR.GetString("Config_empty_lock_element_except", new object[]
                    {
                        "lockAllElementsExcept",
                        "lockElements"
                    }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                }
            }
            string[] array = text.Split(new char[]
            {
                ',',
                ':',
                ';'
            });
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string text2 = array2[i];
                string text3 = text2.Trim();
                if (!string.IsNullOrEmpty(text3))
                {
                    if ((lockType != ConfigurationLockCollectionType.LockedElements && lockType != ConfigurationLockCollectionType.LockedAttributes) || !(text3 == "*"))
                    {
                        ConfigurationProperty configurationProperty = this.Properties[text3];
                        if (configurationProperty == null || text3 == "lockAttributes" || text3 == "lockAllAttributesExcept" || text3 == "lockElements" || (lockType != ConfigurationLockCollectionType.LockedElements && lockType != ConfigurationLockCollectionType.LockedElementsExceptionList && typeof(ConfigurationElement).IsAssignableFrom(configurationProperty.Type)) || ((lockType == ConfigurationLockCollectionType.LockedElements || lockType == ConfigurationLockCollectionType.LockedElementsExceptionList) && !typeof(ConfigurationElement).IsAssignableFrom(configurationProperty.Type)))
                        {
                            ConfigurationElementCollection configurationElementCollection = this as ConfigurationElementCollection;
                            if (configurationElementCollection == null && this.Properties.DefaultCollectionProperty != null)
                            {
                                configurationElementCollection = (this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection);
                            }
                            if (configurationElementCollection == null || lockType == ConfigurationLockCollectionType.LockedAttributes || lockType == ConfigurationLockCollectionType.LockedExceptionList)
                            {
                                this.ReportInvalidLock(text3, lockType, value, null);
                            }
                            else if (!configurationElementCollection.IsLockableElement(text3))
                            {
                                this.ReportInvalidLock(text3, lockType, value, configurationElementCollection.LockableElements);
                            }
                        }
                        if (configurationProperty != null && configurationProperty.IsRequired)
                        {
                            throw new ConfigurationErrorsException(SR.GetString("Config_base_required_attribute_lock_attempt", new object[]
                            {
                                configurationProperty.Name
                            }));
                        }
                    }
                    configurationLockCollection.Add(text3, ConfigurationValueFlags.Default);
                }
            }
            return configurationLockCollection;
        }

        private StringCollection IntersectLockCollections(ConfigurationLockCollection Collection1, ConfigurationLockCollection Collection2)
        {
            ConfigurationLockCollection configurationLockCollection = (Collection1.Count < Collection2.Count) ? Collection1 : Collection2;
            ConfigurationLockCollection configurationLockCollection2 = (Collection1.Count >= Collection2.Count) ? Collection1 : Collection2;
            StringCollection stringCollection = new StringCollection();
            foreach (string text in configurationLockCollection)
            {
                if (configurationLockCollection2.Contains(text) || text == this.ElementTagName)
                {
                    stringCollection.Add(text);
                }
            }
            return stringCollection;
        }

        /// <summary>Reads XML from the configuration file.</summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> that reads from the configuration file.</param>
        /// <param name="serializeCollectionKey">true to serialize only the collection key properties; otherwise, false.</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element to read is locked.- or -An attribute of the current node is not recognized.- or -The lock status of the current node cannot be determined.  </exception>
        protected internal virtual void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            ConfigurationPropertyCollection properties = this.Properties;
            ConfigurationValue configurationValue = null;
            ConfigurationValue configurationValue2 = null;
            ConfigurationValue configurationValue3 = null;
            ConfigurationValue configurationValue4 = null;
            bool flag = false;
            this._bElementPresent = true;
            ConfigurationElement configurationElement = null;
            ConfigurationProperty configurationProperty = (properties != null) ? properties.DefaultCollectionProperty : null;
            if (configurationProperty != null)
            {
                configurationElement = (ConfigurationElement)this[configurationProperty];
            }
            this._elementTagName = reader.Name;
            PropertySourceInfo sourceInfo = new PropertySourceInfo(reader);
            this._values.SetValue(reader.Name, null, ConfigurationValueFlags.Modified, sourceInfo);
            this._values.SetValue("", configurationElement, ConfigurationValueFlags.Modified, sourceInfo);
            if ((this._lockedElementsList != null && (this._lockedElementsList.Contains(reader.Name) || (this._lockedElementsList.Contains("*") && reader.Name != this.ElementTagName))) || (this._lockedAllExceptElementsList != null && this._lockedAllExceptElementsList.Count != 0 && !this._lockedAllExceptElementsList.Contains(reader.Name)) || ((this._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default && (this._fItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default))
            {
                throw new ConfigurationErrorsException(SR.GetString("Config_base_element_locked", new object[]
                {
                    reader.Name
                }), reader);
            }
            if (reader.AttributeCount > 0)
            {
                while (reader.MoveToNextAttribute())
                {
                    string name = reader.Name;
                    if (((this._lockedAttributesList != null && (this._lockedAttributesList.Contains(name) || this._lockedAttributesList.Contains("*"))) || (this._lockedAllExceptAttributesList != null && !this._lockedAllExceptAttributesList.Contains(name))) && name != "lockAttributes" && name != "lockAllAttributesExcept")
                    {
                        throw new ConfigurationErrorsException(SR.GetString("Config_base_attribute_locked", new object[]
                        {
                            name
                        }), reader);
                    }
                    ConfigurationProperty configurationProperty2 = (properties != null) ? properties[name] : null;
                    if (configurationProperty2 != null)
                    {
                        if (serializeCollectionKey && !configurationProperty2.IsKey)
                        {
                            throw new ConfigurationErrorsException(SR.GetString("Config_base_unrecognized_attribute", new object[]
                            {
                                name
                            }), reader);
                        }
                        this._values.SetValue(name, this.DeserializePropertyValue(configurationProperty2, reader), ConfigurationValueFlags.Modified, new PropertySourceInfo(reader));
                    }
                    else
                    {
                        if (name == "lockItem")
                        {
                            try
                            {
                                flag = bool.Parse(reader.Value);
                                continue;
                            }
                            catch
                            {
                                throw new ConfigurationErrorsException(SR.GetString("Config_invalid_boolean_attribute", new object[]
                                {
                                    name
                                }), reader);
                            }
                        }
                        if (name == "lockAttributes")
                        {
                            configurationValue = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                        }
                        else if (name == "lockAllAttributesExcept")
                        {
                            configurationValue2 = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                        }
                        else if (name == "lockElements")
                        {
                            configurationValue3 = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                        }
                        else if (name == "lockAllElementsExcept")
                        {
                            configurationValue4 = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                        }
                        else if (serializeCollectionKey || !this.OnDeserializeUnrecognizedAttribute(name, reader.Value))
                        {
                            throw new ConfigurationErrorsException(SR.GetString("Config_base_unrecognized_attribute", new object[]
                            {
                                name
                            }), reader);
                        }
                    }
                }
            }
            reader.MoveToElement();
            try
            {
                HybridDictionary hybridDictionary = new HybridDictionary();
                if (!reader.IsEmptyElement)
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string name2 = reader.Name;
                            this.CheckLockedElement(name2, null);
                            ConfigurationProperty configurationProperty3 = (properties != null) ? properties[name2] : null;
                            if (configurationProperty3 != null)
                            {
                                if (!configurationProperty3.IsConfigurationElementType)
                                {
                                    throw new ConfigurationErrorsException(SR.GetString("Config_base_property_is_not_a_configuration_element", new object[]
                                    {
                                        name2
                                    }), reader);
                                }
                                if (hybridDictionary.Contains(name2))
                                {
                                    throw new ConfigurationErrorsException(SR.GetString("Config_base_element_cannot_have_multiple_child_elements", new object[]
                                    {
                                        name2
                                    }), reader);
                                }
                                hybridDictionary.Add(name2, name2);
                                ConfigurationElement configurationElement2 = (ConfigurationElement)this[configurationProperty3];
                                configurationElement2.DeserializeElement(reader, serializeCollectionKey);
                                ConfigurationElement.ValidateElement(configurationElement2, configurationProperty3.Validator, false);
                            }
                            else if (!this.OnDeserializeUnrecognizedElement(name2, reader) && (configurationElement == null || !configurationElement.OnDeserializeUnrecognizedElement(name2, reader)))
                            {
                                throw new ConfigurationErrorsException(SR.GetString("Config_base_unrecognized_element_name", new object[]
                                {
                                    name2
                                }), reader);
                            }
                        }
                        else
                        {
                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                            if (reader.NodeType == XmlNodeType.CDATA || reader.NodeType == XmlNodeType.Text)
                            {
                                throw new ConfigurationErrorsException(SR.GetString("Config_base_section_invalid_content"), reader);
                            }
                        }
                    }
                }
                this.EnsureRequiredProperties(serializeCollectionKey);
                ConfigurationElement.ValidateElement(this, null, false);
            }
            catch (ConfigurationException ex)
            {
                if (ex.Filename == null || ex.Filename.Length == 0)
                {
                    throw new ConfigurationErrorsException(ex.Message, reader);
                }
                throw ex;
            }
            if (flag)
            {
                this.SetLocked();
                this._fItemLocked = ConfigurationValueFlags.Locked;
            }
            if (configurationValue != null)
            {
                if (this._lockedAttributesList == null)
                {
                    this._lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                }
                foreach (string name3 in this.ParseLockedAttributes(configurationValue, ConfigurationLockCollectionType.LockedAttributes))
                {
                    if (!this._lockedAttributesList.Contains(name3))
                    {
                        this._lockedAttributesList.Add(name3, ConfigurationValueFlags.Default);
                    }
                    else
                    {
                        this._lockedAttributesList.Add(name3, ConfigurationValueFlags.Inherited | ConfigurationValueFlags.Modified);
                    }
                }
            }
            if (configurationValue2 != null)
            {
                ConfigurationLockCollection configurationLockCollection = this.ParseLockedAttributes(configurationValue2, ConfigurationLockCollectionType.LockedExceptionList);
                if (this._lockedAllExceptAttributesList == null)
                {
                    this._lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, string.Empty, configurationLockCollection);
                    this._lockedAllExceptAttributesList.ClearSeedList();
                }
                StringCollection stringCollection = this.IntersectLockCollections(this._lockedAllExceptAttributesList, configurationLockCollection);
                this._lockedAllExceptAttributesList.ClearInternal(false);
                foreach (string current in stringCollection)
                {
                    this._lockedAllExceptAttributesList.Add(current, ConfigurationValueFlags.Default);
                }
            }
            if (configurationValue3 != null)
            {
                if (this._lockedElementsList == null)
                {
                    this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                }
                ConfigurationLockCollection configurationLockCollection2 = this.ParseLockedAttributes(configurationValue3, ConfigurationLockCollectionType.LockedElements);
                if (properties.DefaultCollectionProperty != null)
                {
                    ConfigurationElementCollection configurationElementCollection = this[properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                    if (configurationElementCollection != null && configurationElementCollection._lockedElementsList == null)
                    {
                        configurationElementCollection._lockedElementsList = this._lockedElementsList;
                    }
                }
                foreach (string text in configurationLockCollection2)
                {
                    if (!this._lockedElementsList.Contains(text))
                    {
                        this._lockedElementsList.Add(text, ConfigurationValueFlags.Default);
                        ConfigurationProperty configurationProperty4 = this.Properties[text];
                        if (configurationProperty4 != null && typeof(ConfigurationElement).IsAssignableFrom(configurationProperty4.Type))
                        {
                            ((ConfigurationElement)this[text]).SetLocked();
                        }
                        if (text == "*")
                        {
                            foreach (ConfigurationProperty configurationProperty5 in this.Properties)
                            {
                                if (!string.IsNullOrEmpty(configurationProperty5.Name) && configurationProperty5.IsConfigurationElementType)
                                {
                                    ((ConfigurationElement)this[configurationProperty5]).SetLocked();
                                }
                            }
                        }
                    }
                }
            }
            if (configurationValue4 != null)
            {
                ConfigurationLockCollection configurationLockCollection3 = this.ParseLockedAttributes(configurationValue4, ConfigurationLockCollectionType.LockedElementsExceptionList);
                if (this._lockedAllExceptElementsList == null)
                {
                    this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, this._elementTagName, configurationLockCollection3);
                    this._lockedAllExceptElementsList.ClearSeedList();
                }
                StringCollection stringCollection2 = this.IntersectLockCollections(this._lockedAllExceptElementsList, configurationLockCollection3);
                if (properties.DefaultCollectionProperty != null)
                {
                    ConfigurationElementCollection configurationElementCollection2 = this[properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                    if (configurationElementCollection2 != null && configurationElementCollection2._lockedAllExceptElementsList == null)
                    {
                        configurationElementCollection2._lockedAllExceptElementsList = this._lockedAllExceptElementsList;
                    }
                }
                this._lockedAllExceptElementsList.ClearInternal(false);
                foreach (string current2 in stringCollection2)
                {
                    if (!this._lockedAllExceptElementsList.Contains(current2) || current2 == this.ElementTagName)
                    {
                        this._lockedAllExceptElementsList.Add(current2, ConfigurationValueFlags.Default);
                    }
                }
                foreach (ConfigurationProperty configurationProperty6 in this.Properties)
                {
                    if (!string.IsNullOrEmpty(configurationProperty6.Name) && !this._lockedAllExceptElementsList.Contains(configurationProperty6.Name) && configurationProperty6.IsConfigurationElementType)
                    {
                        ((ConfigurationElement)this[configurationProperty6]).SetLocked();
                    }
                }
            }
            if (configurationProperty != null)
            {
                configurationElement = (ConfigurationElement)this[configurationProperty];
                if (this._lockedElementsList == null)
                {
                    this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                }
                configurationElement._lockedElementsList = this._lockedElementsList;
                if (this._lockedAllExceptElementsList == null)
                {
                    this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, reader.Name);
                    this._lockedAllExceptElementsList.ClearSeedList();
                }
                configurationElement._lockedAllExceptElementsList = this._lockedAllExceptElementsList;
            }
            this.PostDeserialize();
        }

        private object DeserializePropertyValue(ConfigurationProperty prop, XmlReader reader)
        {
            string value = reader.Value;
            object obj = null;
            try
            {
                obj = prop.ConvertFromString(value);
                prop.Validate(obj);
            }
            catch (ConfigurationException ex)
            {
                if (string.IsNullOrEmpty(ex.Filename))
                {
                    ex = new ConfigurationErrorsException(ex.Message, reader);
                }
                obj = new InvalidPropValue(value, ex);
            }
            catch
            {
            }
            return obj;
        }

        internal static void ValidateElement(ConfigurationElement elem, ConfigurationValidatorBase propValidator, bool recursive)
        {
            ConfigurationValidatorBase configurationValidatorBase = propValidator;
            if (configurationValidatorBase == null && elem.ElementProperty != null)
            {
                configurationValidatorBase = elem.ElementProperty.Validator;
                if (configurationValidatorBase != null && !configurationValidatorBase.CanValidate(elem.GetType()))
                {
                    throw new ConfigurationErrorsException(SR.GetString("Validator_does_not_support_elem_type", new object[]
                    {
                        elem.GetType().Name
                    }));
                }
            }
            try
            {
                if (configurationValidatorBase != null)
                {
                    configurationValidatorBase.Validate(elem);
                }
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(SR.GetString("Validator_element_not_valid", new object[]
                {
                    elem._elementTagName,
                    ex.Message
                }));
            }
            if (recursive)
            {
                if (elem is ConfigurationElementCollection && elem is ConfigurationElementCollection)
                {
                    IEnumerator elementsEnumerator = ((ConfigurationElementCollection)elem).GetElementsEnumerator();
                    while (elementsEnumerator.MoveNext())
                    {
                        ConfigurationElement.ValidateElement((ConfigurationElement)elementsEnumerator.Current, null, true);
                    }
                }
                for (int i = 0; i < elem.Values.Count; i++)
                {
                    ConfigurationElement configurationElement = elem.Values[i] as ConfigurationElement;
                    if (configurationElement != null)
                    {
                        ConfigurationElement.ValidateElement(configurationElement, null, true);
                    }
                }
            }
        }

        private void EnsureRequiredProperties(bool ensureKeysOnly)
        {
            ConfigurationPropertyCollection properties = this.Properties;
            if (properties != null)
            {
                foreach (ConfigurationProperty configurationProperty in properties)
                {
                    if (configurationProperty.IsRequired && !this._values.Contains(configurationProperty.Name) && (!ensureKeysOnly || configurationProperty.IsKey))
                    {
                        this._values[configurationProperty.Name] = this.OnRequiredPropertyNotFound(configurationProperty.Name);
                    }
                }
            }
        }

        /// <summary>Throws an exception when a required property is not found.</summary>
        /// <returns>None.</returns>
        /// <param name="name">The name of the required attribute that was not found.</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">In all cases.</exception>
        protected virtual object OnRequiredPropertyNotFound(string name)
        {
            throw new ConfigurationErrorsException(SR.GetString("Config_base_required_attribute_missing", new object[]
            {
                name
            }), this.PropertyFileName(name), this.PropertyLineNumber(name));
        }

        /// <summary>Called after deserialization.</summary>
        protected virtual void PostDeserialize()
        {
        }

        /// <summary>Called before serialization.</summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> that will be used to serialize the <see cref="T:System.Configuration.ConfigurationElement" />.</param>
        protected virtual void PreSerialize(XmlWriter writer)
        {
        }

        /// <summary>Gets a value indicating whether an unknown attribute is encountered during deserialization.</summary>
        /// <returns>true when an unknown attribute is encountered while deserializing; otherwise, false.</returns>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        protected virtual bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            return false;
        }

        /// <summary>Gets a value indicating whether an unknown element is encountered during deserialization.</summary>
        /// <returns>true when an unknown element is encountered while deserializing; otherwise, false.</returns>
        /// <param name="elementName">The name of the unknown subelement.</param>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> being used for deserialization.</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element identified by <paramref name="elementName" /> is locked.- or -One or more of the element's attributes is locked.- or -<paramref name="elementName" /> is unrecognized, or the element has an unrecognized attribute.- or -The element has a Boolean attribute with an invalid value.- or -An attempt was made to deserialize a property more than once.- or -An attempt was made to deserialize a property that is not a valid member of the element.- or -The element cannot contain a CDATA or text element.</exception>
        protected virtual bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            return false;
        }

        /// <summary>Returns the transformed version of the specified type name.</summary>
        /// <returns>The transformed version of the specified type name. If no transformer is available, the <paramref name="typeName" /> parameter value is returned unchanged. The <see cref="P:System.Configuration.Configuration.TypeStringTransformer" /> property is null if no transformer is available.</returns>
        /// <param name="typeName">The name of the type.</param>
        protected virtual string GetTransformedTypeString(string typeName)
        {
            if (typeName == null || this._configRecord == null || !this._configRecord.TypeStringTransformerIsSet)
            {
                return typeName;
            }
            return this._configRecord.TypeStringTransformer(typeName);
        }

        /// <summary>Returns the transformed version of the specified assembly name. </summary>
        /// <returns>The transformed version of the assembly name. If no transformer is available, the <paramref name="assemblyName" /> parameter value is returned unchanged. The <see cref="P:System.Configuration.Configuration.TypeStringTransformer" /> property is null if no transformer is available.</returns>
        /// <param name="assemblyName">The name of the assembly.</param>
        protected virtual string GetTransformedAssemblyString(string assemblyName)
        {
            if (assemblyName == null || this._configRecord == null || !this._configRecord.AssemblyStringTransformerIsSet)
            {
                return assemblyName;
            }
            return this._configRecord.AssemblyStringTransformer(assemblyName);
        }

        internal ConfigurationLockCollection UnMergeLockList(ConfigurationLockCollection sourceLockList, ConfigurationLockCollection parentLockList, ConfigurationSaveMode saveMode)
        {
            if (!sourceLockList.ExceptionList)
            {
                if (saveMode == ConfigurationSaveMode.Modified)
                {
                    ConfigurationLockCollection configurationLockCollection = new ConfigurationLockCollection(this, sourceLockList.LockType);
                    foreach (string name in sourceLockList)
                    {
                        if (!parentLockList.Contains(name) || sourceLockList.IsValueModified(name))
                        {
                            configurationLockCollection.Add(name, ConfigurationValueFlags.Default);
                        }
                    }
                    return configurationLockCollection;
                }
                if (saveMode == ConfigurationSaveMode.Minimal)
                {
                    ConfigurationLockCollection configurationLockCollection2 = new ConfigurationLockCollection(this, sourceLockList.LockType);
                    foreach (string name2 in sourceLockList)
                    {
                        if (!parentLockList.Contains(name2))
                        {
                            configurationLockCollection2.Add(name2, ConfigurationValueFlags.Default);
                        }
                    }
                    return configurationLockCollection2;
                }
            }
            else if (saveMode == ConfigurationSaveMode.Modified || saveMode == ConfigurationSaveMode.Minimal)
            {
                bool flag = false;
                if (sourceLockList.Count == parentLockList.Count)
                {
                    flag = true;
                    foreach (string name3 in sourceLockList)
                    {
                        if (!parentLockList.Contains(name3) || (sourceLockList.IsValueModified(name3) && saveMode == ConfigurationSaveMode.Modified))
                        {
                            flag = false;
                        }
                    }
                }
                if (flag)
                {
                    return null;
                }
            }
            return sourceLockList;
        }

        internal static bool IsLockAttributeName(string name)
        {
            if (!StringUtil.StartsWith(name, "lock"))
            {
                return false;
            }
            string[] array = ConfigurationElement.s_lockAttributeNames;
            for (int i = 0; i < array.Length; i++)
            {
                string b = array[i];
                if (name == b)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
