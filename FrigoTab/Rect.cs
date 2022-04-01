using System.Drawing;


namespace FrigoTab {
    public struct Rect {
        private readonly Point topLeft;
        private readonly Point bottomRight;

        public Rect(Rectangle bounds)
        {
            topLeft = bounds.Location;
            bottomRight = new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height);
        }

        private Rect(Point topLeft, Point bottomRight)
        {
            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
        }

        public Rect ScreenToClient(WindowHandle window)
        {
            return new(topLeft.ScreenToClient(window), bottomRight.ScreenToClient(window));
        }

        public Size Size()
        {
            return new(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }
    }
}
