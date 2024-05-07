using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PixelRuler
{
    public class ColorAnnotationsBundle : IEqualityComparer<ColorAnnotationsBundle>
    {
        public string Key;
        public System.Drawing.Color AnnotationColor { get; set; }
        public System.Drawing.Color AnnotationColorText { get; set; }
        public System.Drawing.Color LabelColorBackground { get; set; }

        public ColorAnnotationsBundle(
            string key,
            System.Drawing.Color annotationColor,
            System.Drawing.Color? annotationTextColor = null,
            System.Drawing.Color? labelColorBackground = null)
        {
            Key = key;
            AnnotationColor = annotationColor;

            if (annotationTextColor.HasValue)
            {
                AnnotationColorText = annotationTextColor.Value;
            }
            else
            {
                AnnotationColorText = System.Drawing.Color.FromArgb(
                 255,
                 (int)(annotationColor.R * .5),
                 (int)(annotationColor.G * .5),
                 (int)(annotationColor.B * .5));
            }

            if (labelColorBackground.HasValue)
            {
                LabelColorBackground = labelColorBackground.Value;
            }
            else
            {

                var lum = (0.2126 * AnnotationColorText.R + 0.7152 * AnnotationColorText.G + 0.0722 * AnnotationColorText.B);
                if (lum < 150.0 / 256)
                {
                    LabelColorBackground = System.Drawing.Color.White;
                }
                else
                {
                    LabelColorBackground = System.Drawing.Color.Black;
                }
            }
        }

        public bool Equals(ColorAnnotationsBundle? x, ColorAnnotationsBundle? y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x != null && y == null)
            {
                return false;
            }
            if (x == null && y != null)
            {
                return false;
            }
            return x.Key == y.Key;
        }

        public int GetHashCode([DisallowNull] ColorAnnotationsBundle obj)
        {
            return (Key?.GetHashCode() ?? 0);
        }
    }
}
