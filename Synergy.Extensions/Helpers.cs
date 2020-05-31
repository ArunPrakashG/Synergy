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
using static Synergy.Extensions.Enums;

namespace Synergy.Extensions {
	public static class Helpers {
		public static bool ForEachElement<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Action<TKey, TValue> onElementAction, bool shouldNullCheck = true) {
			if(dictionary == null || dictionary.Count <= 0 || onElementAction == null) {
				return false;
			}

			try {
				lock (dictionary) {
					foreach (KeyValuePair<TKey, TValue> pair in dictionary) {
						if(shouldNullCheck && (pair.Key == null || pair.Value == null)) {
							continue;
						}

						onElementAction.Invoke(pair.Key, pair.Value);
					}
				}

				return true;
			}
			catch(Exception) {
				return false;
			}
		}

		public static EOSType GetOSType() {
			OperatingSystem osVer = Environment.OSVersion;
			Version ver = osVer.Version;

			switch (osVer.Platform) {
				case PlatformID.Win32Windows: {
						return ver.Minor switch
						{
							0 => EOSType.Win95,
							10 => EOSType.Win98,
							90 => EOSType.WinME,
							_ => EOSType.WinUnknown,
						};
					}

				case PlatformID.Win32NT: {
						switch (ver.Major) {
							case 4:
								return EOSType.WinNT;

							case 5:
								switch (ver.Minor) {
									case 0:
										return EOSType.Win2000;

									case 1:
										return EOSType.WinXP;

									case 2:
										// Assume nobody runs Windows XP Professional x64 Edition
										// It's an edition of Windows Server 2003 anyway.
										return EOSType.Win2003;
								}

								goto default;

							case 6:
								switch (ver.Minor) {
									case 0:
										return EOSType.WinVista; // Also Server 2008

									case 1:
										return EOSType.Windows7; // Also Server 2008 R2

									case 2:
										return EOSType.Windows8; // Also Server 2012

									// Note: The OSVersion property reports the same version number (6.2.0.0) for both Windows 8 and Windows 8.1.- http://msdn.microsoft.com/en-us/library/system.environment.osversion(v=vs.110).aspx
									// In practice, this will only get hit if the application targets Windows 8.1 in the app manifest.
									// See http://msdn.microsoft.com/en-us/library/windows/desktop/dn481241(v=vs.85).aspx for more info.
									case 3:
										return EOSType.Windows81; // Also Server 2012 R2
								}

								goto default;

							case 10:
								return EOSType.Windows10;

							default:
								return EOSType.WinUnknown;
						}
					}

				case PlatformID.Unix: {
						if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
							switch (ver.Major) {
								case 11:
									return EOSType.MacOS107; // "Lion"

								case 12:
									return EOSType.MacOS108; // "Mountain Lion"

								case 13:
									return EOSType.MacOS109; // "Mavericks"

								case 14:
									return EOSType.MacOS1010; // "Yosemite"

								case 15:
									return EOSType.MacOS1011; // El Capitan

								case 16:
									return EOSType.MacOS1012; // Sierra

								default:
									return EOSType.MacOSUnknown;
							}
						}
						else {
							return EOSType.LinuxUnknown;
						}
					}

				default:
					return EOSType.Unknown;
			}
		}

		private static string FileSeperator { get; set; } = @"\";

		public static void WaitForCompletion(params Task[] tasks) {
			if (tasks == null || tasks.Length <= 0) {
				return;
			}

			Task.WaitAll(tasks);
		}

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

		public static IPAddress GetNetworkByHostName(string hostName) {
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
		/// Blocks the calling thread until the referred boolean value is set to true.
		/// </summary>
		/// <param name="_value">Referred boolean value</param>
		public static void WaitWhile(Func<bool> condition, int interval = 25) {
			while (!condition()) {
				Task.Delay(interval).Wait();
			}
		}

		public static async Task WaitUntilCancellation(CancellationToken _token, int interval = 25) {
			while (!_token.IsCancellationRequested) {
				await Task.Delay(interval).ConfigureAwait(false);
			}
		}

		public static void SetFileSeperator() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				FileSeperator = "//";
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				FileSeperator = "\\";
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				FileSeperator = "//";
			}
		}

		public static bool AsBool(this string value, out bool? booleanValue) {
			if (string.IsNullOrEmpty(value)) {
				booleanValue = null;
				return false;
			}

			bool? temp;
			switch (value) {
				case "1":
					temp = true;
					break;
				case "0":
					temp = false;
					break;
				default:
					temp = null;
					break;
			}

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

		public static float GenerateUniqueIdentifier(Random prng) {
			int sign = prng.Next(2);
			int exponent = prng.Next((1 << 8) - 1);
			int mantissa = prng.Next(1 << 23);
			int bits = (sign << 31) + (exponent << 23) + mantissa;
			return IntBitsToFloat(bits);
		}

		private static float IntBitsToFloat(int bits) {
			unsafe {
				return *(float*) &bits;
			}
		}

		public static Timer? ScheduleTask(Action action, TimeSpan delay) {
			if (action == null) {
				return null;
			}

			Timer? TaskSchedulerTimer = null;

			TaskSchedulerTimer = new Timer(e => {
				InBackgroundThread(action, "Task Scheduler");

				if (TaskSchedulerTimer != null) {
					TaskSchedulerTimer.Dispose();
					TaskSchedulerTimer = null;
				}
			}, null, delay, delay);

			return TaskSchedulerTimer;
		}

		public static bool IsSocketConnected(Socket? s) {
			if (s == null) {
				return false;
			}

			bool part1 = s.Poll(1000, SelectMode.SelectRead);
			bool part2 = s.Available == 0;
			if (part1 && part2) {
				return false;
			}

			return true;
		}

		public static bool IsServerOnline(string? ip) {
			if (string.IsNullOrEmpty(ip)) {
				return false;
			}

			Ping ping = new Ping();
			PingReply pingReply = ping.Send(ip);

			return pingReply.Status == IPStatus.Success;
		}

		public static string ExecuteBashCommand(string command) {
			// according to: https://stackoverflow.com/a/15262019/637142
			// thanks to this we will pass everything as one command
			command = command.Replace("\"", "\"\"");

			var proc = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "/bin/bash",
					Arguments = "-c \"" + command + "\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			proc.Start();
			proc.WaitForExit();

			return proc.StandardOutput.ReadToEnd();
		}

		public static string? ExecuteBash(this string cmd, bool sudo) {
			if (GetPlatform() != OSPlatform.Linux) {
				return null;
			}

			if (string.IsNullOrEmpty(cmd)) {
				return null;
			}

			string escapedArgs = cmd.Replace("\"", "\\\"");
			string args = $"-c \"{escapedArgs}\"";
			string argsWithSudo = $"-c \"sudo {escapedArgs}\"";

			using Process process = new Process() {
				StartInfo = new ProcessStartInfo {
					FileName = "/bin/bash",
					Arguments = sudo ? argsWithSudo : args,
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

		public static string? GetLocalIpAddress() {
			using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
				socket.Connect("8.8.8.8", 65530);
				IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
				if (endPoint != null) {
					return endPoint.Address.ToString();
				}
			}

			return null;
		}

		public static ConsoleKeyInfo? FetchUserInputSingleChar(TimeSpan delay) {
			Task<ConsoleKeyInfo> task = Task.Factory.StartNew(Console.ReadKey);
			ConsoleKeyInfo? result = Task.WaitAny(new Task[] { task }, delay) == 0 ? task.Result : (ConsoleKeyInfo?) null;
			return result;
		}

		public static void SetConsoleTitle(string text) => Console.Title = text;

		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();

		public static async Task<string> GetExternalIp() {
			if (!IsNetworkAvailable()) {
				return null;
			}

			return await "https://api.ipify.org/".RequestAsString();
		}

		public static string? GetEnvironmentVariable(string variable, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine) => Environment.GetEnvironmentVariable(variable, target);

		public static bool SetEnvironmentVariable(string variableName, string variableValue, EnvironmentVariableTarget target) {
			try {
				Environment.SetEnvironmentVariable(variableName, variableValue, target);
				return true;
			}
			catch (Exception e) {
				return false;
			}
		}

		public static DateTime ConvertTo24Hours(DateTime source) =>
			DateTime.TryParse(source.ToString("yyyy MMMM d HH:mm:ss tt"), out DateTime result) ? result : DateTime.Now;

		public static DateTime ConvertTo12Hours(DateTime source) =>
			DateTime.TryParse(source.ToString("dddd, dd MMMM yyyy"), out DateTime result) ? result : DateTime.Now;

		public static string GetLocalIPv4(NetworkInterfaceType typeOfNetwork) {
			string output = "";
			foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) {
				if (item.NetworkInterfaceType == typeOfNetwork && item.OperationalStatus == OperationalStatus.Up) {
					foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses) {
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
							output = ip.Address.ToString();
						}
					}
				}
			}
			return output;
		}

		public static async Task<string> RequestAsString(this string url) {
			if (string.IsNullOrEmpty(url)) {
				return null;
			}

			if (!IsNetworkAvailable()) {
				return null;
			}

			using (HttpClient client = new HttpClient()) {
				using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url)) {
					using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false)) {
						if (!response.IsSuccessStatusCode) {
							return default;
						}

						return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					}
				}
			}
		}

		public static string GetFileName(string? path) {
			if (string.IsNullOrEmpty(path)) {
				return string.Empty;
			}

			if (GetPlatform().Equals(OSPlatform.Windows)) {
				return Path.GetFileName(path) ?? string.Empty;
			}

			return path.Substring(path.LastIndexOf(FileSeperator, StringComparison.Ordinal) + 1);
		}

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

		public static void WriteBytesToFile(byte[] bytesToWrite, string filePath) {
			if (bytesToWrite.Length <= 0 || string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath)) {
				return;
			}

			File.WriteAllBytes(filePath, bytesToWrite);
		}

		public static Thread? InBackgroundThread(Action action, string? threadName, bool longRunning = false) {
			if (action == null) {
				return null;
			}

			ThreadStart threadStart = new ThreadStart(action);
			Thread BackgroundThread = new Thread(threadStart);

			if (longRunning) {
				BackgroundThread.IsBackground = true;
			}

			BackgroundThread.Name = !string.IsNullOrEmpty(threadName) ? threadName : action.GetHashCode().ToString();
			BackgroundThread.Priority = ThreadPriority.Normal;
			BackgroundThread.Start();
			return BackgroundThread;
		}

		public static Thread? InBackgroundThread(Action action, bool longRunning = false) {
			if (action == null) {
				return null;
			}

			ThreadStart threadStart = new ThreadStart(action);
			Thread BackgroundThread = new Thread(threadStart);

			if (longRunning) {
				BackgroundThread.IsBackground = true;
			}

			BackgroundThread.Name = action.GetHashCode().ToString();
			BackgroundThread.Priority = ThreadPriority.Normal;
			BackgroundThread.Start();
			return BackgroundThread;
		}

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

		public static void ExecuteCommand(string command, bool redirectOutput = false, string fileName = "/bin/bash") {
			if (GetPlatform() != OSPlatform.Linux && fileName == "/bin/bash") {
				return;
			}

			try {
				Process proc = new Process {
					StartInfo = {
						FileName = fileName,
						Arguments = "-c \" " + command + " \"",
						UseShellExecute = false,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden
					}
				};


				proc.StartInfo.RedirectStandardOutput = redirectOutput;

				proc.Start();
				proc.WaitForExit(4000);

				if (redirectOutput) {
					while (!proc.StandardOutput.EndOfStream) {
						string? output = proc.StandardOutput.ReadLine();
						if (output != null) {
						}
					}
				}
			}
			catch (Exception) { }
		}

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

		public static async Task<IList<T>> InParallel<T>(IEnumerable<Task<T>> tasks) {
			if (tasks == null) {
				return null;
			}

			IList<T> results = await Task.WhenAll(tasks).ConfigureAwait(false);
			return results;
		}

		public static async Task InParallel(IEnumerable<Task> tasks) {
			if (tasks == null) {
				return;
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}

		public static bool IsNetworkAvailable(string _host = null) {
			try {
				Ping myPing = new Ping();
				string host = _host ?? "8.8.8.8";
				byte[] buffer = new byte[32];
				int timeout = 1000;
				PingOptions pingOptions = new PingOptions();
				PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
				return reply != null && reply.Status == IPStatus.Success;
			}
			catch (Exception e) {
				return false;
			}
		}

		public static void CloseProcess(string processName) {
			if (string.IsNullOrEmpty(processName) || string.IsNullOrWhiteSpace(processName)) {
				return;
			}

			Process[] workers = Process.GetProcessesByName(processName);
			foreach (Process worker in workers) {
				worker.Kill();
				worker.WaitForExit();
				worker.Dispose();
			}
		}
	}
}
