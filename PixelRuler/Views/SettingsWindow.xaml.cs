﻿using PixelRuler.Models;
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
        public SettingsWindow(PixelRulerViewModel viewModel)
        {
            var workArea = SystemParameters.WorkArea;
            this.MaxHeight = workArea.Height - 30;

            this.DataContext = viewModel.Settings;
            viewModel.Settings.EditShortcutCommandEvent += Settings_EditShortcutCommandEvent;
            viewModel.Settings.EditSavePathCommandEvent += Settings_EditSavePathCommandEvent;

            InitializeComponent();
        }

        private async void Settings_EditSavePathCommandEvent(object? sender, PathSaveInfo e)
        {
            //new PathInfoEdit
            var pathInfoEditViewModel = new PathInfoEditViewModel(e);

            var diagContents = new PathInfoEditView();
            diagContents.DataContext = pathInfoEditViewModel;
            var contentDialog = new ContentDialog(RootContentDialog)
            {
                Title = "Set Shortcut",
                Content = diagContents,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
            };
            //contentDialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, new Binding("IsValid") { Source = pendingShortcutInfo });
            contentDialog.Loaded += (object o, RoutedEventArgs e) => diagContents.Focus();

            var result = await contentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.None: // cancel
                    break;
                case ContentDialogResult.Primary:
                    //pendingShortcutInfo.UpdateShortcut();
                    break;
            }
        }

        private async void Settings_EditShortcutCommandEvent(object? sender, ShortcutInfo shortcutInfo)
        {
            //new PathInfoEdit
            var pathInfoEditViewModel = new PathInfoEditViewModel((this.DataContext as SettingsViewModel).DefaultPathSaveInfo);

            var diagContents = new PathInfoEditView();
            diagContents.DataContext = pathInfoEditViewModel;
            var contentDialog = new ContentDialog(RootContentDialog)
            {
                Title = "Set Shortcut",
                Content = diagContents,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
            };
            //contentDialog.SetBinding(ContentDialog.IsPrimaryButtonEnabledProperty, new Binding("IsValid") { Source = pendingShortcutInfo });
            contentDialog.Loaded += (object o, RoutedEventArgs e) => diagContents.Focus();

            var result = await contentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.None: // cancel
                    break;
                case ContentDialogResult.Primary:
                    //pendingShortcutInfo.UpdateShortcut();
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
            var res = FindChild<ToggleButton>(cardExpander, "ExpanderToggleButton");
            BindingOperations.ClearBinding(res, ToggleButton.IsCheckedProperty);
            res.IsChecked = true;
            var chev = FindChild<Grid>(res, "ChevronGrid");
            chev.Visibility = Visibility.Collapsed;
        }
        public static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;

                if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the requested name
                        foundChild = (T)child;
                        break;
                    }
                }

                if (foundChild == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}
