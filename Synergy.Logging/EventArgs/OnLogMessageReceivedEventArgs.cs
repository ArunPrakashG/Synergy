using System;
using static Synergy.Logging.Logger;

namespace Synergy.Logging.EventArgs {
	/// <summary>
	/// Contains information of the fired log message.
	/// </summary>
	public sealed class OnLogMessageReceivedEventArgs {	
		/// <summary>
		/// The identifier provided by the calling instance.
		/// </summary>
		public readonly string LogIdentifier;	

		/// <summary>
		/// The log message.
		/// </summary>
		public readonly string LogMessage;		

		/// <summary>
		/// The event time of the log message.
		/// </summary>
		public readonly DateTime ReceivedTime;

		/// <summary>
		/// The level of this log message.
		/// </summary>		
		public readonly LogLevels LogLevel;

		/// <summary>
		/// The function() name from where the log method was called.
		/// </summary>		
		public readonly string? CallerMemberName;	

		/// <summary>
		/// The line number from where the log method was called.
		/// </summary>	
		public readonly int CallerLineNumber;

		/// <summary>
		/// The path of the file from which the log method was called.
		/// </summary>		
		public readonly string? CallerFilePath;

		/// <summary>
		/// The constructor.
		/// </summary>
		/// <param name="_logId">Sets the <see cref="LogIdentifier"/></param>.
		/// <param name="_logMessage">Sets the <see cref="LogMessage"/></param>.
		/// <param name="_logTime">Sets the <see cref="ReceivedTime"/></param>.
		/// <param name="_logLevel">Sets the <see cref="LogLevel"/></param>.
		/// <param name="_callerMethodName">Sets the <see cref="CallerMemberName"/></param>.
		/// <param name="_callerLineNumber">Sets the <see cref="CallerLineNumber"/></param>.
		/// <param name="_callerFilePath">Sets the <see cref="CallerFilePath"/></param>.
		public OnLogMessageReceivedEventArgs(string _logId, string _logMessage, DateTime _logTime, LogLevels _logLevel, string? _callerMethodName, int _callerLineNumber, string? _callerFilePath) {
			LogIdentifier = _logId ?? throw new ArgumentNullException(nameof(_logId) + " cannot be null or empty.");
			LogMessage = _logMessage?? throw new ArgumentNullException(nameof(_logMessage) + " cannot be null or empty.");;
			ReceivedTime = _logTime;
			LogLevel = _logLevel;
			CallerMemberName = _callerMethodName;
			CallerLineNumber = _callerLineNumber;
			CallerFilePath = _callerFilePath;
		}
	}
}
