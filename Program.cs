#if false
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SnakeForm());
    }
}

internal sealed class SnakeForm : Form
{
    private const int GridWidth = 24;
    private const int GridHeight = 24;
    private const int CellSize = 24;
    private const int UiHeight = 58;

    private readonly LinkedList<GridPoint> _snake = new();
    private readonly HashSet<GridPoint> _occupied = new();
    private readonly Random _random = new();
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Button _startButton;
    private readonly Button _pauseStartButton;

    private GridPoint _food;
    private Direction _direction = Direction.Right;
    private Direction _pendingDirection = Direction.Right;
    private int _score;
    private bool _gameOver;
    private bool _hasGameStarted;
    private bool _isPaused;

    public SnakeForm()
    {
        DoubleBuffered = true;
        KeyPreview = true;
        Text = "Snake Game (GUI)";
        ClientSize = new Size(GridWidth * CellSize, GridHeight * CellSize + UiHeight);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(24, 24, 24);

        _timer = new System.Windows.Forms.Timer { Interval = 95 };
        _timer.Tick += (_, _) => TickGame();

        _startButton = CreateStartButton();
        _pauseStartButton = CreatePauseStartButton();
        Controls.Add(_startButton);
        Controls.Add(_pauseStartButton);

        ResetGame();
    }

    private Button CreateStartButton()
    {
        var button = new Button
        {
            Text = "Start Game",
            Size = new Size(170, 44),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(50, 160, 90),
            ForeColor = Color.White,
            TabStop = false
        };

        button.FlatAppearance.BorderSize = 0;
        button.Location = new Point((GridWidth * CellSize - button.Width) / 2, (GridHeight * CellSize - button.Height) / 2);
        button.Click += (_, _) => StartGame();
        return button;
    }

    private Button CreatePauseStartButton()
    {
        var button = new Button
        {
            Text = "Pause",
            Size = new Size(112, 32),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(56, 56, 56),
            ForeColor = Color.White,
            Location = new Point(GridWidth * CellSize - 122, GridHeight * CellSize + 12),
            Visible = false,
            TabStop = false
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => HandlePauseStartClick();
        return button;
    }

    private void StartGame()
    {
        _hasGameStarted = true;
        _isPaused = false;
        _startButton.Visible = false;
        _pauseStartButton.Visible = true;
        _pauseStartButton.Text = "Pause";
        ResetGame();
        _timer.Start();
        ActiveControl = null;
        Focus();
    }

    private void HandlePauseStartClick()
    {
        if (!_hasGameStarted)
        {
            StartGame();
            return;
        }

        if (_gameOver)
        {
            ResetGame();
            _isPaused = false;
            _pauseStartButton.Text = "Pause";
            _timer.Start();
            Focus();
            return;
        }

        TogglePause();
    }

    private void TogglePause()
    {
        if (_isPaused)
        {
            _isPaused = false;
            _pauseStartButton.Text = "Pause";
            _timer.Start();
        }
        else
        {
            _isPaused = true;
            _pauseStartButton.Text = "Start";
            _timer.Stop();
        }

        Invalidate();
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        HandleGameKey(e.KeyCode);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var boardRect = new Rectangle(0, 0, GridWidth * CellSize, GridHeight * CellSize);
        using var boardBrush = new SolidBrush(Color.FromArgb(34, 34, 34));
        using var borderPen = new Pen(Color.FromArgb(95, 95, 95), 2f);
        using var hudBrush = new SolidBrush(Color.FromArgb(20, 20, 20));

        g.FillRectangle(boardBrush, boardRect);
        g.DrawRectangle(borderPen, 1, 1, boardRect.Width - 2, boardRect.Height - 2);
        g.FillRectangle(hudBrush, new Rectangle(0, boardRect.Height, boardRect.Width, UiHeight));

        DrawFood(g);
        DrawSnake(g);
        DrawHud(g, boardRect.Height);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        var key = keyData & Keys.KeyCode;

        if (HandleGameKey(key))
        {
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private bool HandleGameKey(Keys key)
    {
        if (!_hasGameStarted)
        {
            return false;
        }

        if (!_gameOver && (key == Keys.P || key == Keys.Space))
        {
            TogglePause();
            return true;
        }

        var next = key switch
        {
            Keys.Up or Keys.W => Direction.Up,
            Keys.Down or Keys.S => Direction.Down,
            Keys.Left or Keys.A => Direction.Left,
            Keys.Right or Keys.D => Direction.Right,
            _ => _pendingDirection
        };

        if (!IsOpposite(next, _direction))
        {
            _pendingDirection = next;
        }

        if (_gameOver && (key == Keys.Enter || key == Keys.R || key == Keys.Space))
        {
            ResetGame();
            _timer.Start();
            return true;
        }

        return key is Keys.Up or Keys.Down or Keys.Left or Keys.Right or Keys.W or Keys.A or Keys.S or Keys.D or Keys.P or Keys.Space or Keys.Enter or Keys.R;
    }

    private void TickGame()
    {
        if (_gameOver || _isPaused)
        {
            return;
        }

        _direction = _pendingDirection;

        var head = _snake.First!.Value;
        var newHead = MovePoint(head, _direction);

        if (newHead.X < 0 || newHead.X >= GridWidth || newHead.Y < 0 || newHead.Y >= GridHeight)
        {
            EndGame();
            return;
        }

        var tail = _snake.Last!.Value;
        var grows = newHead == _food;

        var selfCollision = _occupied.Contains(newHead) && !(newHead == tail && !grows);
        if (selfCollision)
        {
            EndGame();
            return;
        }

        _snake.AddFirst(newHead);
        _occupied.Add(newHead);

        if (grows)
        {
            _score += 10;
            _food = SpawnFood();
        }
        else
        {
            _snake.RemoveLast();
            _occupied.Remove(tail);
        }

        Invalidate();
    }

    private void ResetGame()
    {
        _snake.Clear();
        _occupied.Clear();

        var start = new GridPoint(GridWidth / 2, GridHeight / 2);
        _snake.AddFirst(start);
        _occupied.Add(start);

        _direction = Direction.Right;
        _pendingDirection = Direction.Right;
        _food = SpawnFood();
        _score = 0;
        _gameOver = false;
        _isPaused = false;
        if (_hasGameStarted)
        {
            _pauseStartButton.Text = "Pause";
        }

        Invalidate();
    }

    private void EndGame()
    {
        _gameOver = true;
        _timer.Stop();
        _pauseStartButton.Text = "Start";
        Invalidate();
    }

    private GridPoint SpawnFood()
    {
        GridPoint p;
        do
        {
            p = new GridPoint(_random.Next(GridWidth), _random.Next(GridHeight));
        } while (_occupied.Contains(p));

        return p;
    }

    private static GridPoint MovePoint(GridPoint p, Direction d) => d switch
    {
        Direction.Up => p with { Y = p.Y - 1 },
        Direction.Down => p with { Y = p.Y + 1 },
        Direction.Left => p with { X = p.X - 1 },
        Direction.Right => p with { X = p.X + 1 },
        _ => p
    };

    private static bool IsOpposite(Direction a, Direction b) =>
        (a == Direction.Up && b == Direction.Down) ||
        (a == Direction.Down && b == Direction.Up) ||
        (a == Direction.Left && b == Direction.Right) ||
        (a == Direction.Right && b == Direction.Left);

    private void DrawFood(Graphics g)
    {
        var rect = CellBounds(_food, 6);
        using var foodBrush = new SolidBrush(Color.FromArgb(246, 90, 90));
        g.FillEllipse(foodBrush, rect);
    }

    private void DrawSnake(Graphics g)
    {
        var index = 0;
        foreach (var segment in _snake)
        {
            var inset = 3;
            var rect = CellBounds(segment, inset);
            var color = index == 0
                ? Color.FromArgb(70, 210, 100)
                : Color.FromArgb(46, 175, 84);

            using var brush = new SolidBrush(color);
            g.FillRoundedRectangle(brush, rect, 6);
            index++;
        }
    }

    private void DrawHud(Graphics g, int yStart)
    {
        using var scoreBrush = new SolidBrush(Color.WhiteSmoke);
        using var font = new Font("Segoe UI", 11, FontStyle.Bold);

        g.DrawString($"Score: {_score}", font, scoreBrush, new PointF(10, yStart + 10));

        using var hintFont = new Font("Segoe UI", 9);
        using var hintBrush = new SolidBrush(Color.Gainsboro);
        var hint = !_hasGameStarted
            ? "Click 'Start Game' to begin"
            : _gameOver
            ? "Game Over - Press Enter / Space / R to restart"
            : _isPaused
            ? "Paused - Press Start button or P / Space"
            : "Use Arrow Keys or WASD | P / Space = Pause";

        g.DrawString(hint, hintFont, hintBrush, new PointF(10, yStart + 33));
    }

    private static Rectangle CellBounds(GridPoint p, int inset)
    {
        var x = p.X * CellSize + inset;
        var y = p.Y * CellSize + inset;
        var size = CellSize - inset * 2;
        return new Rectangle(x, y, size, size);
    }
}

internal enum Direction
{
    Up,
    Down,
    Left,
    Right
}

internal readonly record struct GridPoint(int X, int Y);

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        using var path = new GraphicsPath();

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        graphics.FillPath(brush, path);
    }
}
#endif
