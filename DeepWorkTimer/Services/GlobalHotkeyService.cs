using DeepWorkTimer.Models;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DeepWorkTimer.Services
{
    /// <summary>
    /// Service managing Global Hotkeys system-wide
    /// </summary>
    public class GlobalHotkeyService : IDisposable
    {
        #region Windows API

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Constants for modifier keys
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private const int WM_HOTKEY = 0x0312;

        #endregion

        private readonly Window _window;
        private readonly Dictionary<int, Action> _hotkeyActions;
        private readonly AppSettings _settings;
        private HwndSource? _source;
        private int _hotkeyCounter = 1000;
        private List<ScreenInfo> _availableScreens = new();

        public event EventHandler<string>? HotkeyTriggered;

        public GlobalHotkeyService(Window window, AppSettings settings)
        {
            _window = window;
            _settings = settings;
            _hotkeyActions = new Dictionary<int, Action>();
            RefreshScreenInfo();
        }

        /// <summary>
        /// Initialize service and register window hook
        /// </summary>
        public void Initialize()
        {
            _window.SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var windowHelper = new WindowInteropHelper(_window);
            _source = HwndSource.FromHwnd(windowHelper.Handle);
            _source?.AddHook(HwndHook);

            if (_settings.GlobalHotkeysEnabled)
            {
                RegisterDefaultHotkeys();
            }
        }

        /// <summary>
        /// Refresh information about available screens
        /// </summary>
        public void RefreshScreenInfo()
        {
            _availableScreens.Clear();
            var screens = Screen.AllScreens;

            for (int i = 0; i < screens.Length; i++)
            {
                var screenInfo = new ScreenInfo(i, screens[i]);
                if (i == _settings.PreferredScreenIndex)
                {
                    screenInfo.IsPreferred = true;
                }
                _availableScreens.Add(screenInfo);
            }

            System.Diagnostics.Debug.WriteLine($"?? Found {_availableScreens.Count} screens");
        }

        /// <summary>
        /// Get list of available screens
        /// </summary>
        public List<ScreenInfo> GetAvailableScreens() => _availableScreens.ToList();

        /// <summary>
        /// Register default hotkeys
        /// </summary>
        private void RegisterDefaultHotkeys()
        {
            var registeredCount = 0;

            // Ctrl+Alt+1-9: Switch directly to screens 1-9
            for (int i = 1; i <= 9; i++)
            {
                if (i <= _availableScreens.Count)
                {
                    var screenIndex = i - 1; // Zero-based index
                    if (RegisterHotkey(
                        MOD_CONTROL | MOD_ALT,
                        (uint)(Keys.D0 + i),
                        () => SwitchToScreen(screenIndex),
                        $"Switch to Screen {i}"
                    ))
                    {
                        registeredCount++;
                    }
                }
            }

            // Ctrl+Alt+M: Switch to next screen
            if (RegisterHotkey(
                MOD_CONTROL | MOD_ALT,
                (uint)Keys.M,
                () => SwitchToNextScreen(),
                "Next Screen"
            ))
            {
                registeredCount++;
            }

            // Ctrl+Alt+N: Switch to previous screen
            if (RegisterHotkey(
                MOD_CONTROL | MOD_ALT,
                (uint)Keys.N,
                () => SwitchToPreviousScreen(),
                "Previous Screen"
            ))
            {
                registeredCount++;
            }

            // Ctrl+Alt+C: Center on current screen
            if (RegisterHotkey(
                MOD_CONTROL | MOD_ALT,
                (uint)Keys.C,
                () => CenterOnCurrentScreen(),
                "Center on Current Screen"
            ))
            {
                registeredCount++;
            }

            // Ctrl+Alt+P: Set current screen as preferred
            if (RegisterHotkey(
                MOD_CONTROL | MOD_ALT,
                (uint)Keys.P,
                () => SetPreferredScreen(),
                "Set Preferred Screen"
            ))
            {
                registeredCount++;
            }

            HotkeyTriggered?.Invoke(this, $"?? Registered {registeredCount} Global Hotkeys successfully!");
        }

        /// <summary>
        /// Register a hotkey
        /// </summary>
        private bool RegisterHotkey(uint modifiers, uint key, Action action, string description)
        {
            if (_source?.Handle == null) return false;

            var hotkeyId = _hotkeyCounter++;
            if (RegisterHotKey(_source.Handle, hotkeyId, modifiers, key))
            {
                _hotkeyActions[hotkeyId] = action;
                System.Diagnostics.Debug.WriteLine($"? Registered hotkey: {description}");
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to register hotkey: {description}");
                return false;
            }
        }

        /// <summary>
        /// Handle Windows messages for hotkeys
        /// </summary>
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                var hotkeyId = wParam.ToInt32();
                if (_hotkeyActions.TryGetValue(hotkeyId, out var action))
                {
                    try
                    {
                        action.Invoke();
                        handled = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"? Hotkey action error: {ex.Message}");
                    }
                }
            }

            return IntPtr.Zero;
        }

        #region Screen Management Actions

        /// <summary>
        /// Switch to screen by index
        /// </summary>
        private void SwitchToScreen(int screenIndex)
        {
            try
            {
                if (screenIndex >= 0 && screenIndex < _availableScreens.Count)
                {
                    var targetScreen = _availableScreens[screenIndex];
                    var screen = Screen.AllScreens[screenIndex];

                    MoveWindowToScreen(screen);
                    SaveCurrentPosition();

                    HotkeyTriggered?.Invoke(this, $"?? {targetScreen}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? SwitchToScreen error: {ex.Message}");
            }
        }

        /// <summary>
        /// Switch to next screen
        /// </summary>
        private void SwitchToNextScreen()
        {
            try
            {
                if (_availableScreens.Count <= 1) return;

                var currentScreen = GetCurrentScreen();
                var currentIndex = Array.IndexOf(Screen.AllScreens, currentScreen);
                var nextIndex = (currentIndex + 1) % _availableScreens.Count;

                MoveWindowToScreen(Screen.AllScreens[nextIndex]);
                SaveCurrentPosition();

                HotkeyTriggered?.Invoke(this, $"?? {_availableScreens[nextIndex]}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? SwitchToNextScreen error: {ex.Message}");
            }
        }

        /// <summary>
        /// Switch to previous screen
        /// </summary>
        private void SwitchToPreviousScreen()
        {
            try
            {
                if (_availableScreens.Count <= 1) return;

                var currentScreen = GetCurrentScreen();
                var currentIndex = Array.IndexOf(Screen.AllScreens, currentScreen);
                var prevIndex = (currentIndex - 1 + _availableScreens.Count) % _availableScreens.Count;

                MoveWindowToScreen(Screen.AllScreens[prevIndex]);
                SaveCurrentPosition();

                HotkeyTriggered?.Invoke(this, $"?? {_availableScreens[prevIndex]}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? SwitchToPreviousScreen error: {ex.Message}");
            }
        }

        /// <summary>
        /// Center window on current screen
        /// </summary>
        private void CenterOnCurrentScreen()
        {
            try
            {
                var currentScreen = GetCurrentScreen();
                CenterWindowOnScreen(currentScreen);
                SaveCurrentPosition();

                var screenIndex = Array.IndexOf(Screen.AllScreens, currentScreen);
                HotkeyTriggered?.Invoke(this, $"?? Centered on {_availableScreens[screenIndex]}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? CenterOnCurrentScreen error: {ex.Message}");
            }
        }

        /// <summary>
        /// Set current screen as preferred
        /// </summary>
        private void SetPreferredScreen()
        {
            try
            {
                var currentScreen = GetCurrentScreen();
                var screenIndex = Array.IndexOf(Screen.AllScreens, currentScreen);

                // Update preferred screen
                foreach (var screen in _availableScreens)
                {
                    screen.IsPreferred = false;
                }
                _availableScreens[screenIndex].IsPreferred = true;

                _settings.PreferredScreenIndex = screenIndex;
                _settings.Save();

                HotkeyTriggered?.Invoke(this, $"? Set {_availableScreens[screenIndex]} as Preferred");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? SetPreferredScreen error: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get current screen containing the window
        /// </summary>
        private Screen GetCurrentScreen()
        {
            var windowBounds = new System.Drawing.Rectangle(
                (int)_window.Left,
                (int)_window.Top,
                (int)_window.Width,
                (int)_window.Height
            );

            return Screen.FromRectangle(windowBounds);
        }

        /// <summary>
        /// Move window to specified screen and center it
        /// </summary>
        private void MoveWindowToScreen(Screen targetScreen)
        {
            var workingArea = targetScreen.WorkingArea;

            _window.Left = workingArea.Left + (workingArea.Width - _window.Width) / 2;
            _window.Top = workingArea.Top + (workingArea.Height - _window.Height) / 2;
        }

        /// <summary>
        /// Center window on specified screen
        /// </summary>
        private void CenterWindowOnScreen(Screen screen)
        {
            var workingArea = screen.WorkingArea;

            _window.Left = workingArea.Left + (workingArea.Width - _window.Width) / 2;
            _window.Top = workingArea.Top + (workingArea.Height - _window.Height) / 2;
        }

        /// <summary>
        /// Save current position to settings
        /// </summary>
        private void SaveCurrentPosition()
        {
            _settings.WindowLeft = _window.Left;
            _settings.WindowTop = _window.Top;
            _settings.Save();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_source?.Handle != null)
            {
                foreach (var hotkeyId in _hotkeyActions.Keys)
                {
                    UnregisterHotKey(_source.Handle, hotkeyId);
                }
            }

            _hotkeyActions.Clear();
            _source?.RemoveHook(HwndHook);
            _source?.Dispose();
        }

        #endregion
    }
}