using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        public static TextBlock CreateTextBlock(string? text)
        {
            var txtBlock = new TextBlock();
            txtBlock.Text = text;
            return txtBlock;
        }

        public static TextBlock CreateFontIcon(string text)
        {
            var txtBlock = CreateTextBlock(text);
            txtBlock.FontFamily = new FontFamily("Segoe MDL2 Assets");
            txtBlock.FontSize = 16;
            return txtBlock;
        }


        public static AccessText CreateAccessText(string? text)
        {
            var accessTxt = new AccessText();
            accessTxt.Text = text;
            return accessTxt;
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

        public static Point TruncatePoint(Point mousePos)
        {
            var roundX = (int)(mousePos.X);
            var roundY = (int)(mousePos.Y);
            return new Point(roundX, roundY);
        }

        public static Point RoundPoint(Point mousePos)
        {
            var roundX = Math.Round(mousePos.X);
            var roundY = Math.Round(mousePos.Y);
            return new Point(roundX, roundY);
        }

        public static System.Drawing.Bitmap CaptureScreen(Rect? bounds = null)
        {
            if (bounds == null)
            {
                var pixelWidth = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width;
                var pixelHeight = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Height;
                bounds = new Rect(0, 0, pixelWidth, pixelHeight);
            }

            var boundsVal = bounds.Value;

            var screenBounds = new System.Drawing.Size((int)boundsVal.Width, (int)boundsVal.Height);//System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var screenshot = new System.Drawing.Bitmap(screenBounds.Width, screenBounds.Height);// PixelFormat.Format32bppArgb);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen((int)boundsVal.X, (int)boundsVal.Y, 0, 0, screenBounds, System.Drawing.CopyPixelOperation.SourceCopy);
            }
            return screenshot;
        }

        public static bool IsMouseWithinBounds(this FrameworkElement element, MouseEventArgs e, double margin = 0)
        {
            bool isWithin = e.GetPosition(element).X >= -margin &&
                e.GetPosition(element).X < element.ActualWidth + margin &&
                e.GetPosition(element).Y >= -margin &&
                e.GetPosition(element).Y < element.ActualHeight + margin;
            return isWithin;
        }

        /// <summary>
        /// Find Child of Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="childName">Optional: specify child name</param>
        /// <returns></returns>
        public static T? FindChild<T>(DependencyObject? parent, string? childName = null)
           where T : DependencyObject
        {
            if (parent == null) return null;

            T? foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else if (child is T childType)
                {
                    foundChild = (T)child;
                }

                if (foundChild == null)
                {
                    foundChild = FindChild<T>(child, childName);

                    if (foundChild != null) break;
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static string FormatColor(System.Drawing.Color color, ColorFormatMode mode)
        {
            if (mode == ColorFormatMode.Hex)
            {
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            throw new NotImplementedException();
        }
    }
}
