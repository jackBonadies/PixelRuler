using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace PixelRuler.ViewModels
{
    public partial class PinViewModel : ObservableObject
    {
        public PinViewModel(PixelRulerViewModel prvm)
        {
            this.MainViewModel = prvm;
        }

        [RelayCommand]
        private void ToggleAlwaysOnTop()
        {
            AlwaysOnTop = !AlwaysOnTop;
        }

        [RelayCommand]
        private void OpenPixelRuler()
        {
            var mainWindow = new MainWindow(this.MainViewModel);
            mainWindow.Show();
        }

        [ObservableProperty]
        private bool alwaysOnTop = true;

        public RelayCommand CloseCommand { get; set; }

        public PixelRulerViewModel MainViewModel { get; set; }

        public static readonly Thickness PinWindowThickness = new Thickness(5);
    }
}
