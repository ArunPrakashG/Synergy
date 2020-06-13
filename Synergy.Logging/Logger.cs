using Synergy.Logging.EventArgs;
using Synergy.Logging.Interfaces;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Synergy.Logging {
	public class Logger : ILogger {
		public string? LogIdentifier { get; }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="_logIdentifier">the log identifier.</param>
		/// <returns></returns>
		public Logger(string _logIdentifier) => LogIdentifier = _logIdentifier ?? throw new ArgumentNullException(nameof(_logIdentifier) + " is null!");

		/// <summary>
		/// Log message received delegate.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event object consisting the fired log message details.</param>
		public delegate void OnLogMessageReceived(object sender, OnLogMessageReceivedEventArgs e);

		/// <summary>
		/// Fired when a log message is received.
		/// </summary>
		public static event OnLogMessageReceived? LogMessageReceived;

		public void Debug(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (string.IsNullOrEmpty(message)) {
				return;
			}

			LogMessageReceived?.Invoke(this, new OnLogMessageReceivedEventArgs(LogIdentifier, message, DateTime.Now, LogLevels.Debug, previousMethodName, callermemberlineNo, calledFilePath));
		}

		public void Error(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (string.IsNullOrEmpty(message)) {
				return;
			}

			LogMessageReceived?.Invoke(this, new OnLogMessageReceivedEventArgs(LogIdentifier, message, DateTime.Now, LogLevels.Error, previousMethodName, callermemberlineNo, calledFilePath));
		}

		public void Exception(Exception? exception,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (exception == null || exception.GetBaseException() == null) {
				return;
			}

			LogMessageReceived?.Invoke(this, new OnLogMessageReceivedEventArgs(LogIdentifier, exception.ToString(), DateTime.Now, LogLevels.Exception, previousMethodName, callermemberlineNo, calledFilePath));
		}

		public void Info(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (string.IsNullOrEmpty(message)) {
				return;
			}

			LogMessageReceived?.Invoke(this, new OnLogMessageReceivedEventArgs(LogIdentifier, message, DateTime.Now, LogLevels.Info, previousMethodName, callermemberlineNo, calledFilePath));
		}

		public void Trace(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (string.IsNullOrEmpty(message)) {
				return;
			}

			LogMessageReceived?.Invoke(this, new OnLogMessageReceivedEventArgs(LogIdentifier, message, DateTime.Now, LogLevels.Trace, previousMethodName, callermemberlineNo, calledFilePath));
		}

		public void Warning(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {

			if (string.IsNullOrEmpty(message)) {
				return;
			}

			LogMessageReceived?.Invoke(this, new OnLogMessageReceivedEventArgs(LogIdentifier, message, DateTime.Now, LogLevels.Warn, previousMethodName, callermemberlineNo, calledFilePath));
		}

		public void WithColor(string? message, ConsoleColor color = ConsoleColor.Cyan,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (string.IsNullOrEmpty(message)) {
				return;
			}

			LogMessageReceived?.Invoke(this, new OnLogMessageReceivedEventArgs(LogIdentifier, message, DateTime.Now, LogLevels.Custom, previousMethodName, callermemberlineNo, calledFilePath));
		}

		public void Input(string? message,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (string.IsNullOrEmpty(message)) {
				return;
			}

			LogMessageReceived?.Invoke(this, new OnLogMessageReceivedEventArgs(LogIdentifier, message, DateTime.Now, LogLevels.Input, previousMethodName, callermemberlineNo, calledFilePath));
		}

		public void Log(Exception? e,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (e == null) {
				return;
			}

			Exception(e, previousMethodName, callermemberlineNo, calledFilePath);
		}

		public void Log(string? message, LogLevels level = LogLevels.Info,
			[CallerMemberName] string? methodName = null,
			[CallerLineNumber] int lineNo = 0,
			[CallerFilePath] string? filePath = null) {
			if (string.IsNullOrEmpty(message)) {
				return;
			}

			switch (level) {
				case LogLevels.Trace:
					Trace($"[{Path.GetFileName(filePath)} | {lineNo}] {message}", methodName);
					break;

				case LogLevels.Debug:
					Debug(message, methodName);
					break;

				case LogLevels.Info:
					Info(message, methodName);
					break;

				case LogLevels.Warn:
					Warning($"[{Path.GetFileName(filePath)} | {lineNo}] " + message, methodName);
					break;

				case LogLevels.Green:
					WithColor(message, ConsoleColor.Green, methodName, lineNo, filePath);
					break;

				case LogLevels.Input:
					Input(message, methodName, lineNo, filePath);
					break;

				case LogLevels.Cyan:
					WithColor(message, ConsoleColor.Cyan, methodName, lineNo, filePath);
					break;

				case LogLevels.Custom:
					Console.WriteLine(message);
					Trace(message, methodName, lineNo, filePath);
					break;

				case LogLevels.Magenta:
					WithColor(message, ConsoleColor.Magenta, methodName, lineNo, filePath);
					break;

				case LogLevels.Error:
					Error(message, methodName, lineNo, filePath);
					break;

				case LogLevels.Red:
					WithColor(message, ConsoleColor.Red, methodName, lineNo, filePath);
					break;

				case LogLevels.Blue:
					WithColor(message, ConsoleColor.Blue, methodName, lineNo, filePath);
					break;

				case LogLevels.Exception:
					WithColor(message, ConsoleColor.DarkRed, methodName, lineNo, filePath);
					break;

				case LogLevels.Fatal:
					WithColor(message, ConsoleColor.DarkYellow, methodName, lineNo, filePath);
					break;

				default:
					goto case LogLevels.Info;
			}
		}

		public enum LogLevels {
			Trace,
			Debug,
			Info,
			Warn,
			Error,
			Exception,
			Fatal,
			Green,
			Red,
			Blue,
			Cyan,
			Magenta,
			Input,
			Custom
		}
	}
}
