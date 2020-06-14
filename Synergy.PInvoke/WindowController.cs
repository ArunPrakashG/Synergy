using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Synergy.PInvoke {
	/// <summary>
	/// Contains various methods to get and manipulate windows and their positional data.
	/// </summary>
	public static class WindowController {
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		/// <summary>
		/// Sets the window position of a specified window.
		/// </summary>
		/// <param name="windowName">The name of the window to set the position to.</param>
		/// <param name="position">The positional coordinates, in <see cref="WindowPosition"/> struct.</param>
		/// <param name="windowFlags">The flags to set for the window.</param>
		/// <returns>status of the execution</returns>
		private static bool SetWindowPosition(string windowName, WindowPosition position, SetWindowPosFlags windowFlags = SetWindowPosFlags.SWP_SHOWWINDOW) {
			if (string.IsNullOrEmpty(windowName)) {
				return false;
			}

			IntPtr hWnd = FindWindow(windowName, null);

			if (hWnd == IntPtr.Zero) {
				return false;
			}

			return SetWindowPos(hWnd, IntPtr.Zero, position.X, position.Y, 0, 0, (uint) windowFlags);
		}

		/// <summary>
		/// Sets the window position of a specified window.
		/// </summary>
		/// <param name="windowName">The name of the window to set the position to.</param>
		/// <param name="position">The positional coordinates, in <see cref="WindowPosition"/> struct.</param>
		/// <param name="specialWindowHandle">Used to pass any special handles for the window on the unmanaged function.</param>
		/// <param name="windowFlags">The flags to set for the window.</param>
		/// <returns>status of the execution</returns>
		private static bool SetWindowPosition(string windowName, WindowPosition position, SpecialWindowHandles specialWindowHandle, SetWindowPosFlags windowFlags = SetWindowPosFlags.SWP_SHOWWINDOW) {
			if (string.IsNullOrEmpty(windowName)) {
				return false;
			}

			IntPtr hWnd = FindWindow(windowName, null);
			IntPtr specialWindowHandlePtr = specialWindowHandle != SpecialWindowHandles.HWND_EMPTY ? (IntPtr) specialWindowHandle : IntPtr.Zero;

			if (hWnd == IntPtr.Zero) {
				return false;
			}

			return SetWindowPos(hWnd, specialWindowHandlePtr, position.X, position.Y, 0, 0, (uint) windowFlags);
		}

		/// <summary>
		/// Gets the specified window position in <see cref="WINDOWPLACEMENT"/> struct.
		/// </summary>
		/// <param name="windowName">The name of the window to get the position of.</param>
		/// <param name="windowPlacement">The position of the window specified.</param>
		/// <returns>status of the execution</returns>
		private static bool GetWindowPosition(string windowName, out WINDOWPLACEMENT windowPlacement) {
			windowPlacement = new WINDOWPLACEMENT();

			if (string.IsNullOrEmpty(windowName)) {
				return false;
			}

			IntPtr hWnd = FindWindow(windowName, null);

			if (hWnd == IntPtr.Zero) {
				return false;
			}

			windowPlacement.length = Marshal.SizeOf(windowPlacement);
			return GetWindowPlacement(hWnd, ref windowPlacement);
		}

		/// <summary>
		/// The struct which stores the window placement data.
		/// </summary>
		private struct WINDOWPLACEMENT {
			public int length;
			public int flags;
			public int showCmd;
			public Point ptMinPosition;
			public Point ptMaxPosition;
			public Rectangle rcNormalPosition;
		}

		/// <summary>
		/// The struct which stores the window positional data, in X and Y coordinates.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct WindowPosition {
			/// <summary>
			/// The x coordinate.
			/// </summary>
			public int X;

			/// <summary>
			/// The y coordinate.
			/// </summary>
			public int Y;

			/// <summary>
			/// Default ctor.
			/// </summary>
			/// <param name="x">the x coordinate.</param>
			/// <param name="y">the y coordinate.</param>
			public WindowPosition(int x, int y) {
				X = x;
				Y = y;
			}
		}

		/// <summary>
		/// Special window handles
		/// </summary>
		public enum SpecialWindowHandles {
			/// <summary>
			///     Places the window at the top of the Z order.
			/// </summary>
			HWND_TOP = 0,
			/// <summary>
			///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
			/// </summary>
			HWND_BOTTOM = 1,
			/// <summary>
			///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
			/// </summary>
			HWND_TOPMOST = -1,
			/// <summary>
			///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
			/// </summary>
			HWND_NOTOPMOST = -2,

			/// <summary>
			/// Disregards the special handle.
			/// </summary>
			HWND_EMPTY = 90
		}

		/// <summary>
		/// Window flags used to specify the action type on the specified window.
		/// <br>see <see cref="http://pinvoke.net/default.aspx/user32/SetWindowPos.html"/></br>
		/// </summary>
		[Flags]
		public enum SetWindowPosFlags : uint {

			/// <summary>
			///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
			/// </summary>
			SWP_ASYNCWINDOWPOS = 0x4000,

			/// <summary>
			///     Prevents generation of the WM_SYNCPAINT message.
			/// </summary>
			SWP_DEFERERASE = 0x2000,

			/// <summary>
			///     Draws a frame (defined in the window's class description) around the window.
			/// </summary>
			SWP_DRAWFRAME = 0x0020,

			/// <summary>
			///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
			/// </summary>
			SWP_FRAMECHANGED = 0x0020,

			/// <summary>
			///     Hides the window.
			/// </summary>
			SWP_HIDEWINDOW = 0x0080,

			/// <summary>
			///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
			/// </summary>
			SWP_NOACTIVATE = 0x0010,

			/// <summary>
			///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
			/// </summary>
			SWP_NOCOPYBITS = 0x0100,

			/// <summary>
			///     Retains the current position (ignores X and Y parameters).
			/// </summary>
			SWP_NOMOVE = 0x0002,

			/// <summary>
			///     Does not change the owner window's position in the Z order.
			/// </summary>
			SWP_NOOWNERZORDER = 0x0200,

			/// <summary>
			///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
			/// </summary>
			SWP_NOREDRAW = 0x0008,

			/// <summary>
			///     Same as the SWP_NOOWNERZORDER flag.
			/// </summary>
			SWP_NOREPOSITION = 0x0200,

			/// <summary>
			///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
			/// </summary>
			SWP_NOSENDCHANGING = 0x0400,

			/// <summary>
			///     Retains the current size (ignores the cx and cy parameters).
			/// </summary>
			SWP_NOSIZE = 0x0001,

			/// <summary>
			///     Retains the current Z order (ignores the hWndInsertAfter parameter).
			/// </summary>
			SWP_NOZORDER = 0x0004,

			/// <summary>
			///     Displays the window.
			/// </summary>
			SWP_SHOWWINDOW = 0x0040
		}
	}
}
