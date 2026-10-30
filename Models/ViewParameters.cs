using System.Drawing;

namespace smartFactoryApp;

public static class ViewParameters
{
    // Top-left corner of the gray area in the window, then its width and height
    public static Rectangle Borders { get; } = new Rectangle(37, 35, 525, 525);

    // The six production lines (white boxes)
    public static Rectangle RectRed { get; } = new Rectangle(Borders.X + 73, Borders.Y + 94, 60, 160);
    public static Rectangle RectBlue { get; } = new Rectangle(Borders.X + 393, Borders.Y + 94, 60, 160);
    public static Rectangle RectRecycle { get; } = new Rectangle(Borders.X + 183, Borders.Y + 254, 160, 60);
    public static Rectangle RectYellow { get; } = new Rectangle(Borders.X + 53, Borders.Y + 394, 160, 60);
    public static Rectangle RectGreen { get; } = new Rectangle(Borders.X + 313, Borders.Y + 394, 160, 60);
    public static Rectangle RectDelivery { get; } = new Rectangle(Borders.X + 183, Borders.Y + 3, 186, 56);

    // The two arrows next to the delivery line
    public static Rectangle RectToRecycle { get; } = new Rectangle(Borders.X + 62, Borders.Y + 3, 121, 56);
    public static Rectangle RectToCustomer { get; } = new Rectangle(Borders.X + 369, Borders.Y + 3, 120, 56);

    // Where an undocked brick is placed (middle of the Delivery cell)
    public static double DeliverySlotX { get; } = Borders.X + 270;
    public static double DeliverySlotY { get; } = Borders.Y + 34;

    // Where the bricks leave the factory
    public static double ToCustomerTargetX { get; } = Borders.X + 469;
    public static double ToRecycleTargetX { get; } = Borders.X + 82;

    // Start pose of the robot (top-left corner of the robot)
    public static Pose RobotPose { get; } = new Pose(Borders.X + 243, Borders.Y + 165, 0);
}