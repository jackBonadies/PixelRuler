using PixelRuler.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for PathInfoEdit.xaml
    /// </summary>
    public partial class PathInfoEditView : UserControl
    {
        public PathInfoEditView()
        {
            InitializeComponent();
            this.DataContextChanged += PathInfoEditView_DataContextChanged;
        }

        private void PathInfoEditView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //(this.DataContext as PathInfoEditViewModel).EditIconClicked += PathInfoEditView_EditIconClicked;
            (this.DataContext as PathInfoEditViewModel).IconViewModel.OnNewIconSelected += NewIconSelected;

        }

        private void PathInfoEditView_EditIconClicked(object? sender, EventArgs e)
        {
        }

        private void NewIconSelected(object? sender, EventArgs e)
        {
            var toggleButton = VisualTreeHelper.GetChild(this.iconShowEdit, 0) as ToggleButton;
            toggleButton.IsChecked = false;
        }
    }
}
