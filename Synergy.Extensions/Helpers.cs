using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synergy.Extensions {
	/// <summary>
	/// The static <see cref="Helpers"/> class.
	/// </summary>
	public static class Helpers {
		/// <summary>
		/// A Global static Random ensures that there will be least chances of repeated results.
		/// <para>Can be overridden on the respective functions.</para>
		/// </summary>
		private static readonly Random Random;

		/// <summary>		
		/// Assigns a Random instance with a unique seed value to <see cref="Random"/> object.
		/// </summary>
		static Helpers() => Random = new Random(new Guid().ToString().GetHashCode());

		/// <summary>
		/// Waits until the semaphore can be freed, and disposes it.
		/// </summary>
		/// <param name="semaphore">the reference to the semaphore.</param>
		public static void WaitAndDispose(ref SemaphoreSlim semaphore) {
			if (semaphore == null) {
				return;
			}

			if (semaphore.CurrentCount == 0) {
				semaphore.Wait();
			}

			semaphore.Release();
			semaphore.Dispose();
		}

		/// <summary>
		/// Execute an Action<<see cref="T"/>>() for each element inside an HashSet<<see cref="T"/>>()
		/// </summary>
		/// <typeparam name="T">The type of the HashSet elements.</typeparam>
		/// <param name="hashset">The HashSet</param>
		/// <param name="onElementAction">The action to execute for each element in <see cref="hashset"/></param>
		/// <param name="shouldNullCheck">Set as true if a null check should be done before executing the Action on the element.</param>
		/// <returns>True if all iteration when successfully.</returns>
		public static bool ForEachElement<T>(this HashSet<T> hashset, Action<T> onElementAction, bool shouldNullCheck = false) {
			if (hashset == null || hashset.Count <= 0 || onElementAction == null) {
				return false;
			}

			try {
				lock (hashset) {
					foreach (T value in hashset) {
						if (shouldNullCheck && value == null) {
							continue;
						}

						onElementAction.Invoke(value);
					}
				}

				return true;
			}
			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Execute an Action<TKey, TValue>() for each element inside a Dictionary<TKey, TValue>()
		/// </summary>
		/// <param name="dictionary">The dictionary to iterate on</param>
		/// <param name="onElementAction">The action to execute for each element in <see cref="dictionary"/></param>
		/// <param name="shouldNullCheck">Set as true if a null check should be done before executing the Action on the element.</param>
		/// <typeparam name="TKey">The Key Type of the <see cref="dictionary"/></typeparam>
		/// <typeparam name="TValue">The Value Type of the <see cref="dictionary"/></typeparam>
		/// <returns>True if all iteration when successfully.</returns>
		public static bool ForEachElement<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Action<TKey, TValue> onElementAction, bool shouldNullCheck = false) {
			if (dictionary == null || dictionary.Count <= 0 || onElementAction == null) {
				return false;
			}

			try {
				lock (dictionary) {
					foreach (KeyValuePair<TKey, TValue> pair in dictionary) {
						if (shouldNullCheck && (pair.Key == null || pair.Value == null)) {
							continue;
						}

						onElementAction.Invoke(pair.Key, pair.Value);
					}
				}

				return true;
			}
			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Waits for an array of tasks to finish execution. Blocks the calling thread until so.
		/// </summary>
		/// <param name="tasks">The task collection</param>
		public static void WaitForCompletion(params Task[] tasks) {
			if (tasks == null || tasks.Length <= 0) {
				return;
			}

			Task.WaitAll(tasks);
		}

		/// <summary>
		/// Gets all LAN networks
		/// </summary>
		/// <returns>Dictionary containing Host Name and IPAddress of the networks.</returns>
		public static Dictionary<string, IPAddress> GetAllLocalNetworks() {
			Dictionary<string, IPAddress> address = new Dictionary<string, IPAddress>();
			foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
				foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses) {
					if (!ip.IsDnsEligible && ip.Address.AddressFamily == AddressFamily.InterNetwork) {
						try {
							address.TryAdd(Dns.GetHostEntry(ip.Address).HostName, ip.Address);
						}
						catch { }
					}
				}
			}

			return address;
		}

		/// <summary>
		/// Tries to get a LAN network by its Host Name
		/// </summary>
		/// <param name="hostName">The name to search for.</param>
		/// <returns>The IPAddress of the resultant LAN network. null if no networks found/search failed.</returns>
		public static IPAddress? GetNetworkByHostName(string hostName) {
			if (string.IsNullOrEmpty(hostName)) {
				return default;
			}

			Dictionary<string, IPAddress> addresses = GetAllLocalNetworks();
			foreach (KeyValuePair<string, IPAddress> pair in addresses) {
				if (string.IsNullOrEmpty(pair.Key) || pair.Value == null) {
					continue;
				}

				if (pair.Key.Equals(hostName, StringComparison.OrdinalIgnoreCase)) {
					return pair.Value;
				}
			}

			return default;
		}

		/// <summary>
		/// Blocks the calling thread until the referred boolean condition returns true.
		/// </summary>
		/// <param name="condition">The state checking condition</param>
		/// <param name="interval">The interval between each blocking loop</param>
		public static void WaitWhile(Func<bool> condition, int interval = 25) {
			while (!condition.Invoke()) {
				Task.Delay(interval).Wait();
			}
		}

		/// <summary>
		/// Blocks the calling thread until the referred CancellationToken is canceled.
		/// </summary>
		/// <param name="_token">The Cancellation Token</param>
		/// <param name="interval">The interval between each blocking loop</param>
		/// <returns></returns>
		public static async Task WaitUntilCancellation(CancellationToken _token, int interval = 25) {
			while (!_token.IsCancellationRequested) {
				await Task.Delay(interval).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Tries to parse the given string value as boolean.
		/// </summary>
		/// <param name="value">The string to parse</param>
		/// <param name="booleanValue">Boolean value if parsing succeeded, else null.</param>
		/// <returns>If the parsing is success or not.</returns>
		public static bool TryParseAsBool(this string value, out bool? booleanValue) {
			if (string.IsNullOrEmpty(value)) {
				booleanValue = null;
				return false;
			}

			bool? temp = value switch
			{
				"1" => true,
				"0" => false,
				_ => null,
			};
			bool parseResult = bool.TryParse(value, out bool parsed);

			if (parseResult && parsed == temp) {
				booleanValue = parsed;
				return true;
			}
			else if (parseResult && parsed != temp) {
				booleanValue = parsed;
				return true;
			}
			else if (!parseResult && parsed != temp) {
				booleanValue = temp;
				return true;
			}
			else {
				booleanValue = null;
				return false;
			}
		}

		/// <summary>
		/// Get the current OS platform
		/// </summary>
		/// <returns>The OSPlatform</returns>
		public static OSPlatform GetPlatform() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				return OSPlatform.Windows;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				return OSPlatform.Linux;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				return OSPlatform.OSX;
			}

			return OSPlatform.Linux;
		}

		/// <summary>
		/// Generate a random, unique float value
		/// </summary>
		/// <param name="rand">The Random instance if the generation should use the specified instance. If not supplied, it will use default one.</param>
		/// <returns>The float value.</returns>
		public static float GenerateUniqueIdentifier(Random? rand = null) {
			rand ??= Random;
			int sign = rand.Next(2);
			int exponent = rand.Next((1 << 8) - 1);
			int mantissa = rand.Next(1 << 23);
			int bits = (sign << 31) + (exponent << 23) + mantissa;
			return BitsToFloat(bits);
		}

		/// <summary>
		/// Converts integer bits to float
		/// </summary>
		/// <param name="bits">the bits.</param>
		/// <returns>the resultant float value.</returns>
		private static float BitsToFloat(this int bits) {
			unsafe {
				return *(float*) &bits;
			}
		}

		/// <summary>
		/// Schedules an Action() to be executed after a specific delay.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="delay">The delay until execution</param>
		/// <returns>The underlying Timer instance used for current schedule.</returns>
		public static void ScheduleTask(Action action, TimeSpan delay) {
			if (action == null) {
				return;
			}

			Timer? TaskSchedulerTimer = null;

			TaskSchedulerTimer = new Timer(e => {
				InBackgroundThread(action, $"Task Scheduler_{action.GetHashCode()}");
				TaskSchedulerTimer?.Dispose();
			}, null, delay, delay);
		}

		/// <summary>
		/// Checks if the specified Socket is connected.
		/// </summary>
		/// <param name="s">The socket</param>
		/// <returns>The status.</returns>
		public static bool IsSocketConnected(Socket? s) {
			if (s == null) {
				return false;
			}

			return s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0);
		}

		/// <summary>
		/// Pings the specified IP address to see if the corresponding server is online.
		/// </summary>
		/// <param name="ip">The IPAddress of the destination server.</param>
		/// <returns></returns>
		public static bool IsServerOnline(IPAddress _ip) {
			if (_ip == null) {
				return false;
			}

			const int timeout = 10000;
			using (Ping ping = new Ping()) {
				PingReply _reply = ping.Send(_ip, timeout);
				return _reply.Status == IPStatus.Success;
			}
		}

		/// <summary>
		/// Pings the specified IP address to see if the corresponding server is online in async.
		/// </summary>
		/// <param name="_ip">The IPAddress of the destination server.</param>
		/// <returns></returns>
		public static async Task<bool> IsServerOnlineAsync(IPAddress _ip) {
			if (_ip == null) {
				return false;
			}

			const int timeout = 10000;
			using (Ping ping = new Ping()) {
				PingReply _reply = await ping.SendPingAsync(_ip, timeout).ConfigureAwait(false);
				return _reply.Status == IPStatus.Success;
			}
		}

		/// <summary>
		/// Executes the specified string command on the Bash Shell.
		/// </summary>
		/// <param name="cmd">The command string.</param>
		/// <returns>The execution result.</returns>
		public static string? ExecuteBash(this string cmd) {
			if (GetPlatform() != OSPlatform.Linux || string.IsNullOrEmpty(cmd)) {
				return null;
			}

			string escapedArgs = cmd.Replace("\"", "\\\"");
			string args = $"-c \"{escapedArgs}\"";

			using Process process = new Process() {
				StartInfo = new ProcessStartInfo {
					FileName = "/bin/bash",
					Arguments = args,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden
				}
			};

			StringBuilder result = new StringBuilder();

			if (process.Start()) {
				result.AppendLine(process.StandardOutput.ReadToEnd());
				result.AppendLine(process.StandardError.ReadToEnd());
				process.WaitForExit(TimeSpan.FromMinutes(6).Milliseconds);
			}

			return result.ToString();
		}

		/// <summary>
		/// Gets the current computers in LAN address.
		/// </summary>
		/// <returns>The LAN IPAddress</returns>
		public static IPAddress? GetLocalIpAddress() {
			using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
				socket.Connect("8.8.8.8", 65530);
				IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
				return endPoint?.Address;
			}
		}

		/// <summary>
		/// Gets a single character input from the user with a timespan delay. if the user fails to input before the delay, default value is returned.
		/// </summary>
		/// <param name="delay">The delay to wait.</param>
		/// <returns>The pressed key else null if nothing pressed.</returns>
		public static ConsoleKeyInfo? FetchUserInputSingleChar(TimeSpan delay) {
			Task<ConsoleKeyInfo> task = Task.Factory.StartNew(Console.ReadKey);
			Task<ConsoleKeyInfo> consoleKeyTask = Task.Factory.StartNew(() => Console.ReadKey(true));
			int exeCount = Task.WaitAny(new Task[]{
				consoleKeyTask
			}, delay);

			return exeCount == 0 ? consoleKeyTask.Result : default;
		}

		/// <summary>
		/// Sets the current console title.
		/// </summary>
		/// <param name="title">The title text.</param>
		public static void SetConsoleTitle(string title) {
			if (string.IsNullOrEmpty(title)) {
				return;
			}

			Console.Title = title;
		}

		/// <summary>
		/// Sets the current console title.
		/// </summary>
		/// <param name="title">The title text.</param>
		public static void AsConsoleTitle(this string title) => SetConsoleTitle(title);

		/// <summary>
		/// Converts UNIX time stamp to DateTime instance.
		/// </summary>
		/// <returns>The DateTime instance</returns>
		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();

		/// <summary>
		/// Converts the time stamp to DateTime instance.
		/// </summary>
		/// <returns>The DateTime instance</returns>
		public static DateTime ToDateTime(this double timestamp) => UnixTimeStampToDateTime(timestamp);

		/// <summary>
		/// Gets the current network's Public IP
		/// </summary>
		/// <returns>the IP if success, else null.</returns>
		public static async Task<string?> GetPublicIP() {
			if (!IsNetworkAvailable()) {
				return null;
			}

			return await "https://api.ipify.org/".RequestAsString().ConfigureAwait(false);
		}

		/// <summary>
		/// Gets an environment variable value, if it exists.
		/// </summary>
		/// <returns>The environment variable value.</returns>
		public static string? GetEnvironmentVariable(string variable, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine) => Environment.GetEnvironmentVariable(variable, target);

		/// <summary>
		/// Sets an environment variable.
		/// </summary>
		/// <param name="variableName">The variable name</param>
		/// <param name="variableValue">The variable value</param>
		/// <param name="target">The target of the environment variable</param>
		/// <returns>True if success, else false.</returns>
		public static bool SetEnvironmentVariable(string variableName, string variableValue, EnvironmentVariableTarget target) {
			try {
				Environment.SetEnvironmentVariable(variableName, variableValue, target);
				return true;
			}
			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Converts specified DateTime instance to 24 hour formate.
		/// </summary>
		/// <param name="source">The source DateTime Instance</param>
		/// <returns>The 12 hour formate DateTime Instance</returns>
		public static DateTime To24Hours(this DateTime source) =>
			DateTime.TryParse(source.ToString("yyyy MMMM d HH:mm:ss tt"), out DateTime result) ? result : DateTime.Now;

		/// <summary>
		/// Converts specified DateTime instance to 12 hour formate.
		/// </summary>
		/// <param name="source">The source DateTime Instance</param>
		/// <returns>The 24 hour formate DateTime Instance</returns>
		public static DateTime To12Hours(this DateTime source) =>
			DateTime.TryParse(source.ToString("dddd, dd MMMM yyyy"), out DateTime result) ? result : DateTime.Now;

		/// <summary>
		/// Downloads the specified URL request result as a string. if the string is not a URL, fails and returns null.
		/// </summary>
		/// <param name="url">The URL</param>
		/// <returns>The string result</returns>
		public static async Task<string?> RequestAsString(this string url) {
			if (string.IsNullOrEmpty(url)) {
				return null;
			}

			Uri requestUri;
			try {
				requestUri = new Uri(url);
			}
			catch {
				return default;
			}

			if (!IsNetworkAvailable()) {
				return null;
			}

			using (HttpClient client = new HttpClient()) {
				using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri)) {
					using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false)) {
						if (!response.IsSuccessStatusCode) {
							return default;
						}

						return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					}
				}
			}
		}

		/// <summary>
		/// Reads a string from the standard input (Console) and masks the entered value. Useful for passwords.
		/// </summary>
		/// <param name="mask">The mask character to use.</param>
		/// <returns>The received string. (Without any masking)</returns>
		public static string ReadLineMasked(char mask = '*') {
			StringBuilder result = new StringBuilder();

			ConsoleKeyInfo keyInfo;
			while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter) {
				if (!char.IsControl(keyInfo.KeyChar)) {
					result.Append(keyInfo.KeyChar);
					Console.Write(mask);
				}
				else if ((keyInfo.Key == ConsoleKey.Backspace) && (result.Length > 0)) {
					result.Remove(result.Length - 1, 1);

					if (Console.CursorLeft == 0) {
						Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
						Console.Write(' ');
						Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
					}
					else {

						// There are two \b characters here
						Console.Write(@" ");
					}
				}
			}

			Console.WriteLine();
			return result.ToString();
		}

		/// <summary>
		/// Writes the bytes to a file specified.
		/// </summary>
		/// <param name="bytesToWrite">The bytes</param>
		/// <param name="filePath">The path to the file, if file doesn't exist, it is created. If exists, it is overwritten.</param>
		public static async Task ToFile(this byte[] bytesToWrite, string filePath) {
			if (bytesToWrite.Length <= 0 || string.IsNullOrEmpty(filePath)) {
				return;
			}

			await File.WriteAllBytesAsync(filePath, bytesToWrite).ConfigureAwait(false);
		}

		/// <summary>
		/// Executes the specified action in a background thread. Not to be confused with InBackground();
		/// </summary>
		/// <param name="action">The action to execute.</param>
		/// <param name="threadName">The thread name. Will use default generated name if not specified.</param>
		/// <param name="longRunning">Set as true if the specified function is a long running one.</param>
		/// <returns>The created thread</returns>
		public static Thread? InBackgroundThread(Action action, string? threadName = null, bool longRunning = false) {
			if (action == null) {
				return null;
			}

			ThreadStart threadStart = new ThreadStart(action);
			Thread backgroundThread = new Thread(threadStart);

			if (longRunning) {
				backgroundThread.IsBackground = true;
			}

			backgroundThread.Name = threadName ?? action.GetHashCode().ToString();
			backgroundThread.Priority = ThreadPriority.Normal;
			backgroundThread.Start();
			return backgroundThread;
		}

		/// <summary>
		/// Schedules execution of an Action in background.
		/// </summary>
		/// <param name="action">The action to execute</param>
		/// <param name="longRunning">Set as true if the specified function is a long running one.</param>
		public static void InBackground(Action action, bool longRunning = false) {
			if (action == null) {
				return;
			}

			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

			if (longRunning) {
				options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
			}

			Task.Factory.StartNew(action, CancellationToken.None, options, TaskScheduler.Default);
		}

		/// <summary>
		/// Schedules execution of a Function<T> in background.
		/// </summary>
		/// <param name="function">The function to execute</param>
		/// <param name="longRunning">Set as true if the specified function is a long running one.</param>
		/// <typeparam name="T">The function type.</typeparam>
		public static void InBackground<T>(Func<T> function, bool longRunning = false) {
			if (function == null) {
				return;
			}

			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

			if (longRunning) {
				options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
			}

			Task.Factory.StartNew(function, CancellationToken.None, options, TaskScheduler.Default);
		}

		/// <summary>
		/// Execute all tasks in IEnumerable<Task<T>> as parallel in async way, and returns the result T when all of them completes.
		/// </summary>
		/// <param name="tasks">The collection of tasks to execute.</param>
		/// <typeparam name="T">The type of task.</typeparam>
		/// <returns></returns>
		public static async Task<IList<T>?> InParallel<T>(IEnumerable<Task<T>> tasks) {
			if (tasks == null) {
				return null;
			}

			IList<T> results = await Task.WhenAll(tasks).ConfigureAwait(false);
			return results;
		}

		/// <summary>
		/// Execute all tasks in IEnumerable<Task> as parallel, and returns when all of them completes.
		/// </summary>
		/// <param name="tasks">The collection of tasks to execute.</param>
		/// <returns></returns>
		public static async Task InParallel(IEnumerable<Task> tasks) {
			if (tasks == null) {
				return;
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}

		/// <summary>
		/// Check if Internet connectivity is present for the current system.
		/// </summary>
		/// <param name="ipAddress">The ip address of the host destination to check with. Will use default (8.8.8.8) if not specified.</param>
		/// <returns></returns>
		public static bool IsNetworkAvailable(IPAddress? ipAddress = null) {
			const int timeout = 1000;
			using (Ping ping = new Ping()) {
				IPAddress host = ipAddress ?? IPAddress.Parse("8.8.8.8");
				PingReply pingReply = ping.Send(host, timeout);
				return pingReply.Status == IPStatus.Success;
			}
		}

		/// <summary>
		/// Close a specified process, if it exists.
		/// </summary>
		/// <param name="processName">The process name</param>
		/// <param name="killSubProcesses">Pass as true if all the child processes of the specified parent process should also be killed off.</param>
		public static void CloseProcess(string processName, bool killSubProcesses = false) {
			if (string.IsNullOrEmpty(processName)) {
				return;
			}

			foreach (Process process in Process.GetProcessesByName(processName)) {
				process.Kill(killSubProcesses);
				process.WaitForExit();
				process.Dispose();
			}
		}
	}
}
