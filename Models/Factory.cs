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
    private List<Rectangle> productionLines = new List<Rectangle>();
    private List<LegoBrick> allBricks = new List<LegoBrick>();

    private int redDelivered;
    private int greenDelivered;
    private int blueDelivered;
    private int yellowDelivered;

    private int undockZaehler;                       // counts the undocked bricks (every 2nd one is defective)
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

        productionRed = new ProductionLine(LegoColor.red, ViewParameters.RectRed, true, Direction.right, true);
        productionBlue = new ProductionLine(LegoColor.blue, ViewParameters.RectBlue, true, Direction.left, true);
        productionYellow = new ProductionLine(LegoColor.yellow, ViewParameters.RectYellow, false, Direction.up, true);
        productionGreen = new ProductionLine(LegoColor.green, ViewParameters.RectGreen, false, Direction.up, true);

        // The recycle line does not move and starts empty (the color is not used there)
        recycleLine = new ProductionLine(LegoColor.red, ViewParameters.RectRecycle, false, Direction.up, false);

        CollectBricks(productionRed);
        CollectBricks(productionGreen);
        CollectBricks(productionBlue);
        CollectBricks(productionYellow);
    }

    public Robot Robot
    {
        get { return robot; }
    }

    public ProductionLine ProductionRed
    {
        get { return productionRed; }
    }

    public ProductionLine ProductionGreen
    {
        get { return productionGreen; }
    }

    public ProductionLine ProductionBlue
    {
        get { return productionBlue; }
    }

    public ProductionLine ProductionYellow
    {
        get { return productionYellow; }
    }

    public ProductionLine RecycleLine
    {
        get { return recycleLine; }
    }

    public List<LegoBrick> AllBricks
    {
        get { return allBricks; }
    }

    public int RedDelivered
    {
        get { return redDelivered; }
    }

    public int GreenDelivered
    {
        get { return greenDelivered; }
    }

    public int BlueDelivered
    {
        get { return blueDelivered; }
    }

    public int YellowDelivered
    {
        get { return yellowDelivered; }
    }

    public UndockResult UndockErgebnis
    {
        get { return undockErgebnis; }
    }

    public bool LetzteQualitaetGut
    {
        get { return letzteQualitaetGut; }
    }

    // ---------- Robot ----------

    public void RobotBewegen(Direction direction)
    {
        robot.Bewegen(direction);
        robot.DockingPruefen(allBricks);
    }

    public void RobotDrehen(int degrees)
    {
        robot.Drehen(degrees);
        robot.DockingPruefen(allBricks);
    }

    public void RobotResetRotation()
    {
        robot.ResetRotation();
        robot.DockingPruefen(allBricks);
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
    }

    public void RobotWeiterBewegen()
    {
        robot.WeiterBewegen();
        robot.DockingPruefen(allBricks);
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

        // Every second brick is defective: 1st good, 2nd defective, 3rd good, ...
        undockZaehler++;
        letzteQualitaetGut = (undockZaehler % 2 == 1);

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

        robot.DockingPruefen(allBricks);
    }

    // ---------- Private helpers ----------

    private void CollectBricks(ProductionLine line)
    {
        foreach (LegoBrick brick in line.Bricks)
        {
            allBricks.Add(brick);
        }
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

    // The brick reached the customer: count it and let the production line make a new one
    private void KundeAbschliessen(LegoBrick brick)
    {
        allBricks.Remove(brick);

        ProductionLine line = productionRed;

        if (brick.Color == LegoColor.red)
        {
            redDelivered++;
            line = productionRed;
        }
        if (brick.Color == LegoColor.green)
        {
            greenDelivered++;
            line = productionGreen;
        }
        if (brick.Color == LegoColor.blue)
        {
            blueDelivered++;
            line = productionBlue;
        }
        if (brick.Color == LegoColor.yellow)
        {
            yellowDelivered++;
            line = productionYellow;
        }

        line.ErsatzProduzieren();
        if (line.NeuesBrick != null)
        {
            allBricks.Add(line.NeuesBrick);
        }
    }

    // The defective brick reached the end of the recycle arrow: it goes to the recycle line
    private void RecycleAufnehmen(LegoBrick brick)
    {
        if (recycleLine.FreierPlatzVorhanden)
        {
            brick.Reparatur();
            recycleLine.PlatzBelegen(brick);
        }
        else
        {
            // No free place yet: wait at the end of the arrow
            brick.SetzePose(ViewParameters.ToRecycleTargetX, brick.Y, 0);
        }
    }
}