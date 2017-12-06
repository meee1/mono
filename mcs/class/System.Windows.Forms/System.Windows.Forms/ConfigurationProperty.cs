using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Configuration
{
    /// <summary>Represents an attribute or a child of a configuration element. This class cannot be inherited.</summary>
    public sealed class ConfigurationProperty
    {
        internal static readonly ConfigurationValidatorBase NonEmptyStringValidator = new StringValidator(1);

        private static readonly ConfigurationValidatorBase DefaultValidatorInstance = new DefaultValidator();

        internal static readonly string DefaultCollectionPropertyName = "";

        private string _name;

        private string _providedName;

        private string _description;

        private Type _type;

        private object _defaultValue;

        private TypeConverter _converter;

        private ConfigurationPropertyOptions _options;

        private ConfigurationValidatorBase _validator;

        private string _addElementName;

        private string _removeElementName;

        private string _clearElementName;

        private volatile bool _isTypeInited;

        private volatile bool _isConfigurationElementType;

        /// <summary>Gets the name of this <see cref="T:System.Configuration.ConfigurationProperty" />.</summary>
        /// <returns>The name of the <see cref="T:System.Configuration.ConfigurationProperty" />.</returns>
        public string Name
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>Gets the description associated with the <see cref="T:System.Configuration.ConfigurationProperty" />.</summary>
        /// <returns>A string value that describes the property.</returns>
        public string Description
        {
            get
            {
                return this._description;
            }
        }

        internal string ProvidedName
        {
            get
            {
                return this._providedName;
            }
        }

        internal bool IsConfigurationElementType
        {
            get
            {
                if (!this._isTypeInited)
                {
                    this._isConfigurationElementType = typeof(ConfigurationElement).IsAssignableFrom(this._type);
                    this._isTypeInited = true;
                }
                return this._isConfigurationElementType;
            }
        }

        /// <summary>Gets the type of this <see cref="T:System.Configuration.ConfigurationProperty" /> object.</summary>
        /// <returns>A <see cref="T:System.Type" /> representing the type of this <see cref="T:System.Configuration.ConfigurationProperty" /> object.</returns>
        public Type Type
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>Gets the default value for this <see cref="T:System.Configuration.ConfigurationProperty" /> property.</summary>
        /// <returns>An <see cref="T:System.Object" /> that can be cast to the type specified by the <see cref="P:System.Configuration.ConfigurationProperty.Type" /> property.</returns>
        public object DefaultValue
        {
            get
            {
                return this._defaultValue;
            }
        }

        /// <summary>Gets a value indicating whether this <see cref="T:System.Configuration.ConfigurationProperty" /> is required.</summary>
        /// <returns>true if the <see cref="T:System.Configuration.ConfigurationProperty" /> is required; otherwise, false. The default is false.</returns>
        public bool IsRequired
        {
            get
            {
                return (this._options & ConfigurationPropertyOptions.IsRequired) > ConfigurationPropertyOptions.None;
            }
        }

        /// <summary>Gets a value indicating whether this <see cref="T:System.Configuration.ConfigurationProperty" /> is the key for the containing <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
        /// <returns>true if this <see cref="T:System.Configuration.ConfigurationProperty" /> object is the key for the containing element; otherwise, false. The default is false.</returns>
        public bool IsKey
        {
            get
            {
                return (this._options & ConfigurationPropertyOptions.IsKey) > ConfigurationPropertyOptions.None;
            }
        }

        /// <summary>Gets a value that indicates whether the property is the default collection of an element. </summary>
        /// <returns>true if the property is the default collection of an element; otherwise, false.</returns>
        public bool IsDefaultCollection
        {
            get
            {
                return (this._options & ConfigurationPropertyOptions.IsDefaultCollection) > ConfigurationPropertyOptions.None;
            }
        }

        /// <summary>Indicates whether the type name for the configuration property requires transformation when it is serialized for an earlier version of the .NET Framework.</summary>
        /// <returns>true if the property requires type-name transformation; otherwise, false.</returns>
        public bool IsTypeStringTransformationRequired
        {
            get
            {
                return (this._options & ConfigurationPropertyOptions.IsTypeStringTransformationRequired) > ConfigurationPropertyOptions.None;
            }
        }

        /// <summary>Indicates whether the assembly name for the configuration property requires transformation when it is serialized for an earlier version of the .NET Framework.</summary>
        /// <returns>true if the property requires assembly name transformation; otherwise, false.</returns>
        public bool IsAssemblyStringTransformationRequired
        {
            get
            {
                return (this._options & ConfigurationPropertyOptions.IsAssemblyStringTransformationRequired) > ConfigurationPropertyOptions.None;
            }
        }

        /// <summary>Indicates whether the configuration property's parent configuration section is queried at serialization time to determine whether the configuration property should be serialized into XML.</summary>
        /// <returns>true if the parent configuration section should be queried; otherwise, false.</returns>
        public bool IsVersionCheckRequired
        {
            get
            {
                return (this._options & ConfigurationPropertyOptions.IsVersionCheckRequired) > ConfigurationPropertyOptions.None;
            }
        }

        /// <summary>Gets the <see cref="T:System.ComponentModel.TypeConverter" /> used to convert this <see cref="T:System.Configuration.ConfigurationProperty" /> into an XML representation for writing to the configuration file.</summary>
        /// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> used to convert this <see cref="T:System.Configuration.ConfigurationProperty" /> into an XML representation for writing to the configuration file.</returns>
        /// <exception cref="T:System.Exception">This <see cref="T:System.Configuration.ConfigurationProperty" /> cannot be converted. </exception>
        public TypeConverter Converter
        {
            get
            {
                this.CreateConverter();
                return this._converter;
            }
        }

        /// <summary>Gets the <see cref="T:System.Configuration.ConfigurationValidatorAttribute" />, which is used to validate this <see cref="T:System.Configuration.ConfigurationProperty" /> object.</summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationValidatorBase" /> validator, which is used to validate this <see cref="T:System.Configuration.ConfigurationProperty" />.</returns>
        public ConfigurationValidatorBase Validator
        {
            get
            {
                return this._validator;
            }
        }

        internal string AddElementName
        {
            get
            {
                return this._addElementName;
            }
        }

        internal string RemoveElementName
        {
            get
            {
                return this._removeElementName;
            }
        }

        internal string ClearElementName
        {
            get
            {
                return this._clearElementName;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
        /// <param name="name">The name of the configuration entity. </param>
        /// <param name="type">The type of the configuration entity. </param>
        public ConfigurationProperty(string name, Type type)
        {
            object defaultValue = null;
            this.ConstructorInit(name, type, ConfigurationPropertyOptions.None, null, null);
            if (type == typeof(string))
            {
                defaultValue = string.Empty;
            }
            else if (type.IsValueType)
            {
                defaultValue = TypeUtil.CreateInstanceWithReflectionPermission(type);
            }
            this.SetDefaultValue(defaultValue);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
        /// <param name="name">The name of the configuration entity. </param>
        /// <param name="type">The type of the configuration entity. </param>
        /// <param name="defaultValue">The default value of the configuration entity. </param>
        public ConfigurationProperty(string name, Type type, object defaultValue) : this(name, type, defaultValue, ConfigurationPropertyOptions.None)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
        /// <param name="name">The name of the configuration entity. </param>
        /// <param name="type">The type of the configuration entity. </param>
        /// <param name="defaultValue">The default value of the configuration entity. </param>
        /// <param name="options">One of the <see cref="T:System.Configuration.ConfigurationPropertyOptions" /> enumeration values.</param>
        public ConfigurationProperty(string name, Type type, object defaultValue, ConfigurationPropertyOptions options) : this(name, type, defaultValue, null, null, options)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
        /// <param name="name">The name of the configuration entity. </param>
        /// <param name="type">The type of the configuration entity.</param>
        /// <param name="defaultValue">The default value of the configuration entity. </param>
        /// <param name="typeConverter">The type of the converter to apply.</param>
        /// <param name="validator">The validator to use. </param>
        /// <param name="options">One of the <see cref="T:System.Configuration.ConfigurationPropertyOptions" /> enumeration values. </param>
        public ConfigurationProperty(string name, Type type, object defaultValue, TypeConverter typeConverter, ConfigurationValidatorBase validator, ConfigurationPropertyOptions options) : this(name, type, defaultValue, typeConverter, validator, options, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
        /// <param name="name">The name of the configuration entity. </param>
        /// <param name="type">The type of the configuration entity. </param>
        /// <param name="defaultValue">The default value of the configuration entity. </param>
        /// <param name="typeConverter">The type of the converter to apply.</param>
        /// <param name="validator">The validator to use. </param>
        /// <param name="options">One of the <see cref="T:System.Configuration.ConfigurationPropertyOptions" /> enumeration values. </param>
        /// <param name="description">The description of the configuration entity. </param>
        public ConfigurationProperty(string name, Type type, object defaultValue, TypeConverter typeConverter, ConfigurationValidatorBase validator, ConfigurationPropertyOptions options, string description)
        {
            this.ConstructorInit(name, type, options, validator, typeConverter);
            this.SetDefaultValue(defaultValue);
        }

        internal ConfigurationProperty(PropertyInfo info)
        {
            ConfigurationPropertyAttribute configurationPropertyAttribute = null;
            DescriptionAttribute descriptionAttribute = null;
            DefaultValueAttribute attribStdDefault = null;
            TypeConverter converter = null;
            ConfigurationValidatorBase configurationValidatorBase = null;
            Attribute[] customAttributes = Attribute.GetCustomAttributes(info);
            for (int i = 0; i < customAttributes.Length; i++)
            {
                Attribute attribute = customAttributes[i];
                if (attribute is TypeConverterAttribute)
                {
                    TypeConverterAttribute typeConverterAttribute = (TypeConverterAttribute)attribute;
                    converter = TypeUtil.CreateInstanceRestricted<TypeConverter>(info.DeclaringType, typeConverterAttribute.ConverterTypeName);
                }
                else if (attribute is ConfigurationPropertyAttribute)
                {
                    configurationPropertyAttribute = (ConfigurationPropertyAttribute)attribute;
                }
                else if (attribute is ConfigurationValidatorAttribute)
                {
                    if (configurationValidatorBase != null)
                    {
                        throw new ConfigurationErrorsException(SR.GetString("Validator_multiple_validator_attributes", new object[]
                        {
                            info.Name
                        }));
                    }
                    ConfigurationValidatorAttribute configurationValidatorAttribute = (ConfigurationValidatorAttribute)attribute;
                    configurationValidatorAttribute.SetDeclaringType(info.DeclaringType);
                    configurationValidatorBase = configurationValidatorAttribute.ValidatorInstance;
                }
                else if (attribute is DescriptionAttribute)
                {
                    descriptionAttribute = (DescriptionAttribute)attribute;
                }
                else if (attribute is DefaultValueAttribute)
                {
                    attribStdDefault = (DefaultValueAttribute)attribute;
                }
            }
            Type propertyType = info.PropertyType;
            if (typeof(ConfigurationElementCollection).IsAssignableFrom(propertyType))
            {
                ConfigurationCollectionAttribute configurationCollectionAttribute = Attribute.GetCustomAttribute(info, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
                if (configurationCollectionAttribute == null)
                {
                    configurationCollectionAttribute = (Attribute.GetCustomAttribute(propertyType, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute);
                }
                if (configurationCollectionAttribute != null)
                {
                    if (configurationCollectionAttribute.AddItemName.IndexOf(',') == -1)
                    {
                        this._addElementName = configurationCollectionAttribute.AddItemName;
                    }
                    this._removeElementName = configurationCollectionAttribute.RemoveItemName;
                    this._clearElementName = configurationCollectionAttribute.ClearItemsName;
                }
            }
            this.ConstructorInit(configurationPropertyAttribute.Name, info.PropertyType, configurationPropertyAttribute.Options, configurationValidatorBase, converter);
            this.InitDefaultValueFromTypeInfo(configurationPropertyAttribute, attribStdDefault);
            if (descriptionAttribute != null && !string.IsNullOrEmpty(descriptionAttribute.Description))
            {
                this._description = descriptionAttribute.Description;
            }
        }

        private void ConstructorInit(string name, Type type, ConfigurationPropertyOptions options, ConfigurationValidatorBase validator, TypeConverter converter)
        {
            if (typeof(ConfigurationSection).IsAssignableFrom(type))
            {
                throw new ConfigurationErrorsException(SR.GetString("Config_properties_may_not_be_derived_from_configuration_section", new object[]
                {
                    name
                }));
            }
            this._providedName = name;
            if ((options & ConfigurationPropertyOptions.IsDefaultCollection) != ConfigurationPropertyOptions.None && string.IsNullOrEmpty(name))
            {
                name = ConfigurationProperty.DefaultCollectionPropertyName;
            }
            else
            {
                this.ValidatePropertyName(name);
            }
            this._name = name;
            this._type = type;
            this._options = options;
            this._validator = validator;
            this._converter = converter;
            if (this._validator == null)
            {
                this._validator = ConfigurationProperty.DefaultValidatorInstance;
                return;
            }
            if (!this._validator.CanValidate(this._type))
            {
                throw new ConfigurationErrorsException(SR.GetString("Validator_does_not_support_prop_type", new object[]
                {
                    this._name
                }));
            }
        }

        private void ValidatePropertyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(SR.GetString("String_null_or_empty"), "name");
            }
            if (BaseConfigurationRecord.IsReservedAttributeName(name))
            {
                throw new ArgumentException(SR.GetString("Property_name_reserved", new object[]
                {
                    name
                }));
            }
        }

        private void SetDefaultValue(object value)
        {
            if (value != null && value != ConfigurationElement.s_nullPropertyValue)
            {
                bool flag = this._type.IsAssignableFrom(value.GetType());
                if (!flag && this.Converter.CanConvertFrom(value.GetType()))
                {
                    value = this.Converter.ConvertFrom(value);
                }
                else if (!flag)
                {
                    throw new ConfigurationErrorsException(SR.GetString("Default_value_wrong_type", new object[]
                    {
                        this._name
                    }));
                }
                this.Validate(value);
                this._defaultValue = value;
            }
        }

        private void InitDefaultValueFromTypeInfo(ConfigurationPropertyAttribute attribProperty, DefaultValueAttribute attribStdDefault)
        {
            object obj = attribProperty.DefaultValue;
            if ((obj == null || obj == ConfigurationElement.s_nullPropertyValue) && attribStdDefault != null)
            {
                obj = attribStdDefault.Value;
            }
            if (obj != null && obj is string && this._type != typeof(string))
            {
                try
                {
                    obj = this.Converter.ConvertFromInvariantString((string)obj);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationErrorsException(SR.GetString("Default_value_conversion_error_from_string", new object[]
                    {
                        this._name,
                        ex.Message
                    }));
                }
            }
            if (obj == null || obj == ConfigurationElement.s_nullPropertyValue)
            {
                if (this._type == typeof(string))
                {
                    obj = string.Empty;
                }
                else if (this._type.IsValueType)
                {
                    obj = TypeUtil.CreateInstanceWithReflectionPermission(this._type);
                }
            }
            this.SetDefaultValue(obj);
        }

        internal object ConvertFromString(string value)
        {
            object result = null;
            try
            {
                result = this.Converter.ConvertFromInvariantString(value);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(SR.GetString("Top_level_conversion_error_from_string", new object[]
                {
                    this._name,
                    ex.Message
                }));
            }
            return result;
        }

        internal string ConvertToString(object value)
        {
            string result = null;
            try
            {
                if (this._type == typeof(bool))
                {
                    result = (((bool)value) ? "true" : "false");
                }
                else
                {
                    result = this.Converter.ConvertToInvariantString(value);
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(SR.GetString("Top_level_conversion_error_to_string", new object[]
                {
                    this._name,
                    ex.Message
                }));
            }
            return result;
        }

        internal void Validate(object value)
        {
            try
            {
                this._validator.Validate(value);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(SR.GetString("Top_level_validation_error", new object[]
                {
                    this._name,
                    ex.Message
                }), ex);
            }
        }

        private void CreateConverter()
        {
            if (this._converter == null)
            {
                if (this._type.IsEnum)
                {
                    this._converter = new GenericEnumConverter(this._type);
                    return;
                }
                if (!this._type.IsSubclassOf(typeof(ConfigurationElement)))
                {
                    this._converter = TypeDescriptor.GetConverter(this._type);
                    if (this._converter == null || !this._converter.CanConvertFrom(typeof(string)) || !this._converter.CanConvertTo(typeof(string)))
                    {
                        throw new ConfigurationErrorsException(SR.GetString("No_converter", new object[]
                        {
                            this._name,
                            this._type.Name
                        }));
                    }
                }
            }
        }
    }
}
