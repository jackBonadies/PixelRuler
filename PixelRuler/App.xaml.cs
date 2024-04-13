using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Wpf.Ui.Tray.Controls;

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public class CommandLineArg

        private const string backgroundCmdLineArg = "background";

        protected override void OnStartup(StartupEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("PixelRulerStartup");
            base.OnStartup(e);

            bool backgroundOnly = false;

            if(e.Args.Length > 0)
            {
                var cmdLineArg = e.Args[0];
                if(cmdLineArg.Replace("-", "").ToLower() == backgroundCmdLineArg)
                {
                    backgroundOnly = true;
                }
            }

            singleProcessMutex = new Mutex(true, "Global\\PixelRuler_SingleProcess_Global", out bool createdNew);
            if(createdNew)
            {
                Task.Run(() => PipeServer());
            }
            else
            {
                PipeClient(e.Args);
                Environment.Exit(0);
            }
            
            settingsViewModel = new SettingsViewModel();
            var mainViewModel = new PixelRulerViewModel(settingsViewModel);

            var rootWindow = new RootWindow(new RootViewModel(settingsViewModel));
            rootWindow.Show();

            settingsViewModel.SetState();

        }

        private Mutex singleProcessMutex = null!;

        private SettingsViewModel settingsViewModel = null!;

        private const string pipe_name = "PixelRuler_0c87c02590af41bda768a872ddd91ee3";

        void PipeServer()
        {
            while(true)
            {
                using (var server = new NamedPipeServerStream(pipe_name))
                {
                        server.WaitForConnection();
                        using (var sr = new StreamReader(server))
                        {
                            string args = sr.ReadLine();
                            //switch (args)
                            //{
                            //    case "focus":
                            //        MainWindow mainWindow = new MainWindow(mainViewModel);
                            //        mainWindow.NewFullScreenshot(false);
                            //        mainWindow.Show();
                            //        break;
                            //}
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                switch(args)
                                {
                                    case "screenshot":
                                        App.NewFullscreenshotLogic(this.settingsViewModel, true);
                                        break;
                                    case "windowed":
                                        App.EnterScreenshotTool(this.settingsViewModel, OverlayMode.Window, true);
                                        break;
                                    default:
                                        if(string.IsNullOrEmpty(args))
                                        {
                                            App.NewFullscreenshotLogic(this.settingsViewModel, true);
                                        }
                                        else
                                        {
                                            App.OpenFileLogic(this.settingsViewModel, true, args);
                                        }
                                        break;
                                }
                            });

                            Console.WriteLine($"Received args from second instance: {args}");
                        }
                }
            }
        }

        public static void OpenFileLogic(SettingsViewModel settingsViewModel, bool newWindow, string filePath)
        {
            MainWindow mainWindow = new MainWindow(new PixelRulerViewModel(settingsViewModel));
            mainWindow.SetImage(filePath);
            mainWindow.Show();
        }

        public static void NewFullscreenshotLogic(SettingsViewModel settingsViewModel, bool newWindow)
        {
            MainWindow mainWindow = new MainWindow(new PixelRulerViewModel(settingsViewModel));
            mainWindow.NewFullScreenshot(newWindow);
            mainWindow.Show();
        }

        public static async Task EnterScreenshotTool(SettingsViewModel settingsViewModel, OverlayMode mode, bool newWindow)
        {
            MainWindow mainWindow = new MainWindow(new PixelRulerViewModel(settingsViewModel));
            var res = await mainWindow.NewWindowedScreenshot(mode, newWindow);
            if(res)
            {
                mainWindow.Show();
            }
        }

        static void PipeClient(string[] args)
        {
            using (var client = new NamedPipeClientStream(pipe_name))
            {
                client.Connect();
                using (var sw = new StreamWriter(client))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(string.Join(' ', args));
                }
            }
        }

        private void Screenshot_Click(object? sender, EventArgs e)
        {

        }

        private void Settings_Click(object? sender, EventArgs e)
        {

        }

        private void QuitItem_Click(object? sender, EventArgs e)
        {

        }

        public static void ShowSettingsWindowSingleInstance(SettingsViewModel settings)
        {
            var settingsWindow = App.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
            if (settingsWindow != null)
            {
                settingsWindow.Activate();
                return;
            }
            new SettingsWindow(settings).Show();
        }

        public static readonly double[] ZoomSelections = new double[]
        {
            50,
            100,
            200,
            400,
            800,
            1600,
            3200,
            6400,
            12800,
            25600,
        };

        public const string DefaultSavePath = "%USERPROFILE%\\Pictures\\Screenshots\\PixelRuler";

        public const double BorderSizeDpiIndependentUnits = 24;

        public static readonly int ResizeSpeedFactor = 1;
        public static readonly bool FloatingZoomBoxPosAllowed = true;

        public static readonly double MinZoomPercent = ZoomSelections[0];
        public static readonly double MaxZoomPercent = ZoomSelections[ZoomSelections.Length - 1];

        public const int SHAPE_INDEX = 500;
        public const int COLOR_PICKER_INDEX = 500;
        public const int LABEL_INDEX = 505;
        public const int MANIPULATE_HITBOX_INDEX = 510;
        public const int RESIZE_INDEX = 515;

        public const int FULLSCREEN_HOTKEY_ID = 8000;
        public const int WINDOWED_HOTKEY_ID = 8001;
        public const int REGION_WINDOWED_HOTKEY_ID = 8002;
        public const int QUICK_MEASURE_HOTKEY_ID = 8003;
        public const int QUICK_COLOR_HOTKEY_ID = 8004;

        public const string AnnotationColorKey = "AnnotationColor";

    }
}
