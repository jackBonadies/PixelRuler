using PixelRuler.Common;
using PixelRuler.Models;
using PixelRuler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler.ViewModels
{
    public class PathInfoEditViewModel : ViewModelBase
    {
        public DateTime DummyDateTime = DateTime.Now;

        public PathInfoEditViewModel(PathSaveInfo pathSaveInfo)
        {
            PathSaveInfo = pathSaveInfo;
            TokenInsertCommand = new RelayCommand((object? tokenObj) =>
            {
                var token = tokenObj as PathSaveInfoToken;
                this.FilePattern += token.DefaultInsert;
            });
            FileNameChanged += PathInfoEditViewModel_FilePatternChanged;
            SetEvaluatedFilePattern();
        }

        private string evaluatedFilePattern;
        public string EvaluatedFilePattern
        {
            get
            {
                return evaluatedFilePattern;
            }
            set
            {
                if(evaluatedFilePattern != value)
                {
                    evaluatedFilePattern = value;
                    OnPropertyChanged();
                }
            }
        }

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
            EvaluatedFilePattern = this.PathSaveInfo.Evaluate(new ScreenshotInfo() { DateTime = DummyDateTime, Height = 1080, Width = 1920, ProcessName = "PixelRuler", WindowTitle = "Settings" }, false, true);
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

        public PathSaveInfo PathSaveInfo { get; set; } 

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
    }
}
