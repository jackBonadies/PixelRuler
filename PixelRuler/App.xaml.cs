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
        public const string ProgramInstancesGUID = "18055682-7245-4d0e-9e60-dcb71ce17da7";

        protected override void OnStartup(StartupEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("PixelRulerStartup");
            Semaphore semaphore = new Semaphore(2, 2, ProgramInstancesGUID, out bool wasCreated);
            bool res = semaphore.WaitOne(0);
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

            m = new Mutex(true, "Global\\PixelRuler_SingleProcess_Global", out bool createdNew);
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

            var rootWindow = new RootWindow(mainViewModel);
            rootWindow.DataContext = mainViewModel;
            rootWindow.SettingsViewModel = settingsViewModel;
            rootWindow.Show();



            settingsViewModel.SetState();

        }

        private Mutex m;

        private SettingsViewModel settingsViewModel;


        private const string pipeName = "PR_GUID12_pipe";

        void PipeServer()
        {
            while(true)
            {
                using (var server = new NamedPipeServerStream(pipeName))
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
                                var mainViewModel = new PixelRulerViewModel(settingsViewModel);
                                MainWindow mainWindow = new MainWindow(mainViewModel);
                                mainWindow.NewFullScreenshot(false);
                                mainWindow.Show();
                            });

                            Console.WriteLine($"Received args from second instance: {args}");
                        }
                }
            }
        }

        static void PipeClient(string[] args)
        {
            using (var client = new NamedPipeClientStream(pipeName))
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

        public const string AnnotationColorKey = "AnnotationColor";

    }
}
