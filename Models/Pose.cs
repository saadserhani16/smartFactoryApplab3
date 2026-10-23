namespace smartFactoryApp;

public struct Pose
{
    private double x;
    private double y;
    private double angle;

    public Pose(double x, double y, double angle)
    {
        this.x = x;
        this.y = y;
        this.angle = angle;
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
}