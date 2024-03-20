using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler.ViewModels
{
    /// <summary>
    /// Main Window with a few extra commands (pop out, close)
    /// </summary>
    public class ScreenshotWindowViewModel : PixelRulerViewModel
    {
        public ScreenshotWindowViewModel(SettingsViewModel? settings = null) : base(settings)
        {
            PopOutWindowCommand = new RelayCommandFull((object? o) => { PopOutNewWindow(); }, System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Control, "New Window");
        }

        private RelayCommandFull popOutWindowCommand;
        public RelayCommandFull PopOutWindowCommand
        {
            get
            {
                return popOutWindowCommand;
            }
            set
            {
                if(popOutWindowCommand != value)
                {
                    popOutWindowCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        public void PopOutNewWindow()
        {
            CloseWindowCommand.Execute(null);
            var mainWindow = new MainWindow(this);
            mainWindow.Show();
        }
    }
}
