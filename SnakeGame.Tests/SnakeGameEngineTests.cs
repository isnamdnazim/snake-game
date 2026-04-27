using Xunit;

namespace SnakeGame.Tests;

public sealed class SnakeGameEngineTests
{
    private sealed class FixedFoodSpawner(GridPoint spawnPoint) : IFoodSpawner
    {
        public GridPoint Spawn(int width, int height, IReadOnlySet<GridPoint> occupiedCells)
        {
            return spawnPoint;
        }
    }

    private static SnakeGameEngine CreateEngine()
    {
        var settings = new GameSettings
        {
            GridWidth = 10,
            GridHeight = 10,
            ScorePerFood = 10
        };

        return new SnakeGameEngine(settings, new FixedFoodSpawner(new GridPoint(0, 0)));
    }

    [Fact]
    public void StartGame_SetsPhaseToRunning_AndResetsScore()
    {
        var engine = CreateEngine();

        engine.StartGame();

        Assert.Equal(GamePhase.Running, engine.Phase);
        Assert.Equal(0, engine.Score);
        Assert.Single(engine.SnakeSegments);
    }

    [Fact]
    public void TogglePause_SwitchesBetweenRunningAndPaused()
    {
        var engine = CreateEngine();
        engine.StartGame();

        engine.TogglePause();
        Assert.Equal(GamePhase.Paused, engine.Phase);

        engine.TogglePause();
        Assert.Equal(GamePhase.Running, engine.Phase);
    }

    [Fact]
    public void Tick_WhenPaused_DoesNotMoveSnake()
    {
        var engine = CreateEngine();
        engine.StartGame();
        engine.TogglePause();

        var before = engine.SnakeSegments.First();
        engine.Tick();
        var after = engine.SnakeSegments.First();

        Assert.Equal(before, after);
    }

    [Fact]
    public void Tick_MovesSnakeRightByDefault()
    {
        var engine = CreateEngine();
        engine.StartGame();

        var before = engine.SnakeSegments.First();
        engine.Tick();
        var after = engine.SnakeSegments.First();

        Assert.Equal(before.X + 1, after.X);
        Assert.Equal(before.Y, after.Y);
    }

    [Fact]
    public void ChangeDirection_OppositeDirection_IsIgnored()
    {
        var engine = CreateEngine();
        engine.StartGame();

        engine.ChangeDirection(Direction.Left);
        var before = engine.SnakeSegments.First();
        engine.Tick();
        var after = engine.SnakeSegments.First();

        Assert.Equal(before.X + 1, after.X);
        Assert.Equal(before.Y, after.Y);
    }

    [Fact]
    public void Tick_WhenCrossingWall_SetsGameOver()
    {
        var engine = CreateEngine();
        engine.StartGame();

        for (var i = 0; i < 20 && engine.Phase == GamePhase.Running; i++)
        {
            engine.Tick();
        }

        Assert.Equal(GamePhase.GameOver, engine.Phase);
    }
}
