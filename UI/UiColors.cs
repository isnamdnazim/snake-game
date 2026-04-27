using System.Drawing;

namespace SnakeGame;

internal static class UiColors
{
    internal static class Food
    {
        internal static readonly Color Pulsing = Color.FromArgb(255, 255, 145, 55);
        internal static readonly Color Resting = Color.FromArgb(246, 90, 90);
    }

    internal static class Snake
    {
        internal static readonly Color BodyPen = Color.FromArgb(42, 162, 78);
        internal static readonly Color BodyFill = Color.FromArgb(46, 175, 84);
        internal static readonly Color HeadPulsing = Color.FromArgb(96, 236, 124);
        internal static readonly Color Head = Color.FromArgb(78, 220, 108);
        internal static readonly Color HeadOutline = Color.FromArgb(36, 128, 62);
        internal static readonly Color Eye = Color.FromArgb(28, 28, 28);
    }

    internal static class Ui
    {
        internal static readonly Color FormBackground = Color.FromArgb(24, 24, 24);
        internal static readonly Color BoardBackground = Color.FromArgb(34, 34, 34);
        internal static readonly Color BoardBorder = Color.FromArgb(95, 95, 95);
        internal static readonly Color HudBackground = Color.FromArgb(20, 20, 20);

        internal static readonly Color MenuPanel = Color.FromArgb(32, 32, 32);
        internal static readonly Color GameOverPanel = Color.FromArgb(34, 34, 34);

        internal static readonly Color ButtonNeutral = Color.FromArgb(56, 56, 56);
        internal static readonly Color ButtonPrimary = Color.FromArgb(50, 160, 90);
        internal static readonly Color ButtonSecondary = Color.FromArgb(70, 70, 70);

        internal static readonly Color Overlay = Color.FromArgb(110, 0, 0, 0);
        internal static readonly Color HighScoreBanner = Color.FromArgb(220, 43, 122, 62);
        internal static readonly Color ScoreDelta = Color.FromArgb(255, 255, 220, 90);
    }

    internal static class Text
    {
        internal static readonly Color Primary = Color.White;
        internal static readonly Color Secondary = Color.WhiteSmoke;
        internal static readonly Color Muted = Color.Gainsboro;
    }
}