namespace Validation.Fody.Tests.DataAttributes
{
    using System;
    using System.Linq;
    using Xunit;

    public sealed class ReferenceTypesAttribute : CombinatorialValuesAttribute
    {
        internal static Type[] ReferenceTypes { get; } = TypesAttribute.Types
            .Where(t => !t.IsValueType)
            .ToArray();

        public ReferenceTypesAttribute() : base(ReferenceTypes)
        {
        }
    }
}