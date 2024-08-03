using System.Windows.Controls.Primitives;

namespace PixelRuler.CustomControls
{
    public class PopupHost : System.Windows.Controls.Primitives.Popup
    {
        public PopupHost(System.Windows.Rect screenRect)
        {
            RootGrid = new System.Windows.Controls.Grid();
            RootGrid.Width = 400;
            RootGrid.Height = 400;
            //RootGrid.Background = new SolidColorBrush(Colors.Black);
            this.Child = RootGrid;
            this.Placement = PlacementMode.AbsolutePoint;
            this.AllowsTransparency = true;
            this.HorizontalOffset = screenRect.Left + screenRect.Width / 2.0 - RootGrid.Width / 2.0;
            this.VerticalOffset = screenRect.Y + screenRect.Height;
        }

        public System.Windows.Controls.Grid RootGrid { get; init; }
    }
}
