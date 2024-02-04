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

        public static System.Drawing.Color Times(this System.Drawing.Color color, double factor)
        {
            return System.Drawing.Color.FromArgb(
                color.A,
                (int)(color.R * factor),
                (int)(color.G * factor),
                (int)(color.B * factor));
        }

        public static string ToRgbHexString(this System.Drawing.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static System.Drawing.Color ToWinFormColorFromRgbHex(this string hexColor)
        {
            hexColor = hexColor.Replace("#", string.Empty);

            byte a = 255;
            if(hexColor.Length == 8)
            {
                a = (byte)(Convert.ToUInt32(hexColor.Substring(0, 2), 16));
                hexColor = hexColor.Substring(2);
            }

            byte r = (byte)(Convert.ToUInt32(hexColor.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hexColor.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hexColor.Substring(4, 2), 16));

            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        private static float Divisor = byte.MaxValue;

        public static System.Drawing.Color AlphaBlend(this System.Drawing.Color backColor, System.Drawing.Color foreColor)
        {
            var fa = foreColor.A / Divisor;
            var fr = foreColor.R / Divisor;
            var fg = foreColor.G / Divisor;
            var fb = foreColor.B / Divisor;

            var ba = backColor.A / Divisor;
            var br = backColor.R / Divisor;
            var bg = backColor.G / Divisor;
            var bb = backColor.B / Divisor;

            var a = fa + ba - fa * ba;

            if (a <= 0)
                return System.Drawing.Color.Transparent;

            var r = (fa * (1 - ba) * fr + fa * ba * fa + (1 - fa) * ba * br) / a;
            var g = (fa * (1 - ba) * fg + fa * ba * fa + (1 - fa) * ba * bg) / a;
            var b = (fa * (1 - ba) * fb + fa * ba * fa + (1 - fa) * ba * bb) / a;

            return System.Drawing.Color.FromArgb(
                (int)(a * byte.MaxValue),
                (int)(r * byte.MaxValue),
                (int)(g * byte.MaxValue),
                (int)(b * byte.MaxValue));
        }
    }

    public static class PointEx
    {
        public static System.Windows.Point Add(this System.Windows.Point pt, System.Windows.Point toAdd)
        {
            return new System.Windows.Point(pt.X + toAdd.X, pt.Y + toAdd.Y);
        }
    }

    public static class LinqEx
    {
        public static void ForEachExt<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                action(item);
            }
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
