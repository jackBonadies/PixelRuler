using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Wpf.Ui.Controls;
using System.Windows.Controls.Primitives;

namespace PixelRuler.CustomControls
{
    public class CardExpanderButtonHidden : CardExpander
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //var toggleButton = FindChild<ToggleButton>(this, string.Empty);
            //var chevronGrid = this.GetTemplateChild("ExpanderToggleButton");
            //chevronGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

    }
}
