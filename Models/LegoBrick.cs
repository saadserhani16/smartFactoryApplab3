using System;
using System.Drawing;

namespace smartFactoryApp;

public class LegoBrick
{
    private LegoColor color;
    private LegoSize size;
    private double x;        // center of the brick
    private double y;
    private double angle;
    private int width;
    private int height;
    private BrickStatus status;
    private bool qualitaetGut;
    private int wartezeit;
    private bool aufDeliveryLinie;

    public LegoBrick(LegoColor color, LegoSize size, int width, int height)
    {
        this.color = color;
        this.size = size;
        this.width = width;
        this.height = height;
        this.status = BrickStatus.onLine;
        this.qualitaetGut = true;
    }

    public LegoColor Color
    {
        get { return color; }
    }

    public LegoSize Size
    {
        get { return size; }
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

    public int Width
    {
        get { return width; }
    }

    public int Height
    {
        get { return height; }
    }

    public BrickStatus Status
    {
        get { return status; }
    }

    public bool QualitaetGut
    {
        get { return qualitaetGut; }
    }

    // Result of PruefeDeliveryLinie
    public bool AufDeliveryLinie
    {
        get { return aufDeliveryLinie; }
    }

    public void SetzePose(double newX, double newY, double newAngle)
    {
        x = newX;
        y = newY;
        angle = ((newAngle % 360) + 360) % 360;
    }

    // The robot grabs the brick
    public void Dock()
    {
        status = BrickStatus.docked;
    }

    // The robot puts the brick on the delivery line. The quality is decided now.
    public void Lieferung(bool gut, double newX, double newY)
    {
        status = BrickStatus.onDeliveryLine;
        qualitaetGut = gut;
        wartezeit = Parameters.deliveryWaitTicks;
        SetzePose(newX, newY, 0);
    }

    // Waiting on the delivery line, or being repaired
    public void Warten()
    {
        wartezeit--;
        if (wartezeit > 0)
        {
            return;
        }

        if (status == BrickStatus.onDeliveryLine)
        {
            if (qualitaetGut)
                status = BrickStatus.toCustomer;
            else
                status = BrickStatus.toRecycle;
        }
        else if (status == BrickStatus.repairing)
        {
            status = BrickStatus.onLine;
            qualitaetGut = true;
        }
    }

    // One step to the right (to the customers)
    public void ZumKunden()
    {
        SetzePose(x + Parameters.deliverySpeed, y, 0);
    }

    // One step to the left (to the recycling)
    public void ZumRecycling()
    {
        SetzePose(x - Parameters.deliverySpeed, y, 0);
    }

    // The brick arrived on the recycle line
    public void Reparatur()
    {
        status = BrickStatus.repairing;
        wartezeit = Parameters.repairTicks;
    }

    // Sets AufDeliveryLinie: true if the brick touches the delivery line
    public void PruefeDeliveryLinie(Rectangle line)
    {
        double radians = angle * Math.PI / 180.0;
        double c = Math.Abs(Math.Cos(radians));
        double s = Math.Abs(Math.Sin(radians));
        double halfW = c * width / 2.0 + s * height / 2.0;
        double halfH = s * width / 2.0 + c * height / 2.0;

        aufDeliveryLinie = x + halfW > line.Left && x - halfW < line.Right &&
                           y + halfH > line.Top && y - halfH < line.Bottom;
    }
}