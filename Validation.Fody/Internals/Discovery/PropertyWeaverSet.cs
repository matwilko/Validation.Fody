namespace Validation.Fody.Internals.Discovery
{
    using System.Collections.Generic;
    using Mono.Cecil;

    internal sealed class PropertyWeaverSet
    {
        public PropertyDefinition Property { get; }
        public IEnumerable<WeaverAttributePair> Weavers { get; }
        
        public PropertyWeaverSet(PropertyDefinition property, IEnumerable<WeaverAttributePair> weavers)
        {
            Property = property;
            Weavers = weavers;
        }
    }
}