using System.Windows.Forms;
using System.Runtime.Versioning;

namespace SnakeGame;

internal static class Program
{
    [STAThread]
    [SupportedOSPlatform("windows")]
    private static void Main()
    {
        var settings = new GameSettings();
        IRandomProvider randomProvider = new SystemRandomProvider();
        IFoodSpawner foodSpawner = new FoodSpawner(randomProvider);
        ISnakeGameEngine engine = new SnakeGameEngine(settings, foodSpawner);

        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SnakeGame");
        var dataPath = Path.Combine(appDataDirectory, "highscore.json");
        var logDirectoryPath = Path.Combine(appDataDirectory, "logs");

        ILogger logger = new CompositeLogger(
            new DebugLogger(),
            new FileLogger(logDirectoryPath));

        Application.ThreadException += (_, exArgs) =>
            logger.Error("Unhandled UI thread exception", exArgs.Exception);

        AppDomain.CurrentDomain.UnhandledException += (_, exArgs) =>
        {
            if (exArgs.ExceptionObject is Exception ex)
            {
                logger.Error("Unhandled non-UI exception", ex);
                return;
            }

            logger.Warn("Unhandled non-UI exception of unknown type");
        };

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        IHighScoreStore highScoreStore = new FileHighScoreStore(dataPath, logger);

        logger.Info("Starting Snake Game");

        try
        {
            Application.Run(new SnakeForm(engine, settings, highScoreStore));
            logger.Info("Snake Game closed normally");
        }
        catch (Exception ex)
        {
            logger.Error("Fatal error", ex);
        }
    }
}
