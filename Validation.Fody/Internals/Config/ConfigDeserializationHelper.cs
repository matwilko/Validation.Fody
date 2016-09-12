namespace Validation.Fody.Internals.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class ConfigDeserializationHelper
    {
        public static bool PropertyIsAutoImplemented(PropertyInfo property)
        {
            // TODO: Could be more thorough here, but seems like a reasonable heuristic for now
            return GetBackingField(property) != null;
        }

        public static FieldInfo GetBackingField(PropertyInfo property)
        {
            return property.DeclaringType.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static IDictionary<Type, Func<string, object>> ValueConvertors { get; } =  new Dictionary<Type, Func<string, object>>
        {
            {typeof (string), s => s},
            {typeof (byte), s => byte.Parse(s)},
            {typeof (sbyte), s => sbyte.Parse(s)},
            {typeof (ushort), s => ushort.Parse(s)},
            {typeof (short), s => short.Parse(s)},
            {typeof (uint), s => uint.Parse(s)},
            {typeof (int), s => int.Parse(s)},
            {typeof (ulong), s => ulong.Parse(s)},
            {typeof (long), s => long.Parse(s)},
            {typeof (float), s => float.Parse(s)},
            {typeof (double), s => double.Parse(s)},
            {typeof (decimal), s => decimal.Parse(s)},
            {typeof (DateTime), s => DateTime.Parse(s)},
            {typeof (DateTimeOffset), s => DateTimeOffset.Parse(s)}
        };

        public static object DefaultValueFor(Type type)
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        public static object ConvertPropertyValue(string value, Type fieldType)
        {
            Func<string, object> conversion;
            if (fieldType.IsEnum)
            {
                conversion = s => Enum.Parse(fieldType, s);
            }
            else if (!ValueConvertors.TryGetValue(fieldType, out conversion))
            {
                throw new InvalidOperationException($"Cannot deserialize type `{fieldType.Name}` for configuration");
            }

            try
            {
                return conversion(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"There was an error deserializing the value `{value}` for type `{fieldType.Name}` for configuration", ex);
            }
        }
    }
}