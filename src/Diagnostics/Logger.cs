using System;
using System.Runtime.CompilerServices;

namespace OwlCore.Diagnostics
{
    /// <summary>
    /// Routes log messages in through methods and out through an event.
    /// </summary>
    /// <remarks>
    /// If System.Diagnostics.Debug can be static, so can we.
    /// </remarks>
    public static class Logger
    {
        /// <summary>
        /// Logs an unrecoverable application or system crash, or a catastrophic failure that requires immediate attention.
        /// </summary>
        public static void LogCritical(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            MessageReceived?.Invoke(null, new LoggerMessageEventArgs(msg, LogLevel.Critical, memberName, fileName, lineNumber));
        }

        /// <summary>
        /// Logs when the current flow of execution is stopped due to a failure. These should indicate a failure in the current activity, not an application-wide failure.
        /// </summary>
        public static void LogError(string msg, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            MessageReceived?.Invoke(null, new LoggerMessageEventArgs(msg, LogLevel.Error, memberName, fileName, lineNumber)
            {
                Exception = exception,
            });
        }
        
        /// <summary>
        /// Logs when the current flow of execution is stopped due to a failure. These should indicate a failure in the current activity, not an application-wide failure.
        /// </summary>
        public static void LogWarning(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            MessageReceived?.Invoke(null, new LoggerMessageEventArgs(msg, LogLevel.Warning, memberName, fileName, lineNumber));
        }
        
        /// <summary>
        /// Logs the general flow of the application. Can be used for interactive investigation during development.
        /// </summary>
        public static void LogInformation(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            MessageReceived?.Invoke(null, new LoggerMessageEventArgs(msg, LogLevel.Information, memberName, fileName, lineNumber));
        }
        
        /// <summary>
        /// Logs an extremely detailed message. May contain sensitive application data. These messages should never be enabled in a production environment.
        /// </summary>
        public static void LogTrace(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            MessageReceived?.Invoke(null, new LoggerMessageEventArgs(msg, LogLevel.Trace, memberName, fileName, lineNumber));
        }

        /// <summary>
        /// Logs a message of variable severity.
        /// </summary>
        public static void Log(string msg, LogLevel level, [CallerMemberName] string memberName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            MessageReceived?.Invoke(null, new LoggerMessageEventArgs(msg, LogLevel.Critical, memberName, fileName, lineNumber));
        }

        /// <summary>
        /// Raised when a message is logged.
        /// </summary>
        public static event EventHandler<LoggerMessageEventArgs>? MessageReceived;
    }
}