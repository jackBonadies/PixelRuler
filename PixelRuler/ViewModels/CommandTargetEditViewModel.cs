using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using PixelRuler.Models;
using PixelRuler.Views;
using System.ComponentModel;

namespace PixelRuler.ViewModels
{
    public partial class CommandTargetEditViewModel : ObservableObject, IDataErrorInfo
    {
        public CommandTargetEditViewModel(CommandTargetInfo commandTargetInfo, bool newEntry)
        {
            CommandTargetInfo = commandTargetInfo;
            NewEntry = newEntry;
            SelectExecutableCommand = new RelayCommand((object? obj) =>
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "Exe|*.exe";
                bool? res = ofd.ShowDialog();
                if (res is true)
                {
                    ExecutableName = ofd.FileName;
                }
            });
            IconViewModel = new IconViewModel();
            IconViewModel.CurrentIcon = commandTargetInfo.Icon;
        }

        public IconViewModel IconViewModel { get; set; }


        public RelayCommand SelectExecutableCommand { get; set; }

        public string? ExecutableName
        {
            get
            {
                return CommandTargetInfo.CommandExe;
            }
            set
            {
                if (CommandTargetInfo.CommandExe != value)
                {
                    CommandTargetInfo.CommandExe = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CommandArgs
        {
            get
            {
                return CommandTargetInfo.CommandArgs;
            }
            set
            {
                if (CommandTargetInfo.CommandArgs != value)
                {
                    CommandTargetInfo.CommandArgs = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? DisplayName
        {
            get
            {
                return CommandTargetInfo.DisplayName;
            }
            set
            {
                if (CommandTargetInfo.DisplayName != value)
                {
                    CommandTargetInfo.DisplayName = value;
                    OnPropertyChanged();
                }
            }
        }

        public CommandTargetInfo CommandTargetInfo { get; set; }
        public bool NewEntry { get; set; }
        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(DisplayName))
                {
                    if (string.IsNullOrWhiteSpace(DisplayName))
                    {
                        return "Display Name cannot be blank.";
                    }
                }
                else if (columnName == nameof(ExecutableName))
                {
                    if (string.IsNullOrWhiteSpace(ExecutableName))
                    {
                        return "Command cannot be blank.";
                    }
                }
                return string.Empty;
            }
        }

        public string Error => string.Empty;
    }
}
