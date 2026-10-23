using Avalonia.Media;

namespace smartFactoryApp.ViewModels;

public class LegoBrickViewModel : ViewModelBase
{
    private LegoBrick brick;
    private double centerX;
    private double centerY;
    private double brickWidth;
    private double brickHeight;
    private IBrush fill = Brushes.Gray;

    public LegoBrickViewModel(LegoBrick brick, double centerX, double centerY, double brickWidth, double brickHeight)
    {
        this.brick = brick;
        this.centerX = centerX;
        this.centerY = centerY;
        this.brickWidth = brickWidth;
        this.brickHeight = brickHeight;

        if (brick.Color == LegoColor.red) fill = new SolidColorBrush(Color.Parse("#A5142C"));
        if (brick.Color == LegoColor.green) fill = new SolidColorBrush(Color.Parse("#2E9E2E"));
        if (brick.Color == LegoColor.blue) fill = new SolidColorBrush(Color.Parse("#4169E1"));
        if (brick.Color == LegoColor.yellow) fill = new SolidColorBrush(Color.Parse("#F5DC1E"));
    }

    public double X
    {
        get { return centerX - brickWidth / 2; }
    }

    public double Y
    {
        get { return centerY - brickHeight / 2; }
    }

    public double BrickWidth
    {
        get { return brickWidth; }
    }

    public double BrickHeight
    {
        get { return brickHeight; }
    }

    public IBrush Fill
    {
        get { return fill; }
    }

    public void Refresh(double newCenterX, double newCenterY)
    {
        centerX = newCenterX;
        centerY = newCenterY;
        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
    }
}