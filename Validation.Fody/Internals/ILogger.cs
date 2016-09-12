namespace Validation.Fody.Internals
{
    using Mono.Cecil.Cil;

    internal interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Warning(string message, SequencePoint sequencePoint);
        void Error(string message);
        void Error(string message, SequencePoint sequencePoint);
    }
}
