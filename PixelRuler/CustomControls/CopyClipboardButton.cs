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
                // OpenClipboard Failed (CLIPBRD_E_CANT_OPEN)
                //var proc = NativeHelpers.GetProcessHoldingClipboard();
                //if (proc == null)
                //{
                    Clipboard.SetText(txtBlock.Text);
                //}
            }
            catch (Exception)
            {
                //var proc = NativeHelpers.GetProcessHoldingClipboard();
                //Clipboard.SetDataObject(txtBlock.Text);
                // occurs if Copy is spammed
                // or if another process is holding the clipboard (android emulator open)
            }
            base.OnClick(); // otherwise triggers wont fire.
        }
    }
}
