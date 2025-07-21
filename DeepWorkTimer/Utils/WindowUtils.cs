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
        private const int WS_EX_NOACTIVATE = 0x08000000;

        #endregion

        #region Win32 API Imports

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        #endregion

        #region Public Methods

        /// <summary>
        /// Make window completely transparent to mouse events (click-through) with improved stability
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

                // Add WS_EX_TRANSPARENT, WS_EX_LAYERED, and WS_EX_NOACTIVATE flags
                // WS_EX_NOACTIVATE prevents window activation which can cause flicker
                var newStyle = extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE;

                // Set the new extended style
                var result = SetWindowLong(hwnd, GWL_EXSTYLE, newStyle);

                // Verify the change was applied
                var verifyStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                var isTransparent = (verifyStyle & WS_EX_TRANSPARENT) != 0;
                var isNoActivate = (verifyStyle & WS_EX_NOACTIVATE) != 0;

                System.Diagnostics.Debug.WriteLine($"? Window click-through: Transparent={isTransparent}, NoActivate={isNoActivate}");
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

                // Remove WS_EX_TRANSPARENT and WS_EX_NOACTIVATE flags (keep WS_EX_LAYERED for transparency)
                var newStyle = extendedStyle & ~WS_EX_TRANSPARENT & ~WS_EX_NOACTIVATE;

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

        /// <summary>
        /// Enhanced click-through setup with delay for better stability
        /// </summary>
        /// <param name="window">The WPF window to make click-through</param>
        public static void EnsureClickThrough(Window window)
        {
            // Apply click-through immediately
            MakeWindowClickThrough(window);

            // Also set it again after a short delay to ensure it sticks
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                MakeWindowClickThrough(window);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        #endregion
    }
}