using System;
using System.Collections.Generic;
using System.Drawing;

namespace smartFactoryApp;

public class Beweger
{
    private double angle;
    private Rectangle borders;
    private double x;
    private double y;
    private int width;
    private int height;
    private List<Rectangle> obstacles;
    private bool kontakt;

    public Beweger(double startX, double startY, int width, int height, Rectangle borders, List<Rectangle> obstacles)
    {
        this.x = startX;
        this.y = startY;
        this.width = width;
        this.height = height;
        this.borders = borders;
        this.obstacles = obstacles;
        this.angle = 0;
        this.kontakt = false;
    }

    public double X
    {
        get { return x; }
    }

    public double Y
    {
        get { return y; }
    }

    public double Angle
    {
        get { return angle; }
    }

    public Rectangle Borders
    {
        get { return borders; }
    }

    // True if the last attempted move was blocked
    public bool Kontakt
    {
        get { return kontakt; }
    }

    public void Bewegen(Direction direction)
    {
        Bewegen(direction, Parameters.step);
    }

    public void Bewegen(Direction direction, int stepSize)
    {
        double dx = 0;
        double dy = 0;

        if (direction == Direction.left || direction == Direction.upLeft || direction == Direction.downLeft)
            dx = -stepSize;
        if (direction == Direction.right || direction == Direction.upRight || direction == Direction.downRight)
            dx = stepSize;
        if (direction == Direction.up || direction == Direction.upLeft || direction == Direction.upRight)
            dy = -stepSize;
        if (direction == Direction.down || direction == Direction.downLeft || direction == Direction.downRight)
            dy = stepSize;

        VersucheZuSetzen(x + dx, y + dy, angle);

        // Diagonal move blocked: try to slide along the obstacle
        if (kontakt && dx != 0 && dy != 0)
        {
            VersucheZuSetzen(x + dx, y, angle);
            if (kontakt)
            {
                VersucheZuSetzen(x, y + dy, angle);
            }
        }
    }

    public void Drehen(int degrees)
    {
        double newAngle = ((angle + degrees) % 360 + 360) % 360;
        VersucheZuSetzen(x, y, newAngle);
    }

    public void ResetRotation()
    {
        VersucheZuSetzen(x, y, 0);
    }

    // Applies the new pose only if there is no collision
    private void VersucheZuSetzen(double newX, double newY, double newAngle)
    {
        PruefeKollision(newX, newY, newAngle);
        if (!kontakt)
        {
            x = newX;
            y = newY;
            angle = newAngle;
        }
    }

    // Sets the variable kontakt to true if the robot at this pose
    // leaves the factory or touches one of the production lines
    private void PruefeKollision(double testX, double testY, double testAngle)
    {
        double radians = testAngle * Math.PI / 180.0;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);
        double centerX = testX + width / 2.0;
        double centerY = testY + height / 2.0;

        double[] localX = { -width / 2.0, width / 2.0, width / 2.0, -width / 2.0 };
        double[] localY = { -height / 2.0, -height / 2.0, height / 2.0, height / 2.0 };

        double minX = double.MaxValue;
        double maxX = double.MinValue;
        double minY = double.MaxValue;
        double maxY = double.MinValue;

        for (int i = 0; i < 4; i++)
        {
            double cornerX = centerX + localX[i] * cos - localY[i] * sin;
            double cornerY = centerY + localX[i] * sin + localY[i] * cos;
            if (cornerX < minX) minX = cornerX;
            if (cornerX > maxX) maxX = cornerX;
            if (cornerY < minY) minY = cornerY;
            if (cornerY > maxY) maxY = cornerY;
        }

        kontakt = false;

        // 1. Factory border
        if (minX < borders.Left || maxX > borders.Right || minY < borders.Top || maxY > borders.Bottom)
        {
            kontakt = true;
        }

        // 2. Production lines
        double[] axisX = { cos, -sin };
        double[] axisY = { sin, cos };
        double[] radius = { width / 2.0, height / 2.0 };

        foreach (Rectangle box in obstacles)
        {
            bool separated = false;

            if (maxX <= box.Left || minX >= box.Right) separated = true;
            if (maxY <= box.Top || minY >= box.Bottom) separated = true;

            double[] boxCornerX = { box.Left, box.Right, box.Right, box.Left };
            double[] boxCornerY = { box.Top, box.Top, box.Bottom, box.Bottom };

            for (int a = 0; a < 2; a++)
            {
                double robotCenter = centerX * axisX[a] + centerY * axisY[a];
                double boxMin = double.MaxValue;
                double boxMax = double.MinValue;

                for (int c = 0; c < 4; c++)
                {
                    double p = boxCornerX[c] * axisX[a] + boxCornerY[c] * axisY[a];
                    if (p < boxMin) boxMin = p;
                    if (p > boxMax) boxMax = p;
                }

                if (boxMax <= robotCenter - radius[a] || boxMin >= robotCenter + radius[a])
                {
                    separated = true;
                }
            }

            if (!separated)
            {
                kontakt = true;
            }
        }
    }
}