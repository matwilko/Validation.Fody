namespace Validation.Fody.Internals
{
    using System;
    using Mono.Cecil.Cil;

    internal sealed class Logger : ILogger
    {
        private Action<string> LogDebug { get; }
        private Action<string> LogInfo { get; }
        private Action<string> LogWarning { get; }
        private Action<string, SequencePoint> LogWarningPoint { get; }
        private Action<string> LogError { get; }
        private Action<string, SequencePoint> LogErrorPoint { get; }

        public Logger(Action<string> logDebug, Action<string> logInfo, Action<string> logWarning, Action<string, SequencePoint> logWarningPoint, Action<string> logError, Action<string, SequencePoint> logErrorPoint)
        {
            LogDebug = logDebug;
            LogInfo = logInfo;
            LogWarning = logWarning;
            LogWarningPoint = logWarningPoint;
            LogError = logError;
            LogErrorPoint = logErrorPoint;
        }

        public void Debug(string message) => LogDebug(message);
        public void Info(string message) => LogInfo(message);
        public void Warning(string message) => LogWarning(message);
        public void Warning(string message, SequencePoint sequencePoint) => LogWarningPoint(message, sequencePoint);
        public void Error(string message) => LogError(message);
        public void Error(string message, SequencePoint sequencePoint) => LogErrorPoint(message, sequencePoint);
    }
}