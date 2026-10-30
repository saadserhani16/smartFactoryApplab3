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

// Where a brick is in its "life"
public enum BrickStatus
{
    onLine,          // waits on a production line or on the recycle line (can be docked)
    docked,          // held by the robot
    onDeliveryLine,  // just undocked on the delivery line, quality is checked
    toCustomer,      // good brick, moves to the right
    toRecycle,       // defective brick, moves to the left
    repairing        // on the recycle line, is being repaired (cannot be docked yet)
}

// Result of pressing the Undock button
public enum UndockResult
{
    success,
    noBrick,
    notOnDeliveryLine
}

public class Parameters
{
    public const int step = 10;               // pixels per click in Step mode
    public const int rotationStep = 15;       // degrees per click in Step mode
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

    public const int dockDistance = 10;       // gripper must be this close (pixels) to grab a brick

    public const int deliverySpeed = 3;       // pixels per tick for bricks that leave the delivery line
    public const int deliveryWaitTicks = 25;  // a brick rests this long on the delivery line (25 x 40 ms = 1 s)
    public const int repairTicks = 60;        // repair time on the recycle line (60 x 40 ms = 2.4 s)
}