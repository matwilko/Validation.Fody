namespace Validation.Fody.Internals
{
    using System.Linq;
    using Mono.Cecil.Cil;

    internal sealed class IlProcessorAppender
    {
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

        public IlProcessorAppender(ILProcessor processor, Instruction firstInstruction)
        {
            Processor = processor;
            PreviousInstruction = firstInstruction;
        }
    }
}