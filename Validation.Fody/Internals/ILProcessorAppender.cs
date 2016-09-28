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

        public Instruction Append(Instruction instruction)
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

            return instruction;
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

        public Instruction Append(OpCode opcode) => Append(Instruction.Create(opcode));
        public Instruction Append(OpCode opcode, TypeReference type) => Append(Instruction.Create(opcode, type));
        public Instruction Append(OpCode opcode, CallSite site) => Append(Instruction.Create(opcode, site));
        public Instruction Append(OpCode opcode, MethodReference method) => Append(Instruction.Create(opcode, method));
        public Instruction Append(OpCode opcode, FieldReference field) => Append(Instruction.Create(opcode, field));
        public Instruction Append(OpCode opcode, string value) => Append(Instruction.Create(opcode, value));
        public Instruction Append(OpCode opcode, sbyte value) => Append(Instruction.Create(opcode, value));
        public Instruction Append(OpCode opcode, byte value) => Append(Instruction.Create(opcode, value));
        public Instruction Append(OpCode opcode, int value) => Append(Instruction.Create(opcode, value));
        public Instruction Append(OpCode opcode, long value) => Append(Instruction.Create(opcode, value));
        public Instruction Append(OpCode opcode, float value) => Append(Instruction.Create(opcode, value));
        public Instruction Append(OpCode opcode, double value) => Append(Instruction.Create(opcode, value));
        public Instruction Append(OpCode opcode, Instruction target) => Append(Instruction.Create(opcode, target));
        public Instruction Append(OpCode opcode, Instruction[] targets) => Append(Instruction.Create(opcode, targets));
        public Instruction Append(OpCode opcode, VariableDefinition variable) => Append(Instruction.Create(opcode, variable));
        public Instruction Append(OpCode opcode, ParameterDefinition parameter) => Append(Instruction.Create(opcode, parameter));
    }
}