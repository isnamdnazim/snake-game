using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SnakeGame;

/// <summary>
/// Partial class containing rendering and drawing methods.
/// </summary>
internal sealed partial class SnakeForm
{
    /// <summary>
    /// Draws the food on the game board.
    /// </summary>
    private void DrawFood(Graphics g, DateTime now)
    {
        if (_engine.Phase == GamePhase.NotStarted)
        {
            return;
        }

        var pulseFactor = GetFoodPulseFactor(now);
        var inset = Math.Max(3, 6 - (int)Math.Round(pulseFactor * 2));
        var rect = CellBounds(_engine.Food, inset);
        var color = pulseFactor > 0 ? UiColors.Food.Pulsing : UiColors.Food.Resting;
        using var foodBrush = new SolidBrush(color);
        g.FillEllipse(foodBrush, rect);
    }

    /// <summary>
    /// Draws the snake on the game board.
    /// </summary>
    private void DrawSnake(Graphics g, DateTime now)
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

        using (var bodyPen = new Pen(UiColors.Snake.Body, _settings.CellSize * UiConstants.Drawing.SnakeBodyPenWidth)
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
            using var brush = new SolidBrush(UiColors.Snake.Body);
            g.FillEllipse(brush, rect);
        }

        DrawHead(g, segments, GetFoodPulseFactor(now));
    }

    /// <summary>
    /// Draws the snake's head with eyes.
    /// </summary>
    private void DrawHead(Graphics g, IReadOnlyList<GridPoint> segments, float pulseFactor)
    {
        var head = segments[0];
        var inset = Math.Max(1, 2 - (int)Math.Round(pulseFactor));
        var rect = CellBounds(head, inset);

        var headColor = pulseFactor > 0 ? UiColors.Snake.HeadPulsing : UiColors.Snake.Head;

        using (var headBrush = new SolidBrush(headColor))
        {
            g.FillEllipse(headBrush, rect);
        }

        using (var outlinePen = new Pen(UiColors.Snake.HeadOutline, UiConstants.Drawing.EyePenWidth))
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

        using var eyeBrush = new SolidBrush(UiColors.Snake.Eye);
        g.FillEllipse(eyeBrush, eye1.X - eyeRadius, eye1.Y - eyeRadius, eyeRadius * 2, eyeRadius * 2);
        g.FillEllipse(eyeBrush, eye2.X - eyeRadius, eye2.Y - eyeRadius, eyeRadius * 2, eyeRadius * 2);
    }

    /// <summary>
    /// Gets the direction the snake's head is facing based on its segments.
    /// </summary>
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

    /// <summary>
    /// Draws the HUD (heads-up display) with score and difficulty.
    /// </summary>
    private void DrawHud(Graphics g, int yStart, DateTime now)
    {
        using var scoreBrush = new SolidBrush(UiColors.Ui.Text);
        using var scoreFont = new Font("Segoe UI", 11, FontStyle.Bold);
        g.DrawString($"Score: {_engine.Score}", scoreFont, scoreBrush, new PointF(10, yStart + 10));

        using var metaFont = new Font("Segoe UI", 9);
        using var metaBrush = new SolidBrush(UiColors.Ui.TextSecondary);
        var difficulty = GetSelectedDifficulty();
        g.DrawString($"Difficulty: {difficulty}    Best: {_bestScore}", metaFont, metaBrush, new PointF(10, yStart + 33));

        if (_engine.Score > _lastKnownScore && now < _foodPulseUntilUtc)
        {
            using var plusFont = new Font("Segoe UI", 10, FontStyle.Bold);
            using var plusBrush = new SolidBrush(Color.FromArgb(255, 255, 220, 90));
            g.DrawString($"+{_engine.Score - _lastKnownScore}", plusFont, plusBrush, new PointF(110, yStart + 10));
        }
    }

    /// <summary>
    /// Draws a semi-transparent overlay with text (e.g., "Paused").
    /// </summary>
    private void DrawOverlay(Graphics g, string text)
    {
        var boardWidthPx = _settings.GridWidth * _settings.CellSize;
        var boardHeightPx = _settings.GridHeight * _settings.CellSize;
        using var overlayBrush = new SolidBrush(Color.FromArgb(110, 0, 0, 0));
        g.FillRectangle(overlayBrush, new Rectangle(0, 0, boardWidthPx, boardHeightPx));

        using var font = new Font("Segoe UI", 24, FontStyle.Bold);
        using var textBrush = new SolidBrush(UiColors.Ui.Text);
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, textBrush, (boardWidthPx - size.Width) / 2, (boardHeightPx - size.Height) / 2);
    }

    /// <summary>
    /// Draws the "New High Score!" banner.
    /// </summary>
    private void DrawNewHighScoreBanner(Graphics g)
    {
        var boardWidthPx = _settings.GridWidth * _settings.CellSize;
        var bannerRect = new Rectangle(
            (boardWidthPx - UiConstants.HighScoreBanner.Width) / 2,
            UiConstants.HighScoreBanner.OffsetTopY,
            UiConstants.HighScoreBanner.Width,
            UiConstants.HighScoreBanner.Height);

        using var bgBrush = new SolidBrush(UiColors.Ui.HighScoreBanner);
        using var textBrush = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI", UiConstants.HighScoreBanner.FontSize, FontStyle.Bold);

        g.FillRoundedRectangle(bgBrush, bannerRect, UiConstants.HighScoreBanner.BorderRadius);
        g.DrawString("New High Score!", font, textBrush, new PointF(bannerRect.X + 52, bannerRect.Y + 9));
    }

    /// <summary>
    /// Gets the pulse animation factor for the current time.
    /// </summary>
    private float GetFoodPulseFactor(DateTime now)
    {
        if (now >= _foodPulseUntilUtc)
        {
            return 0f;
        }

        var elapsed = FoodPulseDurationMs - (_foodPulseUntilUtc - now).TotalMilliseconds;
        var t = Math.Clamp((float)(elapsed / FoodPulseDurationMs), 0f, 1f);
        return (float)Math.Sin(t * Math.PI);
    }

    /// <summary>
    /// Gets the text to display during countdown.
    /// </summary>
    private string GetCountdownText(DateTime now)
    {
        var remaining = (_countdownEndUtc - now).TotalSeconds;
        if (remaining <= 0)
        {
            return "Go!";
        }

        var seconds = Math.Max(1, (int)Math.Ceiling(remaining));
        return seconds.ToString();
    }

    /// <summary>
    /// Gets the bounding rectangle for a grid cell.
    /// </summary>
    private Rectangle CellBounds(GridPoint point, int inset)
    {
        var x = point.X * _settings.CellSize + inset;
        var y = point.Y * _settings.CellSize + inset;
        var size = _settings.CellSize - inset * 2;
        return new Rectangle(x, y, size, size);
    }

    /// <summary>
    /// Gets the center point of a grid cell.
    /// </summary>
    private PointF CellCenter(GridPoint point)
    {
        var x = point.X * _settings.CellSize + _settings.CellSize / 2f;
        var y = point.Y * _settings.CellSize + _settings.CellSize / 2f;
        return new PointF(x, y);
    }
}
