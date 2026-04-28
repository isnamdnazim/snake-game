namespace SnakeGame;

public sealed class FoodSpawner(IRandomProvider randomProvider) : IFoodSpawner
{
    private readonly IRandomProvider _randomProvider = randomProvider;

    public GridPoint Spawn(int width, int height, IReadOnlySet<GridPoint> occupiedCells)
    {
        if (occupiedCells.Count >= width * height)
        {
            return new GridPoint(-1, -1);
        }

        GridPoint point;
        do
        {
            point = new GridPoint(_randomProvider.Next(width), _randomProvider.Next(height));
        }
        while (occupiedCells.Contains(point));

        return point;
    }
}
