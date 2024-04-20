using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Clipboard.SetText(txtBlock.Text);
            base.OnClick(); // otherwise triggers wont fire.
        }
    }
}
