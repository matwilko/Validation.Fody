namespace Validation.Fody.Internals
{
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal sealed class IlProcessorAppender
    {
        private MethodDefinition Method { get; }
        private ILProcessor Processor { get; }
        private Instruction PreviousInstruction { get; set; }

        public void Append(Instruction instruction)
        {
            if (PreviousInstruction == null)
            {
                var firstInstruction = Processor.Body.Instructions.First();
                Processor.InsertBefore(firstInstruction, instruction);
            }
            else if (PreviousInstruction.OpCode.Code == Code.Nop)
            {
                Processor.Replace(PreviousInstruction, instruction);
            }
            else
            {
                Processor.InsertAfter(PreviousInstruction, instruction);
            }

            PreviousInstruction = instruction;
        }

        public IlProcessorAppender(MethodDefinition method, Instruction firstInstruction)
        {
            Method = Method;
            Processor = method.Body.GetILProcessor();
            PreviousInstruction = firstInstruction;
        }

        /// <summary>
        /// Creates a new local variable in the method with the given type and name
        /// </summary>
        public VariableDefinition DeclareLocal(TypeReference type, string name)
        {
            var varDef = new VariableDefinition(name, type);
            Method.Body.Variables.Add(varDef);
            return varDef;
        }
    }
}