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
    private string undockMessage = "";

    private ObservableCollection<LegoBrickViewModel> allBricks = new ObservableCollection<LegoBrickViewModel>();

    public MainViewModel()
    {
        factory = new Factory();

        SyncBricks();

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

    public string RobotInfo
    {
        get
        {
            return "X: " + factory.Robot.X.ToString("F1") + Environment.NewLine +
                   "Y: " + factory.Robot.Y.ToString("F1") + Environment.NewLine +
                   "Angle: " + factory.Robot.Angle.ToString("F1") + "°";
        }
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

    // Feedback of the Undock button
    public string UndockMessage
    {
        get { return undockMessage; }
    }

    public string YellowDelivered
    {
        get { return factory.YellowDelivered.ToString(); }
    }

    public string BlueDelivered
    {
        get { return factory.BlueDelivered.ToString(); }
    }

    public string RedDelivered
    {
        get { return factory.RedDelivered.ToString(); }
    }

    public string GreenDelivered
    {
        get { return factory.GreenDelivered.ToString(); }
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

    // The Undock button: the robot can only let go of a brick on the delivery line
    public void Undock()
    {
        if (!running)
        {
            undockMessage = "Press Start first.";
            OnPropertyChanged(nameof(UndockMessage));
            return;
        }

        factory.RobotUndock();

        if (factory.UndockErgebnis == UndockResult.noBrick)
        {
            undockMessage = "No brick available to undock.";
        }
        else if (factory.UndockErgebnis == UndockResult.notOnDeliveryLine)
        {
            undockMessage = "Undocking failed: only possible on the delivery line.";
        }
        else if (factory.LetzteQualitaetGut)
        {
            undockMessage = "Undocking successful: brick is OK and goes to the customer.";
        }
        else
        {
            undockMessage = "Undocking successful: brick is defective and goes to recycling.";
        }

        OnPropertyChanged(nameof(UndockMessage));
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
        SyncBricks();
        NotifyBricksChanged();
        NotifyDeliveredChanged();
        OnPropertyChanged(nameof(DockInfo));
    }

    // Creates a view model for every new brick and removes the ones of bricks that are gone
    private void SyncBricks()
    {
        foreach (LegoBrick brick in factory.AllBricks)
        {
            bool found = false;
            foreach (LegoBrickViewModel brickViewModel in allBricks)
            {
                if (brickViewModel.Brick == brick)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                allBricks.Add(new LegoBrickViewModel(brick));
            }
        }

        for (int i = allBricks.Count - 1; i >= 0; i--)
        {
            if (!factory.AllBricks.Contains(allBricks[i].Brick))
            {
                allBricks.RemoveAt(i);
            }
        }
    }

    private void NotifyBricksChanged()
    {
        foreach (LegoBrickViewModel brickViewModel in allBricks)
        {
            brickViewModel.Refresh();
        }
    }

    private void NotifyDeliveredChanged()
    {
        OnPropertyChanged(nameof(YellowDelivered));
        OnPropertyChanged(nameof(BlueDelivered));
        OnPropertyChanged(nameof(RedDelivered));
        OnPropertyChanged(nameof(GreenDelivered));
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