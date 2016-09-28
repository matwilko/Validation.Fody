namespace Validation.Fody.Weavers
{
    using System;
    using System.Linq;
    using Helpers;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Internals;
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

        public void Execute(NotNullAttribute attribute, ParameterDefinition parameter, IlProcessorAppender ilProcessor)
        {
            GenerateNullCheck(
                parameter,
                ilProcessor,
                parameter.Name
            );
        }
        
        public void Execute(NotNullAttribute attribute, PropertyDefinition property, IlProcessorAppender ilProcessor)
        {
            GenerateNullCheck(
                property.SetMethod.Parameters.First(),
                ilProcessor,
                "value"
            );
        }

        private void GenerateNullCheck(ParameterDefinition parameter, IlProcessorAppender ilProcessor, string name)
        {
            var endInstruction = Instruction.Create(OpCodes.Nop);
            if (parameter.ParameterType.IsValueType)
            {
                ilProcessor.Append(OpCodes.Ldarga_S, parameter);
                ilProcessor.Append(OpCodes.Call, ModuleDefinition.NullableHasValue(parameter.ParameterType.GetNullableInnerType()));
            }
            else
            {
                ilProcessor.Append(OpCodes.Ldarg, parameter);
            }

            ilProcessor.Append(OpCodes.Brtrue_S, endInstruction);
            ilProcessor.Append(OpCodes.Ldstr, name);
            ilProcessor.Append(OpCodes.Newobj, GetArgumentNullExceptionConstructor());
            ilProcessor.Append(OpCodes.Throw);
            ilProcessor.Append(endInstruction);
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
