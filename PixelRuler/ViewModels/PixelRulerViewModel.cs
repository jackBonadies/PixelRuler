﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using PixelRuler.CanvasElements;
using PixelRuler.Common;
using PixelRuler.Models;
using PixelRuler.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PixelRuler
{
    /// <summary>
    /// Per window view model
    /// </summary>
    public partial class PixelRulerViewModel : ObservableObject
    {
        public PixelRulerViewModel()
        {
            init();
        }

        public PixelRulerViewModel(SettingsViewModel settingsViewModel)
        {
            init(settingsViewModel);
        }

        private void init(SettingsViewModel? settingsViewModel = null)
        {
            BoundsMeasureSelectedCommand = new RelayCommandFull((object? o) => SelectedTool = Tool.BoundingBox, System.Windows.Input.Key.B, System.Windows.Input.ModifierKeys.None, "Bounds Measure");
            ColorPickerSelectedCommand = new RelayCommandFull((object? o) => SelectedTool = Tool.ColorPicker, System.Windows.Input.Key.C, System.Windows.Input.ModifierKeys.None, "Color Picker");
            RulerSelectedCommand = new RelayCommandFull((object? o) => SelectedTool = Tool.Ruler, System.Windows.Input.Key.L, System.Windows.Input.ModifierKeys.None, "Line Measure");
            ZoomInCommand = new RelayCommandFull((object? o) => zoomIn(), System.Windows.Input.Key.OemPlus, System.Windows.Input.ModifierKeys.Control, "Zoom In");
            ZoomOutCommand = new RelayCommandFull((object? o) => zoomOut(), System.Windows.Input.Key.OemMinus, System.Windows.Input.ModifierKeys.Control, "Zoom Out");
            FitWindowCommand = new RelayCommandFull((object? o) => fitWindow(), System.Windows.Input.Key.D0, System.Windows.Input.ModifierKeys.Control, "Fit Window");
            ClearAllMeasureElementsCommand = new RelayCommandFull((object? o) => clearAllMeasureElements(), System.Windows.Input.Key.C, System.Windows.Input.ModifierKeys.Shift, "Clear All");
            DeleteAllSelectedCommand = new RelayCommandFull((object? o) => deleteAllSelectedElements(), System.Windows.Input.Key.Delete, System.Windows.Input.ModifierKeys.None, "Delete All Selected");
            SelectAllElementsCommand = new RelayCommandFull((object? o) => selectAllElements(), System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Control, "Select All Elements");
            CopyRawImageToClipboardCommand = new RelayCommandFull(async (object? o) => await CopyRawImageToClipboard(), System.Windows.Input.Key.C, System.Windows.Input.ModifierKeys.Control, "Copy Image");

            if (settingsViewModel != null)
            {
                this.Settings = settingsViewModel;
                if (settingsViewModel.DefaultTool == DefaultTool.LastSelectedTool)
                {
                    this.SelectedTool = (Tool)Properties.Recents.Default.SelectedTool;
                }
                else
                {
                    this.SelectedTool = (Tool)settingsViewModel.DefaultTool;
                }

                this.ShowGridLines = Properties.Recents.Default.ShowGridLines;
            }
        }

        #region MainCanvasElements

        public ObservableCollection<MeasurementElementZoomCanvasShape> MeasurementElements { get; private set; } = new();

        #endregion MainCanvasElements

        /// <summary>
        /// Whether in fullscreen screenshow window mode (QuickTool or Screenshot Selection)
        /// </summary>
        public bool FullscreenScreenshotMode { get; set; } = false;

        public virtual bool IsInWindowSelection()
        {
            return false;
        }

        public event EventHandler? RawImageCopied;

        public async Task CopyRawImageToClipboard()
        {
            if (this.ImageSource != null)
            {
                await CopyRawImageToClipboardImp();
                RawImageCopied?.Invoke(this, EventArgs.Empty);
            }
        }

        private void _CopyRawImageToClipboardSyncImp()
        {
            Clipboard.SetImage(this.ImageSource);
        }

        private async Task CopyRawImageToClipboardImp()
        {
            await Task.Run(() =>
            {
                //System.Threading.Thread.Sleep(2000);
                Thread thread = new Thread(() =>
                {
                    //System.Threading.Thread.Sleep(2000);
                    // cannot use Clipboard.SetImage(ImageSource) as ImageSource
                    //   can only be accessed from UI thread.
                    DataObject dataObject = new DataObject();
                    dataObject.SetData(DataFormats.Bitmap, this.Image, true);
                    Clipboard.SetDataObject(dataObject, true);
                    //System.Threading.Thread.Sleep(2000);
                });
                // cannot use task as Clipboard requires thread to be 
                //   set to single thread apartment.
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            });
        }

        private void zoomIn()
        {
            if (CurrentZoomPercent >= App.MaxZoomPercent)
            {
                return;
            }

            SetZoomSpecial(currentZoomPercent * 2, ZoomBehavior.KeepMousePositionFixed);
        }

        private void zoomOut()
        {
            if (CurrentZoomPercent <= App.MinZoomPercent)
            {
                return;
            }

            SetZoomSpecial(currentZoomPercent * .5, ZoomBehavior.KeepMousePositionFixed);
        }

        private void fitWindow()
        {
            SetZoomSpecial(100, ZoomBehavior.ResetWindow);
        }

        public virtual bool IsScreenshotMode()
        {
            return false;
        }

        private void clearAllMeasureElements()
        {
            ClearAllMeasureElements?.Invoke(this, EventArgs.Empty);
        }

        private void selectAllElements()
        {
            AllElementsSelected?.Invoke(this, EventArgs.Empty);
        }

        private void deleteAllSelectedElements()
        {
            DeleteAllSelectedElements?.Invoke(this, EventArgs.Empty);
        }

        private void SetZoomSpecial(double newZoomPercent, ZoomBehavior zoomBehavior)
        {
            currentZoomPercent = newZoomPercent;
            ZoomChanged?.Invoke(this, zoomBehavior);
            CurrentZoomPercent = currentZoomPercent;
            OnPropertyChanged(nameof(CurrentZoomPercent));
        }

        public Bitmap CropImage(Rect rect)
        {
            _ = Image ?? throw new Exception("No Image to Crop");
            // we may be outside bounds i.e. if a window is partially occluded.
            var imageBounds = new Rect(0, 0, Image.Width, Image.Height);
            rect.Intersect(imageBounds);
            return Image.Clone(rect.ToRectangle(), this.Image.PixelFormat);
        }

        private Bitmap? mainImage;
        public Bitmap? Image
        {
            get
            {
                return mainImage;
            }
            set
            {
                mainImage = value;
                ImageSource = ConvertToImageSource(mainImage);
            }
        }

        private BitmapSource? imageSource;

        public BitmapSource? ImageSource
        {
            get
            {
                return imageSource;
            }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged();
                    ImageSourceChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<EventArgs>? ImageSourceChanged;

        public void SetImage(Bitmap bmp, ScreenshotInfo screenshotInfo)
        {
            this.Image = bmp;
            // TODO populate timestamp if not there
            this.ScreenshotInfo = screenshotInfo;
        }

        public ScreenshotInfo ScreenshotInfo
        {
            get; set;
        }

        public static BitmapSource? ConvertToImageSource(Bitmap? bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }
            return Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }


        MeasurementElementZoomCanvasShape? activeMeasureElement = null;
        public MeasurementElementZoomCanvasShape? ActiveMeasureElement
        {
            get
            {
                return activeMeasureElement;
            }
            set
            {
                if (activeMeasureElement != value)
                {
                    activeMeasureElement = value;
                    if (activeMeasureElement != null)
                    {
                        activeMeasureElement.PropertyChanged += ActiveMeasureElement_PropertyChanged;
                    }
                    OnPropertyChanged(nameof(ShapeWidth));
                    OnPropertyChanged(nameof(ShapeHeight));
                }
            }
        }

        [RelayCommand]
        public void ShowSettings()
        {
            App.ShowSettingsWindowSingleInstance();
        }

        [RelayCommand]
        public void SaveToTarget(PathSaveInfo pathSaveInfo)
        {
            pathSaveInfo.SaveImage(this.Image, this.ScreenshotInfo);
        }

        [RelayCommand]
        public void SendToTarget(CommandTargetInfo commandTargetInfo)
        {
            if (string.IsNullOrEmpty(this.ScreenshotInfo.LastSavedPath))
            {
                this.Settings.DefaultPathSaveInfo.SaveImage(this.Image, this.ScreenshotInfo);
            }
            commandTargetInfo.Execute(ScreenshotInfo.LastSavedPath);
        }

        [RelayCommand]
        public void SaveAs()
        {
            var fullFilename = this.Settings.DefaultPathSaveInfo.Evaluate(this.ScreenshotInfo, true);
            var initDir = System.IO.Path.GetDirectoryName(fullFilename);
            Directory.CreateDirectory(initDir);

            var sfd = new SaveFileDialog();
            sfd.InitialDirectory = initDir;
            sfd.FileName = System.IO.Path.GetFileName(fullFilename);
            sfd.DefaultExt = this.Settings.DefaultPathSaveInfo.Extension;
            sfd.AddExtension = true;
            sfd.Filter = "PNG|*.png|JPEG|*.jpg;*.jpeg|GIF|*.gif|BMP|*.bmp";
            var sfdres = sfd.ShowDialog();
            if (sfdres is true)
            {
                if (this.Image == null)
                {
                    throw new Exception("Save As - Missing Image");
                }
                ImageCommon.SaveImage(sfd.FileName, this.Image);
            }
        }

        private void ActiveMeasureElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }


        private double boundingBoxWidth;
        public double ShapeWidth
        {
            get
            {
                if (activeMeasureElement != null)
                {
                    return activeMeasureElement.ShapeWidth;
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                if (boundingBoxWidth != value)
                {
                    boundingBoxWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        private double shapeHeight;
        public double ShapeHeight
        {
            get
            {
                if (activeMeasureElement != null)
                {
                    return activeMeasureElement.ShapeHeight;
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                if (shapeHeight != value)
                {
                    shapeHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommandFull closeWindowCommand;
        public RelayCommandFull CloseWindowCommand
        {
            get
            {
                return closeWindowCommand;
            }
            set
            {
                if (closeWindowCommand != value)
                {
                    closeWindowCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        public RelayCommandFull newScreenshotRegionCommand;

        [ObservableProperty]
        private RelayCommandFull newScreenshotFullCommand;

        [ObservableProperty]
        private RelayCommandFull pasteCanvasContents;

        [ObservableProperty]
        private RelayCommandFull copyCanvasContents;

        public RelayCommandFull CopyRawImageToClipboardCommand { get; set; }

        private RelayCommandFull selectAllElementsCommand;
        public RelayCommandFull SelectAllElementsCommand
        {
            get
            {
                return selectAllElementsCommand;
            }
            set
            {
                if (selectAllElementsCommand != value)
                {
                    selectAllElementsCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommandFull deleteAllSelectedCommand;
        public RelayCommandFull DeleteAllSelectedCommand
        {
            get
            {
                return deleteAllSelectedCommand;
            }
            set
            {
                if (deleteAllSelectedCommand != value)
                {
                    deleteAllSelectedCommand = value;
                    OnPropertyChanged();
                }
            }
        }


        public RelayCommandFull ClearAllMeasureElementsCommand
        {
            get; set;
        }

        private RelayCommandFull zoomInCommand;
        public RelayCommandFull ZoomInCommand
        {
            get
            {
                return zoomInCommand;
            }
            set
            {
                if (zoomInCommand != value)
                {
                    zoomInCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommandFull zoomOutCommand;
        public RelayCommandFull ZoomOutCommand
        {
            get
            {
                return zoomOutCommand;
            }
            set
            {
                if (zoomOutCommand != value)
                {
                    zoomOutCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommandFull fitWindowCommand;
        public RelayCommandFull FitWindowCommand
        {
            get
            {
                return fitWindowCommand;
            }
            set
            {
                if (fitWindowCommand != value)
                {
                    fitWindowCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommandFull BoundsMeasureSelectedCommand
        {
            get; set;
        }


        public RelayCommandFull ColorPickerSelectedCommand
        {
            get; set;
        }

        public RelayCommandFull RulerSelectedCommand
        {
            get; set;
        }

        [RelayCommand]
        public void ToggleShowGridLines()
        {
            ShowGridLines = !ShowGridLines;
        }

        [RelayCommand]
        public void OpenImageFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "PNG|*.png|JPEG|*.jpg;*.jpeg|GIF|*.gif|BMP|*.bmp";
            bool? res = ofd.ShowDialog();
            if (res is true)
            {
                var bmp = System.Drawing.Bitmap.FromFile(ofd.FileName);
                this.Image = bmp as System.Drawing.Bitmap;
                this.ImageUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Cleanup()
        {
            BoundsMeasureSelectedCommand = null!;
            ColorPickerSelectedCommand = null!;
            RulerSelectedCommand = null!;
            ZoomInCommand = null!;
            ZoomOutCommand = null!;
            FitWindowCommand = null!;
            ClearAllMeasureElementsCommand = null!;
            DeleteAllSelectedCommand = null!;
            SelectAllElementsCommand = null!;
            CopyRawImageToClipboardCommand = null!;
        }

        public event EventHandler? ColorCopied;

        [RelayCommand]
        public void CopyColorToClipboard()
        {
            Clipboard.SetText(UiUtils.FormatColor(this.Color, this.Settings.ColorFormatMode));
            ColorCopied?.Invoke(this, EventArgs.Empty);
        }

        public void OnColorSelected()
        {
            ColorSelected?.Invoke(this, EventArgs.Empty);
        }

        public void OnColorStartSelect()
        {
            ColorStartSelect?.Invoke(this, EventArgs.Empty);
        }

        public void ShowToastIfApplicable(AfterScreenshotAction saveAs, object additionalArgs)
        {
            ShowToastAfter ourCase = ShowToastAfter.AnyAction;
            switch (saveAs)
            {
                case AfterScreenshotAction.Save:
                    if (additionalArgs is PathSaveInfo psinfo)
                    {
                        if (psinfo.IsDefault)
                        {
                            ourCase = ShowToastAfter.DefaultSave;
                        }
                        else
                        {
                            ourCase = ShowToastAfter.AnySave;
                        }
                    }
                    else
                    {
                        ourCase = ShowToastAfter.DefaultSave;
                    }
                    break;
                case AfterScreenshotAction.SaveAs:
                    ourCase = ShowToastAfter.AnySave;
                    break;
            }
            if (ourCase <= this.Settings.ShowToastAfter)
            {
                new ToastContentBuilder()
                    .AddText("Pixel Ruler", AdaptiveTextStyle.Header)
                    .AddText($"Image Saved to: {this.ScreenshotInfo.LastSavedPath}")
                    .SetToastDuration(ToastDuration.Short)
                    .Show(toast =>
                    {
                        toast.ExpirationTime = DateTime.Now.AddDays(1);
                        toast.ExpiresOnReboot = true;
                        ToastHelper.ConfigureToast(toast, true, (ToastArguments args) =>
                        {
                            Process.Start("explorer", $"/select, {this.ScreenshotInfo.LastSavedPath}");
                        });
                    });
            }
        }

        public event EventHandler<EventArgs> ColorSelected;
        public event EventHandler<EventArgs> ColorStartSelect;
        public event EventHandler<EventArgs> ImageUpdated;
        public event EventHandler<EventArgs> ShowGridLinesChanged;
        private bool showGridlines = false;
        public bool ShowGridLines
        {
            get
            {
                return showGridlines;
            }
            set
            {
                if (showGridlines != value)
                {
                    showGridlines = value;
                    OnPropertyChanged();
                    ShowGridLinesChanged?.Invoke(this, EventArgs.Empty);

                    if (Properties.Recents.Default.ShowGridLines != value)
                    {
                        Properties.Recents.Default.ShowGridLines = value;
                        Properties.Recents.Default.Save();
                    }
                }
            }
        }

        private Tool selectedTool;
        public Tool SelectedTool
        {
            get
            {
                return selectedTool;
            }
            set
            {
                if (selectedTool != value)
                {
                    selectedTool = value;
                    SelectedToolChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();

                    if (Properties.Recents.Default.SelectedTool != (int)value)
                    {
                        Properties.Recents.Default.SelectedTool = (int)value;
                        Properties.Recents.Default.Save();
                    }
                }
            }
        }
        public event EventHandler<EventArgs>? SelectedToolChanged;

        private System.Windows.Point currentPosition;
        public System.Windows.Point CurrentPosition
        {
            get
            {
                return currentPosition;
            }
            set
            {
                currentPosition = value;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        private bool test;

        public double[] AvailableZooms
        {
            get;
            set;
        } = App.ZoomSelections;

        private double currentZoomPercent = 100;
        public double CurrentZoomPercent
        {
            get
            {
                return currentZoomPercent;
            }
            set
            {
                if (currentZoomPercent != value)
                {
                    currentZoomPercent = value;
                    OnPropertyChanged();
                    ZoomChanged?.Invoke(this, ZoomBehavior.KeepCenterFixed);
                }
            }
        }

        public double CurrentZoom
        {
            get
            {
                return CurrentZoomPercent * .01;
            }
        }

        private System.Drawing.Color color;
        public System.Drawing.Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                OnPropertyChanged();
            }
        }

        public SettingsViewModel Settings { get; set; }

        public EventHandler<ZoomBehavior>? ZoomChanged;
        public EventHandler<EventArgs>? ClearAllMeasureElements;
        public EventHandler<EventArgs>? DeleteAllSelectedElements;
        public EventHandler<EventArgs>? AllElementsSelected;
    }

    public enum ZoomBehavior
    {
        KeepMousePositionFixed = 0,
        KeepCenterFixed = 1,
        ResetWindow = 2,
    }
}
