namespace Validation.Fody.Internals.Discovery
{
    using System.Collections.Generic;
    using Mono.Cecil;

    internal sealed class ParameterWeaverSet
    {
        public MethodDefinition Method { get; }
        public ParameterDefinition Parameter { get; }
        public IEnumerable<WeaverAttributePair> Weavers { get; }
        
        public ParameterWeaverSet(MethodDefinition method, ParameterDefinition parameter, IEnumerable<WeaverAttributePair> weavers)
        {
            Method = method;
            Parameter = parameter;
            Weavers = weavers;
        }
    }
}