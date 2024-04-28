using PixelRuler.Common;
using PixelRuler.Models;
using PixelRuler.ViewModels;
using PixelRuler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public SettingsWindow(SettingsViewModel viewModel)
        {
            var workArea = SystemParameters.WorkArea;
            this.MaxHeight = workArea.Height - 30;

            this.DataContext = viewModel;
            InitializeComponent();

            this.BindToViewModel(true);
        }

        protected override void OnClosed(EventArgs e)
        {
            BindToViewModel(false);
            base.OnClosed(e);
        }

        public SettingsViewModel ViewModel
        {
            get
            {
                var settings = this.DataContext as SettingsViewModel;
                if(settings == null)
                {
                    throw new Exception("No View Model");
                }
                return settings;
            }
        }

        private void BindToViewModel(bool bind)
        {
            if(bind)
            {
                this.ViewModel.DeleteSavePathInfoCommand = new RelayCommand(
                async (object? o) =>
                {
                    if (o is PathSaveInfo pathSaveInfo)
                    {
                        var contentDialog = new ContentDialog(RootContentDialog)
                        {
                            Title = pathSaveInfo.DisplayName ?? string.Empty,
                            Content = this.Resources["DeleteConfirmation"],
                            MinHeight = 0,
                            DialogHeight = 130,
                            CloseButtonText = "Cancel",
                            PrimaryButtonText = "OK",
                        };
                        contentDialog.UseLayoutRounding = true;
                        contentDialog.Loaded += (object o, RoutedEventArgs e) => LimitPaddingForContentDialog(o, 6);
                        var res = await contentDialog.ShowAsync();
                        if (res == ContentDialogResult.Primary)
                        {
                            ViewModel.DeletePathItem(pathSaveInfo);
                        }
                    }
                });
                ViewModel.EditShortcutCommandEvent += Settings_EditShortcutCommandEvent;
                ViewModel.EditSavePathCommandEvent += Settings_EditSavePathCommandEvent;
                ViewModel.EditCommandTargetCommandEvent += Settings_EditCommandTargetCommandEvent;
            }
            else
            {
                this.ViewModel.DeleteSavePathInfoCommand = null;
                ViewModel.EditShortcutCommandEvent -= Settings_EditShortcutCommandEvent;
                ViewModel.EditSavePathCommandEvent -= Settings_EditSavePathCommandEvent;
                ViewModel.EditCommandTargetCommandEvent -= Settings_EditCommandTargetCommandEvent;
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            var border = UiUtils.FindChild<Border>(sender as DependencyObject, null);
            border.MaxHeight = 200;
        }

        private async void Settings_EditCommandTargetCommandEvent(object? sender, (CommandTargetInfo, bool) commandTargetArgs)
        {
            //new PathInfoEdit
            var cmdTargetInfo = commandTargetArgs.Item1;
            var pending = cmdTargetInfo.Clone();
            var pathInfoEditViewModel = new CommandTargetEditViewModel(pending, commandTargetArgs.Item2);

            var diagContents = new CommandTargetEditView();
            diagContents.DataContext = pathInfoEditViewModel;
            diagContents.Background = new SolidColorBrush(Colors.White);
            var contentDialog = new ContentDialog(RootContentDialog)
            {
                DialogMargin = new Thickness(60,0,60,0),
                DialogWidth = 2000,
                Title = commandTargetArgs.Item2 ? "Add Save Path" : "Edit Save Path",
                Content = diagContents,
                CloseButtonText = "Cancel",
                PrimaryButtonText = commandTargetArgs.Item2 ? "Save" : "Update",
            };
            //contentDialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, new Binding("IsValid") { Source = pendingShortcutInfo });
            contentDialog.UseLayoutRounding = true;
            contentDialog.Loaded += (object o, RoutedEventArgs e) => diagContents.Focus();
            contentDialog.Loaded += SetMaxWidthForContentDialog;
            contentDialog.Loaded += (object o, RoutedEventArgs e) => LimitPaddingForContentDialog(o, 20);

            var result = await contentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.None: // cancel
                    break;
                case ContentDialogResult.Primary:
                    pending.Icon = pathInfoEditViewModel.IconViewModel.CurrentIcon;
                    if(commandTargetArgs.Item2)
                    {
                        (this.DataContext as SettingsViewModel).AddCommandTargetInfo(pending);
                    }
                    else
                    {
                        (this.DataContext as SettingsViewModel).UpdateCommandTargetInfo(cmdTargetInfo, pending);
                    }

                    //pendingShortcutInfo.UpdateShortcut();
                    break;
            }
        }

        private async void Settings_EditSavePathCommandEvent(
            object? sender, 
            (PathSaveInfo pathSaveInfo, bool newPath) pathInfoArgs)
        {
            //new PathInfoEdit
            var pathSaveInfo = pathInfoArgs.pathSaveInfo;
            var pending = pathSaveInfo.Clone();
            var pathInfoEditViewModel = new PathInfoEditViewModel(pending, pathInfoArgs.newPath);

            var diagContents = new PathInfoEditView();
            diagContents.DataContext = pathInfoEditViewModel;
            diagContents.Background = new SolidColorBrush(Colors.White);
            var contentDialog = new ContentDialog(RootContentDialog)
            {
                DialogMargin = new Thickness(60,0,60,0),
                DialogWidth = 2000,
                Title = pathInfoArgs.newPath ? "Add Save Path" : "Edit Save Path",
                Content = diagContents,
                CloseButtonText = "Cancel",
                PrimaryButtonText = pathInfoArgs.newPath ? "Save" : "Update",
            };
            //contentDialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, new Binding("IsValid") { Source = pendingShortcutInfo });
            contentDialog.UseLayoutRounding = true;
            contentDialog.Loaded += (object o, RoutedEventArgs e) => diagContents.Focus();
            contentDialog.Loaded += SetMaxWidthForContentDialog;

            var result = await contentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.None: // cancel
                    break;
                case ContentDialogResult.Primary:
                    pending.Icon = pathInfoEditViewModel.IconViewModel.CurrentIcon;
                    if(pathInfoArgs.newPath)
                    {
                        (this.DataContext as SettingsViewModel).AddPathInfo(pending);
                    }
                    else
                    {
                        (this.DataContext as SettingsViewModel).UpdatePathInfo(pathSaveInfo, pending);
                    }

                    //pendingShortcutInfo.UpdateShortcut();
                    break;
            }
        }

        private void LimitPaddingForContentDialog(object sender, int padding)
        {
            var border = UiUtils.FindChild<Border>(sender as DependencyObject, null);
            ((sender as ContentDialog).Content as System.Windows.FrameworkElement).Measure(new Size(double.MaxValue, double.MaxValue));
            var maxHeight = ((sender as ContentDialog).Content as System.Windows.FrameworkElement).DesiredSize.Height + 180 + padding;
            border.MaxHeight = maxHeight;
        }

        private void SetMaxWidthForContentDialog(object sender, RoutedEventArgs e)
        {
            var border = UiUtils.FindChild<Border>(sender as DependencyObject, null);
            border.MaxWidth = double.PositiveInfinity;
        }

        private async void Settings_EditShortcutCommandEvent(object? sender, ShortcutInfo shortcutInfo)
        {
            var pendingShortcutInfo = new PendingShortcutInfo(shortcutInfo);
            var diagContents = new ConfigureShortcutView(pendingShortcutInfo);
            var contentDialog = new ContentDialog(RootContentDialog)
            {
                Title = "Set Shortcut",
                Content = diagContents,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            contentDialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, new Binding("IsValid") { Source = pendingShortcutInfo });
            contentDialog.Loaded += (object o, RoutedEventArgs e) => diagContents.Focus();

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

        private void CardExpander_Loaded(object sender, RoutedEventArgs e)
        {
            var cardExpander = sender as CardExpander;
            var toggleButton = UiUtils.FindChild<ToggleButton>(cardExpander, "ExpanderToggleButton");
            _ = toggleButton ?? throw new NullReferenceException("ToggleButton not found in CardExpander");
            BindingOperations.ClearBinding(toggleButton, ToggleButton.IsCheckedProperty);
            toggleButton.IsChecked = true;
            var chev = UiUtils.FindChild<Grid>(toggleButton, "ChevronGrid");
            _ = chev ?? throw new NullReferenceException("ChevronGrid not found in CardExpander");
            chev.Visibility = Visibility.Collapsed;
        }

    }
}
