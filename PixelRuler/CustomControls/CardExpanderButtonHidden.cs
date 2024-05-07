using Wpf.Ui.Controls;

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
