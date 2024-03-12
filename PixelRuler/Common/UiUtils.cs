using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfScreenHelper;

namespace PixelRuler.Common
{
    public static class UiUtils
    {
        public static Line CreateLine()
        {
            var line = new Line();
            line.StrokeThickness = 1;
            line.SnapsToDevicePixels = true;
            line.UseLayoutRounding = true;
            return line;
        }

        public static Rectangle CreateRectangle()
        {
            var rect = new Rectangle();
            rect.Width = 0;
            rect.Height = 0;
            rect.StrokeThickness = 1;
            rect.SnapsToDevicePixels = true;
            return rect;
        }

        public static int GetBorderPixelSize(double dpi)
        {
            return (int)Math.Round(App.BorderSizeDpiIndependentUnits * dpi);
        }

        public static System.Windows.Rect GetFullBounds(IEnumerable<Screen> screens)
        {
            System.Windows.Rect unionRect = screens.First().Bounds;
            foreach (Screen screen in screens.Skip(1))
            {
                unionRect.Union(screen.Bounds);
            }
            return unionRect;
        }
    }
}
