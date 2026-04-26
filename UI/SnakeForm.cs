using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SnakeGame;

internal sealed class SnakeForm : Form
{
    private readonly ISnakeGameEngine _engine;
    private readonly GameSettings _settings;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Button _startButton;
    private readonly Button _pauseStartButton;

    public SnakeForm(ISnakeGameEngine engine, GameSettings settings)
    {
        _engine = engine;
        _settings = settings;

        DoubleBuffered = true;
        KeyPreview = true;
        Text = "Snake Game (GUI)";
        ClientSize = new Size(_settings.GridWidth * _settings.CellSize, _settings.GridHeight * _settings.CellSize + _settings.UiHeight);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(24, 24, 24);

        _timer = new System.Windows.Forms.Timer { Interval = _settings.TickIntervalMs };
        _timer.Tick += (_, _) => TickFrame();

        _startButton = CreateStartButton();
        _pauseStartButton = CreatePauseStartButton();

        Controls.Add(_startButton);
        Controls.Add(_pauseStartButton);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        var key = keyData & Keys.KeyCode;
        if (HandleInput(key))
        {
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var boardWidthPx = _settings.GridWidth * _settings.CellSize;
        var boardHeightPx = _settings.GridHeight * _settings.CellSize;

        var boardRect = new Rectangle(0, 0, boardWidthPx, boardHeightPx);

        using var boardBrush = new SolidBrush(Color.FromArgb(34, 34, 34));
        using var borderPen = new Pen(Color.FromArgb(95, 95, 95), 2f);
        using var hudBrush = new SolidBrush(Color.FromArgb(20, 20, 20));

        g.FillRectangle(boardBrush, boardRect);
        g.DrawRectangle(borderPen, 1, 1, boardRect.Width - 2, boardRect.Height - 2);
        g.FillRectangle(hudBrush, new Rectangle(0, boardRect.Height, boardRect.Width, _settings.UiHeight));

        DrawFood(g);
        DrawSnake(g);
        DrawHud(g, boardRect.Height);
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
            TabStop = false,
            Location = new Point((_settings.GridWidth * _settings.CellSize - 170) / 2, (_settings.GridHeight * _settings.CellSize - 44) / 2)
        };

        button.FlatAppearance.BorderSize = 0;
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
            Location = new Point(_settings.GridWidth * _settings.CellSize - 122, _settings.GridHeight * _settings.CellSize + 12),
            Visible = false,
            TabStop = false
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => TogglePauseStart();
        return button;
    }

    private void StartGame()
    {
        _engine.StartGame();
        _startButton.Visible = false;
        _pauseStartButton.Visible = true;
        _pauseStartButton.Text = "Pause";
        _timer.Start();
        ActiveControl = null;
        Focus();
        Invalidate();
    }

    private void TogglePauseStart()
    {
        if (_engine.Phase == GamePhase.NotStarted)
        {
            StartGame();
            return;
        }

        if (_engine.Phase == GamePhase.GameOver)
        {
            _engine.RestartGame();
            _pauseStartButton.Text = "Pause";
            _timer.Start();
            Focus();
            Invalidate();
            return;
        }

        _engine.TogglePause();
        SyncTimerAndButtonForPhase();
        Invalidate();
    }

    private void TickFrame()
    {
        _engine.Tick();

        if (_engine.Phase == GamePhase.GameOver)
        {
            _timer.Stop();
            _pauseStartButton.Text = "Start";
        }

        Invalidate();
    }

    private void SyncTimerAndButtonForPhase()
    {
        if (_engine.Phase == GamePhase.Paused)
        {
            _timer.Stop();
            _pauseStartButton.Text = "Start";
            return;
        }

        if (_engine.Phase == GamePhase.Running)
        {
            _timer.Start();
            _pauseStartButton.Text = "Pause";
        }
    }

    private bool HandleInput(Keys key)
    {
        if (_engine.Phase == GamePhase.NotStarted)
        {
            return false;
        }

        if (_engine.Phase != GamePhase.GameOver && (key == Keys.P || key == Keys.Space))
        {
            _engine.TogglePause();
            SyncTimerAndButtonForPhase();
            Invalidate();
            return true;
        }

        if (_engine.Phase == GamePhase.GameOver && (key == Keys.Enter || key == Keys.R || key == Keys.Space))
        {
            _engine.RestartGame();
            _pauseStartButton.Text = "Pause";
            _timer.Start();
            Invalidate();
            return true;
        }

        var direction = key switch
        {
            Keys.Up or Keys.W => Direction.Up,
            Keys.Down or Keys.S => Direction.Down,
            Keys.Left or Keys.A => Direction.Left,
            Keys.Right or Keys.D => Direction.Right,
            _ => (Direction?)null
        };

        if (direction is null)
        {
            return false;
        }

        _engine.ChangeDirection(direction.Value);
        return true;
    }

    private void DrawFood(Graphics g)
    {
        if (_engine.Phase == GamePhase.NotStarted)
        {
            return;
        }

        var rect = CellBounds(_engine.Food, 6);
        using var foodBrush = new SolidBrush(Color.FromArgb(246, 90, 90));
        g.FillEllipse(foodBrush, rect);
    }

    private void DrawSnake(Graphics g)
    {
        if (_engine.Phase == GamePhase.NotStarted)
        {
            return;
        }

        var index = 0;
        foreach (var segment in _engine.SnakeSegments)
        {
            var rect = CellBounds(segment, 3);
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
        using var scoreFont = new Font("Segoe UI", 11, FontStyle.Bold);
        g.DrawString($"Score: {_engine.Score}", scoreFont, scoreBrush, new PointF(10, yStart + 10));

        using var hintFont = new Font("Segoe UI", 9);
        using var hintBrush = new SolidBrush(Color.Gainsboro);

        var hint = _engine.Phase switch
        {
            GamePhase.NotStarted => "Click 'Start Game' to begin",
            GamePhase.Paused => "Paused - press Start button or P / Space",
            GamePhase.GameOver => "Game Over - press Enter / Space / R to restart",
            _ => "Use Arrow Keys or WASD | P / Space = Pause"
        };

        g.DrawString(hint, hintFont, hintBrush, new PointF(10, yStart + 33));
    }

    private Rectangle CellBounds(GridPoint point, int inset)
    {
        var x = point.X * _settings.CellSize + inset;
        var y = point.Y * _settings.CellSize + inset;
        var size = _settings.CellSize - inset * 2;
        return new Rectangle(x, y, size, size);
    }
}
