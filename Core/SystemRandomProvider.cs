namespace SnakeGame;

public sealed class SystemRandomProvider : IRandomProvider
{
    private readonly Random _random = new();

    public int Next(int maxValue) => _random.Next(maxValue);
}
