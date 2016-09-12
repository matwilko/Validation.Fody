namespace Validation.Fody
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using Internals;
    using Internals.Config;
    using Internals.Discovery;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Performs weaving operations on the supplied assembly
    /// </summary>
    public sealed class ModuleWeaver
    {
        #region Fody Injected Properties

        /// <summary>The definition of the CIL module to rewrite</summary>
        public ModuleDefinition ModuleDefinition { get; set; }
        
        /// <summary>The configuration element for this Fody module, if present</summary>
        public XElement Config { get; set; }

        /// <summary>Logs a Debug level MSBuild message</summary>
        public Action<string> LogDebug { get; set; } = m => { };
        
        /// <summary>Logs an Info level MSBuild message</summary>
        public Action<string> LogInfo { get; set; } = m => { };
        
        /// <summary>Logs a Warning level MSBuild message</summary>
        public Action<string> LogWarning { get; set; } = m => { };
        
        /// <summary>Logs a Warning level MSBuild message with a sequence point</summary>
        public Action<string, SequencePoint> LogWarningPoint { get; set; } = (m, sp) => { };

        /// <summary>Logs an Error level MSBuild message</summary>
        public Action<string> LogError { get; set; } = m => { };
        
        /// <summary>Logs an Error level MSBuild message with a sequence point</summary>
        public Action<string, SequencePoint> LogErrorPoint { get; set; } = (m, sp ) => { };

        #endregion

        private Injector Injector { get; } = new Injector();

        /// <summary>Execute the weaving operations</summary>
        public void Execute()
        {
            RegisterServices();

            var weavers = Discovery.GetTransformableAttributes(Injector);

            var weavableProperties = Discovery.GetWeavableProperties(ModuleDefinition.Types, weavers);
            Processor.ProcessWeavableProperties(Injector.Resolve<ILogger>(), weavableProperties);

            var weavableParameters = Discovery.GetWeavableParameters(ModuleDefinition.Types, weavers);
            Processor.ProcessWeavableParameters(Injector.Resolve<ILogger>(), weavableParameters);

            RemoveValidationAttributesReference();
        }
        
        private void RegisterServices()
        {
            Injector.Register(ModuleDefinition);
            Injector.Register<ILogger>(new Logger(LogDebug, LogInfo, LogWarning, LogWarningPoint, LogError, LogErrorPoint));
            Injector.Register<Config, Config>(requestingType => new Config(requestingType, Config));
            Injector.RegisterOpenGeneric(typeof(IConfig<>), typeof(Config<>), CreateTypedConfig);
        }

        private object CreateTypedConfig(Type[] genericArguments, Type requestingType)
        {
            return typeof (Config<>).MakeGenericType(genericArguments)
                .GetConstructors().Single()
                .Invoke(null, new object[] { requestingType, Config });
        }

        private void RemoveValidationAttributesReference()
        {
            var attributeReferences = ModuleDefinition.AssemblyReferences
                .Where(ar => ar.Name == "ValidationAttributes")
                .ToArray();

            foreach (var reference in attributeReferences)
            {
                ModuleDefinition.AssemblyReferences.Remove(reference);
            }
        }
    }
}
