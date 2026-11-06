using System;
using System.Collections.Generic;

namespace smartFactoryApp;

public class Logistics
{
    private int[] ordered = new int[4];      // index = (int)LegoColor
    private int[] delivered = new int[4];
    private bool hasOrder;
    private Random random;
    private LegoBrick? nextBrick;
    private ProductionLine? pickLine;

    public Logistics(Random random)
    {
        this.random = random;
    }

    public bool HasOrder
    {
        get { return hasOrder; }
    }

    public int RedOrdered
    {
        get { return ordered[(int)LegoColor.red]; }
    }

    public int GreenOrdered
    {
        get { return ordered[(int)LegoColor.green]; }
    }

    public int BlueOrdered
    {
        get { return ordered[(int)LegoColor.blue]; }
    }

    public int YellowOrdered
    {
        get { return ordered[(int)LegoColor.yellow]; }
    }

    public int RedDelivered
    {
        get { return delivered[(int)LegoColor.red]; }
    }

    public int GreenDelivered
    {
        get { return delivered[(int)LegoColor.green]; }
    }

    public int BlueDelivered
    {
        get { return delivered[(int)LegoColor.blue]; }
    }

    public int YellowDelivered
    {
        get { return delivered[(int)LegoColor.yellow]; }
    }

    // The brick the robot should pick up next, and the line it is on
    public LegoBrick? NextBrick
    {
        get { return nextBrick; }
    }

    public ProductionLine? PickLine
    {
        get { return pickLine; }
    }

    // True when every ordered brick was delivered
    public bool OrderComplete
    {
        get
        {
            if (!hasOrder)
            {
                return false;
            }
            for (int i = 0; i < 4; i++)
            {
                if (delivered[i] < ordered[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    // For example "2 yellow, 1 red"
    public string OrderText
    {
        get
        {
            List<string> parts = new List<string>();
            if (YellowOrdered > 0) parts.Add(YellowOrdered + " yellow");
            if (BlueOrdered > 0) parts.Add(BlueOrdered + " blue");
            if (RedOrdered > 0) parts.Add(RedOrdered + " red");
            if (GreenOrdered > 0) parts.Add(GreenOrdered + " green");
            return string.Join(", ", parts);
        }
    }

    // What the robot should do next
    public string PlanText
    {
        get
        {
            if (!hasOrder)
            {
                return "";
            }
            if (OrderComplete)
            {
                return "Order complete!";
            }
            if (nextBrick != null && nextBrick.Status == BrickStatus.onLine && pickLine != null)
            {
                return "Next: " + nextBrick.Color + " brick from the " + pickLine.Name;
            }
            if (nextBrick != null && nextBrick.Status == BrickStatus.docked)
            {
                return "Now: bring the brick to the delivery line";
            }
            return "Press GoToPickUp for the next brick";
        }
    }

    // A new order with 1 to orderSize bricks. Every brick has a 25% chance for each color.
    public void CreateRandomOrder()
    {
        ordered = new int[4];
        delivered = new int[4];
        nextBrick = null;
        pickLine = null;

        int total = random.Next(1, Parameters.orderSize + 1);
        for (int i = 0; i < total; i++)
        {
            ordered[random.Next(4)]++;
        }

        hasOrder = true;
    }

    // A brick of this color reached the customer
    public void BrickDelivered(LegoColor color)
    {
        delivered[(int)color]++;
    }

    // Chooses the next brick: it must be a color that is still needed, and the closest one to the robot
    public void PlanBerechnen(List<LegoBrick> bricks, List<ProductionLine> lines, double fromX, double fromY)
    {
        nextBrick = null;
        pickLine = null;

        if (!hasOrder)
        {
            return;
        }

        // How many bricks of each color are still missing?
        int[] open = new int[4];
        for (int i = 0; i < 4; i++)
        {
            open[i] = ordered[i] - delivered[i];
        }

        // Bricks that are already on their way to the customer count as done
        foreach (LegoBrick brick in bricks)
        {
            bool unterwegs = brick.Status == BrickStatus.docked ||
                             brick.Status == BrickStatus.toCustomer ||
                             (brick.Status == BrickStatus.onDeliveryLine && brick.QualitaetGut);
            if (unterwegs)
            {
                open[(int)brick.Color]--;
            }
        }

        double best = double.MaxValue;
        foreach (LegoBrick brick in bricks)
        {
            if (brick.Status != BrickStatus.onLine)
            {
                continue;
            }
            if (open[(int)brick.Color] <= 0)
            {
                continue;
            }

            double dx = brick.X - fromX;
            double dy = brick.Y - fromY;
            double distance = dx * dx + dy * dy;
            if (distance < best)
            {
                best = distance;
                nextBrick = brick;
            }
        }

        if (nextBrick != null)
        {
            foreach (ProductionLine line in lines)
            {
                if (line.Bricks.Contains(nextBrick))
                {
                    pickLine = line;
                }
            }
        }
    }
}