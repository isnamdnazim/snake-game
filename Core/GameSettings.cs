namespace SnakeGame;

internal sealed class GameSettings
{
    public int GridWidth { get; init; } = 24;
    public int GridHeight { get; init; } = 24;
    public int TickIntervalMs { get; init; } = 95;
    public int ScorePerFood { get; init; } = 10;
    public int CellSize { get; init; } = 24;
    public int UiHeight { get; init; } = 58;
}
