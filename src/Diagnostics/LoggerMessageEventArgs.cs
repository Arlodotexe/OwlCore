using System;
using System.Runtime.CompilerServices;

namespace OwlCore.Diagnostics
{
    /// <summary>
    /// Contains information about a logged message.
    /// </summary>
    public class LoggerMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="LoggerMessageEventArgs"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerFilePath"></param>
        /// <param name="callerLineNumber"></param>
        public LoggerMessageEventArgs(string message, LogLevel level, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            Message = message;
            Level = level;
            CallerMemberName = callerMemberName;
            CallerFilePath = callerFilePath;
            CallerLineNumber = callerLineNumber;
        }
        
        /// <summary>
        /// The logged message.
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// The name of the method where the message was logged.
        /// </summary>
        public string CallerMemberName { get; }
        
        /// <summary>
        /// The name of the file where the message was logged.
        /// </summary>
        public string CallerFilePath { get; }
        
        /// <summary>
        /// The line number in the file where the message was logged.
        /// </summary>
        public int CallerLineNumber { get; }
        
        /// <summary>
        /// The severity of the logged message. 
        /// </summary>
        public LogLevel Level { get; }
        
        /// <summary>
        /// An exception associated with this log message, if any.
        /// </summary>
        public Exception? Exception { get; set; }
    }
}