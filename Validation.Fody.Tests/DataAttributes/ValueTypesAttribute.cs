namespace Validation.Fody.Tests.DataAttributes
{
    using System;
    using System.Linq;
    using Xunit;

    public sealed class ValueTypesAttribute : CombinatorialValuesAttribute
    {
        private static Type[] NonNullableValueTypes { get; } = TypesAttribute.Types
            .Where(t => t.IsValueType)
            .Where(t => !t.IsGenericType  || t.GetGenericTypeDefinition() != typeof(Nullable<>))
            .ToArray();

        private static Type[] ValueTypes { get; } = TypesAttribute.Types
            .Where(t => t.IsValueType)
            .ToArray();

        public ValueTypesAttribute(bool excludeNullables)
            : base(excludeNullables ? NonNullableValueTypes : ValueTypes)
        {
        }
    }
}