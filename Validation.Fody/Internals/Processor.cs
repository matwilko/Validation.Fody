namespace Validation.Fody.Internals
{
    using System.Collections.Generic;
    using Discovery;
    using Mono.Cecil.Cil;
    using Helpers;

    internal static class Processor
    {
        public static void ProcessWeavableProperties(ILogger log, IEnumerable<PropertyWeaverSet> properties)
        {
            foreach (var propertyInfo in properties)
            {
                var property = propertyInfo.Property;

                if (property.SetMethod == null)
                {
                    log.Error($"The property `{property.Name}` on type `{property.DeclaringType.FullName}` cannot be rewritten to include validation because it does not have a setter.");
                    continue;
                }

                var appender = new IlProcessorAppender(property.SetMethod, null);
                
                foreach (var weaver in propertyInfo.Weavers)
                {
                    if (!weaver.Weaver.IsValidOn(property.PropertyType))
                    {
                        log.Error($"The attribute `{weaver.Weaver.AttributeTypeName}` cannot be applied to a property of type `{property.PropertyType.Name}`");
                    }
                    else
                    {
                        weaver.Weaver.Execute(weaver.GetInstantiatedAttribute(), property, appender);
                        property.CustomAttributes.Remove(weaver.CustomAttribute);
                    }
                }
            }
        }

        public static void ProcessWeavableParameters(ILogger log, IEnumerable<ParameterWeaverSet> parameters)
        {
            foreach (var parameterInfo in parameters)
            {
                var parameter = parameterInfo.Parameter;
                var method = parameterInfo.Method;

                Instruction firstInstruction;

                if (parameterInfo.Method.IsConstructor)
                {
                    // We make sure the validation occurs after the this/base call
                    // in order to preserve the behaviour that the C# compiler would
                    // have generated had the checks been written by hand
                    firstInstruction = parameterInfo.Method.FindChainedCall();
                }
                else
                {
                    firstInstruction = null;
                }
                
                var appender = new IlProcessorAppender(method, firstInstruction);
                
                foreach (var weaver in parameterInfo.Weavers)
                {
                    if (!weaver.Weaver.IsValidOn(parameter.ParameterType))
                    {
                        log.Error($"The attribute `{weaver.Weaver.AttributeTypeName}` cannot be applied to a method parameter of type `{parameter.ParameterType.Name}`");
                    }
                    else
                    {
                        weaver.Weaver.Execute(weaver.GetInstantiatedAttribute(), parameter, appender);
                        parameter.CustomAttributes.Remove(weaver.CustomAttribute);
                    }
                }
            }
        }
    }
} 
