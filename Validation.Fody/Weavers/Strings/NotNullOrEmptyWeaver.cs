namespace Validation.Fody.Weavers.Strings
{
    using System;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Internals;
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

        public void Execute(NotNullOrEmptyAttribute attribute, ParameterDefinition parameter, IlProcessorAppender ilProcessor)
        {
            GenerateCheck(
                parameter,
                ilProcessor,
                parameter.Name
            );
        }

        public void Execute(NotNullOrEmptyAttribute attribute, PropertyDefinition property, IlProcessorAppender ilProcessor)
        {
            GenerateCheck(
                property.SetMethod.Parameters.First(),
                ilProcessor,
                "value"
            );
        }

        private void GenerateCheck(ParameterDefinition parameter, IlProcessorAppender ilProcessor, string name)
        {
            var endInstruction = Instruction.Create(OpCodes.Nop);

            // Null Check
            var emptyCheckLdArg = Instruction.Create(OpCodes.Ldarg, parameter);

            ilProcessor.Append(OpCodes.Ldarg, parameter);
            ilProcessor.Append(OpCodes.Brtrue_S, emptyCheckLdArg);

            ilProcessor.Append(OpCodes.Ldstr, name);
            ilProcessor.Append(OpCodes.Newobj, GetArgumentNullExceptionConstructor());
            ilProcessor.Append(OpCodes.Throw);

            // Empty Check
            ilProcessor.Append(emptyCheckLdArg);
            
            if (parameter.ParameterType.FullName == "System.String")
            {
                var stringLength = ModuleDefinition.Import(typeof(string)).Resolve()
                    .Properties.Single(p => p.Name == "Length")
                    .GetMethod;

                ilProcessor.Append(OpCodes.Call, ModuleDefinition.Import(stringLength));
            }
            else if (parameter.ParameterType.FullName == "System.Char[]")
            {
                var arrayLength = ModuleDefinition.Import(typeof(Array)).Resolve()
                    .Properties.Single(p => p.Name == "Length")
                    .GetMethod;

                ilProcessor.Append(OpCodes.Call, ModuleDefinition.Import(arrayLength));
            }

            ilProcessor.Append(OpCodes.Ldc_I4_0);
            ilProcessor.Append(OpCodes.Bne_Un_S, endInstruction);

            ilProcessor.Append(OpCodes.Ldstr, "The given string cannot be empty");
            ilProcessor.Append(OpCodes.Ldstr, name);
            ilProcessor.Append(OpCodes.Newobj, GetArgumentExceptionConstructor());
            ilProcessor.Append(OpCodes.Throw);

            ilProcessor.Append(endInstruction);
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
