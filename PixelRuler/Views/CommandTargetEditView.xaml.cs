using PixelRuler.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
