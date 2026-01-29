namespace PolarityGrid.Core
{
    public enum BlockType
    {
        Empty = 0,
        Positive = 1,
        Negative = 2,
        Obstacle = 3
    }

    public enum Direction
    {
        Up, Down, Left, Right
    }
    public enum GameState
    {
        MainMenu , Playing , LevelWon , LevelLost
    }
}