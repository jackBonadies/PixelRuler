using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelRuler.Models
{
    public partial class ToastNotifColorViewModel : ObservableObject
    {
        public ToastNotifColorViewModel(System.Drawing.Color color, string colorText)
        {
            this.Color = color;
            this.ColorText = colorText;
        }

        [ObservableProperty]
        System.Drawing.Color color;
        [ObservableProperty]
        string colorText;
    }
}
