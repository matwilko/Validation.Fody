namespace Validation.Fody.Internals.Config
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    internal class Config<TConfigType> : Config, IConfig<TConfigType>
        where TConfigType : class, new()
    {
        public TConfigType Configuration { get; }

        protected Config(Type requestingType, XElement configurationElement) : base(requestingType, configurationElement)
        {
            Configuration = DeserialiseConfig();
        }

        private TConfigType DeserialiseConfig()
        {
            var type = typeof(TConfigType);
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructors.Length > 1 || constructors.Single().GetParameters().Length > 0)
            {
                throw new InvalidOperationException($"Could not deserialise configuration to type `{type.FullName}` as it has unsupported constructors beyond the default constructor");
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (!properties.All(ConfigDeserializationHelper.PropertyIsAutoImplemented))
            {
                throw new InvalidOperationException($"Could not deserialise configuration to type `{type.FullName}` as it has non-auto-implemented instance properties");
            }

            var fieldValues = properties
                .Select(prop => new
                {
                    Field = ConfigDeserializationHelper.GetBackingField(prop),
                    Value = GetPropertyStringValue(prop.Name)
                });

            var configInstance = new TConfigType();

            foreach (var fieldValue in fieldValues)
            {
                var value = fieldValue.Value != null
                    ? ConfigDeserializationHelper.ConvertPropertyValue(fieldValue.Value, fieldValue.Field.FieldType)
                    : ConfigDeserializationHelper.DefaultValueFor(fieldValue.Field.FieldType);

                fieldValue.Field.SetValue(configInstance, value);
            }

            return configInstance;
        }

        private string GetPropertyStringValue(string name)
        {
            var elementValue = ConfigurationElement.Element(name)?.Value;
            var attributeValue = ConfigurationElement.Attribute(name)?.Value;
            return elementValue ?? attributeValue;
        }
    }
}
