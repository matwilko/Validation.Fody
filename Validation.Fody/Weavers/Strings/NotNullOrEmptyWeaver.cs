namespace Validation.Fody.Weavers.Strings
{
    using System;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using ValidationAttributes.Strings;

    internal sealed class NotNullOrEmptyWeaver : IAttributeWeaver<NotNullOrEmptyAttribute>
    {
        private ModuleDefinition ModuleDefinition { get; }
        
        public NotNullOrEmptyWeaver(ModuleDefinition moduleDefinition)
        {
            ModuleDefinition = moduleDefinition;
        }

        public bool IsValidOn(TypeReference type)
        {
            return type.FullName == "System.String" || type.FullName == "System.Char[]";
        }

        public void Execute(NotNullOrEmptyAttribute attribute, ParameterDefinition parameter, Action<Instruction> appendInstruction)
        {
            GenerateCheck(
                parameter,
                appendInstruction,
                parameter.Name
            );
        }

        public void Execute(NotNullOrEmptyAttribute attribute, PropertyDefinition property, Action<Instruction> appendInstruction)
        {
            GenerateCheck(
                property.SetMethod.Parameters.First(),
                appendInstruction,
                "value"
            );
        }

        private void GenerateCheck(ParameterDefinition parameter, Action<Instruction> append, string name)
        {
            var endInstruction = Instruction.Create(OpCodes.Nop);

            // Null Check
            var emptyCheckLdArg = Instruction.Create(OpCodes.Ldarg, parameter);
            
            append(Instruction.Create(OpCodes.Ldarg, parameter));
            append(Instruction.Create(OpCodes.Brtrue_S, emptyCheckLdArg));

            append(Instruction.Create(OpCodes.Ldstr, name));
            append(Instruction.Create(OpCodes.Newobj, GetArgumentNullExceptionConstructor()));
            append(Instruction.Create(OpCodes.Throw));
            
            // Empty Check
            append(emptyCheckLdArg);
            
            if (parameter.ParameterType.FullName == "System.String")
            {
                var stringLength = ModuleDefinition.Import(typeof(string)).Resolve()
                    .Properties.Single(p => p.Name == "Length")
                    .GetMethod;

                append(Instruction.Create(OpCodes.Call, ModuleDefinition.Import(stringLength)));
            }
            else if (parameter.ParameterType.FullName == "System.Char[]")
            {
                var arrayLength = ModuleDefinition.Import(typeof(Array)).Resolve()
                    .Properties.Single(p => p.Name == "Length")
                    .GetMethod;

                append(Instruction.Create(OpCodes.Call, ModuleDefinition.Import(arrayLength)));
            }

            append(Instruction.Create(OpCodes.Ldc_I4_0));
            append(Instruction.Create(OpCodes.Bne_Un_S, endInstruction));
            
            append(Instruction.Create(OpCodes.Ldstr, "The given string cannot be empty"));
            append(Instruction.Create(OpCodes.Ldstr, name));
            append(Instruction.Create(OpCodes.Newobj, GetArgumentExceptionConstructor()));
            append(Instruction.Create(OpCodes.Throw));

            append(endInstruction);
        }

        private MethodReference GetArgumentExceptionConstructor()
        {
            var type = ModuleDefinition
                .Import(typeof(ArgumentException))
                .Resolve();
            var constructor = type.GetConstructors()
                .Single(c => c.Parameters.Count == 2 && c.Parameters.All(p => p.ParameterType.FullName == "System.String"));
            return ModuleDefinition.Import(constructor);
        }

        private MethodReference GetArgumentNullExceptionConstructor()
        {
            var type = ModuleDefinition
                .Import(typeof(ArgumentNullException))
                .Resolve();
            var constructor = type.GetConstructors()
                .Single(c => c.Parameters.Count == 1 && c.Parameters.Single().ParameterType.FullName == "System.String");
            return ModuleDefinition.Import(constructor);
        }
    }
}
