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
    private List<Rectangle> productionLines = new List<Rectangle>();
    private List<LegoBrick> allBricks = new List<LegoBrick>();

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

        productionRed = new ProductionLine(LegoColor.red, ViewParameters.RectRed, true, Direction.right);
        productionBlue = new ProductionLine(LegoColor.blue, ViewParameters.RectBlue, true, Direction.left);
        productionYellow = new ProductionLine(LegoColor.yellow, ViewParameters.RectYellow, false, Direction.up);
        productionGreen = new ProductionLine(LegoColor.green, ViewParameters.RectGreen, false, Direction.up);

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

    public List<LegoBrick> AllBricks
    {
        get { return allBricks; }
    }

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

    public void RobotUndock()
    {
        robot.Undock();
    }

    public void LegoBricksBewegen()
    {
        productionRed.Bewegen();
        productionGreen.Bewegen();
        productionBlue.Bewegen();
        productionYellow.Bewegen();
        robot.DockingPruefen(allBricks);
    }

    private void CollectBricks(ProductionLine line)
    {
        foreach (LegoBrick brick in line.Bricks)
        {
            allBricks.Add(brick);
        }
    }
}