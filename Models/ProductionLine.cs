using System.Collections.Generic;
using System.Drawing;

namespace smartFactoryApp;

public class ProductionLine
{
    private LegoColor color;
    private Rectangle box;
    private bool vertical;
    private Direction brickSide;
    private List<LegoBrick> bricks = new List<LegoBrick>();
    private int brickWidth;
    private int brickHeight;
    private double shift;
    private int direction = 1;
    private double maxShift;

    // brickSide = the edge of the box where the bricks sit (left, right, up or down)
    public ProductionLine(LegoColor color, Rectangle box, bool vertical, Direction brickSide)
    {
        this.color = color;
        this.box = box;
        this.vertical = vertical;
        this.brickSide = brickSide;

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

        for (int i = 0; i < Parameters.bricksPerLine; i++)
        {
            bricks.Add(new LegoBrick(color, LegoSize.large, brickWidth, brickHeight));
        }

        double boxLength = box.Width;
        if (vertical)
        {
            boxLength = box.Height;
        }
        double halfSpan = (Parameters.bricksPerLine - 1) / 2.0 * Parameters.brickSpacing + Parameters.brickLong / 2.0;
        maxShift = boxLength / 2.0 - 2 - halfSpan;

        UpdatePositions();
    }

    public LegoColor Color
    {
        get { return color; }
    }

    public Rectangle Box
    {
        get { return box; }
    }

    public List<LegoBrick> Bricks
    {
        get { return bricks; }
    }

    public int BrickWidth
    {
        get { return brickWidth; }
    }

    public int BrickHeight
    {
        get { return brickHeight; }
    }

    public void Bewegen()
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

        UpdatePositions();
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < bricks.Count; i++)
        {
            // A brick that was grabbed by the robot is no longer moved by the line
            if (!bricks[i].AufLinie)
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

            bricks[i].SetzePose(centerX, centerY, 0);
        }
    }
}