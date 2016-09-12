namespace Validation.Fody.Tests.DataAttributes
{
    using System;
    using System.Linq;
    using Xunit;

    public sealed class NullableValueTypesAttribute : CombinatorialValuesAttribute
    {
        internal static Type[] NullableValueTypes { get; } = TypesAttribute.Types
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            .ToArray();
        
        public NullableValueTypesAttribute() : base(NullableValueTypes)
        {
        }
    }
}
