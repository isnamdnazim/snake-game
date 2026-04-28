namespace SnakeGame;

public interface IFoodSpawner
{
    GridPoint Spawn(int width, int height, IReadOnlySet<GridPoint> occupiedCells);
}
