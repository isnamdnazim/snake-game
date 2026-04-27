namespace SnakeGame;

/// <summary>
/// UI layout constants for consistent sizing and positioning.
/// Centralized configuration for easy theme adjustments.
/// </summary>
internal static class UiConstants
{
    /// <summary>
    /// Menu panel layout dimensions and styling.
    /// </summary>
    public static class MenuPanel
    {
        public const int Width = 330;
        public const int Height = 280;
        public const int Padding = 32;
        public const int TitleFontSize = 20;
        public const int SubtitleFontSize = 10;
        public const int LabelFontSize = 9;
        public const int ComboBoxFontSize = 10;
        public const int ButtonFontSize = 10;
    }

    /// <summary>
    /// Game over panel layout dimensions and styling.
    /// </summary>
    public static class GameOverPanel
    {
        public const int Width = 300;
        public const int Height = 200;
        public const int TitleFontSize = 18;
        public const int ScoreFontSize = 11;
        public const int BestScoreFontSize = 10;
        public const int ButtonHeight = 36;
        public const int ButtonSpacing = 30;
    }

    /// <summary>
    /// Pause/Start button dimensions and positioning.
    /// </summary>
    public static class PauseStartButton
    {
        public const int Width = 112;
        public const int Height = 32;
        public const int OffsetFromRightX = 122;
        public const int OffsetFromBottomY = 12;
    }

    /// <summary>
    /// New high score banner dimensions and positioning.
    /// </summary>
    public static class HighScoreBanner
    {
        public const int Width = 250;
        public const int Height = 36;
        public const int BorderRadius = 12;
        public const int OffsetTopY = 10;
        public const int FontSize = 10;
    }

    /// <summary>
    /// Drawing-related constants.
    /// </summary>
    public static class Drawing
    {
        public const float BorderPenWidth = 2f;
        public const float EyePenWidth = 1.2f;
        public const float SnakeBodyPenWidth = 0.52f;
    }
}
