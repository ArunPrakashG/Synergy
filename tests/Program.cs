using Synergy.PInvoke;
using System;
using System.Diagnostics;

namespace tests {
	class Program {
		static void Main(string[] args) {
			Process[] processlist = Process.GetProcesses();

			foreach (Process process in processlist) {
				if (!String.IsNullOrEmpty(process.MainWindowTitle)) {
					Console.WriteLine("Process: {0} ID: {1} Window title: {2}", process.ProcessName, process.Id, process.MainWindowTitle);
				}

				if(process.ProcessName == "csgo") {
					WindowController.GetWindowPosition(process.MainWindowTitle, out WindowController.WINDOWPLACEMENT pos);
					Console.WriteLine($"{pos.rcNormalPosition.Height} : {pos.rcNormalPosition.Width}");
					WindowController.BringWindowToForeground(process.MainWindowHandle);
					Console.WriteLine("Window infront");
					Mouse.SetCursorPosition(pos.rcNormalPosition.Left + 20, pos.rcNormalPosition.Top + 80);
					Mouse.Click(Mouse.MouseEventFlags.LeftDown);
					Mouse.Click(Mouse.MouseEventFlags.LeftUp);
					Mouse.Click(Mouse.MouseEventFlags.LeftDown);
					Mouse.Click(Mouse.MouseEventFlags.LeftUp);
				}
			}

			
			//while (true) {
			//	//Console.WriteLine($"{Mouse.GetCursorPosition().X} : {Mouse.GetCursorPosition().Y}");
			//	Mouse.SetCursorPosition(1301, 93);
			//	Mouse.Click(Mouse.MouseEventFlags.LeftDown);
			//	Mouse.Click(Mouse.MouseEventFlags.LeftUp);
			//	Mouse.Click(Mouse.MouseEventFlags.LeftDown);
			//	Mouse.Click(Mouse.MouseEventFlags.LeftUp);
			//	Console.WriteLine("Mouse clicked");
			//	Console.ReadKey();
			//}			
		}
	}
}
