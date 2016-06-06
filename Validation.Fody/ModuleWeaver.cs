namespace Validation.Fody
{
    using System;
    using System.Xml.Linq;
    using Mono.Cecil;

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
        /// <summary>Execute the weaving operations</summary>
        public void Execute()
        {
        }
    }
}
