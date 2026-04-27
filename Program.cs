using System.Windows.Forms;

namespace SnakeGame;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var settings = new GameSettings();
        IRandomProvider randomProvider = new SystemRandomProvider();
        IFoodSpawner foodSpawner = new FoodSpawner(randomProvider);
        ISnakeGameEngine engine = new SnakeGameEngine(settings, foodSpawner);
        ILogger logger = new DebugLogger();
        var dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SnakeGame",
            "highscore.json");
        IHighScoreStore highScoreStore = new FileHighScoreStore(dataPath, logger);

        Application.Run(new SnakeForm(engine, settings, highScoreStore));
    }
}
