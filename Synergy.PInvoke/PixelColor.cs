using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace Synergy.PInvoke {
	/// <summary>
	/// Contains various methods and events to get color at a specific pixel.
	/// </summary>
	public static class PixelColor {
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr GetWindowDC(IntPtr window);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern uint GetPixel(IntPtr dc, int x, int y);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int ReleaseDC(IntPtr window, IntPtr dc);

		/// <summary>
		/// Gets the color at the specified coordinates.
		/// </summary>
		/// <param name="point">The point from where the color is fetched.</param>
		/// <returns>The <see cref="Color"/></returns>
		public static Color GetColorAt(Point point) {
			if (point.IsEmpty) {
				return default;
			}

			IntPtr desk = GetDesktopWindow();
			IntPtr dc = GetWindowDC(desk);
			int a = (int) GetPixel(dc, point.X, point.Y);
			ReleaseDC(desk, dc);
			return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
		}

		/// <summary>
		/// Polls the specified point and fires events when a specific color is detected to the specified function until the passed function returns true or when the <see cref="CancellationToken"/> is canceled.
		/// </summary>
		/// <param name="point">The point to poll.</param>
		/// <param name="fireAtColor">The color to which event is fired.</param>
		/// <param name="cancellationToken">The token to cancel the polling.</param>
		/// <param name="onColorDetected">The function which is invoked when the color is detected.</param>
		/// <param name="pollDelayMs">The delay of each loop, default to 250 ms.</param>
		public static void PollPixel(Point point, Color fireAtColor, CancellationToken cancellationToken, Func<Color, bool> onColorDetected, int pollDelayMs = 250) {
			if (point.IsEmpty || fireAtColor.IsEmpty || !cancellationToken.CanBeCanceled || onColorDetected == null) {
				return;
			}

			while (!cancellationToken.IsCancellationRequested) {
				Color color = GetColorAt(point);

				if (color.IsEmpty) {
					return;
				}

				if ((fireAtColor.R == color.R && fireAtColor.G == color.G && fireAtColor.B == color.B) && onColorDetected.Invoke(color)) {
					return;
				}

				Thread.Sleep(pollDelayMs);
			}
		}
	}
}
