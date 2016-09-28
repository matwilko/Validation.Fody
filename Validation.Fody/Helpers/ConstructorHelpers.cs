namespace Validation.Fody.Helpers
{
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal static class ConstructorHelpers
    {
        public static Instruction FindChainedCall(this MethodDefinition method)
        {
            return method.Body.Instructions
                .First(i => i.OpCode.Code == Code.Call && ((MethodReference) i.Operand).Name == ".ctor");
        }
    }
}
