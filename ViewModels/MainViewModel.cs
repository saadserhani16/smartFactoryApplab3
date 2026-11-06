using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace smartFactoryApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    // What the robot does by itself at the moment
    private enum AutoModus
    {
        keiner,
        abholen,
        liefern
    }

    private Factory factory;
    private DispatcherTimer brickTimer;
    private DispatcherTimer robotTimer;
    private bool continuousMode = true;
    private bool running = false;   // false until the Start button is pressed
    private string undockMessage = "";
    private string robotMessage = "";
    private AutoModus autoModus = AutoModus.keiner;
    private int autoVersuche;

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
            return "Holding: " + holding;
        }
    }

    // The order, the logistics plan and the messages of the robot
    public string OrderInfo
    {
        get
        {
            Logistics logistics = factory.Logistik;
            string text;

            if (!logistics.HasOrder)
            {
                text = "No order yet: click \"Order\"";
            }
            else
            {
                text = "Order: " + logistics.OrderText + Environment.NewLine + logistics.PlanText;
            }

            if (robotMessage != "")
            {
                text += Environment.NewLine + robotMessage;
            }
            return text;
        }
    }

    // Feedback of the Undock button
    public string UndockMessage
    {
        get { return undockMessage; }
    }

    public string YellowOrdered
    {
        get { return factory.Logistik.YellowOrdered.ToString(); }
    }

    public string BlueOrdered
    {
        get { return factory.Logistik.BlueOrdered.ToString(); }
    }

    public string RedOrdered
    {
        get { return factory.Logistik.RedOrdered.ToString(); }
    }

    public string GreenOrdered
    {
        get { return factory.Logistik.GreenOrdered.ToString(); }
    }

    public string YellowDelivered
    {
        get { return factory.Logistik.YellowDelivered.ToString(); }
    }

    public string BlueDelivered
    {
        get { return factory.Logistik.BlueDelivered.ToString(); }
    }

    public string RedDelivered
    {
        get { return factory.Logistik.RedDelivered.ToString(); }
    }

    public string GreenDelivered
    {
        get { return factory.Logistik.GreenDelivered.ToString(); }
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

    // ---------- Order, setup and automatic driving ----------

    // Button "Order": a new random order
    public void CreateOrder()
    {
        ManuellStoppen();
        factory.CreateOrder();
        robotMessage = "";
        NotifyOrderChanged();
        NotifyRobotPositionChanged();
    }

    // Button "Setup Production Lines": 4 random bricks on every production line
    public void SetupProductionLines()
    {
        ManuellStoppen();
        factory.SetupProductionLines();
        robotMessage = "";
        SyncBricks();
        NotifyBricksChanged();
        NotifyOrderChanged();
        NotifyRobotPositionChanged();
    }

    // Button "GoToPickUp": the robot drives to the next brick of the logistics plan and grabs it
    public void GoToPickUp()
    {
        if (!running)
        {
            robotMessage = "Press Start first.";
            OnPropertyChanged(nameof(OrderInfo));
            return;
        }

        ManuellStoppen();
        autoVersuche = 0;
        factory.GoToPickUp();
        GoToErgebnisAuswerten(AutoModus.abholen);
    }

    // Button "GoToDelivery": the robot drives to the delivery line with its brick
    public void GoToDelivery()
    {
        if (!running)
        {
            robotMessage = "Press Start first.";
            OnPropertyChanged(nameof(OrderInfo));
            return;
        }

        ManuellStoppen();
        factory.GoToDelivery();
        GoToErgebnisAuswerten(AutoModus.liefern);
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
        ManuellStoppen();
        factory.RobotResetRotation();
        NotifyRobotPositionChanged();
    }

    // The black square: stops the robot (also an automatic drive)
    public void StopRobot()
    {
        ManuellStoppen();
        OnPropertyChanged(nameof(OrderInfo));
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
        OnPropertyChanged(nameof(OrderInfo));
        NotifyRobotPositionChanged();
    }

    // ---------- Private helpers ----------

    // Stops the continuous movement and an automatic drive
    private void ManuellStoppen()
    {
        factory.RobotStoppen();
        robotTimer.Stop();
        autoModus = AutoModus.keiner;
    }

    private void RobotCommand(Direction direction)
    {
        if (!running)
        {
            return;
        }

        if (autoModus != AutoModus.keiner)
        {
            ManuellStoppen();
            robotMessage = "";
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

        if (autoModus != AutoModus.keiner)
        {
            ManuellStoppen();
            robotMessage = "";
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

    // Starts the automatic drive, or shows why it is not possible
    private void GoToErgebnisAuswerten(AutoModus modus)
    {
        GoToResult ergebnis = factory.GoToErgebnis;

        if (ergebnis == GoToResult.started)
        {
            autoModus = modus;
            if (modus == AutoModus.abholen)
                robotMessage = "Driving to the brick...";
            else
                robotMessage = "Driving to the delivery line...";
            robotTimer.Start();
        }
        else
        {
            autoModus = AutoModus.keiner;
            robotTimer.Stop();

            if (ergebnis == GoToResult.noOrder)
                robotMessage = "Click \"Order\" first.";
            else if (ergebnis == GoToResult.orderComplete)
                robotMessage = "Nothing left to pick up.";
            else if (ergebnis == GoToResult.noBrickAvailable)
                robotMessage = "No needed brick on the lines: click Setup.";
            else if (ergebnis == GoToResult.alreadyHolding)
                robotMessage = "The robot holds a brick: use GoToDelivery.";
            else if (ergebnis == GoToResult.noBrickHeld)
                robotMessage = "No brick to deliver: use GoToPickUp.";
            else
                robotMessage = "No route found.";
        }

        OnPropertyChanged(nameof(OrderInfo));
        NotifyRobotPositionChanged();
    }

    // Called when the automatic route is finished
    private void AutoAbschliessen()
    {
        if (autoModus == AutoModus.abholen)
        {
            if (factory.Robot.Dockobjekt != null)
            {
                robotMessage = "Picked up a " + factory.Robot.Dockobjekt.Color + " brick.";
                ManuellStoppen();
            }
            else if (autoVersuche < Parameters.maxPickupTries)
            {
                // The brick moved away on its line: plan the route again
                autoVersuche++;
                factory.GoToPickUp();
                GoToErgebnisAuswerten(AutoModus.abholen);
                return;
            }
            else
            {
                robotMessage = "Could not grab the brick: try GoToPickUp.";
                ManuellStoppen();
            }
        }
        else if (autoModus == AutoModus.liefern)
        {
            if (factory.Robot.RouteBlockiert)
                robotMessage = "The route was blocked.";
            else
                robotMessage = "At the delivery line: press Undock.";
            ManuellStoppen();
        }

        OnPropertyChanged(nameof(OrderInfo));
    }

    private void RobotTimer_Tick(object? sender, EventArgs e)
    {
        factory.RobotWeiterBewegen();
        NotifyRobotPositionChanged();

        if (autoModus != AutoModus.keiner && !factory.Robot.RouteAktiv)
        {
            AutoAbschliessen();
        }
    }

    private void BrickTimer_Tick(object? sender, EventArgs e)
    {
        factory.LegoBricksBewegen();
        SyncBricks();
        NotifyBricksChanged();
        NotifyOrderChanged();
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

    private void NotifyOrderChanged()
    {
        OnPropertyChanged(nameof(YellowOrdered));
        OnPropertyChanged(nameof(BlueOrdered));
        OnPropertyChanged(nameof(RedOrdered));
        OnPropertyChanged(nameof(GreenOrdered));
        OnPropertyChanged(nameof(YellowDelivered));
        OnPropertyChanged(nameof(BlueDelivered));
        OnPropertyChanged(nameof(RedDelivered));
        OnPropertyChanged(nameof(GreenDelivered));
        OnPropertyChanged(nameof(OrderInfo));
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