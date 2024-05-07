using PixelRuler.Common;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PixelRuler.CustomControls
{
    public class CopyClipboardButton : Button
    {
        protected override void OnClick()
        {
            var txtBlock = UiUtils.FindChild<TextBlock>(this.Content as DependencyObject);
            if (txtBlock == null)
            {
                throw new ArgumentNullException("Cannot find copy element");
            }
            try
            {
                Clipboard.SetText(txtBlock.Text);
            }
            catch (Exception)
            {
                // occurs if Copy is spammed
            }
            base.OnClick(); // otherwise triggers wont fire.
        }
    }
}
