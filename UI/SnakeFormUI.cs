using System.Windows.Forms;
using System.Runtime.Versioning;

namespace SnakeGame;

[SupportedOSPlatform("windows")]
/// <summary>
/// Partial class containing UI initialization and panel creation methods.
/// </summary>
internal sealed partial class SnakeForm
{
    /// <summary>
    /// Creates the pause/start button.
    /// </summary>
    private Button CreatePauseStartButton()
    {
        var button = new Button
        {
            Text = "Pause",
            Size = new Size(UiConstants.PauseButton.Width, UiConstants.PauseButton.Height),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = UiColors.Ui.ButtonNeutral,
            ForeColor = UiColors.Text.Primary,
            Location = new Point(
                _settings.GridWidth * _settings.CellSize - UiConstants.PauseButton.RightOffset,
                _settings.GridHeight * _settings.CellSize + UiConstants.PauseButton.TopOffset),
            Visible = false,
            TabStop = false
        };

        button.FlatAppearance.BorderSize = UiConstants.PauseButton.BorderSize;
        button.Click += (_, _) => TogglePauseStart();
        return button;
    }

    /// <summary>
    /// Creates the main menu panel with difficulty selection and start button.
    /// </summary>
    private Panel CreateMenuPanel(out ComboBox difficultyComboBox, out Label bestScoreLabel)
    {
        var boardWidthPx = _settings.GridWidth * _settings.CellSize;
        var boardHeightPx = _settings.GridHeight * _settings.CellSize;

        var panel = new Panel
        {
            Size = new Size(UiConstants.MenuPanel.Width, UiConstants.MenuPanel.Height),
            Location = new Point(
                (boardWidthPx - UiConstants.MenuPanel.Width) / 2,
                (boardHeightPx - UiConstants.MenuPanel.Height) / 2),
            BackColor = UiColors.Ui.MenuPanel
        };

        var title = new Label
        {
            Text = "Snake Game",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = UiColors.Text.Secondary,
            AutoSize = false,
            Width = panel.Width,
            Height = UiConstants.MenuPanel.TitleHeight,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = UiConstants.MenuPanel.TitleTop
        };

        var subtitle = new Label
        {
            Text = "Pick difficulty and start",
            Font = new Font("Segoe UI", 10),
            ForeColor = UiColors.Text.Muted,
            AutoSize = false,
            Width = panel.Width,
            Height = UiConstants.MenuPanel.SubtitleHeight,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = UiConstants.MenuPanel.SubtitleTop
        };

        var difficultyLabel = new Label
        {
            Text = "Difficulty",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = UiColors.Text.Primary,
            AutoSize = true,
            Left = UiConstants.MenuPanel.HorizontalPadding,
            Top = UiConstants.MenuPanel.DifficultyLabelTop
        };

        difficultyComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Left = UiConstants.MenuPanel.HorizontalPadding,
            Top = UiConstants.MenuPanel.DifficultyComboTop,
            Width = panel.Width - UiConstants.MenuPanel.HorizontalPadding * 2,
            Font = new Font("Segoe UI", 10)
        };

        difficultyComboBox.Items.AddRange(Enum.GetNames(typeof(DifficultyLevel)));
        difficultyComboBox.SelectedItem = DifficultyLevel.Normal.ToString();

        var startButton = new Button
        {
            Text = "Start Game",
            Size = new Size(panel.Width - UiConstants.MenuPanel.HorizontalPadding * 2, UiConstants.MenuPanel.StartButtonHeight),
            Left = UiConstants.MenuPanel.HorizontalPadding,
            Top = UiConstants.MenuPanel.StartButtonTop,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = UiColors.Ui.ButtonPrimary,
            ForeColor = UiColors.Text.Primary,
            TabStop = false
        };
        startButton.FlatAppearance.BorderSize = UiConstants.PauseButton.BorderSize;
        startButton.Click += (_, _) => StartGame();

        var exitButton = new Button
        {
            Text = "Exit",
            Size = new Size(panel.Width - UiConstants.MenuPanel.HorizontalPadding * 2, UiConstants.MenuPanel.ExitButtonHeight),
            Left = UiConstants.MenuPanel.HorizontalPadding,
            Top = UiConstants.MenuPanel.ExitButtonTop,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = UiColors.Ui.ButtonSecondary,
            ForeColor = UiColors.Text.Primary,
            TabStop = false
        };
        exitButton.FlatAppearance.BorderSize = UiConstants.PauseButton.BorderSize;
        exitButton.Click += (_, _) => Close();

        bestScoreLabel = new Label
        {
            Text = $"Best Score: {_bestScore}",
            Font = new Font("Segoe UI", 9),
            ForeColor = UiColors.Text.Muted,
            AutoSize = false,
            Width = panel.Width,
            Height = UiConstants.MenuPanel.BestScoreHeight,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = UiConstants.MenuPanel.BestScoreTop
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

    /// <summary>
    /// Creates the game over panel with restart and menu buttons.
    /// </summary>
    private Panel CreateGameOverPanel(out Label scoreLabel, out Label bestLabel)
    {
        var boardWidthPx = _settings.GridWidth * _settings.CellSize;
        var boardHeightPx = _settings.GridHeight * _settings.CellSize;

        var panel = new Panel
        {
            Size = new Size(UiConstants.GameOverPanel.Width, UiConstants.GameOverPanel.Height),
            Location = new Point(
                (boardWidthPx - UiConstants.GameOverPanel.Width) / 2,
                (boardHeightPx - UiConstants.GameOverPanel.Height) / 2),
            BackColor = UiColors.Ui.GameOverPanel,
            Visible = false
        };

        var title = new Label
        {
            Text = "Game Over",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = UiColors.Text.Secondary,
            AutoSize = false,
            Width = panel.Width,
            Height = UiConstants.GameOverPanel.TitleHeight,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = UiConstants.GameOverPanel.TitleTop
        };

        scoreLabel = new Label
        {
            Text = "Score: 0",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = UiColors.Text.Primary,
            AutoSize = false,
            Width = panel.Width,
            Height = UiConstants.GameOverPanel.ScoreHeight,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = UiConstants.GameOverPanel.ScoreTop
        };

        bestLabel = new Label
        {
            Text = "Best: 0",
            Font = new Font("Segoe UI", 10),
            ForeColor = UiColors.Text.Muted,
            AutoSize = false,
            Width = panel.Width,
            Height = UiConstants.GameOverPanel.BestHeight,
            TextAlign = ContentAlignment.MiddleCenter,
            Top = UiConstants.GameOverPanel.BestTop
        };

        var restartButton = new Button
        {
            Text = "Play Again",
            Size = new Size(panel.Width - UiConstants.GameOverPanel.ButtonHorizontalPadding, UiConstants.GameOverPanel.RestartHeight),
            Left = UiConstants.GameOverPanel.ButtonLeft,
            Top = UiConstants.GameOverPanel.RestartTop,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = UiColors.Ui.ButtonPrimary,
            ForeColor = UiColors.Text.Primary,
            TabStop = false
        };
        restartButton.FlatAppearance.BorderSize = UiConstants.PauseButton.BorderSize;
        restartButton.Click += (_, _) => RestartGame();

        var menuButton = new Button
        {
            Text = "Main Menu",
            Size = new Size(panel.Width - UiConstants.GameOverPanel.ButtonHorizontalPadding, UiConstants.GameOverPanel.MenuHeight),
            Left = UiConstants.GameOverPanel.ButtonLeft,
            Top = UiConstants.GameOverPanel.MenuTop,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = UiColors.Ui.ButtonSecondary,
            ForeColor = UiColors.Text.Primary,
            TabStop = false
        };
        menuButton.FlatAppearance.BorderSize = UiConstants.PauseButton.BorderSize;
        menuButton.Click += (_, _) => ShowMenu();

        panel.Controls.Add(title);
        panel.Controls.Add(scoreLabel);
        panel.Controls.Add(bestLabel);
        panel.Controls.Add(restartButton);
        panel.Controls.Add(menuButton);

        return panel;
    }
}

