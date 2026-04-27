using System.Drawing;

namespace SnakeGame;

/// <summary>
/// Color palette for the UI and game elements.
/// Centralized color management for consistent theming and easy customization.
/// </summary>
internal static class UiColors
{
    /// <summary>
    /// Colors used for the food element.
    /// </summary>
    public static class Food
    {
        public static readonly Color Pulsing = Color.FromArgb(255, 255, 145, 55);  // Orange
        public static readonly Color Resting = Color.FromArgb(246, 90, 90);        // Red
    }

    /// <summary>
    /// Colors used for the snake.
    /// </summary>
    public static class Snake
    {
        public static readonly Color Body = Color.FromArgb(46, 175, 84);      // Green
        public static readonly Color Head = Color.FromArgb(78, 220, 108);     // Light Green
        public static readonly Color HeadPulsing = Color.FromArgb(96, 236, 124);  // Bright Green
        public static readonly Color HeadOutline = Color.FromArgb(36, 128, 62);   // Dark Green
        public static readonly Color Eye = Color.FromArgb(28, 28, 28);        // Black
    }

    /// <summary>
    /// Colors used for the UI elements.
    /// </summary>
    public static class Ui
    {
        public static readonly Color MenuPanel = Color.FromArgb(32, 32, 32);        // Dark Gray
        public static readonly Color GameOverPanel = Color.FromArgb(34, 34, 34);    // Slightly Lighter Gray
        public static readonly Color Board = Color.FromArgb(34, 34, 34);            // Board Background
        public static readonly Color BoardBorder = Color.FromArgb(95, 95, 95);      // Medium Gray
        public static readonly Color HudBackground = Color.FromArgb(20, 20, 20);    // Very Dark Gray
        public static readonly Color Background = Color.FromArgb(24, 24, 24);       // Very Dark Gray
        public static readonly Color Text = Color.WhiteSmoke;                       // Off-White
        public static readonly Color TextSecondary = Color.Gainsboro;              // Light Gray
        public static readonly Color Button = Color.FromArgb(56, 56, 56);           // Dark Gray
        public static readonly Color ButtonHover = Color.FromArgb(70, 70, 70);      // Medium Gray
        public static readonly Color ButtonPrimary = Color.FromArgb(50, 160, 90);   // Green
        public static readonly Color HighScoreBanner = Color.FromArgb(220, 43, 122, 62);  // Dark Green
    }
}
