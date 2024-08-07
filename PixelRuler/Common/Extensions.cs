﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

        public static void SetStrokeToAnnotationColor(this Shape shape)
        {
            shape.SetResourceReference(Line.StrokeProperty, App.AnnotationColorKey);
        }

        public static string SanitizeUnderscores(this string uiString)
        {
            if (uiString.Contains('_'))
            {
                return uiString.Replace("_", "__");
            }
            return uiString;
        }
    }

    public static class RectExtensions
    {
        /// <summary>
        /// SysWin Rect to Drawing Rect
        /// </summary>
        public static System.Drawing.Rectangle ToRectangle(this System.Windows.Rect rect)
        {
            return new System.Drawing.Rectangle(
                new System.Drawing.Point((int)rect.Left, (int)rect.Top),
                new System.Drawing.Size((int)rect.Size.Width, (int)rect.Size.Height));
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
            if (hexColor.Length == 8)
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

    public static class WindowExt
    {
        public static void EnsureWithinBounds(this System.Windows.Window window)
        {
            if (window.WindowState == System.Windows.WindowState.Normal)
            {
                var xCenter = window.Left + window.ActualWidth / 2;
                var yCenter = window.Top + window.ActualHeight / 2;
                // todo: Use non scaled point?
                var relevantScreen = WpfScreenHelper.Screen.FromPoint(new(xCenter, yCenter));
                if (window.ActualHeight > relevantScreen.WpfWorkingArea.Height)
                {
                    window.Height = relevantScreen.WpfWorkingArea.Height;
                }

                var bottomWpf = window.ActualHeight + window.Top;
                if (bottomWpf > relevantScreen.WpfWorkingArea.Height)
                {
                    var availableSpace = relevantScreen.WpfWorkingArea.Height;
                    window.Top -= (bottomWpf - availableSpace);
                }
            }
        }
    }

    public static class ServicesExt
    {
        public static IServiceCollection AddTransientFromNamespace(
            this IServiceCollection services,
            string namespaceName,
            params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<Type> types = assembly
                    .GetTypes()
                    .Where(
                        x =>
                            x.IsClass
                            && x.Namespace != null
                            && x.Namespace.StartsWith(namespaceName, StringComparison.InvariantCultureIgnoreCase)
                    );

                foreach (Type? type in types)
                {
                    if (services.All(x => x.ServiceType != type))
                    {
                        _ = services.AddTransient(type);
                    }
                }
            }

            return services;
        }
    }
}
