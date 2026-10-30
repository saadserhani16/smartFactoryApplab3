using Avalonia.Media;

namespace smartFactoryApp.ViewModels;

public class LegoBrickViewModel : ViewModelBase
{
    private LegoBrick brick;
    private IBrush fill = Brushes.Gray;

    public LegoBrickViewModel(LegoBrick brick)
    {
        this.brick = brick;

        if (brick.Color == LegoColor.red) fill = new SolidColorBrush(Color.Parse("#A5142C"));
        if (brick.Color == LegoColor.green) fill = new SolidColorBrush(Color.Parse("#2E9E2E"));
        if (brick.Color == LegoColor.blue) fill = new SolidColorBrush(Color.Parse("#4169E1"));
        if (brick.Color == LegoColor.yellow) fill = new SolidColorBrush(Color.Parse("#F5DC1E"));
    }

    // Canvas wants the top-left corner, the brick stores its center
    public double X
    {
        get { return brick.X - brick.Width / 2.0; }
    }

    public double Y
    {
        get { return brick.Y - brick.Height / 2.0; }
    }

    public double Angle
    {
        get { return brick.Angle; }
    }

    public double BrickWidth
    {
        get { return brick.Width; }
    }

    public double BrickHeight
    {
        get { return brick.Height; }
    }

    public IBrush Fill
    {
        get { return fill; }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
        OnPropertyChanged(nameof(Angle));
    }
}