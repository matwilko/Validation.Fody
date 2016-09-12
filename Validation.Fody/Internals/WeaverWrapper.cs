namespace Validation.Fody.Internals
{
    using System;
    using System.Text.RegularExpressions;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal sealed class WeaverWrapper
    {
        private Action<Attribute, ParameterDefinition, Action<Instruction>> ExecuteParameter { get; }
        private Action<Attribute, PropertyDefinition, Action<Instruction>> ExecuteProperty { get; }
        private Func<TypeReference, bool> IsValidOnFunc { get; }
        public string AttributeTypeName { get; }

        public void Execute(Attribute attribute, ParameterDefinition parameter, Action<Instruction> appendInstruction)
            => ExecuteParameter(attribute, parameter, appendInstruction);

        public void Execute(Attribute attribute, PropertyDefinition property, Action<Instruction> appendInstruction)
            => ExecuteProperty(attribute, property, appendInstruction);

        public bool IsValidOn(TypeReference type) => IsValidOnFunc(type);

        private WeaverWrapper(Action<Attribute, ParameterDefinition, Action<Instruction>> executeParameter, Action<Attribute, PropertyDefinition, Action<Instruction>> executeProperty, Func<TypeReference, bool> isValidOnFunc, string attributeTypeName)
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