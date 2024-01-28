using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixelRuler
{
    public static class Extensions
    {
        public static ScaleTransform GetScaleTransform(this Canvas canvas)
        {
            return (canvas.RenderTransform as TransformGroup).Children.First(it => it is ScaleTransform) as ScaleTransform;
        }

        public static TranslateTransform GetTranslateTransform(this Canvas canvas)
        {
            return (canvas.RenderTransform as TransformGroup).Children.First(it => it is TranslateTransform) as TranslateTransform;
        }

        public static double GetDpi(this Visual element)
        {
            System.Windows.PresentationSource source = System.Windows.PresentationSource.FromVisual(element);
            double dpiX = source.CompositionTarget.TransformToDevice.M11;
            return dpiX;
        }

    }


    public static class WinFormColorExtensions
    {
        public static System.Windows.Media.Color ConvertToWpfColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static string ToRgbHexString(this System.Drawing.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static System.Drawing.Color ToWinFormColorFromRgbHex(this string hexColor)
        {
            hexColor = hexColor.Replace("#", string.Empty);

            byte r = (byte)(Convert.ToUInt32(hexColor.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hexColor.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hexColor.Substring(4, 2), 16));

            return System.Drawing.Color.FromArgb(255, r, g, b);
        }
    }

    public static class WpfColorExtensions
    {
        public static System.Drawing.Color ToWinformColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static string ToRgbHexString(this System.Windows.Media.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static System.Windows.Media.Color ToWpfColorFromRgbHex(this string hexColor)
        {
            hexColor = hexColor.Replace("#", string.Empty);

            byte r = (byte)(Convert.ToUInt32(hexColor.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hexColor.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hexColor.Substring(4, 2), 16));

            return System.Windows.Media.Color.FromArgb(255, r, g, b);
        }
    }
}
