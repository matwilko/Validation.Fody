namespace Validation.Fody.Tests.Compilation
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.IO;
    using Mono.Cecil;
    using ValidationAttributes;

    internal static class Compilation
    {
        private static MetadataReference[] CompilationReferences { get; } =
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ReferenceType).Assembly.Location)
        };

        private static CSharpCompilationOptions CompilationOptions { get; } = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release,
            concurrentBuild: true,
            generalDiagnosticOption: ReportDiagnostic.Hidden,
            warningLevel: 0
        );

        private static CSharpParseOptions ParseOptions { get; } = new CSharpParseOptions(
            LanguageVersion.CSharp6,
            DocumentationMode.None
        );

        public static CSharpCompilation CompileSource(string source) => CSharpCompilation.Create(
            assemblyName: Guid.NewGuid().ToString(),
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, ParseOptions) },
            references: CompilationReferences,
            options: CompilationOptions
        );

        private static RecyclableMemoryStreamManager MemoryStreamManager { get; } = new RecyclableMemoryStreamManager();

        public static Stream GetAssemblyAsStream(CSharpCompilation compilation)
        {
            var ms = MemoryStreamManager.GetStream();
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

        private static ReaderParameters ReaderParameters { get; } = new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            ReadSymbols = false
        };

        public static AssemblyDefinition GetAssemblyDefinitionFromStream(Stream assemblyStream)
        {
            using (assemblyStream)
            {
                return AssemblyDefinition.ReadAssembly(assemblyStream, ReaderParameters);
            }
        }

        public static Assembly GetLoadedAssemblyFromDefinition(AssemblyDefinition assemblyDefinition)
        {
            using (var ms = MemoryStreamManager.GetStream())
            {
                assemblyDefinition.Write(ms);
                return Assembly.Load(ms.GetBuffer());
            }
        }
    }
}