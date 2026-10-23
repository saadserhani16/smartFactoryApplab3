using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace smartFactoryApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Factory factory;
    private DispatcherTimer brickTimer;
    private DispatcherTimer robotTimer;
    private bool continuousMode = true;

    private ObservableCollection<LegoBrickViewModel> allBricks = new ObservableCollection<LegoBrickViewModel>();
    private List<LegoBrickViewModel> redBricks = new List<LegoBrickViewModel>();
    private List<LegoBrickViewModel> greenBricks = new List<LegoBrickViewModel>();
    private List<LegoBrickViewModel> blueBricks = new List<LegoBrickViewModel>();
    private List<LegoBrickViewModel> yellowBricks = new List<LegoBrickViewModel>();

    public MainViewModel()
    {
        factory = new Factory();

        CreateBrickViewModels(redBricks, factory.ProductionRed);
        CreateBrickViewModels(greenBricks, factory.ProductionGreen);
        CreateBrickViewModels(blueBricks, factory.ProductionBlue);
        CreateBrickViewModels(yellowBricks, factory.ProductionYellow);

        brickTimer = new DispatcherTimer();
        brickTimer.Interval = TimeSpan.FromMilliseconds(Parameters.brickTimerInterval);
        brickTimer.Tick += BrickTimer_Tick;

        robotTimer = new DispatcherTimer();
        robotTimer.Interval = TimeSpan.FromMilliseconds(Parameters.robotTimerInterval);
        robotTimer.Tick += RobotTimer_Tick;
    }

    // ---------- Properties for the AXAML ----------

    public ObservableCollection<LegoBrickViewModel> AllBricks
    {
        get { return allBricks; }
    }

    public double FloorX
    {
        get { return ViewParameters.Borders.X; }
    }

    public double FloorY
    {
        get { return ViewParameters.Borders.Y; }
    }

    public double RobotX
    {
        get { return factory.Robot.X; }
    }

    public double RobotY
    {
        get { return factory.Robot.Y; }
    }

    public double RobotAngle
    {
        get { return factory.Robot.Angle; }
    }

    public string RobotInfo
    {
        get
        {
            return "X: " + factory.Robot.X.ToString("F1") + Environment.NewLine +
                   "Y: " + factory.Robot.Y.ToString("F1") + Environment.NewLine +
                   "Angle: " + factory.Robot.Angle.ToString("F1") + "°";
        }
    }

    // ---------- Robot buttons ----------

    public void MoveUp()
    {
        RobotCommand(Direction.up);
    }

    public void MoveDown()
    {
        RobotCommand(Direction.down);
    }

    public void MoveLeft()
    {
        RobotCommand(Direction.left);
    }

    public void MoveRight()
    {
        RobotCommand(Direction.right);
    }

    public void MoveUpLeft()
    {
        RobotCommand(Direction.upLeft);
    }

    public void MoveUpRight()
    {
        RobotCommand(Direction.upRight);
    }

    public void MoveDownLeft()
    {
        RobotCommand(Direction.downLeft);
    }

    public void MoveDownRight()
    {
        RobotCommand(Direction.downRight);
    }

    public void MoveRotateLeft()
    {
        RotateCommand(-1);
    }

    public void MoveRotateRight()
    {
        RotateCommand(1);
    }

    public void ResetRotation()
    {
        factory.RobotDrehrichtung(0);
        factory.RobotResetRotation();
        NotifyRobotPositionChanged();
    }

    public void StopRobot()
    {
        factory.RobotStoppen();
        robotTimer.Stop();
    }

    public void SetMoveMode()
    {
        continuousMode = true;
    }

    public void SetStepMode()
    {
        StopRobot();
        continuousMode = false;
    }

    // ---------- Brick timer ----------

    public void StartTimer()
    {
        brickTimer.Start();
    }

    public void StopTimer()
    {
        brickTimer.Stop();
    }

    // ---------- Private helpers ----------

    private void RobotCommand(Direction direction)
    {
        if (continuousMode)
        {
            factory.RobotRichtung(direction);
            robotTimer.Start();
        }
        else
        {
            factory.RobotBewegen(direction);
        }
        NotifyRobotPositionChanged();
    }

    private void RotateCommand(int sign)
    {
        if (continuousMode)
        {
            factory.RobotDrehrichtung(sign);
            robotTimer.Start();
        }
        else
        {
            factory.RobotDrehen(sign * Parameters.rotationStep);
        }
        NotifyRobotPositionChanged();
    }

    private void RobotTimer_Tick(object? sender, EventArgs e)
    {
        factory.RobotWeiterBewegen();
        NotifyRobotPositionChanged();
    }

    private void BrickTimer_Tick(object? sender, EventArgs e)
    {
        factory.LegoBricksBewegen();
        RefreshBricks(redBricks, factory.ProductionRed);
        RefreshBricks(greenBricks, factory.ProductionGreen);
        RefreshBricks(blueBricks, factory.ProductionBlue);
        RefreshBricks(yellowBricks, factory.ProductionYellow);
    }

    private void CreateBrickViewModels(List<LegoBrickViewModel> list, ProductionLine line)
    {
        for (int i = 0; i < line.Bricks.Count; i++)
        {
            LegoBrickViewModel brickViewModel = new LegoBrickViewModel(
                line.Bricks[i], line.BrickX[i], line.BrickY[i], line.BrickWidth, line.BrickHeight);
            list.Add(brickViewModel);
            allBricks.Add(brickViewModel);
        }
    }

    private void RefreshBricks(List<LegoBrickViewModel> list, ProductionLine line)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Refresh(line.BrickX[i], line.BrickY[i]);
        }
    }

    private void NotifyRobotPositionChanged()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
        OnPropertyChanged(nameof(RobotInfo));
    }
}