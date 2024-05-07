#if DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN //&& !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using Game.Networking;

#endif

namespace Game
{
	public class CustomDebugStart
	{
#if DEVELOPMENT_BUILD && UNITY_STANDALONE_WIN //&& !UNITY_EDITOR
		private static readonly object handle = new object();
		private static HandleRef unityHandle = new HandleRef();

		static class HWND
		{
			public static IntPtr
				NoTopMost = new IntPtr(-2),
				TopMost = new IntPtr(-1),
				Top = new IntPtr(0),
				Bottom = new IntPtr(1);
		}

		static class SWP
		{
			public static readonly int
				NOSIZE = 0x0001,
				NOMOVE = 0x0002,
				NOZORDER = 0x0004,
				NOREDRAW = 0x0008,
				NOACTIVATE = 0x0010,
				DRAWFRAME = 0x0020,
				FRAMECHANGED = 0x0020,
				SHOWWINDOW = 0x0040,
				HIDEWINDOW = 0x0080,
				NOCOPYBITS = 0x0100,
				NOOWNERZORDER = 0x0200,
				NOREPOSITION = 0x0200,
				NOSENDCHANGING = 0x0400,
				DEFERERASE = 0x2000,
				ASYNCWINDOWPOS = 0x4000;
		}

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetSystemMetrics(int index);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

		private static bool Window(IntPtr hWnd, IntPtr lParam)
		{
			int unityPid = System.Diagnostics.Process.GetCurrentProcess().Id;

			GetWindowThreadProcessId(new HandleRef(handle, hWnd), out int pid);

			if (pid != unityPid)
				return true;

			unityHandle = new HandleRef(handle, hWnd);
			return false;
		}

		public static void PositionWindow()
		{
			EnumWindows(Window, IntPtr.Zero);
			if (unityHandle.Wrapper != null)
			{
				int ww = UnityEngine.Screen.width;
				int wh = UnityEngine.Screen.height;

				int x = 0;
				int y = 0;
				int w = GetSystemMetrics(0);
				int h = GetSystemMetrics(1);

				if (NetworkManager.Instance.IsServer)
				{
					x = w / 2 - (ww / 2);
					y = h / 2 - (wh / 2);
				}
				else
				{
					// Client IDs start from 2
					int windowIndex = NetworkManager.Instance.ClientID - 2;
					switch (windowIndex % 4)
					{
						case 1: x = w - ww; break;
						case 2: y = h - wh; break;
						case 3:
							x = w - ww;
							y = h - wh;
							break;
					}
				}

				SetWindowPos(unityHandle.Handle, HWND.Top, x, y, ww, wh, SWP.NOSIZE);
			}
		}
#else
		public static void PositionWindow()
		{

		}
#endif
	}
}
