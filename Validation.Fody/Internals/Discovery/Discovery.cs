namespace Validation.Fody.Internals.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    internal static class Discovery
    {
        public static IDictionary<string, WeaverWrapper> GetTransformableAttributes(Injector injector)
        {
            return typeof(IAttributeWeaver<>).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .Select(t => new
                {
                    Type = t,
                    AttributeTypes = GetAttributeWeavingTypes(t)
                })
                .SelectMany(t => t.AttributeTypes.Select(at => new
                {
                    t.Type,
                    AttributeType = at
                }))
                .ToDictionary(r => r.AttributeType.FullName, r => WeaverGenerator.CreateWeaver(injector, r.Type, r.AttributeType));
        }

        private static IEnumerable<Type> GetAttributeWeavingTypes(Type type)
        {
            return type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IAttributeWeaver<>))
                .Select(i => i.GetGenericArguments().Single());
        }

        public static IEnumerable<PropertyWeaverSet> GetWeavableProperties(IEnumerable<TypeDefinition> types, IDictionary<string, WeaverWrapper> weavers)
        {
            return types.SelectMany(t => t.Properties)
                .Select(p => new
                {
                    Property = p,
                    Weavers = GetWeaverAttributePairs(p.CustomAttributes, weavers)
                })
                .Where(p => p.Weavers.Any())
                .Select(p => new PropertyWeaverSet(p.Property, p.Weavers));
        }

        public static IEnumerable<ParameterWeaverSet> GetWeavableParameters(IEnumerable<TypeDefinition> types, IDictionary<string, WeaverWrapper> weavers)
        {
            return types
                .SelectMany(t => t.Methods)
                .SelectMany(m => m.Parameters.Select(p => new
                {
                    Method = m,
                    Parameter = p,
                    Weavers = GetWeaverAttributePairs(p.CustomAttributes, weavers)
                }))
                .Where(p => p.Weavers.Any())
                .Select(p => new ParameterWeaverSet(p.Method, p.Parameter, p.Weavers));
        }

        private static IEnumerable<WeaverAttributePair> GetWeaverAttributePairs(IEnumerable<CustomAttribute> attributes, IDictionary<string, WeaverWrapper> weavers)
        {
            return attributes.Select(ca =>
            {
                WeaverWrapper weaver;
                return weavers.TryGetValue(ca.AttributeType.FullName, out weaver)
                    ? new WeaverAttributePair(ca, weaver)
                    : null;
            })
            .Where(pair => pair != null)
            .ToList();
        }
    }
}
