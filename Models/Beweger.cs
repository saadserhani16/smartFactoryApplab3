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

    // Docking
    private LegoBrick? dockobjekt;
    private double dockWinkelOffset;
    private double greifpunktX;            // TCP = middle of the upper edge
    private double greifpunktY;
    private double abstand;

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
        BerechneGreifpunkt();
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

    public bool Kontakt
    {
        get { return kontakt; }
    }

    public double GreifpunktX
    {
        get { return greifpunktX; }
    }

    public double GreifpunktY
    {
        get { return greifpunktY; }
    }

    public LegoBrick? Dockobjekt
    {
        get { return dockobjekt; }
    }

    // ---------- Movement ----------

    public void Bewegen(Direction direction)
    {
        Bewegen(direction, Parameters.step);
    }

    // The direction is relative to the robot: "up" = forward in the direction the robot faces
    public void Bewegen(Direction direction, int stepSize)
    {
        double localX = 0;
        double localY = 0;

        if (direction == Direction.left || direction == Direction.upLeft || direction == Direction.downLeft)
            localX -= 1;
        if (direction == Direction.right || direction == Direction.upRight || direction == Direction.downRight)
            localX += 1;
        if (direction == Direction.up || direction == Direction.upLeft || direction == Direction.upRight)
            localY -= 1;
        if (direction == Direction.down || direction == Direction.downLeft || direction == Direction.downRight)
            localY += 1;

        if (localX == 0 && localY == 0)
        {
            return;
        }

        // Same speed for straight and diagonal moves
        double length = Math.Sqrt(localX * localX + localY * localY);
        localX = localX / length * stepSize;
        localY = localY / length * stepSize;

        // Turn the local movement by the robot's angle into a movement on the floor
        double radians = angle * Math.PI / 180.0;
        double dx = localX * Math.Cos(radians) - localY * Math.Sin(radians);
        double dy = localX * Math.Sin(radians) + localY * Math.Cos(radians);

        VersucheZuSetzen(x + dx, y + dy, angle);

        // Blocked: try to slide along the wall or the production line
        if (kontakt)
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

    // ---------- Docking ----------

    // Called after every movement: grabs a brick if the TCP is close enough.
    // Only bricks that wait on a production line or the recycle line can be grabbed,
    // never bricks that are on the delivery line.
    public void DockingPruefen(List<LegoBrick> bricks)
    {
        if (dockobjekt != null)
        {
            return;
        }

        foreach (LegoBrick brick in bricks)
        {
            if (brick.Status != BrickStatus.onLine)
            {
                continue;
            }

            BerechneAbstand(brick);
            if (abstand <= Parameters.dockDistance)
            {
                Dock(brick);
                return;
            }
        }
    }

    public void Dock(LegoBrick brick)
    {
        dockobjekt = brick;
        dockWinkelOffset = brick.Angle - angle;
        brick.Dock();
        DockobjektNachziehen();
    }

    // The robot lets go of the brick (the Factory decides what happens to the brick)
    public void Undock()
    {
        dockobjekt = null;
    }

    // ---------- Private helpers ----------

    // Applies the new pose only if there is no collision
    private void VersucheZuSetzen(double newX, double newY, double newAngle)
    {
        PruefeKollision(newX, newY, newAngle);
        if (!kontakt)
        {
            x = newX;
            y = newY;
            angle = newAngle;
            BerechneGreifpunkt();
            DockobjektNachziehen();
        }
    }

    // TCP = middle of the upper edge of the robot, also when it is rotated
    private void BerechneGreifpunkt()
    {
        double radians = angle * Math.PI / 180.0;
        greifpunktX = x + width / 2.0 + (height / 2.0) * Math.Sin(radians);
        greifpunktY = y + height / 2.0 - (height / 2.0) * Math.Cos(radians);
    }

    // The docked brick sits on the TCP and turns with the robot
    private void DockobjektNachziehen()
    {
        if (dockobjekt != null)
        {
            dockobjekt.SetzePose(greifpunktX, greifpunktY, angle + dockWinkelOffset);
        }
    }

    // Distance from the TCP to the (possibly rotated) rectangle of a brick
    private void BerechneAbstand(LegoBrick brick)
    {
        double radians = -brick.Angle * Math.PI / 180.0;
        double dx = greifpunktX - brick.X;
        double dy = greifpunktY - brick.Y;
        double localX = dx * Math.Cos(radians) - dy * Math.Sin(radians);
        double localY = dx * Math.Sin(radians) + dy * Math.Cos(radians);
        double outsideX = Math.Max(Math.Abs(localX) - brick.Width / 2.0, 0);
        double outsideY = Math.Max(Math.Abs(localY) - brick.Height / 2.0, 0);
        abstand = Math.Sqrt(outsideX * outsideX + outsideY * outsideY);
    }

    // Sets kontakt to true if the robot at this pose (or its docked brick)
    // leaves the factory, or if the robot touches one of the production lines
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

        // 2. The docked brick must stay inside the factory, too
        if (dockobjekt != null)
        {
            double tcpX = centerX + (height / 2.0) * sin;
            double tcpY = centerY - (height / 2.0) * cos;
            double brickRadians = (testAngle + dockWinkelOffset) * Math.PI / 180.0;
            double bc = Math.Abs(Math.Cos(brickRadians));
            double bs = Math.Abs(Math.Sin(brickRadians));
            double halfW = bc * dockobjekt.Width / 2.0 + bs * dockobjekt.Height / 2.0;
            double halfH = bs * dockobjekt.Width / 2.0 + bc * dockobjekt.Height / 2.0;

            if (tcpX - halfW < borders.Left || tcpX + halfW > borders.Right ||
                tcpY - halfH < borders.Top || tcpY + halfH > borders.Bottom)
            {
                kontakt = true;
            }
        }

        // 3. Production lines (only the robot itself, not the docked brick)
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