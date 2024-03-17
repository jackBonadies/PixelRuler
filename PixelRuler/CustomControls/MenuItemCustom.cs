using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixelRuler.CustomControls
{
    /// <summary>
    /// Clean up default WpfUI MenuItem regarding Input Gesture Text
    /// </summary>
    /// <remarks>
    /// Fixes vertical alignment, make font darker and slightly larger
    /// </remarks>
    public class MenuItemCustom : Wpf.Ui.Controls.MenuItem
    {
        public override void OnApplyTemplate()
        {
            var inputGestureText = this.GetTemplateChild("InputGestureText") as System.Windows.Controls.TextBlock;
            _ = inputGestureText ?? throw new ArgumentNullException(nameof(inputGestureText));
            inputGestureText.VerticalAlignment = VerticalAlignment.Center;
            inputGestureText.Opacity = .75;
            inputGestureText.FontSize = 12;
            inputGestureText.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            base.OnApplyTemplate();
        }
    }
}
