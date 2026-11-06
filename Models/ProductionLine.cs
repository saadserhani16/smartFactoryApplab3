using System;
using System.Collections.Generic;
using System.Drawing;

namespace smartFactoryApp;

public class ProductionLine
{
    private string name;
    private Rectangle box;
    private bool vertical;
    private Direction brickSide;
    private bool beweglich;     // true = the bricks slide back and forth (production lines)
    private LegoBrick?[] slots = new LegoBrick?[Parameters.bricksPerLine];
    private int brickWidth;
    private int brickHeight;
    private double shift;
    private int direction = 1;
    private double maxShift;
    private LegoBrick? neuesBrick;
    private List<LegoBrick> neueBricks = new List<LegoBrick>();

    // brickSide = the edge of the box where the bricks sit (left, right, up or down)
    public ProductionLine(string name, Rectangle box, bool vertical, Direction brickSide, bool beweglich)
    {
        this.name = name;
        this.box = box;
        this.vertical = vertical;
        this.brickSide = brickSide;
        this.beweglich = beweglich;

        if (vertical)
        {
            brickWidth = Parameters.brickShort;
            brickHeight = Parameters.brickLong;
        }
        else
        {
            brickWidth = Parameters.brickLong;
            brickHeight = Parameters.brickShort;
        }

        double boxLength = box.Width;
        if (vertical)
        {
            boxLength = box.Height;
        }
        double halfSpan = (Parameters.bricksPerLine - 1) / 2.0 * Parameters.brickSpacing + Parameters.brickLong / 2.0;
        maxShift = boxLength / 2.0 - 2 - halfSpan;
    }

    public string Name
    {
        get { return name; }
    }

    public Rectangle Box
    {
        get { return box; }
    }

    public int BrickWidth
    {
        get { return brickWidth; }
    }

    public int BrickHeight
    {
        get { return brickHeight; }
    }

    // The angle the robot needs so that its gripper points at the bricks of this line
    // (0 = up, 90 = right, 180 = down, 270 = left)
    public int AnfahrWinkel
    {
        get
        {
            if (vertical)
            {
                if (brickSide == Direction.right)
                    return 270;
                return 90;
            }
            if (brickSide == Direction.down)
                return 0;
            return 180;
        }
    }

    // The bricks that are on this line at the moment
    public List<LegoBrick> Bricks
    {
        get
        {
            List<LegoBrick> list = new List<LegoBrick>();
            foreach (LegoBrick? brick in slots)
            {
                if (brick != null)
                {
                    list.Add(brick);
                }
            }
            return list;
        }
    }

    // The brick that ErsatzProduzieren made (null if there was no free place)
    public LegoBrick? NeuesBrick
    {
        get { return neuesBrick; }
    }

    // The bricks that Fuellen made
    public List<LegoBrick> NeueBricks
    {
        get { return neueBricks; }
    }

    // True if at least one place on this line is free
    public bool FreierPlatzVorhanden
    {
        get
        {
            for (int i = 0; i < slots.Length; i++)
            {
                LegoBrick? brick = slots[i];
                if (brick == null || (brick.Status != BrickStatus.onLine && brick.Status != BrickStatus.repairing))
                {
                    return true;
                }
            }
            return false;
        }
    }

    // One tick: the bricks slide along the line and turn around at the ends
    public void Bewegen()
    {
        SlotsFreigeben();

        if (beweglich)
        {
            shift += direction * Parameters.brickSpeed;

            if (shift > maxShift)
            {
                shift = maxShift;
                direction = -1;
            }
            if (shift < -maxShift)
            {
                shift = -maxShift;
                direction = 1;
            }
        }

        UpdatePositions();
    }

    // Removes all bricks that wait on this line
    public void Leeren()
    {
        SlotsFreigeben();

        for (int i = 0; i < slots.Length; i++)
        {
            LegoBrick? brick = slots[i];
            if (brick != null && brick.Status == BrickStatus.onLine)
            {
                slots[i] = null;
            }
        }
    }

    // Puts a new brick with a random color into every free place (25% chance for each color)
    public void Fuellen(Random random)
    {
        neueBricks = new List<LegoBrick>();
        SlotsFreigeben();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                LegoColor color = (LegoColor)random.Next(4);
                LegoBrick brick = new LegoBrick(color, LegoSize.large, brickWidth, brickHeight);
                brick.SetzeHerkunft(this);
                slots[i] = brick;
                neueBricks.Add(brick);
            }
        }

        UpdatePositions();
    }

    // A brick was delivered to the customer: the line makes a new one in a free place
    public void ErsatzProduzieren(LegoColor color)
    {
        neuesBrick = null;
        SlotsFreigeben();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                neuesBrick = new LegoBrick(color, LegoSize.large, brickWidth, brickHeight);
                neuesBrick.SetzeHerkunft(this);
                slots[i] = neuesBrick;
                break;
            }
        }

        UpdatePositions();
    }

    // Puts a brick (that comes from the delivery line) into the first free place
    public void PlatzBelegen(LegoBrick brick)
    {
        SlotsFreigeben();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = brick;
                break;
            }
        }

        UpdatePositions();
    }

    // A place is free again when its brick was taken away by the robot
    private void SlotsFreigeben()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            LegoBrick? brick = slots[i];
            if (brick != null && brick.Status != BrickStatus.onLine && brick.Status != BrickStatus.repairing)
            {
                slots[i] = null;
            }
        }
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            LegoBrick? brick = slots[i];
            if (brick == null)
            {
                continue;
            }
            if (brick.Status != BrickStatus.onLine && brick.Status != BrickStatus.repairing)
            {
                continue;
            }

            double along = shift + (i - (Parameters.bricksPerLine - 1) / 2.0) * Parameters.brickSpacing;
            double centerX;
            double centerY;

            if (vertical)
            {
                if (brickSide == Direction.left)
                    centerX = box.Left + brickWidth / 2.0;
                else
                    centerX = box.Right - brickWidth / 2.0;

                centerY = box.Top + box.Height / 2.0 + along;
            }
            else
            {
                if (brickSide == Direction.down)
                    centerY = box.Bottom - brickHeight / 2.0;
                else
                    centerY = box.Top + brickHeight / 2.0;

                centerX = box.Left + box.Width / 2.0 + along;
            }

            brick.SetzePose(centerX, centerY, 0);
        }
    }
}