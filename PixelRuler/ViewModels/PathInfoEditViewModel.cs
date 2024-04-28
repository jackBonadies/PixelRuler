using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using PixelRuler.Common;
using PixelRuler.Models;
using PixelRuler.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler.ViewModels
{
    public partial class PathInfoEditViewModel : ObservableObject, IDataErrorInfo
    {
        public DateTime DummyDateTime = DateTime.Now;

        public PathInfoEditViewModel(PathSaveInfo pathSaveInfo, bool newPath)
        {
            PathSaveInfo = pathSaveInfo;
            NewPath = newPath;
            TokenInsertCommand = new RelayCommand((object? tokenObj) =>
            {
                var token = tokenObj as PathSaveInfoToken;
                this.FilePattern += token.DefaultInsert;
            });
            TokenEnterCommand = new RelayCommand((object? tokenObj) =>
            {
                var token = tokenObj as PathSaveInfoToken;
                this.TokenHintText = token.HelperText;
            });
            TokenLeaveCommand = new RelayCommand((object? tokenObj) =>
            {
                this.TokenHintText = string.Empty;
            });
            SelectDirectoryCommand = new RelayCommand((object? obj) =>
            {
                var baseDir = Environment.ExpandEnvironmentVariables(this.BaseDirectory);
                // Ookii = Wrapper around COM Native File Open Dialog
                var folderBrowserDialog = new VistaFolderBrowserDialog();
                var res = folderBrowserDialog.ShowDialog();
                if (res is true)
                {
                    this.BaseDirectory = folderBrowserDialog.SelectedPath;
                }
            });

            FileNameChanged += PathInfoEditViewModel_FilePatternChanged;
            IconViewModel = new IconViewModel();
            IconViewModel.CurrentIcon = pathSaveInfo.Icon;
            SetEvaluatedFilePattern();
        }

        public IconViewModel IconViewModel { get; init; }

        private string evaluatedFilePatternDisplay;
        public string EvaluatedFilePatternDisplay
        {
            get
            {
                return evaluatedFilePatternDisplay;
            }
            set
            {
                if(evaluatedFilePatternDisplay != value)
                {
                    evaluatedFilePatternDisplay = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand SelectDirectoryCommand { get; set; }

        [ObservableProperty]
        public bool filePatternHasError;

        public string Extension
        {
            get
            {
                return PathSaveInfo.Extension;
            }
            set
            {
                if(PathSaveInfo.Extension != value)
                {
                    PathSaveInfo.Extension = value;
                    FileNameChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();
                }
            }
        }

        private void PathInfoEditViewModel_FilePatternChanged(object? sender, EventArgs e)
        {
            SetEvaluatedFilePattern();
        }

        private void SetEvaluatedFilePattern()
        {
            try
            {
                EvaluatedFilePatternDisplay = this.PathSaveInfo.Evaluate(new ScreenshotInfo() { DateTime = DummyDateTime, Height = 1080, Width = 1920, ProcessName = "PixelRuler", WindowTitle = "Settings" }, false, true);
                FilePatternHasError = false;
            }
            catch(Exception e)
            {
                EvaluatedFilePatternDisplay = e.Message;
                FilePatternHasError = true;
            }
        }

        public string? DisplayName
        {
            get
            {
                return PathSaveInfo.DisplayName;
            }
            set
            {
                if(PathSaveInfo.DisplayName != value)
                {
                    PathSaveInfo.DisplayName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? BaseDirectory
        {
            get
            {
                return PathSaveInfo.BaseDirectory;
            }
            set
            {
                if(PathSaveInfo.BaseDirectory != value)
                {
                    PathSaveInfo.BaseDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        private event EventHandler<EventArgs>? FileNameChanged;

        public string? FilePattern
        {
            get
            {
                return PathSaveInfo.FilePattern;
            }
            set
            {
                if(PathSaveInfo.FilePattern != value)
                {
                    PathSaveInfo.FilePattern = value;
                    FileNameChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand TokenEnterCommand { get; set; } 

        public RelayCommand TokenLeaveCommand { get; set; }

        [ObservableProperty]
        public string tokenHintText;

        public PathSaveInfo PathSaveInfo { get; set; } 
        public bool NewPath { get; set; } 

        public List<PathSaveInfoToken> AllTokens
        {
            get
            {
                return PathSaveInfoUtil.AllTokens;
            }
        }

        public List<string> AllExtensions
        {
            get
            {
                return new List<string>()
                {
                    "png", "jpg", "gif", "bmp"
                };
            }
        }

        public RelayCommand TokenInsertCommand { get; private set; }

        public string Error => string.Empty;


        public string this[string columnName]
        {
            get
            {
                if(columnName == nameof(DisplayName))
                {
                    if(string.IsNullOrWhiteSpace(DisplayName))
                    {
                        return "Display Name cannot be blank.";
                    }
                }
                return string.Empty;
            }
        }
    }
}
