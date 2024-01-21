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
}
