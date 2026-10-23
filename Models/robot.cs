using System.Collections.Generic;
using System.Drawing;

namespace smartFactoryApp;

public class Robot : Beweger
{
    private Direction robotDirection = Direction.stop;
    private int rotationDirection = 0;

    public Robot(double startX, double startY, Rectangle borders, List<Rectangle> obstacles)
        : base(startX, startY, Parameters.robotWidth, Parameters.robotHeight, borders, obstacles)
    {
    }

    public Direction RobotDirection
    {
        get { return robotDirection; }
    }

    public int RotationDirection
    {
        get { return rotationDirection; }
    }

    public void SetzeRichtung(Direction direction)
    {
        robotDirection = direction;
    }

    public void SetzeDrehrichtung(int direction)
    {
        rotationDirection = direction;
    }

    public void Stoppen()
    {
        robotDirection = Direction.stop;
        rotationDirection = 0;
    }

    // One tick of the continuous movement
    public void WeiterBewegen()
    {
        if (rotationDirection != 0)
        {
            Drehen(rotationDirection * Parameters.continuousRotation);
        }
        if (robotDirection != Direction.stop)
        {
            Bewegen(robotDirection, Parameters.continuousStep);
        }
    }
}