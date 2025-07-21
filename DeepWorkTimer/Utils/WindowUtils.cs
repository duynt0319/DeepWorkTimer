using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DeepWorkTimer.Utils
{
    /// <summary>
    /// Utility class for Win32 window operations to make windows click-through
    /// </summary>
    public static class WindowUtils
    {
        #region Win32 API Constants

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        #endregion

        #region Win32 API Imports

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        #endregion

        #region Public Methods

        /// <summary>
        /// Make window completely transparent to mouse events (click-through)
        /// </summary>
        /// <param name="window">The WPF window to make click-through</param>
        public static void MakeWindowClickThrough(Window window)
        {
            try
            {
                var helper = new WindowInteropHelper(window);
                var hwnd = helper.Handle;

                if (hwnd == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("?? Window handle is null - cannot make click-through");
                    return;
                }

                // Get current extended window style
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

                // Add WS_EX_TRANSPARENT and WS_EX_LAYERED flags
                var newStyle = extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED;

                // Set the new extended style
                SetWindowLong(hwnd, GWL_EXSTYLE, newStyle);

                System.Diagnostics.Debug.WriteLine("? Window made click-through successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to make window click-through: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove click-through behavior from window (make it interactive again)
        /// </summary>
        /// <param name="window">The WPF window to make interactive</param>
        public static void MakeWindowInteractive(Window window)
        {
            try
            {
                var helper = new WindowInteropHelper(window);
                var hwnd = helper.Handle;

                if (hwnd == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("?? Window handle is null - cannot make interactive");
                    return;
                }

                // Get current extended window style
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

                // Remove WS_EX_TRANSPARENT flag (keep WS_EX_LAYERED for transparency)
                var newStyle = extendedStyle & ~WS_EX_TRANSPARENT;

                // Set the new extended style
                SetWindowLong(hwnd, GWL_EXSTYLE, newStyle);

                System.Diagnostics.Debug.WriteLine("? Window made interactive successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to make window interactive: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if window is currently click-through
        /// </summary>
        /// <param name="window">The WPF window to check</param>
        /// <returns>True if window is click-through, false otherwise</returns>
        public static bool IsWindowClickThrough(Window window)
        {
            try
            {
                var helper = new WindowInteropHelper(window);
                var hwnd = helper.Handle;

                if (hwnd == IntPtr.Zero)
                    return false;

                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                return (extendedStyle & WS_EX_TRANSPARENT) != 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to check window click-through status: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}