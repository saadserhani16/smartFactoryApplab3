using System;
using System.Collections.Generic;
using System.Drawing;

namespace smartFactoryApp;

public class Robot : Beweger
{
    private Direction robotDirection = Direction.stop;
    private int rotationDirection = 0;

    // Automatic driving (GoToPickUp / GoToDelivery)
    private List<Pose> route = new List<Pose>();
    private int routeIndex;
    private bool routeAktiv;
    private bool routeGefunden;
    private bool routeBlockiert;
    private double planTcpX;
    private double planTcpY;

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

    // True while the robot drives an automatic route
    public bool RouteAktiv
    {
        get { return routeAktiv; }
    }

    // True if the last planning found a route
    public bool RouteGefunden
    {
        get { return routeGefunden; }
    }

    // True if the route was stopped because something was in the way
    public bool RouteBlockiert
    {
        get { return routeBlockiert; }
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
        routeAktiv = false;
    }

    public void RouteAbbrechen()
    {
        routeAktiv = false;
    }

    // One tick: follows the automatic route, or the continuous movement of the buttons
    public void WeiterBewegen()
    {
        if (routeAktiv)
        {
            Pose next = route[routeIndex];
            PoseSetzen(next.X, next.Y, next.Angle);

            if (Kontakt)
            {
                routeAktiv = false;
                routeBlockiert = true;
                return;
            }

            routeIndex++;
            if (routeIndex >= route.Count)
            {
                routeAktiv = false;
            }
            return;
        }

        if (rotationDirection != 0)
        {
            Drehen(rotationDirection * Parameters.continuousRotation);
        }
        if (robotDirection != Direction.stop)
        {
            Bewegen(robotDirection, Parameters.continuousStep);
        }
    }

    // Plans a route to a brick. zielIndex = angle / 90 (0 = up, 1 = right, 2 = down, 3 = left).
    public void RouteZumBrick(LegoBrick brick, int zielIndex)
    {
        PlaneRoute(1, brick, zielIndex, new Rectangle(0, 0, 0, 0));
    }

    // Plans a route to the delivery line (the robot ends facing up, the gripper at the line)
    public void RouteZurDelivery(Rectangle delivery)
    {
        PlaneRoute(2, null, 0, delivery);
    }

    // Gripper position (TCP) for a robot pose, stored in planTcpX and planTcpY
    private void BerechneTcp(double x, double y, double angleDegrees)
    {
        double radians = angleDegrees * Math.PI / 180.0;
        planTcpX = x + Parameters.robotWidth / 2.0 + (Parameters.robotHeight / 2.0) * Math.Sin(radians);
        planTcpY = y + Parameters.robotHeight / 2.0 - (Parameters.robotHeight / 2.0) * Math.Cos(radians);
    }

    // A* search over the poses (x, y in steps of routeStep, angle in steps of 90 degrees).
    // art 1 = pick up a brick, art 2 = go to the delivery line.
    private void PlaneRoute(int art, LegoBrick? brick, int zielIndex, Rectangle delivery)
    {
        route = new List<Pose>();
        routeIndex = 0;
        routeAktiv = false;
        routeBlockiert = false;
        routeGefunden = false;

        int step = Parameters.routeStep;
        int half = Borders.Width / step + 1;
        int size = 2 * half + 1;
        int layer = size * size;

        double startX = X;
        double startY = Y;
        int startA = ((int)Math.Round(Angle / 90.0)) % 4;

        int[] parent = new int[4 * layer];
        int[] gScore = new int[4 * layer];
        bool[] closed = new bool[4 * layer];
        for (int i = 0; i < parent.Length; i++)
        {
            parent[i] = -2;              // -2 = not visited yet
            gScore[i] = int.MaxValue;
        }

        // The point the route should come close to (only used to guide the search)
        double goalX = (delivery.Left + delivery.Right) / 2.0;
        double goalY = delivery.Bottom;
        if (art == 1 && brick != null)
        {
            goalX = brick.X;
            goalY = brick.Y;
        }

        PriorityQueue<int, int> queue = new PriorityQueue<int, int>();
        int startIndex = startA * layer + half * size + half;
        parent[startIndex] = -1;         // -1 = start
        gScore[startIndex] = 0;
        queue.Enqueue(startIndex, 0);

        int goalIndex = -1;
        int[] dix = { 1, -1, 0, 0 };
        int[] diy = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            if (closed[current])
            {
                continue;
            }
            closed[current] = true;

            int a = current / layer;
            int rest = current % layer;
            int iy = rest / size;
            int ix = rest % size;
            double x = startX + (ix - half) * step;
            double y = startY + (iy - half) * step;

            // Is this pose a goal?
            BerechneTcp(x, y, a * 90);
            bool goal = false;
            if (art == 1 && brick != null && a == zielIndex)
            {
                double outsideX = Math.Max(Math.Abs(planTcpX - brick.X) - brick.Width / 2.0, 0);
                double outsideY = Math.Max(Math.Abs(planTcpY - brick.Y) - brick.Height / 2.0, 0);
                goal = Math.Sqrt(outsideX * outsideX + outsideY * outsideY) <= Parameters.routeGoalDistance;
            }
            if (art == 2 && a == 0)
            {
                goal = planTcpY >= delivery.Top && planTcpY <= delivery.Bottom + 3 &&
                       planTcpX >= delivery.Left + 20 && planTcpX <= delivery.Right - 20;
            }
            if (goal)
            {
                goalIndex = current;
                break;
            }

            // Move one step left, right, up or down
            for (int k = 0; k < 4; k++)
            {
                int nx = ix + dix[k];
                int ny = iy + diy[k];
                if (nx < 0 || nx >= size || ny < 0 || ny >= size)
                {
                    continue;
                }

                int next = a * layer + ny * size + nx;
                if (closed[next])
                {
                    continue;
                }
                int newG = gScore[current] + 1;
                if (newG >= gScore[next])
                {
                    continue;
                }

                double nextX = startX + (nx - half) * step;
                double nextY = startY + (ny - half) * step;
                PruefeKollision(nextX, nextY, a * 90);
                if (Kontakt)
                {
                    continue;
                }

                gScore[next] = newG;
                parent[next] = current;
                BerechneTcp(nextX, nextY, a * 90);
                int h = (int)((Math.Abs(planTcpX - goalX) + Math.Abs(planTcpY - goalY)) / step);
                queue.Enqueue(next, newG + h);
            }

            // Turn by 90 degrees (every 15-degree step on the way must be free)
            for (int r = -1; r <= 1; r += 2)
            {
                int na = (a + r + 4) % 4;
                int next = na * layer + iy * size + ix;
                if (closed[next])
                {
                    continue;
                }
                int newG = gScore[current] + 6;
                if (newG >= gScore[next])
                {
                    continue;
                }

                bool frei = true;
                for (int s = 1; s <= 6; s++)
                {
                    PruefeKollision(x, y, a * 90 + r * 15 * s);
                    if (Kontakt)
                    {
                        frei = false;
                        break;
                    }
                }
                if (!frei)
                {
                    continue;
                }

                gScore[next] = newG;
                parent[next] = current;
                BerechneTcp(x, y, na * 90);
                int h = (int)((Math.Abs(planTcpX - goalX) + Math.Abs(planTcpY - goalY)) / step);
                queue.Enqueue(next, newG + h);
            }
        }

        if (goalIndex == -1)
        {
            return;   // no route found
        }

        // Collect the poses from the start to the goal
        List<int> states = new List<int>();
        int walk = goalIndex;
        while (walk != -1)
        {
            states.Add(walk);
            walk = parent[walk];
        }
        states.Reverse();

        // 1. Turn to the nearest multiple of 90 degrees (in steps of at most 5 degrees)
        double diff = startA * 90 - Angle;
        while (diff > 180) diff -= 360;
        while (diff < -180) diff += 360;
        int snapSteps = (int)Math.Ceiling(Math.Abs(diff) / 5.0);
        for (int s = 1; s <= snapSteps; s++)
        {
            route.Add(new Pose(startX, startY, Angle + diff * s / snapSteps));
        }

        // 2. Follow the found path
        for (int i = 1; i < states.Count; i++)
        {
            int prev = states[i - 1];
            int cur = states[i];
            int pa = prev / layer;
            int a = cur / layer;
            int rest = cur % layer;
            double x = startX + (rest % size - half) * step;
            double y = startY + (rest / size - half) * step;

            if (a == pa)
            {
                route.Add(new Pose(x, y, a * 90));
            }
            else
            {
                int r = 1;
                if ((a - pa + 4) % 4 == 3)
                {
                    r = -1;
                }
                for (int s = 1; s <= 6; s++)
                {
                    route.Add(new Pose(x, y, pa * 90 + r * 15 * s));
                }
            }
        }

        routeGefunden = true;
        routeAktiv = route.Count > 0;
    }
}