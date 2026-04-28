namespace SnakeGame;

public interface ISnakeGameEngine
{
    int Score { get; }
    GamePhase Phase { get; }
    GridPoint Food { get; }
    IReadOnlyCollection<GridPoint> SnakeSegments { get; }

    void StartGame();
    void RestartGame();
    void TogglePause();
    void ChangeDirection(Direction direction);
    void Tick();
}
