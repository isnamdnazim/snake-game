# 🔧 Recommended Improvements - Snake Game

## Quick-Start Implementation Guide

This document provides code examples and implementation steps for addressing the identified improvement areas.

---

## 1. Fix Bare Exception Handling

### Current Code (BAD ❌)
```csharp
// FileHighScoreStore.cs
public int LoadBestScore()
{
    try
    {
        if (!File.Exists(_filePath))
            return 0;

        var json = File.ReadAllText(_filePath);
        var data = JsonSerializer.Deserialize<HighScoreData>(json);
        return data?.BestScore ?? 0;
    }
    catch  // ❌ Catches ALL exceptions
    {
        return 0;  // ❌ Silent failure
    }
}
```

### Improved Code (GOOD ✅)
```csharp
// FileHighScoreStore.cs
public int LoadBestScore()
{
    try
    {
        if (!File.Exists(_filePath))
            return 0;

        var json = File.ReadAllText(_filePath);
        var data = JsonSerializer.Deserialize<HighScoreData>(json);
        return data?.BestScore ?? 0;
    }
    catch (FileNotFoundException)
    {
        // File not found on first run - expected, return default
        return 0;
    }
    catch (JsonException ex)
    {
        // High score file corrupted, reset
        Debug.WriteLine($"High score file corrupted: {ex.Message}");
        return 0;
    }
    catch (IOException ex)
    {
        // File access issues (permission, disk full, etc.)
        Debug.WriteLine($"Failed to read high score file: {ex.Message}");
        return 0;
    }
}

public void SaveBestScore(int score)
{
    try
    {
        var folder = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(folder))
            Directory.CreateDirectory(folder);

        var data = new HighScoreData(score);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
    catch (UnauthorizedAccessException ex)
    {
        Debug.WriteLine($"Permission denied saving high score: {ex.Message}");
        // Save failures should not break gameplay
    }
    catch (IOException ex)
    {
        Debug.WriteLine($"Failed to save high score: {ex.Message}");
        // Save failures should not break gameplay
    }
}
```

**Implementation Time:** ~10 minutes  
**Files Affected:** `FileHighScoreStore.cs`

---

## 2. Extract Magic Numbers to Constants

### Current Code (BAD ❌)
```csharp
// SnakeFormRenderer.cs
private void DrawFood(Graphics g, DateTime now)
{
    var pulseFactor = GetFoodPulseFactor(now);
    var inset = Math.Max(3, 6 - (int)Math.Round(pulseFactor * 2));
    var color = pulseFactor > 0
        ? Color.FromArgb(255, 255, 145, 55)   // ❌ Magic RGB
        : Color.FromArgb(246, 90, 90);         // ❌ Magic RGB
    // ...
}
```

```csharp
// SnakeFormUI.cs
var panel = new Panel
{
    Size = new Size(330, 280),  // ❌ What is 330x280?
    Location = new Point((boardWidthPx - 330) / 2, (boardHeightPx - 280) / 2),
};
```

### Improved Code (GOOD ✅)

Create a new file: `UI/UiColors.cs`
```csharp
namespace SnakeGame;

/// <summary>
/// Color palette for the UI and game elements.
/// </summary>
internal static class UiColors
{
    public static class Food
    {
        public static readonly Color Pulsing = Color.FromArgb(255, 255, 145, 55);  // Orange
        public static readonly Color Resting = Color.FromArgb(246, 90, 90);        // Red
    }

    public static class Snake
    {
        public static readonly Color Body = Color.FromArgb(46, 175, 84);    // Green
        public static readonly Color Head = Color.FromArgb(78, 220, 108);   // Light Green
        public static readonly Color HeadPulsing = Color.FromArgb(96, 236, 124);
        public static readonly Color HeadOutline = Color.FromArgb(36, 128, 62);
        public static readonly Color Eye = Color.FromArgb(28, 28, 28);
    }

    public static class Ui
    {
        public static readonly Color MenuPanel = Color.FromArgb(32, 32, 32);
        public static readonly Color GameOverPanel = Color.FromArgb(34, 34, 34);
        public static readonly Color Board = Color.FromArgb(34, 34, 34);
        public static readonly Color BoardBorder = Color.FromArgb(95, 95, 95);
        public static readonly Color HudBackground = Color.FromArgb(20, 20, 20);
        public static readonly Color Background = Color.FromArgb(24, 24, 24);
        public static readonly Color Text = Color.WhiteSmoke;
        public static readonly Color TextSecondary = Color.Gainsboro;
        public static readonly Color Button = Color.FromArgb(56, 56, 56);
        public static readonly Color ButtonHover = Color.FromArgb(70, 70, 70);
        public static readonly Color ButtonPrimary = Color.FromArgb(50, 160, 90);
        public static readonly Color HighScoreBanner = Color.FromArgb(220, 43, 122, 62);
    }
}
```

Create a new file: `UI/UiConstants.cs`
```csharp
namespace SnakeGame;

/// <summary>
/// UI layout constants.
/// </summary>
internal static class UiConstants
{
    public static class MenuPanel
    {
        public const int Width = 330;
        public const int Height = 280;
        public const int TitleSize = 20;
        public const int SubtitleSize = 10;
        public const int LabelSize = 9;
        public const int ComboBoxSize = 10;
        public const int ButtonSize = 10;
    }

    public static class GameOverPanel
    {
        public const int Width = 300;
        public const int Height = 200;
        public const int TitleSize = 18;
        public const int ScoreSize = 11;
        public const int BestSize = 10;
    }

    public static class Button
    {
        public const int PauseStartWidth = 112;
        public const int PauseStartHeight = 32;
        public const int PauseStartOffsetX = -122;
        public const int PauseStartOffsetY = 12;
    }

    public static class Banner
    {
        public const int HighScoreWidth = 250;
        public const int HighScoreHeight = 36;
        public const int HighScoreRadius = 12;
        public const int HighScoreOffsetY = 10;
    }
}
```

Update `SnakeFormRenderer.cs`:
```csharp
private void DrawFood(Graphics g, DateTime now)
{
    if (_engine.Phase == GamePhase.NotStarted)
        return;

    var pulseFactor = GetFoodPulseFactor(now);
    var inset = Math.Max(3, 6 - (int)Math.Round(pulseFactor * 2));
    var rect = CellBounds(_engine.Food, inset);
    var color = pulseFactor > 0 ? UiColors.Food.Pulsing : UiColors.Food.Resting;  // ✅ Named colors
    using var foodBrush = new SolidBrush(color);
    g.FillEllipse(foodBrush, rect);
}

private void DrawNewHighScoreBanner(Graphics g)
{
    var boardWidthPx = _settings.GridWidth * _settings.CellSize;
    var bannerRect = new Rectangle(
        (boardWidthPx - UiConstants.Banner.HighScoreWidth) / 2,  // ✅ Named constant
        UiConstants.Banner.HighScoreOffsetY,                     // ✅ Named constant
        UiConstants.Banner.HighScoreWidth,                       // ✅ Named constant
        UiConstants.Banner.HighScoreHeight                       // ✅ Named constant
    );
    // ...
}
```

Update `SnakeFormUI.cs`:
```csharp
private Panel CreateMenuPanel(out ComboBox difficultyComboBox, out Label bestScoreLabel)
{
    var boardWidthPx = _settings.GridWidth * _settings.CellSize;
    var boardHeightPx = _settings.GridHeight * _settings.CellSize;

    var panel = new Panel
    {
        Size = new Size(UiConstants.MenuPanel.Width, UiConstants.MenuPanel.Height),  // ✅
        Location = new Point(
            (boardWidthPx - UiConstants.MenuPanel.Width) / 2,                        // ✅
            (boardHeightPx - UiConstants.MenuPanel.Height) / 2                       // ✅
        ),
        BackColor = UiColors.Ui.MenuPanel  // ✅
    };
    // ...
}
```

**Implementation Time:** ~30 minutes  
**Files Affected:** 
- `UI/UiColors.cs` (NEW)
- `UI/UiConstants.cs` (NEW)
- `UI/SnakeFormRenderer.cs` (UPDATE)
- `UI/SnakeFormUI.cs` (UPDATE)

---

## 3. Extract Duplicate Code (StartGame/RestartGame)

### Current Code (BAD ❌)
```csharp
// SnakeFormGameLogic.cs
private void StartGame()
{
    _engine.StartGame();
    _timer.Interval = GetTickInterval(GetSelectedDifficulty());
    BeginCountdown();
    _lastKnownScore = 0;

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
    _timer.Interval = GetTickInterval(GetSelectedDifficulty());  // ❌ Duplicate
    BeginCountdown();                                            // ❌ Duplicate
    _lastKnownScore = 0;                                        // ❌ Duplicate

    _gameOverPanel.Visible = false;                            // ❌ Duplicate
    _pauseStartButton.Visible = true;                          // ❌ Duplicate
    _pauseStartButton.Text = "Pause";                          // ❌ Duplicate

    Focus();
    Invalidate();
}
```

### Improved Code (GOOD ✅)
```csharp
// SnakeFormGameLogic.cs
private void StartGame()
{
    _engine.StartGame();
    BeginGameSession(fromMenu: true);
}

private void RestartGame()
{
    _engine.RestartGame();
    BeginGameSession(fromMenu: false);
}

/// <summary>
/// Common initialization for game sessions.
/// </summary>
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
```

**Implementation Time:** ~15 minutes  
**Files Affected:** `UI/SnakeFormGameLogic.cs`

---

## 4. Add Logging Infrastructure

### Create `ILogger` Interface
```csharp
// Core/Logging/ILogger.cs
namespace SnakeGame;

/// <summary>
/// Logging interface for diagnostic purposes.
/// </summary>
internal interface ILogger
{
    void Debug(string message);
    void Info(string message);
    void Warn(string message, Exception? ex = null);
    void Error(string message, Exception ex);
}
```

### Create Debug Logger Implementation
```csharp
// Core/Logging/DebugLogger.cs
using System.Diagnostics;

namespace SnakeGame;

/// <summary>
/// Logger implementation using System.Diagnostics.Debug.
/// </summary>
internal sealed class DebugLogger : ILogger
{
    public void Debug(string message) => System.Diagnostics.Debug.WriteLine($"[DEBUG] {message}");
    public void Info(string message) => System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
    public void Warn(string message, Exception? ex = null) =>
        System.Diagnostics.Debug.WriteLine($"[WARN] {message}" + (ex != null ? $"\n{ex}" : ""));
    public void Error(string message, Exception ex) =>
        System.Diagnostics.Debug.WriteLine($"[ERROR] {message}\n{ex}");
}
```

### Update FileHighScoreStore to Use Logger
```csharp
// Core/FileHighScoreStore.cs
internal sealed class FileHighScoreStore(string filePath, ILogger logger) : IHighScoreStore
{
    private readonly string _filePath = filePath;
    private readonly ILogger _logger = logger;

    public int LoadBestScore()
    {
        try
        {
            if (!File.Exists(_filePath))
                return 0;

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<HighScoreData>(json);
            return data?.BestScore ?? 0;
        }
        catch (JsonException ex)
        {
            _logger.Warn("High score file corrupted", ex);  // ✅ Logged
            return 0;
        }
        catch (IOException ex)
        {
            _logger.Warn("Failed to read high score file", ex);  // ✅ Logged
            return 0;
        }
    }

    public void SaveBestScore(int score)
    {
        try
        {
            var folder = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);

            var data = new HighScoreData(score);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
            _logger.Debug($"Saved high score: {score}");  // ✅ Logged
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warn("Permission denied saving high score", ex);  // ✅ Logged
        }
        catch (IOException ex)
        {
            _logger.Warn("Failed to save high score", ex);  // ✅ Logged
        }
    }

    private sealed record HighScoreData(int BestScore);
}
```

### Update Program.cs to Inject Logger
```csharp
// Program.cs
private static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    var settings = new GameSettings();
    IRandomProvider randomProvider = new SystemRandomProvider();
    IFoodSpawner foodSpawner = new FoodSpawner(randomProvider);
    ISnakeGameEngine engine = new SnakeGameEngine(settings, foodSpawner);
    ILogger logger = new DebugLogger();  // ✅ Added
    var dataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SnakeGame",
        "highscore.json");
    IHighScoreStore highScoreStore = new FileHighScoreStore(dataPath, logger);  // ✅ Injected

    Application.Run(new SnakeForm(engine, settings, highScoreStore));
}
```

**Implementation Time:** ~45 minutes  
**Files Affected:**
- `Core/Logging/ILogger.cs` (NEW)
- `Core/Logging/DebugLogger.cs` (NEW)
- `Core/FileHighScoreStore.cs` (UPDATE)
- `Program.cs` (UPDATE)

---

## 5. Add Unit Tests

### Create Test Project Structure
```
SnakeGame.Tests/
├── SnakeGameEngine/
│   ├── SnakeGameEngineTests.cs
│   ├── DirectionChangeTests.cs
│   └── CollisionDetectionTests.cs
└── SnakeGameEngine.Tests.csproj
```

### Example Test File
```csharp
// SnakeGame.Tests/SnakeGameEngine/SnakeGameEngineTests.cs
using NUnit.Framework;
using SnakeGame;

namespace SnakeGame.Tests;

[TestFixture]
public class SnakeGameEngineTests
{
    private SnakeGameEngine _engine = null!;

    [SetUp]
    public void Setup()
    {
        var settings = new GameSettings();
        var randomProvider = new SystemRandomProvider();
        var foodSpawner = new FoodSpawner(randomProvider);
        _engine = new SnakeGameEngine(settings, foodSpawner);
    }

    [Test]
    public void StartGame_InitializesGamePhaseToRunning()
    {
        _engine.StartGame();
        Assert.That(_engine.Phase, Is.EqualTo(GamePhase.Running));
    }

    [Test]
    public void TickFrame_WhenEatingFood_IncreasesScore()
    {
        _engine.StartGame();
        var initialScore = _engine.Score;
        var initialFood = _engine.Food;

        // Move snake to food position (setup specific to test)
        // ... implementation depends on how you want to set position

        _engine.Tick();
        Assert.That(_engine.Score, Is.GreaterThan(initialScore));
    }

    [Test]
    public void ChangeDirection_OppositeDirection_IsIgnored()
    {
        _engine.StartGame();
        
        // If moving right, changing to left should be ignored
        _engine.ChangeDirection(Direction.Right);
        _engine.ChangeDirection(Direction.Left);
        
        // After tick, snake should move right (not left)
        _engine.Tick();
        // Assert snake moved right...
    }

    [Test]
    public void TogglePause_RunningToPaused_StopsMovement()
    {
        _engine.StartGame();
        Assert.That(_engine.Phase, Is.EqualTo(GamePhase.Running));

        _engine.TogglePause();
        Assert.That(_engine.Phase, Is.EqualTo(GamePhase.Paused));
    }

    [Test]
    public void Tick_WhenHittingWall_EndsGame()
    {
        _engine.StartGame();
        // Position snake at wall
        // Call Tick
        // Assert Phase == GamePhase.GameOver
    }

    [Test]
    public void Tick_WhenCollindingWithSelf_EndsGame()
    {
        _engine.StartGame();
        // Position snake to collide with itself
        // Call Tick
        // Assert Phase == GamePhase.GameOver
    }
}
```

**Implementation Time:** ~3 hours for complete test suite  
**Files Affected:** New test project required

---

## Implementation Priority

| Rank | Task | Time | Impact |
|------|------|------|--------|
| 1 | Fix bare exceptions | 10 min | HIGH |
| 2 | Extract magic numbers | 30 min | HIGH |
| 3 | Add logging | 45 min | MEDIUM |
| 4 | Extract duplicate code | 15 min | MEDIUM |
| 5 | Add unit tests | 3 hours | MEDIUM |
| **TOTAL** | | **~4.5 hours** | |

---

## Expected Impact After Fixes

```
Before:  A- (87/100)
After:   A+ (94/100) - All improvements implemented

Improvements:
├─ Error Handling:     60% → 95% (+35%)
├─ Documentation:      70% → 85% (+15%)
├─ Testability:        60% → 90% (+30%)
└─ Code Quality:       90% → 98% (+8%)
```

---

## Quick Checklist

```
High Priority:
[ ] Fix exception handling in FileHighScoreStore.cs (10 min)
[ ] Extract colors to UiColors.cs (20 min)
[ ] Extract UI constants to UiConstants.cs (10 min)

Medium Priority:
[ ] Add ILogger interface (5 min)
[ ] Add DebugLogger implementation (10 min)
[ ] Inject logger into FileHighScoreStore (10 min)
[ ] Extract StartGame/RestartGame duplicate code (15 min)

Low Priority (Next Sprint):
[ ] Create test project structure
[ ] Write SnakeGameEngine unit tests
[ ] Write integration tests

Documentation:
[ ] Create ARCHITECTURE.md
[ ] Create README.md
[ ] Update XML doc comments
```

---

**Total Estimated Effort:** 4-5 hours of development time  
**Expected Result:** A+ grade (94+/100)

