namespace Validation.Fody.Internals.Discovery
{
    using System;
    using System.Linq;
    using Mono.Cecil;
    using ValidationAttributes;

    internal sealed class WeaverAttributePair
    {
        public CustomAttribute CustomAttribute { get; }
        public WeaverWrapper Weaver { get; }

        public Attribute GetInstantiatedAttribute()
        {
            return (Attribute) typeof (ReferenceType).Assembly.GetType(CustomAttribute.AttributeType.FullName)
                .GetConstructors()
                .Single(c =>
                    c.GetParameters().Length == CustomAttribute.ConstructorArguments.Count
                    &&
                    c.GetParameters()
                        .Select(p => p.ParameterType.FullName)
                        .SequenceEqual(CustomAttribute.ConstructorArguments.Select(a => a.Type.FullName.Replace("/", "+")))
                ).Invoke(CustomAttribute.ConstructorArguments.Select(a => a.Value).ToArray());
        }

        public WeaverAttributePair(CustomAttribute customAttribute, WeaverWrapper weaver)
        {
            CustomAttribute = customAttribute;
            Weaver = weaver;
        }
    }
}