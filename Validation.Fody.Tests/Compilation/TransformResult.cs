namespace Validation.Fody.Tests.Compilation
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    public sealed class TransformResult<T>
    {
        public bool Success => !BuildMessages.Any(m => m.Level == MessageLevel.Error);
        public ImmutableList<BuildMessage> BuildMessages { get; }
        public Action<T> Action { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public TransformResult(ImmutableList<BuildMessage> buildMessages)
        {
            BuildMessages = buildMessages;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public TransformResult(Action<T> action, ImmutableList<BuildMessage> buildMessages)
        {
            Action = action;
            BuildMessages = buildMessages;
        }
    }

    public sealed class TransformResult
    {
        public bool Success => !BuildMessages.Any(m => m.Level == MessageLevel.Error);
        public ImmutableList<BuildMessage> BuildMessages { get; }
        public Assembly Assembly { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public TransformResult(ImmutableList<BuildMessage> buildMessages)
        {
            BuildMessages = buildMessages;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public TransformResult(Assembly assembly, ImmutableList<BuildMessage> buildMessages)
        {
            Assembly = assembly;
            BuildMessages = buildMessages;
        }
    }
}