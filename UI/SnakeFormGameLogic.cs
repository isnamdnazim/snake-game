using System.Windows.Forms;

namespace SnakeGame;

/// <summary>
/// Partial class containing game logic and state management methods.
/// </summary>
internal sealed partial class SnakeForm
{
    /// <summary>
    /// Called on each game tick to update game state.
    /// </summary>
    private void TickFrame()
    {
        var previousScore = _engine.Score;
        _engine.Tick();

        if (_engine.Score > previousScore)
        {
            _foodPulseUntilUtc = DateTime.UtcNow.AddMilliseconds(FoodPulseDurationMs);
        }

        if (_engine.Score > _bestScore)
        {
            _bestScore = _engine.Score;
            _highScoreStore.SaveBestScore(_bestScore);
            _newHighScoreBannerUntilUtc = DateTime.UtcNow.AddMilliseconds(NewHighScoreBannerMs);
            _bestScoreMenuLabel.Text = $"Best Score: {_bestScore}";
        }

        _lastKnownScore = previousScore;

        if (_engine.Phase == GamePhase.GameOver)
        {
            _timer.Stop();
            _pauseStartButton.Text = "Start";
            OnGameOver();
        }

        Invalidate();
    }

    /// <summary>
    /// Starts a new game.
    /// </summary>
    private void StartGame()
    {
        _engine.StartGame();
        BeginGameSession(fromMenu: true);
    }

    /// <summary>
    /// Restarts the current game.
    /// </summary>
    private void RestartGame()
    {
        _engine.RestartGame();
        BeginGameSession(fromMenu: false);
    }

    /// <summary>
    /// Common initialization for game sessions (start or restart).
    /// Extracted to eliminate duplication between StartGame() and RestartGame().
    /// </summary>
    /// <param name="fromMenu">True if starting from menu, false if restarting from game over</param>
    private void BeginGameSession(bool fromMenu)
    {
        _timer.Interval = GetTickInterval(GetSelectedDifficulty());
        BeginCountdown();
        _lastKnownScore = 0;

        _gameOverPanel.Visible = false;
        _pauseStartButton.Visible = true;
        _pauseStartButton.Text = "Pause";

        if (fromMenu)
        {
            _menuPanel.Visible = false;
            ActiveControl = null;
        }

        Focus();
        Invalidate();
    }

    /// <summary>
    /// Toggles pause state or starts the game.
    /// </summary>
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

    /// <summary>
    /// Shows the main menu.
    /// </summary>
    private void ShowMenu()
    {
        _timer.Stop();
        _isCountdownActive = false;
        _pauseStartButton.Visible = false;
        _menuPanel.Visible = true;
        _gameOverPanel.Visible = false;
        _bestScoreMenuLabel.Text = $"Best Score: {_bestScore}";
        Invalidate();
    }

    /// <summary>
    /// Called when the game is over.
    /// </summary>
    private void OnGameOver()
    {
        _gameOverScoreLabel.Text = $"Score: {_engine.Score}";
        _gameOverBestLabel.Text = $"Best: {_bestScore}";

        _pauseStartButton.Visible = false;
        _gameOverPanel.Visible = true;
        _menuPanel.Visible = false;
    }

    /// <summary>
    /// Synchronizes timer and button state based on game phase.
    /// </summary>
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

    /// <summary>
    /// Handles keyboard input during gameplay.
    /// </summary>
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

        if (_isCountdownActive)
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

    /// <summary>
    /// Begins the countdown before the game starts.
    /// </summary>
    private void BeginCountdown()
    {
        _timer.Stop();
        _isCountdownActive = true;
        _countdownEndUtc = DateTime.UtcNow.AddSeconds(CountdownSeconds);
    }

    /// <summary>
    /// Advances animation effects.
    /// </summary>
    private void AdvanceEffects()
    {
        var now = DateTime.UtcNow;
        var shouldInvalidate = false;

        if (_isCountdownActive)
        {
            shouldInvalidate = true;

            if (now >= _countdownEndUtc)
            {
                _isCountdownActive = false;
                if (_engine.Phase == GamePhase.Running)
                {
                    _timer.Start();
                }
            }
        }

        if (now < _foodPulseUntilUtc || now < _newHighScoreBannerUntilUtc)
        {
            shouldInvalidate = true;
        }

        if (shouldInvalidate)
        {
            Invalidate();
        }
    }

    /// <summary>
    /// Gets the difficulty level selected by the user.
    /// </summary>
    private DifficultyLevel GetSelectedDifficulty()
    {
        var selected = _difficultyComboBox.SelectedItem?.ToString();
        return Enum.TryParse<DifficultyLevel>(selected, out var level)
            ? level
            : DifficultyLevel.Normal;
    }

    /// <summary>
    /// Gets the tick interval for a given difficulty level.
    /// </summary>
    private static int GetTickInterval(DifficultyLevel level) => level switch
    {
        DifficultyLevel.Easy => 130,
        DifficultyLevel.Hard => 70,
        _ => 95
    };
}
