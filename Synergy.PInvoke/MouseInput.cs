using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Synergy.PInvoke {
	/// <summary>
	/// Contains various methods to get and manipulate cursor positions and clicks.
	/// </summary>
	public static class MouseInput {
		[DllImport("user32.dll", EntryPoint = "SetCursorPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetCursorPos(int x, int y);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out MousePoint lpMousePoint);

		[DllImport("user32.dll")]
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Imported event")]
		private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

		/// <summary>
		/// Sets the cursor position to the specified <see cref="x"/> and <see cref="y"/> coordinates.
		/// </summary>
		/// <param name="x">the <see cref="x"/> coordinate</param>
		/// <param name="y">the <see cref="y"/> coordinate</param>
		/// <returns>bool result indicating execution status.</returns>
		public static bool SetCursorPosition(int x, int y) => SetCursorPos(x, y);

		/// <summary>
		/// Sets the cursor position to the coordinates specified in <see cref="MousePoint"/> parameter.
		/// </summary>
		/// <param name="point">the struct containing x and y coordinates</param>
		/// <returns>bool result indicating execution status.</returns>
		public static bool SetCursorPosition(MousePoint point) => SetCursorPos(point.X, point.Y);

		/// <summary>
		/// Gets the current cursor position, in <see cref="MousePoint"/> struct.
		/// <br>returns (0,0) coordinates if the execution failed.</br>
		/// </summary>
		/// <returns>the <see cref="MousePoint"/> struct containing the coordinates</returns>
		public static MousePoint GetCursorPosition() => GetCursorPos(out MousePoint currentPos) ? currentPos : new MousePoint(0, 0);

		/// <summary>
		/// Performs a mouse click event with the specified click type as <see cref="MouseEventFlags"/> flags.
		/// </summary>
		/// <param name="value">the <see cref="MouseEventFlags"/> flag to specify the click type.</param>
		public static void Click(MouseEventFlags value) {
			MousePoint position = GetCursorPosition();
			mouse_event((int) value, position.X, position.Y, 0, 0);
		}

		/// <summary>
		/// Performs a mouse click event with the specified click type as <see cref="MouseEventFlags"/> flags and at the coordinates specified in <see cref="MousePoint"/> struct.
		/// </summary>
		/// <param name="value">the <see cref="MouseEventFlags"/> flag to specify the click type.</param>
		/// <param name="point">the <see cref="MousePoint"/> struct containing the X and Y coordinates of click target.</param>
		public static void Click(MouseEventFlags value, MousePoint point) => mouse_event((int) value, point.X, point.Y, 0, 0);

		/// <summary>
		/// The struct which stores the mouse positional data, in X and Y coordinates.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]		
		public struct MousePoint {
			public int X;
			public int Y;

			/// <summary>
			/// Default ctor.
			/// </summary>
			/// <param name="x">the x coordinate.</param>
			/// <param name="y">the y coordinate.</param>
			public MousePoint(int x, int y) {
				X = x;
				Y = y;
			}
		}

		/// <summary>
		/// Mouse event flags used to specify the click type on performing the click.
		/// </summary>
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
