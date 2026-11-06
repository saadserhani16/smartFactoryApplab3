using System;
using System.Collections.Generic;
using System.Drawing;

namespace smartFactoryApp;

public class Factory
{
    private Robot robot;
    private ProductionLine productionRed;
    private ProductionLine productionGreen;
    private ProductionLine productionBlue;
    private ProductionLine productionYellow;
    private ProductionLine recycleLine;
    private List<ProductionLine> allLines = new List<ProductionLine>();
    private List<Rectangle> productionLines = new List<Rectangle>();
    private List<LegoBrick> allBricks = new List<LegoBrick>();

    private Random random = new Random();
    private Logistics logistics;
    private LegoBrick? zielBrick;                    // the brick GoToPickUp wants to grab
    private GoToResult goToErgebnis;
    private UndockResult undockErgebnis;
    private bool letzteQualitaetGut;

    public Factory()
    {
        // All six lines are obstacles for the robot
        productionLines.Add(ViewParameters.RectRed);
        productionLines.Add(ViewParameters.RectBlue);
        productionLines.Add(ViewParameters.RectYellow);
        productionLines.Add(ViewParameters.RectGreen);
        productionLines.Add(ViewParameters.RectRecycle);
        productionLines.Add(ViewParameters.RectDelivery);

        robot = new Robot(ViewParameters.RobotPose.X, ViewParameters.RobotPose.Y,
                          ViewParameters.Borders, productionLines);

        // The production lines start empty
        productionRed = new ProductionLine("red line", ViewParameters.RectRed, true, Direction.right, true);
        productionBlue = new ProductionLine("blue line", ViewParameters.RectBlue, true, Direction.left, true);
        productionYellow = new ProductionLine("yellow line", ViewParameters.RectYellow, false, Direction.up, true);
        productionGreen = new ProductionLine("green line", ViewParameters.RectGreen, false, Direction.up, true);

        // The recycle line does not move and has 4 places
        recycleLine = new ProductionLine("recycle line", ViewParameters.RectRecycle, false, Direction.up, false);

        allLines.Add(productionRed);
        allLines.Add(productionBlue);
        allLines.Add(productionYellow);
        allLines.Add(productionGreen);
        allLines.Add(recycleLine);

        logistics = new Logistics(random);
    }

    public Robot Robot
    {
        get { return robot; }
    }

    public Logistics Logistik
    {
        get { return logistics; }
    }

    public ProductionLine RecycleLine
    {
        get { return recycleLine; }
    }

    public List<LegoBrick> AllBricks
    {
        get { return allBricks; }
    }

    public GoToResult GoToErgebnis
    {
        get { return goToErgebnis; }
    }

    public UndockResult UndockErgebnis
    {
        get { return undockErgebnis; }
    }

    public bool LetzteQualitaetGut
    {
        get { return letzteQualitaetGut; }
    }

    // ---------- Order and setup ----------

    // Click on "Setup Production Lines": every line gets 4 new bricks with random colors
    public void SetupProductionLines()
    {
        robot.Stoppen();
        zielBrick = null;

        ProductionLine[] lines = { productionRed, productionBlue, productionYellow, productionGreen };
        foreach (ProductionLine line in lines)
        {
            foreach (LegoBrick brick in line.Bricks)
            {
                if (brick.Status == BrickStatus.onLine)
                {
                    allBricks.Remove(brick);
                }
            }

            line.Leeren();
            line.Fuellen(random);

            foreach (LegoBrick neu in line.NeueBricks)
            {
                allBricks.Add(neu);
            }
        }

        PlanAktualisieren();
    }

    // Click on "Order": a new random order
    public void CreateOrder()
    {
        robot.Stoppen();
        zielBrick = null;
        logistics.CreateRandomOrder();
        PlanAktualisieren();
    }

    // ---------- Robot ----------

    public void RobotBewegen(Direction direction)
    {
        robot.Bewegen(direction);
        DockingAusfuehren();
    }

    public void RobotDrehen(int degrees)
    {
        robot.Drehen(degrees);
        DockingAusfuehren();
    }

    public void RobotResetRotation()
    {
        robot.ResetRotation();
        DockingAusfuehren();
    }

    public void RobotRichtung(Direction direction)
    {
        robot.SetzeRichtung(direction);
    }

    public void RobotDrehrichtung(int direction)
    {
        robot.SetzeDrehrichtung(direction);
    }

    public void RobotStoppen()
    {
        robot.Stoppen();
        zielBrick = null;
    }

    public void RobotWeiterBewegen()
    {
        robot.WeiterBewegen();
        DockingAusfuehren();
    }

    // The robot drives to the next brick of the logistics plan and grabs it.
    // The result is stored in GoToErgebnis.
    public void GoToPickUp()
    {
        if (robot.Dockobjekt != null)
        {
            goToErgebnis = GoToResult.alreadyHolding;
            return;
        }
        if (!logistics.HasOrder)
        {
            goToErgebnis = GoToResult.noOrder;
            return;
        }

        PlanAktualisieren();

        if (logistics.OrderComplete)
        {
            goToErgebnis = GoToResult.orderComplete;
            return;
        }

        LegoBrick? ziel = logistics.NextBrick;
        ProductionLine? line = logistics.PickLine;
        if (ziel == null || line == null)
        {
            goToErgebnis = GoToResult.noBrickAvailable;
            return;
        }

        zielBrick = ziel;
        robot.RouteZumBrick(ziel, line.AnfahrWinkel / 90);
        if (!robot.RouteGefunden)
        {
            zielBrick = null;
            goToErgebnis = GoToResult.noRoute;
            return;
        }

        DockingAusfuehren();   // maybe the gripper is already close enough
        goToErgebnis = GoToResult.started;
    }

    // The robot drives to the delivery line with the brick it holds
    public void GoToDelivery()
    {
        if (robot.Dockobjekt == null)
        {
            goToErgebnis = GoToResult.noBrickHeld;
            return;
        }

        zielBrick = null;
        robot.RouteZurDelivery(ViewParameters.RectDelivery);
        if (!robot.RouteGefunden)
        {
            goToErgebnis = GoToResult.noRoute;
            return;
        }

        goToErgebnis = GoToResult.started;
    }

    // Undocking only works on the delivery line. The result is stored in UndockErgebnis.
    public void RobotUndock()
    {
        LegoBrick? brick = robot.Dockobjekt;

        if (brick == null)
        {
            undockErgebnis = UndockResult.noBrick;
            return;
        }

        brick.PruefeDeliveryLinie(ViewParameters.RectDelivery);
        if (!brick.AufDeliveryLinie)
        {
            undockErgebnis = UndockResult.notOnDeliveryLine;
            return;
        }

        // 50% chance: the brick is good or defective (random, not 1 out of 2)
        letzteQualitaetGut = (random.Next(2) == 0);

        robot.Undock();
        brick.Lieferung(letzteQualitaetGut, ViewParameters.DeliverySlotX, ViewParameters.DeliverySlotY);
        undockErgebnis = UndockResult.success;
    }

    // ---------- Bricks (called by the brick timer) ----------

    public void LegoBricksBewegen()
    {
        productionRed.Bewegen();
        productionGreen.Bewegen();
        productionBlue.Bewegen();
        productionYellow.Bewegen();
        recycleLine.Bewegen();

        LieferungVerarbeiten();

        DockingAusfuehren();
    }

    // ---------- Private helpers ----------

    // The robot grabs a brick if the gripper is close enough.
    // During GoToPickUp only the chosen brick may be grabbed.
    private void DockingAusfuehren()
    {
        robot.DockingPruefen(allBricks, zielBrick);

        if (zielBrick != null && robot.Dockobjekt != null)
        {
            zielBrick = null;
            robot.RouteAbbrechen();
        }
    }

    private void PlanAktualisieren()
    {
        logistics.PlanBerechnen(allBricks, allLines,
                                robot.X + Parameters.robotWidth / 2.0,
                                robot.Y + Parameters.robotHeight / 2.0);
    }

    // Moves all bricks that are on their way after the undocking
    private void LieferungVerarbeiten()
    {
        List<LegoBrick> kopie = new List<LegoBrick>(allBricks);

        foreach (LegoBrick brick in kopie)
        {
            if (brick.Status == BrickStatus.onDeliveryLine || brick.Status == BrickStatus.repairing)
            {
                brick.Warten();
            }
            else if (brick.Status == BrickStatus.toCustomer)
            {
                brick.ZumKunden();
                if (brick.X >= ViewParameters.ToCustomerTargetX)
                {
                    KundeAbschliessen(brick);
                }
            }
            else if (brick.Status == BrickStatus.toRecycle)
            {
                brick.ZumRecycling();
                if (brick.X <= ViewParameters.ToRecycleTargetX)
                {
                    RecycleAufnehmen(brick);
                }
            }
        }
    }

    // The brick reached the customer: count it, and the line it came from makes a new brick
    private void KundeAbschliessen(LegoBrick brick)
    {
        allBricks.Remove(brick);
        logistics.BrickDelivered(brick.Color);

        ProductionLine? line = brick.Herkunft;
        if (line != null)
        {
            line.ErsatzProduzieren((LegoColor)random.Next(4));
            if (line.NeuesBrick != null)
            {
                allBricks.Add(line.NeuesBrick);
            }
        }

        PlanAktualisieren();
    }

    // The defective brick reached the end of the recycle arrow: it goes to the recycle line
    // (capacity: 4 bricks). If the line is full, the brick waits at the end of the arrow.
    private void RecycleAufnehmen(LegoBrick brick)
    {
        if (recycleLine.FreierPlatzVorhanden)
        {
            brick.Reparatur();
            recycleLine.PlatzBelegen(brick);
        }
        else
        {
            brick.SetzePose(ViewParameters.ToRecycleTargetX, brick.Y, 0);
        }
    }
}