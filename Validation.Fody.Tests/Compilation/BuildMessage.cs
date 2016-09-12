namespace Validation.Fody.Tests.Compilation
{
    public sealed class BuildMessage
    {
        public MessageLevel Level { get; }
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public BuildMessage(MessageLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }
}