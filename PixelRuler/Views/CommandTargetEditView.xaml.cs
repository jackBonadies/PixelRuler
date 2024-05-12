using PixelRuler.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for CommandTargetEditView.xaml
    /// </summary>
    public partial class CommandTargetEditView : UserControl
    {
        public CommandTargetEditView()
        {
            InitializeComponent();
            this.DataContextChanged += CommandTargetEditView_DataContextChanged;
        }

        private void CommandTargetEditView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (this.DataContext as CommandTargetEditViewModel).IconViewModel.OnNewIconSelected += IconViewModel_OnNewIconSelected;
        }

        private void IconViewModel_OnNewIconSelected(object? sender, EventArgs e)
        {
            var toggleButton = VisualTreeHelper.GetChild(this.iconShowEdit, 0) as ToggleButton;
            toggleButton.IsChecked = false;
        }
    }
}
