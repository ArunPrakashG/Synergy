using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Synergy.PInvoke {
	public class MouseInput {
		[DllImport("user32.dll", EntryPoint = "SetCursorPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetCursorPos(int x, int y);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out MousePoint lpMousePoint);

		[DllImport("user32.dll")]
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Imported method")]
		private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

		public static bool SetCursorPosition(int x, int y) => SetCursorPos(x, y);

		public static bool SetCursorPosition(MousePoint point) => SetCursorPos(point.X, point.Y);

		public static MousePoint GetCursorPosition() => GetCursorPos(out MousePoint currentPos) ? currentPos : new MousePoint(0, 0);

		public static void MouseEvent(MouseEventFlags value) {
			MousePoint position = GetCursorPosition();
			mouse_event((int) value, position.X, position.Y, 0, 0);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MousePoint {
			public int X;
			public int Y;

			public MousePoint(int x, int y) {
				X = x;
				Y = y;
			}
		}

		[Flags]
		public enum MouseEventFlags {
			LeftDown = 0x00000002,
			LeftUp = 0x00000004,
			MiddleDown = 0x00000020,
			MiddleUp = 0x00000040,
			Move = 0x00000001,
			Absolute = 0x00008000,
			RightDown = 0x00000008,
			RightUp = 0x00000010
		}
	}
}
