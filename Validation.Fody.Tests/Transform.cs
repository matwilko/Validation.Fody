namespace Validation.Fody.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Mono.Cecil;
    using ValidationAttributes;

    internal static class Transform
    {
        public static TransformResult FromSource(string source)
        {
            var compilation = CompileSource(source);
            var assemblyStream = GetAssemblyAsStream(compilation);
            var module = CreateModuleFromStream(assemblyStream);

            var messageList = new List<BuildMessage>();

            new ModuleWeaver
            {
                LogError = m => new BuildMessage(MessageLevel.Error, m),
                LogInfo = m => new BuildMessage(MessageLevel.Info, m),
                ModuleDefinition = module
            }.Execute();

            if (messageList.Any(m => m.Level == MessageLevel.Error))
            {
                return new TransformResult(messageList.ToImmutableList());
            }

            using (var ms = new MemoryStream())
            {
                module.Write(ms);
                var assembly = Assembly.Load(ms.GetBuffer());
                return new TransformResult(assembly, messageList.ToImmutableList());
            }
        }

        private static CSharpCompilation CompileSource(string source) => CSharpCompilation.Create(
            assemblyName: Guid.NewGuid().ToString(),
            syntaxTrees: new[] {CSharpSyntaxTree.ParseText(source)},
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof (object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof (ReferenceType).Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        private static Stream GetAssemblyAsStream(CSharpCompilation compilation)
        {
            var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                ms.Dispose();

                throw new AggregateException(
                    emitResult.Diagnostics.Select(d => new Exception(d.ToString())).ToList()
                );
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private static ModuleDefinition CreateModuleFromStream(Stream assemblyStream)
        {
            return ModuleDefinition.ReadModule(assemblyStream, new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                ReadSymbols = false
            });
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

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public TransformResult(Assembly assembly, ImmutableList<BuildMessage> buildMessages)
            {
                Assembly = assembly;
                BuildMessages = buildMessages;
            }
        }

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

        public enum MessageLevel
        {
            Error,
            Info
        }
    }
}
