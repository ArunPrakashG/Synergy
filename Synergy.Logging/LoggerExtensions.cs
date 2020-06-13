using Synergy.Logging.Interfaces;
using System;
using System.Runtime.CompilerServices;
using static Synergy.Logging.Logger;

namespace Synergy.Logging {
	/// <summary>
	/// Contains extension methods which helps to log messages easily.
	/// </summary>
	public static class LoggerExtensions {		
		public static void LogInfo(this string msg, ILogger logger,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (!string.IsNullOrEmpty(msg)) {
				logger.Log(msg, LogLevels.Info, previousMethodName, callermemberlineNo, calledFilePath);
			}
		}

		public static void LogTrace(this string msg, ILogger logger,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (!string.IsNullOrEmpty(msg)) {
				logger.Log(msg, LogLevels.Trace, previousMethodName, callermemberlineNo, calledFilePath);
			}
		}

		public static void LogDebug(this string msg, ILogger logger,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (!string.IsNullOrEmpty(msg)) {
				logger.Log(msg, LogLevels.Debug, previousMethodName, callermemberlineNo, calledFilePath);
			}
		}

		public static void LogException(this Exception e, ILogger logger,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (e != null) {
				logger.Log(e, previousMethodName, callermemberlineNo, calledFilePath);
			}
		}

		public static void LogWarning(this string msg, ILogger logger,
			[CallerMemberName] string? previousMethodName = null,
			[CallerLineNumber] int callermemberlineNo = 0,
			[CallerFilePath] string? calledFilePath = null) {
			if (!string.IsNullOrEmpty(msg)) {
				logger.Log(msg, LogLevels.Warn, previousMethodName, callermemberlineNo, calledFilePath);
			}
		}
	}
}
