namespace smartFactoryApp;

public class LegoBrick
{
    private LegoColor color;
    private LegoSize size;

    public LegoBrick(LegoColor color, LegoSize size)
    {
        this.color = color;
        this.size = size;
    }

    public LegoColor Color
    {
        get { return color; }
    }

    public LegoSize Size
    {
        get { return size; }
    }
}