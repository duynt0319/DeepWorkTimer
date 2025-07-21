using DeepWorkTimer.Models;
using DeepWorkTimer.Services;
using DeepWorkTimer.Utils;
using System.Windows;

namespace DeepWorkTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlobalHotkeyService? _hotkeyService;
        private AppSettings _settings = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            InitializeHotkeys();
            ApplySettings();
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        private void LoadSettings()
        {
            _settings = AppSettings.Load();
            System.Diagnostics.Debug.WriteLine($"?? Loaded settings - Preferred Screen: {_settings.PreferredScreenIndex + 1}");
        }

        /// <summary>
        /// Apply settings to window
        /// </summary>
        private void ApplySettings()
        {
            try
            {
                // Apply transparency
                Opacity = _settings.Opacity;

                // Restore window position
                Left = _settings.WindowLeft;
                Top = _settings.WindowTop;

                System.Diagnostics.Debug.WriteLine($"? Applied settings - Position: ({Left}, {Top}), Opacity: {Opacity}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to apply settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize Global Hotkeys service
        /// </summary>
        private void InitializeHotkeys()
        {
            try
            {
                _hotkeyService = new GlobalHotkeyService(this, _settings);
                _hotkeyService.HotkeyTriggered += OnHotkeyTriggered;
                _hotkeyService.Initialize();

                System.Diagnostics.Debug.WriteLine("? Global Hotkeys initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to initialize Global Hotkeys: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle event when hotkey is triggered
        /// </summary>
        private void OnHotkeyTriggered(object? sender, string message)
        {
            // Log hotkey trigger information
            System.Diagnostics.Debug.WriteLine($"?? Hotkey triggered: {message}");

            // TODO: Can add temporary notification UI here
        }

        /// <summary>
        /// Override SourceInitialized to make window click-through after handle is created
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Make window completely click-through (transparent to mouse events) with enhanced stability
            WindowUtils.EnsureClickThrough(this);

            System.Diagnostics.Debug.WriteLine("?? Window made click-through with enhanced stability - no more flicker!");
        }

        /// <summary>
        /// Override OnActivated to ensure click-through remains active
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // Re-apply click-through in case it was lost during activation
            WindowUtils.MakeWindowClickThrough(this);
        }

        /// <summary>
        /// Override OnDeactivated to maintain click-through behavior
        /// </summary>
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            // Ensure click-through remains active even when deactivated
            WindowUtils.MakeWindowClickThrough(this);
        }

        /// <summary>
        /// Handle window movement event to save position
        /// </summary>
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            // Save new position
            if (_settings != null && IsLoaded)
            {
                _settings.WindowLeft = Left;
                _settings.WindowTop = Top;
                // Don't save immediately to avoid spam I/O
            }
        }

        /// <summary>
        /// Cleanup when window closes
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Save final settings
                _settings?.Save();

                // Cleanup hotkey service
                _hotkeyService?.Dispose();

                // Cleanup ViewModel
                if (DataContext is ViewModels.CountdownViewModel viewModel)
                {
                    viewModel.Cleanup();
                }

                System.Diagnostics.Debug.WriteLine("? Window cleanup completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Cleanup error: {ex.Message}");
            }

            base.OnClosed(e);
        }

        /// <summary>
        /// Handle local hotkeys (fallback)
        /// </summary>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // Only handle when Global Hotkeys are not working
            if (_hotkeyService == null || !_settings.GlobalHotkeysEnabled)
            {
                HandleLocalHotkeys(e);
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Handle local hotkeys
        /// </summary>
        private void HandleLocalHotkeys(System.Windows.Input.KeyEventArgs e)
        {
            var modifiers = System.Windows.Input.Keyboard.Modifiers;

            // Ctrl+Alt+C: Center on screen
            if (modifiers == (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt) &&
                e.Key == System.Windows.Input.Key.C)
            {
                CenterWindow();
                e.Handled = true;
                return;
            }

            // Ctrl+Alt+M: Next screen
            if (modifiers == (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt) &&
                e.Key == System.Windows.Input.Key.M)
            {
                SwitchToNextScreen();
                e.Handled = true;
                return;
            }

            // Ctrl+Alt+N: Previous screen
            if (modifiers == (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt) &&
                e.Key == System.Windows.Input.Key.N)
            {
                SwitchToPreviousScreen();
                e.Handled = true;
                return;
            }

            // Ctrl+Alt+1-9: Switch to screen 1-9
            if (modifiers == (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt) &&
                e.Key >= System.Windows.Input.Key.D1 && e.Key <= System.Windows.Input.Key.D9)
            {
                var screenIndex = e.Key - System.Windows.Input.Key.D1; // Zero-based
                SwitchToScreen(screenIndex);
                e.Handled = true;
            }
        }

        #region Local Screen Management Methods

        /// <summary>
        /// Center window on current screen
        /// </summary>
        private void CenterWindow()
        {
            try
            {
                var screen = System.Windows.Forms.Screen.FromPoint(
                    new System.Drawing.Point((int)Left, (int)Top));

                var workingArea = screen.WorkingArea;
                Left = workingArea.Left + (workingArea.Width - Width) / 2;
                Top = workingArea.Top + (workingArea.Height - Height) / 2;

                System.Diagnostics.Debug.WriteLine("?? Window centered locally");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Center window error: {ex.Message}");
            }
        }

        /// <summary>
        /// Switch to next screen (local fallback)
        /// </summary>
        private void SwitchToNextScreen()
        {
            try
            {
                var screens = System.Windows.Forms.Screen.AllScreens;
                if (screens.Length <= 1) return;

                var currentScreen = System.Windows.Forms.Screen.FromPoint(
                    new System.Drawing.Point((int)Left, (int)Top));
                var currentIndex = Array.IndexOf(screens, currentScreen);
                var nextIndex = (currentIndex + 1) % screens.Length;

                MoveToScreen(screens[nextIndex]);
                System.Diagnostics.Debug.WriteLine($"?? Switched to screen {nextIndex + 1} locally");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Switch next screen error: {ex.Message}");
            }
        }

        /// <summary>
        /// Switch to previous screen (local fallback)
        /// </summary>
        private void SwitchToPreviousScreen()
        {
            try
            {
                var screens = System.Windows.Forms.Screen.AllScreens;
                if (screens.Length <= 1) return;

                var currentScreen = System.Windows.Forms.Screen.FromPoint(
                    new System.Drawing.Point((int)Left, (int)Top));
                var currentIndex = Array.IndexOf(screens, currentScreen);
                var prevIndex = (currentIndex - 1 + screens.Length) % screens.Length;

                MoveToScreen(screens[prevIndex]);
                System.Diagnostics.Debug.WriteLine($"?? Switched to screen {prevIndex + 1} locally");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Switch previous screen error: {ex.Message}");
            }
        }

        /// <summary>
        /// Switch to screen by index (local fallback)
        /// </summary>
        private void SwitchToScreen(int screenIndex)
        {
            try
            {
                var screens = System.Windows.Forms.Screen.AllScreens;
                if (screenIndex >= 0 && screenIndex < screens.Length)
                {
                    MoveToScreen(screens[screenIndex]);
                    System.Diagnostics.Debug.WriteLine($"?? Switched to screen {screenIndex + 1} locally");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Switch to screen error: {ex.Message}");
            }
        }

        /// <summary>
        /// Move window to specified screen
        /// </summary>
        private void MoveToScreen(System.Windows.Forms.Screen screen)
        {
            var workingArea = screen.WorkingArea;
            Left = workingArea.Left + (workingArea.Width - Width) / 2;
            Top = workingArea.Top + (workingArea.Height - Height) / 2;
        }

        #endregion
    }
}