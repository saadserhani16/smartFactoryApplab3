using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace smartFactoryApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Factory factory;
    private DispatcherTimer brickTimer;
    private DispatcherTimer robotTimer;
    private bool continuousMode = true;
    private bool running = false;   // false until the Start button is pressed

    private ObservableCollection<LegoBrickViewModel> allBricks = new ObservableCollection<LegoBrickViewModel>();

    public MainViewModel()
    {
        factory = new Factory();

        foreach (LegoBrick brick in factory.AllBricks)
        {
            allBricks.Add(new LegoBrickViewModel(brick));
        }

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

 public double GripperX
    {
        get { return factory.Robot.GreifpunktX - 3; }
    }

    public double GripperY
    {
        get { return factory.Robot.GreifpunktY - 3; }
    }

    public string DockInfo
    {
        get
        {
            string holding = "nothing";
            if (factory.Robot.Dockobjekt != null)
            {
                holding = factory.Robot.Dockobjekt.Color.ToString();
            }
            return "Gripper: " + factory.Robot.GreifpunktX.ToString("F0") + ", " +
                   factory.Robot.GreifpunktY.ToString("F0") + Environment.NewLine +
                   "Holding: " + holding;
        }
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

    // ---------- Start / Stop ----------

    // Start: bricks move and the robot can be controlled
    public void StartTimer()
    {
        running = true;
        brickTimer.Start();
    }

    // Stop: everything stops, and the robot stays locked until Start is pressed again
    public void StopTimer()
    {
        running = false;
        brickTimer.Stop();
        StopRobot();
    }

    // ---------- Robot buttons (all directions are relative to the robot) ----------

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
        if (!running)
        {
            return;
        }
        factory.RobotDrehrichtung(0);
        factory.RobotResetRotation();
        NotifyRobotPositionChanged();
    }

    // The black square: stops the continuous movement of the robot
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

    public void Undock()
    {
        factory.RobotUndock();
        NotifyRobotPositionChanged();
    }

    // ---------- Private helpers ----------

    private void RobotCommand(Direction direction)
    {
        if (!running)
        {
            return;
        }

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
        if (!running)
        {
            return;
        }

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
        NotifyBricksChanged();
    }

    private void NotifyBricksChanged()
    {
        foreach (LegoBrickViewModel brickViewModel in allBricks)
        {
            brickViewModel.Refresh();
        }
    }

    private void NotifyRobotPositionChanged()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
        OnPropertyChanged(nameof(RobotInfo));
        OnPropertyChanged(nameof(GripperX));
        OnPropertyChanged(nameof(GripperY));
        OnPropertyChanged(nameof(DockInfo));
        NotifyBricksChanged();   // a docked brick moves with the robot
    }
    }