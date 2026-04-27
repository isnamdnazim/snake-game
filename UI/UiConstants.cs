namespace SnakeGame;

internal static class UiConstants
{
    internal static class Board
    {
        internal const float BorderThickness = 2f;
        internal const int BorderInset = 1;
    }

    internal static class MenuPanel
    {
        internal const int Width = 330;
        internal const int Height = 280;
        internal const int HorizontalPadding = 32;

        internal const int TitleTop = 14;
        internal const int TitleHeight = 50;
        internal const int SubtitleTop = 62;
        internal const int SubtitleHeight = 24;

        internal const int DifficultyLabelTop = 102;
        internal const int DifficultyComboTop = 124;

        internal const int StartButtonTop = 166;
        internal const int StartButtonHeight = 40;
        internal const int ExitButtonTop = 212;
        internal const int ExitButtonHeight = 34;

        internal const int BestScoreTop = 252;
        internal const int BestScoreHeight = 20;
    }

    internal static class GameOverPanel
    {
        internal const int Width = 300;
        internal const int Height = 200;

        internal const int TitleTop = 10;
        internal const int TitleHeight = 42;
        internal const int ScoreTop = 58;
        internal const int ScoreHeight = 26;
        internal const int BestTop = 86;
        internal const int BestHeight = 24;

        internal const int ButtonLeft = 30;
        internal const int RestartTop = 118;
        internal const int RestartHeight = 36;
        internal const int MenuTop = 158;
        internal const int MenuHeight = 30;
        internal const int ButtonHorizontalPadding = 60;
    }

    internal static class PauseButton
    {
        internal const int Width = 112;
        internal const int Height = 32;
        internal const int RightOffset = 122;
        internal const int TopOffset = 12;
        internal const int BorderSize = 0;
    }

    internal static class Drawing
    {
        internal const float SnakeBodyPenFactor = 0.52f;
        internal const float EyeDistanceFactor = 0.13f;
        internal const float EyeRadiusFactor = 0.08f;
        internal const float EyeForwardOffsetFactor = 0.16f;

        internal const int HighScoreBannerWidth = 250;
        internal const int HighScoreBannerHeight = 36;
        internal const int HighScoreBannerTop = 10;
        internal const int HighScoreBannerCornerRadius = 12;
        internal const int HighScoreBannerTextOffsetX = 52;
        internal const int HighScoreBannerTextOffsetY = 9;
    }
}