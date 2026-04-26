namespace SnakeGame;

internal interface IHighScoreStore
{
    int LoadBestScore();
    void SaveBestScore(int score);
}
