#if false
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

        var segments = _engine.SnakeSegments.ToList();
        if (segments.Count == 0)
        {
            return;
        }

        using (var bodyPen = new Pen(Color.FromArgb(42, 162, 78), _settings.CellSize * 0.52f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        })
        {
            for (var i = 0; i < segments.Count - 1; i++)
            {
                var from = CellCenter(segments[i]);
                var to = CellCenter(segments[i + 1]);
                g.DrawLine(bodyPen, from, to);
            }
        }

        for (var i = segments.Count - 1; i >= 1; i--)
        {
            var segment = segments[i];
            var inset = i == segments.Count - 1 ? 6 : 4;
            var rect = CellBounds(segment, inset);
            using var brush = new SolidBrush(Color.FromArgb(46, 175, 84));
            g.FillEllipse(brush, rect);
        }

        DrawHead(g, segments);
    }

    private void DrawHead(Graphics g, IReadOnlyList<GridPoint> segments)
    {
        var head = segments[0];
        var rect = CellBounds(head, 2);

        using (var headBrush = new SolidBrush(Color.FromArgb(78, 220, 108)))
        {
            g.FillEllipse(headBrush, rect);
        }

        using (var outlinePen = new Pen(Color.FromArgb(36, 128, 62), 1.2f))
        {
            g.DrawEllipse(outlinePen, rect);
        }

        var headDirection = GetHeadDirection(segments);
        var center = CellCenter(head);
        var eyeDistance = _settings.CellSize * 0.13f;
        var eyeRadius = Math.Max(2f, _settings.CellSize * 0.08f);
        var eyeForwardOffset = _settings.CellSize * 0.16f;

        var (fx, fy, px, py) = headDirection switch
        {
            Direction.Up => (0f, -1f, 1f, 0f),
            Direction.Down => (0f, 1f, -1f, 0f),
            Direction.Left => (-1f, 0f, 0f, -1f),
            _ => (1f, 0f, 0f, 1f)
        };

        var eye1 = new PointF(
            center.X + fx * eyeForwardOffset + px * eyeDistance,
            center.Y + fy * eyeForwardOffset + py * eyeDistance);
        var eye2 = new PointF(
            center.X + fx * eyeForwardOffset - px * eyeDistance,
            center.Y + fy * eyeForwardOffset - py * eyeDistance);

        using var eyeBrush = new SolidBrush(Color.FromArgb(28, 28, 28));
        g.FillEllipse(eyeBrush, eye1.X - eyeRadius, eye1.Y - eyeRadius, eyeRadius * 2, eyeRadius * 2);
        g.FillEllipse(eyeBrush, eye2.X - eyeRadius, eye2.Y - eyeRadius, eyeRadius * 2, eyeRadius * 2);
    }

    private Direction GetHeadDirection(IReadOnlyList<GridPoint> segments)
    {
        if (segments.Count < 2)
        {
            return Direction.Right;
        }

        var head = segments[0];
        var neck = segments[1];

        if (head.X > neck.X)
        {
            return Direction.Right;
        }

        if (head.X < neck.X)
        {
            return Direction.Left;
        }

        if (head.Y < neck.Y)
        {
            return Direction.Up;
        }

        return Direction.Down;
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

    private PointF CellCenter(GridPoint point)
    {
        var x = point.X * _settings.CellSize + _settings.CellSize / 2f;
        var y = point.Y * _settings.CellSize + _settings.CellSize / 2f;
        return new PointF(x, y);
    }
}
#endif

using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SnakeGame;

internal sealed class SnakeForm : Form
{
    private readonly ISnakeGameEngine _engine;
    private readonly GameSettings _settings;
    private readonly IHighScoreStore _highScoreStore;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Button _pauseStartButton;
    private readonly Panel _menuPanel;
    private readonly Panel _gameOverPanel;
    private readonly ComboBox _difficultyComboBox;
    private readonly Label _bestScoreMenuLabel;
    private readonly Label _gameOverScoreLabel;
    private readonly Label _gameOverBestLabel;

    private int _bestScore;

    public SnakeForm(ISnakeGameEngine engine, GameSettings settings, IHighScoreStore highScoreStore)
    {
        _engine = engine;
        _settings = settings;
        _highScoreStore = highScoreStore;
        _bestScore = _highScoreStore.LoadBestScore();

        DoubleBuffered = true;
        KeyPreview = true;
        Text = "Snake Game";
        ClientSize = new Size(_settings.GridWidth * _settings.CellSize, _settings.GridHeight * _settings.CellSize + _settings.UiHeight);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(24, 24, 24);

        _timer = new System.Windows.Forms.Timer { Interval = _settings.TickIntervalMs };
        _timer.Tick += (_, _) => TickFrame();

        _pauseStartButton = CreatePauseStartButton();
        _menuPanel = CreateMenuPanel(out _difficultyComboBox, out _bestScoreMenuLabel);
        _gameOverPanel = CreateGameOverPanel(out _gameOverScoreLabel, out _gameOverBestLabel);

        Controls.Add(_pauseStartButton);
        Controls.Add(_menuPanel);
        Controls.Add(_gameOverPanel);

        ShowMenu();
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

        if (_engine.Phase == GamePhase.Paused)
        {
            DrawOverlay(g, "Paused");
        }
    }

    private void TickFrame()
    {
        _engine.Tick();

        if (_engine.Phase == GamePhase.GameOver)
        {
            _timer.Stop();
            _pauseStartButton.Text = "Start";
            OnGameOver();
        }

        Invalidate();
    }

    private void StartGame()
    {
        _engine.StartGame();
        _timer.Interval = GetTickInterval(GetSelectedDifficulty());
        _timer.Start();

        _menuPanel.Visible = false;
        _gameOverPanel.Visible = false;
        _pauseStartButton.Visible = true;
        _pauseStartButton.Text = "Pause";

        ActiveControl = null;
        Focus();
        Invalidate();
    }

    private void RestartGame()
    {
        _engine.RestartGame();
        _timer.Interval = GetTickInterval(GetSelectedDifficulty());
        _timer.Start();

        _gameOverPanel.Visible = false;
        _pauseStartButton.Visible = true;
        _pauseStartButton.Text = "Pause";

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
            RestartGame();
            return;
        }

        _engine.TogglePause();
        SyncTimerAndButtonForPhase();
        Invalidate();
    }

    private void ShowMenu()
    {
        _timer.Stop();
        _pauseStartButton.Visible = false;
        _menuPanel.Visible = true;
        _gameOverPanel.Visible = false;
        _bestScoreMenuLabel.Text = $"Best Score: {_bestScore}";
        Invalidate();
    }

    private void OnGameOver()
    {
        if (_engine.Score > _bestScore)
        {
            _bestScore = _engine.Score;
            _highScoreStore.SaveBestScore(_bestScore);
        }

        _gameOverScoreLabel.Text = $"Score: {_engine.Score}";
        _gameOverBestLabel.Text = $"Best: {_bestScore}";

        _pauseStartButton.Visible = false;
        _gameOverPanel.Visible = true;
        _menuPanel.Visible = false;
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
        if (_menuPanel.Visible)
        {
            if (key == Keys.Enter)
            {
                StartGame();
                return true;
            }

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
            RestartGame();
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

        using var metaFont = new Font("Segoe UI", 9);
        using var metaBrush = new SolidBrush(Color.Gainsboro);
        var difficulty = GetSelectedDifficulty();
        g.DrawString($"Difficulty: {difficulty}    Best: {_bestScore}", metaFont, metaBrush, new PointF(10, yStart + 33));
    }

    private void DrawOverlay(Graphics g, string text)
    {
        var boardWidthPx = _settings.GridWidth * _settings.CellSize;
        var boardHeightPx = _settings.GridHeight * _settings.CellSize;
        using var overlayBrush = new SolidBrush(Color.FromArgb(110, 0, 0, 0));
        g.FillRectangle(overlayBrush, new Rectangle(0, 0, boardWidthPx, boardHeightPx));

        using var font = new Font("Segoe UI", 24, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.WhiteSmoke);
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, textBrush, (boardWidthPx - size.Width) / 2, (boardHeightPx - size.Height) / 2);
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

    private Panel CreateMenuPanel(out ComboBox difficultyComboBox, out Label bestScoreLabel)
    {
        var boardWidthPx = _settings.GridWidth * _settings.CellSize;
        var boardHeightPx = _settings.GridHeight * _settings.CellSize;

        var panel = new Panel
        {
            Size = new Size(330, 280),
            Location = new Point((boardWidthPx - 330) / 2, (boardHeightPx - 280) / 2),
            BackColor = Color.FromArgb(32, 32, 32)
        };

        var title = new Label
        {
            Text = "Snake Game",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.WhiteSmoke,
            AutoSize = false,
            Width = panel.Width,
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = 14
        };

        var subtitle = new Label
        {
            Text = "Pick difficulty and start",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gainsboro,
            AutoSize = false,
            Width = panel.Width,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = 62
        };

        var difficultyLabel = new Label
        {
            Text = "Difficulty",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Left = 32,
            Top = 102
        };

        difficultyComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Left = 32,
            Top = 124,
            Width = panel.Width - 64,
            Font = new Font("Segoe UI", 10)
        };

        difficultyComboBox.Items.AddRange(Enum.GetNames(typeof(DifficultyLevel)));
        difficultyComboBox.SelectedItem = DifficultyLevel.Normal.ToString();

        var startButton = new Button
        {
            Text = "Start Game",
            Size = new Size(panel.Width - 64, 40),
            Left = 32,
            Top = 166,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(50, 160, 90),
            ForeColor = Color.White,
            TabStop = false
        };
        startButton.FlatAppearance.BorderSize = 0;
        startButton.Click += (_, _) => StartGame();

        var exitButton = new Button
        {
            Text = "Exit",
            Size = new Size(panel.Width - 64, 34),
            Left = 32,
            Top = 212,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(70, 70, 70),
            ForeColor = Color.White,
            TabStop = false
        };
        exitButton.FlatAppearance.BorderSize = 0;
        exitButton.Click += (_, _) => Close();

        bestScoreLabel = new Label
        {
            Text = $"Best Score: {_bestScore}",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gainsboro,
            AutoSize = false,
            Width = panel.Width,
            Height = 20,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = 252
        };

        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        panel.Controls.Add(difficultyLabel);
        panel.Controls.Add(difficultyComboBox);
        panel.Controls.Add(startButton);
        panel.Controls.Add(exitButton);
        panel.Controls.Add(bestScoreLabel);

        return panel;
    }

    private Panel CreateGameOverPanel(out Label scoreLabel, out Label bestLabel)
    {
        var boardWidthPx = _settings.GridWidth * _settings.CellSize;
        var boardHeightPx = _settings.GridHeight * _settings.CellSize;

        var panel = new Panel
        {
            Size = new Size(300, 200),
            Location = new Point((boardWidthPx - 300) / 2, (boardHeightPx - 200) / 2),
            BackColor = Color.FromArgb(34, 34, 34),
            Visible = false
        };

        var title = new Label
        {
            Text = "Game Over",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.WhiteSmoke,
            AutoSize = false,
            Width = panel.Width,
            Height = 42,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = 10
        };

        scoreLabel = new Label
        {
            Text = "Score: 0",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Width = panel.Width,
            Height = 26,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = 58
        };

        bestLabel = new Label
        {
            Text = "Best: 0",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gainsboro,
            AutoSize = false,
            Width = panel.Width,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = 86
        };

        var restartButton = new Button
        {
            Text = "Play Again",
            Size = new Size(panel.Width - 60, 36),
            Left = 30,
            Top = 118,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(50, 160, 90),
            ForeColor = Color.White,
            TabStop = false
        };
        restartButton.FlatAppearance.BorderSize = 0;
        restartButton.Click += (_, _) => RestartGame();

        var menuButton = new Button
        {
            Text = "Main Menu",
            Size = new Size(panel.Width - 60, 30),
            Left = 30,
            Top = 158,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(70, 70, 70),
            ForeColor = Color.White,
            TabStop = false
        };
        menuButton.FlatAppearance.BorderSize = 0;
        menuButton.Click += (_, _) => ShowMenu();

        panel.Controls.Add(title);
        panel.Controls.Add(scoreLabel);
        panel.Controls.Add(bestLabel);
        panel.Controls.Add(restartButton);
        panel.Controls.Add(menuButton);

        return panel;
    }

    private DifficultyLevel GetSelectedDifficulty()
    {
        var selected = _difficultyComboBox.SelectedItem?.ToString();
        return Enum.TryParse<DifficultyLevel>(selected, out var level)
            ? level
            : DifficultyLevel.Normal;
    }

    private static int GetTickInterval(DifficultyLevel level) => level switch
    {
        DifficultyLevel.Easy => 130,
        DifficultyLevel.Hard => 70,
        _ => 95
    };

    private Rectangle CellBounds(GridPoint point, int inset)
    {
        var x = point.X * _settings.CellSize + inset;
        var y = point.Y * _settings.CellSize + inset;
        var size = _settings.CellSize - inset * 2;
        return new Rectangle(x, y, size, size);
    }
}
