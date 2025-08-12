using System;

using UnityEngine;

namespace Solution.Common.Misc { 
    
    /**
    * <summary>
    * Defines a common logging interface for application components.
    * Provides methods for logging error, warning and informational messages,
    * allowing for consistent logging practices across different implementations.
    * </summary>
    */
    public interface ICommonLogger {

        static ICommonLogger CreateDefaultLogger() => new DefaultLogger();
        
        void LogError(string message, Exception exception = null);
        void LogWarning(string message);
        void LogInfo(string message);

        private class DefaultLogger : ICommonLogger {
            public void LogError(string message, Exception exception = null) {
                Debug.unityLogger.Log(LogType.Error, message + exception.Message + exception.Source + exception.StackTrace);
            }

            public void LogInfo(string message) {
                Debug.unityLogger.Log(LogType.Log, message);
            }

            public void LogWarning(string message) {
                Debug.unityLogger.Log(LogType.Warning, message);
            }
        }
    }
}