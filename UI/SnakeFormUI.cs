using System.Windows.Forms;

namespace SnakeGame;

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

    /// <summary>
    /// Creates the main menu panel with difficulty selection and start button.
    /// </summary>
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

    /// <summary>
    /// Creates the game over panel with restart and menu buttons.
    /// </summary>
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
}
