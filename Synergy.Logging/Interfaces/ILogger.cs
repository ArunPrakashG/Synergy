using System;
using System.Runtime.CompilerServices;
using static Synergy.Logging.Logger;

namespace Synergy.Logging.Interfaces {
	/// <summary>
	/// Implements Logger.
	/// </summary>
	public interface ILogger {		
		/// <summary>
		/// The identifier name to be appended with each log message.
		/// </summary>
		/// <value></value>
		string? LogIdentifier { get; }
		
		/// <summary>
		/// Logs a Debug level message.
		/// </summary>
		/// <param name="message">the log message</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Debug(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);
		
		/// <summary>
		/// Logs a Info level message.
		/// </summary>
		/// <param name="message">the log message</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Info(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);

		/// <summary>
		/// Logs a Error level message.
		/// </summary>
		/// <param name="message">the log message</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Error(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);

		/// <summary>
		/// Logs a Warning level message.
		/// </summary>
		/// <param name="message">the log message</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Warning(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);

		/// <summary>
		/// Logs a Exception level message.
		/// </summary>
		/// <param name="e">the exception object.</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Exception(Exception? e,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);

		/// <summary>
		/// Logs a Trace level message.
		/// </summary>
		/// <param name="message">the log message</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Trace(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);

		/// <summary>
		/// Logs a Info level message with <see cref="ConsoleColor"/> as foreground color for the console.
		/// </summary>
		/// <param name="message">the log message</param>
		/// <param name="color">the console forground color to use while displaying this message.</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void WithColor(string? message, ConsoleColor color = ConsoleColor.Cyan,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);

		/// <summary>
		/// Logs a Input level message.
		/// <para>Used to log any inputs received from the user.</para>
		/// </summary>
		/// <param name="message">the log message</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Input(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);

		/// <summary>
		/// Logs a message at the specified <see cref="LogLevels"/> level.
		/// </summary>
		/// <param name="message">the log message</param>
		/// <param name="level">the level at which the message is to be logged.</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Log(string? message, LogLevels level = LogLevels.Info,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);

		/// <summary>
		/// Logs an exception.
		/// </summary>
		/// <param name="e">the exception object.</param>
		/// <param name="previousMethodName">the caller method name were this call orginated from. Will be filled automatically.</param>
		/// <param name="callermemberlineNo">the caller line number were this call orginated from. Will be filled automatically.</param>
		/// <param name="calledFilePath">the caller file path were this call orginated from. Will be filled automatically.</param>
		void Log(Exception? e,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null);
	}
}
