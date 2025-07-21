using System;

namespace DeepWorkTimer.Models
{
    /// <summary>
    /// Model containing monitor information
    /// </summary>
    public class ScreenInfo
    {
        /// <summary>
        /// Monitor index (0-based)
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Display name of the monitor
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Resolution (Width x Height)
        /// </summary>
        public string Resolution { get; set; } = "";

        /// <summary>
        /// Is this the primary monitor
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Is this the preferred monitor
        /// </summary>
        public bool IsPreferred { get; set; }

        /// <summary>
        /// Monitor position
        /// </summary>
        public System.Drawing.Rectangle Bounds { get; set; }

        /// <summary>
        /// Working area (excluding taskbar)
        /// </summary>
        public System.Drawing.Rectangle WorkingArea { get; set; }

        public ScreenInfo(int index, System.Windows.Forms.Screen screen)
        {
            Index = index;
            Name = screen.DeviceName;
            Resolution = $"{screen.Bounds.Width}x{screen.Bounds.Height}";
            IsPrimary = screen.Primary;
            Bounds = screen.Bounds;
            WorkingArea = screen.WorkingArea;
        }

        public override string ToString()
        {
            var markers = "";
            if (IsPrimary) markers += " [Primary]";
            if (IsPreferred) markers += " [Preferred]";
            
            return $"Screen {Index + 1}: {Resolution}{markers}";
        }
    }
}