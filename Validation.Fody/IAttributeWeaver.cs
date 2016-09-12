namespace Validation.Fody
{
    using System;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Marks a class as being able to weave for a particular attribute
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute that will be used for discovery</typeparam>
    internal interface IAttributeWeaver<TAttribute>
        where TAttribute : Attribute
    {
        bool IsValidOn(TypeReference type);

        void Execute(TAttribute attribute, ParameterDefinition parameter, Action<Instruction> appendInstruction);
        void Execute(TAttribute attribute, PropertyDefinition property, Action<Instruction> appendInstruction);
    }
}
