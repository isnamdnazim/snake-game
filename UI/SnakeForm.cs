using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SnakeGame;

internal sealed partial class SnakeForm : Form
{
    private const int CountdownSeconds = 3;
    private const int FoodPulseDurationMs = 320;
    private const int NewHighScoreBannerMs = 2200;

    private readonly ISnakeGameEngine _engine;
    private readonly GameSettings _settings;
    private readonly IHighScoreStore _highScoreStore;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly System.Windows.Forms.Timer _effectsTimer;
    private readonly Button _pauseStartButton;
    private readonly Panel _menuPanel;
    private readonly Panel _gameOverPanel;
    private readonly ComboBox _difficultyComboBox;
    private readonly Label _bestScoreMenuLabel;
    private readonly Label _gameOverScoreLabel;
    private readonly Label _gameOverBestLabel;

    private int _bestScore;
    private int _lastKnownScore;
    private bool _isCountdownActive;
    private DateTime _countdownEndUtc;
    private DateTime _foodPulseUntilUtc;
    private DateTime _newHighScoreBannerUntilUtc;

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
        BackColor = UiColors.Ui.FormBackground;

        _timer = new System.Windows.Forms.Timer { Interval = _settings.TickIntervalMs };
        _timer.Tick += (_, _) => TickFrame();
        _effectsTimer = new System.Windows.Forms.Timer { Interval = 33 };
        _effectsTimer.Tick += (_, _) => AdvanceEffects();

        _pauseStartButton = CreatePauseStartButton();
        _menuPanel = CreateMenuPanel(out _difficultyComboBox, out _bestScoreMenuLabel);
        _gameOverPanel = CreateGameOverPanel(out _gameOverScoreLabel, out _gameOverBestLabel);

        Controls.Add(_pauseStartButton);
        Controls.Add(_menuPanel);
        Controls.Add(_gameOverPanel);

        ShowMenu();
        _effectsTimer!.Start();
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

        using var boardBrush = new SolidBrush(UiColors.Ui.BoardBackground);
        using var borderPen = new Pen(UiColors.Ui.BoardBorder, UiConstants.Board.BorderThickness);
        using var hudBrush = new SolidBrush(UiColors.Ui.HudBackground);

        g.FillRectangle(boardBrush, boardRect);
        g.DrawRectangle(
            borderPen,
            UiConstants.Board.BorderInset,
            UiConstants.Board.BorderInset,
            boardRect.Width - UiConstants.Board.BorderInset * 2,
            boardRect.Height - UiConstants.Board.BorderInset * 2);
        g.FillRectangle(hudBrush, new Rectangle(0, boardRect.Height, boardRect.Width, _settings.UiHeight));

        var now = DateTime.UtcNow;

        DrawFood(g, now);
        DrawSnake(g, now);
        DrawHud(g, boardRect.Height, now);

        if (_isCountdownActive)
        {
            DrawOverlay(g, GetCountdownText(now));
        }
        else if (_engine.Phase == GamePhase.Paused)
        {
            DrawOverlay(g, "Paused");
        }

        if (now < _newHighScoreBannerUntilUtc)
        {
            DrawNewHighScoreBanner(g);
        }
    }
}


