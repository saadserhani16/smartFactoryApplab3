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
    private bool gedockt;
    private bool aufLinie;   // true = still moved by its production line

    public LegoBrick(LegoColor color, LegoSize size, int width, int height)
    {
        this.color = color;
        this.size = size;
        this.width = width;
        this.height = height;
        this.gedockt = false;
        this.aufLinie = true;
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

    public bool Gedockt
    {
        get { return gedockt; }
    }

    public bool AufLinie
    {
        get { return aufLinie; }
    }

    public void SetzePose(double newX, double newY, double newAngle)
    {
        x = newX;
        y = newY;
        angle = ((newAngle % 360) + 360) % 360;
    }

    public void Dock()
    {
        gedockt = true;
        aufLinie = false;   // once grabbed, the line never moves it again
    }

    public void Undock()
    {
        gedockt = false;
    }
}