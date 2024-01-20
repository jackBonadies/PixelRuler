using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace DesignRuler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);



            //void TrayIconOnClick(object? sender, EventArgs e) =>
            //    MessageBox.Show("Tray icon clicked!");

            System.Windows.Forms.NotifyIcon icon = new System.Windows.Forms.NotifyIcon();            //icon.Click += new EventHandler(TrayIconOnClick);
            icon.Icon = new Icon(@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\c1fdqost.uo1\ItemTemplates\ViewModel\cppwinrt.ico");
            icon.Visible = true;
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();

            var screenshot = new System.Windows.Forms.ToolStripMenuItem();
            screenshot.Text = "Screenshot";
            screenshot.Click += Screenshot_Click; ;

            var settings = new System.Windows.Forms.ToolStripMenuItem();
            settings.Text = "Settings";
            settings.Click += Settings_Click;

            var quitItem = new System.Windows.Forms.ToolStripMenuItem();
            quitItem.Text = "Quit";
            quitItem.Click += QuitItem_Click;

            contextMenu.Items.Add(quitItem);

            icon.ContextMenuStrip = contextMenu;

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
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
    }
}
