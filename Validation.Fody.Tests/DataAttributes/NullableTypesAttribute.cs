namespace Validation.Fody.Tests.DataAttributes
{
    using System;
    using System.Linq;
    using Xunit;

    public sealed class NullableTypesAttribute : CombinatorialValuesAttribute
    {
        internal static Type[] NullableTypes { get; } = NullableValueTypesAttribute.NullableValueTypes
            .Concat(ReferenceTypesAttribute.ReferenceTypes)
            .ToArray();

        public NullableTypesAttribute() : base(NullableTypes)
        {
        }
    }
}