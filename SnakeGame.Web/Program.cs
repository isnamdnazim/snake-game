using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SnakeGame;
using SnakeGame.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<GameSettings>();
builder.Services.AddSingleton<IRandomProvider, SystemRandomProvider>();
builder.Services.AddSingleton<IFoodSpawner, FoodSpawner>();
builder.Services.AddSingleton<ISnakeGameEngine>(sp => new SnakeGameEngine(sp.GetRequiredService<GameSettings>(), sp.GetRequiredService<IFoodSpawner>()));
builder.Services.AddSingleton<IHighScoreStore, FileHighScoreStore>();

await builder.Build().RunAsync();
