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

        Application.Run(new SnakeForm(engine, settings));
    }
}
