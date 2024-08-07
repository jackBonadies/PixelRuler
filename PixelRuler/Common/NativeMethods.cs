﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static PixelRuler.NativeMethods;

namespace PixelRuler
{
    public static class NativeMethods
    {
        public const int WM_NCPAINT = 0x0085;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public const uint GW_HWNDNEXT = 2;

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

#if DEBUG

        /// <summary>
        /// This is to debug CLIPBRD_E_CANT_OPEN
        /// </summary>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetOpenClipboardWindow();

#endif


        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);


        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

        [DllImport("User32.dll")]
        public static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        public static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);
    }

    public static class NativeHelpers
    {
        public static bool PtInRect(ref RECT r, POINT p)
        {
            return p.X >= r.Left && p.X < r.Right && p.Y >= r.Top && p.Y < r.Bottom;
        }

        /// <summary>
        /// We basically search from our Z order down to find a visible window that contains the point
        /// </summary>
        /// <param name="ownWindowHandle"></param>
        /// <returns></returns>
        public static IntPtr GetWindowUnderPointExcludingOwn(NativeMethods.POINT point, IntPtr ownWindowHandle)
        {
            IntPtr foundWindow = NativeMethods.WindowFromPoint(point);
            if (foundWindow == ownWindowHandle)
            {
                foundWindow = NativeMethods.GetWindow(foundWindow, NativeMethods.GW_HWNDNEXT);
            }
            while (true)
            {
                foundWindow = NativeMethods.GetWindow(foundWindow, NativeMethods.GW_HWNDNEXT);
                NativeMethods.GetWindowRect(foundWindow, out RECT rect);
                if (PtInRect(ref rect, point))
                {
                    if (IsIconic(foundWindow) || !IsWindowVisible(foundWindow))
                    {
                        continue;
                    }
                    if (!VirtualDesktopHelper.IsWindowOnCurrentVirtualDesktop(foundWindow))
                    {
                        continue;
                    }
                    return foundWindow;
                }
            }
            return IntPtr.Zero;
        }

        public static (int width, int height) GetWindowSize(IntPtr hWnd)
        {
            NativeMethods.RECT rect;
            if (NativeMethods.GetWindowRect(hWnd, out rect))
            {
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                return (width, height);
            }
            return (0, 0);
        }

        public static string GetProcessNameFromWindowHandle(IntPtr hWnd)
        {
            int processId;
            NativeMethods.GetWindowThreadProcessId(hWnd, out processId);

            try
            {
                Process proc = Process.GetProcessById(processId);
                return proc.ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }


        public static string GetWindowTitle(IntPtr hWnd)
        {
            int length = NativeMethods.GetWindowTextLength(hWnd) + 1;
            StringBuilder sb = new StringBuilder(length);
            if (NativeMethods.GetWindowText(hWnd, sb, length) > 0)
            {
                return sb.ToString();
            }
            return string.Empty;
        }

#if DEBUG
        /// <summary>
        /// Gets the process holding the clipboard
        /// </summary>
        /// <returns></returns>
        public static Process? GetProcessHoldingClipboard()
        {
            Process? theProc = null;

            IntPtr hwnd = GetOpenClipboardWindow();

            if (hwnd != IntPtr.Zero)
            {
                int processId;
                uint threadId = GetWindowThreadProcessId(hwnd, out processId);

                Process[] procs = Process.GetProcesses();
                foreach (Process proc in procs)
                {
                    IntPtr handle = proc.MainWindowHandle;

                    if (handle == hwnd)
                    {
                        theProc = proc;
                    }
                    else if (processId == proc.Id)
                    {
                        theProc = proc;
                    }
                }
            }

            return theProc;
        }
#endif
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
    interface IVirtualDesktopManager
    {
        [PreserveSig]
        int IsWindowOnCurrentVirtualDesktop(
            IntPtr topLevelWindowHandle,
            out bool onCurrentDesktop);

        [PreserveSig]
        int GetWindowDesktopId(
            IntPtr topLevelWindowHandle,
            out Guid desktopId);

        [PreserveSig]
        int MoveWindowToDesktop(
            IntPtr topLevelWindowHandle,
            ref Guid desktopId);
    }

    class VirtualDesktopHelper
    {
        static IVirtualDesktopManager? desktopManager;

        static VirtualDesktopHelper()
        {
            var virtualDesktopManagerType = Type.GetTypeFromCLSID(new Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a"));
            if (virtualDesktopManagerType != null)
            {
                var instance = Activator.CreateInstance(virtualDesktopManagerType);
                if (instance is IVirtualDesktopManager virtualDesktopManagerInstance)
                {
                    desktopManager = virtualDesktopManagerInstance;
                }
            }
        }

        public static bool IsWindowOnCurrentVirtualDesktop(IntPtr windowHandle)
        {
            if (desktopManager == null)
            {
                return true; // better to be safe
            }
            desktopManager.IsWindowOnCurrentVirtualDesktop(windowHandle, out bool isOnCurrentDesktop);
            return isOnCurrentDesktop;
        }
    }
}
