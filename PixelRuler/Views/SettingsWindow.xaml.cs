using PixelRuler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.ApplicationModel;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : ThemeWindow
    {
        public SettingsWindow(PixelRulerViewModel viewModel)
        {
            var workArea = SystemParameters.WorkArea;
            this.MaxHeight = workArea.Height - 30;

            this.DataContext = viewModel.Settings;
            viewModel.Settings.EditShortcutCommandEvent += Settings_EditShortcutCommandEvent;

            InitializeComponent();
        }

        private async void Settings_EditShortcutCommandEvent(object? sender, ShortcutInfo shortcutInfo)
        {
            var pendingShortcutInfo = new PendingShortcutInfo(shortcutInfo);
            var contentDialog = new ContentDialog(RootContentDialog)
            {
                Title = "Set Shortcut",
                Content = new ConfigureShortcutView(pendingShortcutInfo),
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                MinWidth = 400,
            };
            contentDialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, new Binding("IsValid") { Source = pendingShortcutInfo });

            var result = await contentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.None: // cancel
                    break;
                case ContentDialogResult.Primary:
                    pendingShortcutInfo.UpdateShortcut();
                    break;
            }
        }

        private void CardExpander_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as Expander).IsExpanded = !(sender as Expander).IsExpanded;
        }

        private void CardExpanderContent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // do not close on mouse up
            e.Handled = true;
        }
    }
}
