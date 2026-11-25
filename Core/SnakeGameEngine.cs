namespace SnakeGame;

public sealed class SnakeGameEngine(GameSettings settings, IFoodSpawner foodSpawner) : ISnakeGameEngine
{
    private const int MaxQueuedDirections = 2;

    private readonly GameSettings _settings = settings;
    private readonly IFoodSpawner _foodSpawner = foodSpawner;
    private readonly LinkedList<GridPoint> _snake = new();
    private readonly HashSet<GridPoint> _occupied = new();
    private readonly Queue<Direction> _directionQueue = new(MaxQueuedDirections);

    private Direction _direction = Direction.Right;
    private Direction _lastQueuedDirection = Direction.Right;

    public int Score { get; private set; }
    public GamePhase Phase { get; private set; } = GamePhase.NotStarted;
    public GridPoint Food { get; private set; }
    public IReadOnlyCollection<GridPoint> SnakeSegments => _snake;

    public void StartGame()
    {
        ResetBoard();
        Phase = GamePhase.Running;
    }

    public void RestartGame()
    {
        if (Phase is not GamePhase.GameOver and not GamePhase.Running and not GamePhase.Paused)
        {
            return;
        }

        StartGame();
    }

    public void TogglePause()
    {
        if (Phase == GamePhase.Running)
        {
            Phase = GamePhase.Paused;
            return;
        }

        if (Phase == GamePhase.Paused)
        {
            Phase = GamePhase.Running;
        }
    }

    public void ChangeDirection(Direction direction)
    {
        if (Phase != GamePhase.Running)
        {
            return;
        }

        var referenceDirection = _directionQueue.Count > 0 ? _lastQueuedDirection : _direction;
        if (direction == referenceDirection || IsOpposite(direction, referenceDirection))
        {
            return;
        }

        if (_directionQueue.Count >= MaxQueuedDirections)
        {
            return;
        }

        _directionQueue.Enqueue(direction);
        _lastQueuedDirection = direction;
    }

    public void Tick()
    {
        if (Phase != GamePhase.Running)
        {
            return;
        }

        if (_directionQueue.TryDequeue(out var nextDirection))
        {
            _direction = nextDirection;
        }

        var head = _snake.First!.Value;
        var newHead = Move(head, _direction);

        if (newHead.X < 0 || newHead.X >= _settings.GridWidth || newHead.Y < 0 || newHead.Y >= _settings.GridHeight)
        {
            Phase = GamePhase.GameOver;
            return;
        }

        var tail = _snake.Last!.Value;
        var grows = newHead == Food;

        var selfCollision = _occupied.Contains(newHead) && !(newHead == tail && !grows);
        if (selfCollision)
        {
            Phase = GamePhase.GameOver;
            return;
        }

        _snake.AddFirst(newHead);
        _occupied.Add(newHead);

        if (grows)
        {
            Score += _settings.ScorePerFood;

            if (_snake.Count >= _settings.GridWidth * _settings.GridHeight)
            {
                Phase = GamePhase.GameOver;
                return;
            }

            Food = _foodSpawner.Spawn(_settings.GridWidth, _settings.GridHeight, _occupied);
        }
        else
        {
            _snake.RemoveLast();
            _occupied.Remove(tail);
        }
    }

    private void ResetBoard()
    {
        _snake.Clear();
        _occupied.Clear();

        var start = new GridPoint(_settings.GridWidth / 2, _settings.GridHeight / 2);
        _snake.AddFirst(start);
        _occupied.Add(start);

        _directionQueue.Clear();
        _direction = Direction.Right;
        _lastQueuedDirection = Direction.Right;
        Score = 0;
        Food = _foodSpawner.Spawn(_settings.GridWidth, _settings.GridHeight, _occupied);
    }

    private static GridPoint Move(GridPoint point, Direction direction) => direction switch
    {
        Direction.Up => point with { Y = point.Y - 1 },
        Direction.Down => point with { Y = point.Y + 1 },
        Direction.Left => point with { X = point.X - 1 },
        Direction.Right => point with { X = point.X + 1 },
        _ => point
    };

    private static bool IsOpposite(Direction next, Direction current) =>
        (next == Direction.Up && current == Direction.Down) ||
        (next == Direction.Down && current == Direction.Up) ||
        (next == Direction.Left && current == Direction.Right) ||
        (next == Direction.Right && current == Direction.Left);
}
