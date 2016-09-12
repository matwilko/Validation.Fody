namespace Validation.Fody.Weavers
{
    using System;
    using System.Linq;
    using Helpers;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using ValidationAttributes;

    internal sealed class NotNullWeaver : IAttributeWeaver<NotNullAttribute>
    {
        private ModuleDefinition ModuleDefinition { get; }
        
        public NotNullWeaver(ModuleDefinition moduleDefinition)
        {
            ModuleDefinition = moduleDefinition;
        }

        public bool IsValidOn(TypeReference type)
        {
            if (!type.IsValueType)
            {
                return true;
            }

            if (!type.IsGenericInstance)
            {
                return false;
            }

            return ((GenericInstanceType) type).ElementType.FullName == "System.Nullable`1";
        }

        public void Execute(NotNullAttribute attribute, ParameterDefinition parameter, Action<Instruction> appendInstruction)
        {
            GenerateNullCheck(
                parameter,
                appendInstruction,
                parameter.Name
            );
        }
        
        public void Execute(NotNullAttribute attribute, PropertyDefinition property, Action<Instruction> appendInstruction)
        {
            GenerateNullCheck(
                property.SetMethod.Parameters.First(),
                appendInstruction,
                "value"
            );
        }

        private void GenerateNullCheck(ParameterDefinition parameter, Action<Instruction> append, string name)
        {
            var endInstruction = Instruction.Create(OpCodes.Nop);
            if (parameter.ParameterType.IsValueType)
            {
                append(Instruction.Create(OpCodes.Ldarga_S, parameter));
                append(Instruction.Create(OpCodes.Call, ModuleDefinition.NullableHasValue(parameter.ParameterType.GetNullableInnerType())));
            }
            else
            {
                append(Instruction.Create(OpCodes.Ldarg, parameter));
            }
            
            append(Instruction.Create(OpCodes.Brtrue_S, endInstruction));
            append(Instruction.Create(OpCodes.Ldstr, name));
            append(Instruction.Create(OpCodes.Newobj, GetArgumentNullExceptionConstructor()));
            append(Instruction.Create(OpCodes.Throw));
            append(endInstruction);
        }

        private MethodReference GetArgumentNullExceptionConstructor()
        {
            var type = ModuleDefinition
                .Import(typeof (ArgumentNullException))
                .Resolve();
            var constructor = type.GetConstructors()
                .Single(c => c.Parameters.Count == 1 && c.Parameters.Single().ParameterType.FullName == "System.String");
            return ModuleDefinition.Import(constructor);
        }
    }
}
