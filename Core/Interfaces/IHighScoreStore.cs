namespace SnakeGame;

public interface IHighScoreStore
{
    int LoadBestScore();
    void SaveBestScore(int score);
}
