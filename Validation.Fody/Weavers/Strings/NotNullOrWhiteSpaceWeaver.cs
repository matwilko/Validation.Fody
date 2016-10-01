namespace Validation.Fody.Weavers.Strings
{
    using System;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Internals;
    using ValidationAttributes.Strings;

    internal sealed class NotNullOrWhiteSpaceWeaver : IAttributeWeaver<NotNullOrWhiteSpaceAttribute>
    {
        private ModuleDefinition ModuleDefinition { get; }

        public NotNullOrWhiteSpaceWeaver(ModuleDefinition moduleDefinition)
        {
            ModuleDefinition = moduleDefinition;
        }

        public bool IsValidOn(TypeReference type)
        {
            return type.FullName == "System.String" || type.FullName == "System.Char[]";
        }

        public void Execute(NotNullOrWhiteSpaceAttribute attribute, ParameterDefinition parameter, IlProcessorAppender ilProcessor)
        {
            GenerateCheck(
                parameter,
                ilProcessor,
                parameter.Name
            );
        }

        public void Execute(NotNullOrWhiteSpaceAttribute attribute, PropertyDefinition property, IlProcessorAppender ilProcessor)
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
            var whiteSpaceCheckInitializer = Instruction.Create(OpCodes.Ldc_I4_0);

            ilProcessor.Append(OpCodes.Ldarg, parameter);
            ilProcessor.Append(OpCodes.Brtrue_S, whiteSpaceCheckInitializer);

            ilProcessor.Append(OpCodes.Ldstr, name);
            ilProcessor.Append(OpCodes.Newobj, GetArgumentNullExceptionConstructor());
            ilProcessor.Append(OpCodes.Throw);

            // Whitespace Check
            ilProcessor.Append(whiteSpaceCheckInitializer);
            var counter = ilProcessor.DeclareLocal(ModuleDefinition.TypeSystem.Int32, "<>__" + parameter.Name + "__WhiteSpaceCheckCounter");
            ilProcessor.Append(OpCodes.Stloc, counter);

            var loopCondition = Instruction.Create(OpCodes.Ldloc, counter);
            ilProcessor.Append(OpCodes.Br, loopCondition);
            
            // Loop body
            var loopBody = ilProcessor.Append(OpCodes.Ldarg, parameter);
            ilProcessor.Append(OpCodes.Ldloc, counter);
            ilProcessor.Append(GetElement(parameter));
            ilProcessor.Append(OpCodes.Call, GetIsWhiteSpace());
            ilProcessor.Append(OpCodes.Brfalse, endInstruction);

            // i++
            ilProcessor.Append(OpCodes.Ldloc, counter);
            ilProcessor.Append(OpCodes.Ldc_I4_1);
            ilProcessor.Append(OpCodes.Add);
            ilProcessor.Append(OpCodes.Stloc, counter);

            // Loop condition
            ilProcessor.Append(loopCondition); // Ldloc counter
            ilProcessor.Append(OpCodes.Ldarg, parameter);
            ilProcessor.Append(OpCodes.Call, GetLength(parameter));
            ilProcessor.Append(OpCodes.Clt);
            ilProcessor.Append(OpCodes.Brtrue, loopBody);

            // Fall through if we got to the end of the string without detecting non-whitespace
            ilProcessor.Append(OpCodes.Ldstr, "The given string cannot be empty or only contain whitespace");
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

        private MethodReference GetLength(ParameterDefinition parameter)
        {
            var type = parameter.ParameterType.FullName == "System.String"
                ? ModuleDefinition.TypeSystem.String
                : ModuleDefinition.Import(typeof(Array));
            var method = type.Resolve()
                .Properties.Single(p => p.Name == "Length")
                .GetMethod;
            return ModuleDefinition.Import(method);
        }

        private Instruction GetElement(ParameterDefinition parameter)
        {
            if (parameter.ParameterType.FullName == "System.String")
            {
                var method = ModuleDefinition.TypeSystem.String
                    .Resolve()
                    .Properties.Single(p => p.Name == "Chars")
                    .GetMethod;
                return Instruction.Create(OpCodes.Call, ModuleDefinition.Import(method));
            }
            else
            {
                return Instruction.Create(OpCodes.Ldelem_U2);
            }
        } 

        private MethodReference GetIsWhiteSpace()
        {
            var method = ModuleDefinition.TypeSystem.Char
                .Resolve()
                .Methods.Single(m =>
                    m.IsStatic
                    && m.Name == "IsWhiteSpace"
                    && m.Parameters.Count == 1
                    && m.Parameters.Single().ParameterType.FullName == "System.Char"
                );
            return ModuleDefinition.Import(method);
        }


    }
}
