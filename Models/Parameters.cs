namespace smartFactoryApp;

public enum Direction
{
    stop,
    left,
    right,
    up,
    down,
    upLeft,
    upRight,
    downLeft,
    downRight
}

public enum LegoColor
{
    red,
    green,
    blue,
    yellow
}

public enum LegoSize
{
    small,
    large
}

public class Parameters
{
    public const int step = 10;               // pixels per click in Step mode
    public const int rotationStep = 5;       // degrees per click in Step mode
    public const int continuousStep = 4;      // pixels per tick in Move mode
    public const int continuousRotation = 2;  // degrees per tick in Move mode
    public const int robotTimerInterval = 20; // ms
    public const int brickTimerInterval = 40; // ms

    public const int bricksPerLine = 4;
    public const int brickSpacing = 40;
    public const int brickLong = 20;
    public const int brickShort = 12;
    public const int brickSpeed = 1;

    public const int robotWidth = 30;
    public const int robotHeight = 65;
}