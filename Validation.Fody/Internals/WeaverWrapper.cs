namespace Validation.Fody.Internals
{
    using System;
    using System.Text.RegularExpressions;
    using Mono.Cecil;

    internal sealed class WeaverWrapper
    {
        private Action<Attribute, ParameterDefinition, IlProcessorAppender> ExecuteParameter { get; }
        private Action<Attribute, PropertyDefinition, IlProcessorAppender> ExecuteProperty { get; }
        private Func<TypeReference, bool> IsValidOnFunc { get; }
        public string AttributeTypeName { get; }

        public void Execute(Attribute attribute, ParameterDefinition parameter, IlProcessorAppender ilProcessor)
            => ExecuteParameter(attribute, parameter, ilProcessor);

        public void Execute(Attribute attribute, PropertyDefinition property, IlProcessorAppender ilProcessor)
            => ExecuteProperty(attribute, property, ilProcessor);

        public bool IsValidOn(TypeReference type) => IsValidOnFunc(type);

        private WeaverWrapper(Action<Attribute, ParameterDefinition, IlProcessorAppender> executeParameter, Action<Attribute, PropertyDefinition, IlProcessorAppender> executeProperty, Func<TypeReference, bool> isValidOnFunc, string attributeTypeName)
        {
            ExecuteParameter = executeParameter;
            ExecuteProperty = executeProperty;
            IsValidOnFunc = isValidOnFunc;
            AttributeTypeName = attributeTypeName;
        }

        private static Regex AttributeNameRemoval { get; } = new Regex("Attribute$", RegexOptions.Compiled);

        public static WeaverWrapper CreateFromWeaver<TAttribute>(IAttributeWeaver<TAttribute> weaver)
            where TAttribute : Attribute
        {
            return new WeaverWrapper(
                (attr, param, append) => weaver.Execute((TAttribute) attr, param, append),
                (attr, prop, append) => weaver.Execute((TAttribute) attr, prop, append),
                weaver.IsValidOn,
                AttributeNameRemoval.Replace(typeof(TAttribute).Name, string.Empty)
            );
        }
    }
}