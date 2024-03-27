using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using PixelRuler.CanvasElements;

namespace PixelRuler
{
    /// <summary>
    /// Per window view model
    /// </summary>
    public class PixelRulerViewModel : INotifyPropertyChanged
    {
        public PixelRulerViewModel(SettingsViewModel? settingsViewModel = null)
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
            CopyRawImageToClipboardCommand = new RelayCommandFull((object? o) => CopyRawImageToClipboard(), System.Windows.Input.Key.C, System.Windows.Input.ModifierKeys.Control, "Copy Image");

            if(settingsViewModel != null)
            {
                this.Settings = settingsViewModel;
                this.SelectedTool = settingsViewModel.DefaultTool;
            }
        }

        /// <summary>
        /// Whether in fullscreen screenshow window mode (QuickTool or Screenshot Selection)
        /// </summary>
        public bool FullscreenScreenshotMode { get; set; } = false;

        public virtual bool IsInWindowSelection()
        {
            return false;
        }

        public void CopyRawImageToClipboard()
        {
            if(this.ImageSource != null)
            {
                Clipboard.SetImage(this.ImageSource);
                // event for UI feedback.
            }
        }

        private void zoomIn()
        {
            if(CurrentZoomPercent >= App.MaxZoomPercent)
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
                if(imageSource != value)
                {
                    imageSource = value;
                    OnPropertyChanged();
                    ImageSourceChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<EventArgs>? ImageSourceChanged;

        public static BitmapSource? ConvertToImageSource(Bitmap? bitmap)
        {
            if(bitmap == null)
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
                    if(activeMeasureElement != null)
                    {
                        activeMeasureElement.PropertyChanged += ActiveMeasureElement_PropertyChanged;
                    }
                    OnPropertyChanged(nameof(ShapeWidth));
                    OnPropertyChanged(nameof(ShapeHeight));
                }
            }
        }

        private void ActiveMeasureElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
        }

        private double boundingBoxWidth;
        public double ShapeWidth
        {
            get
            {
                if(activeMeasureElement != null)
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

        private RelayCommandFull newScreenshotFullCommand;
        public RelayCommandFull NewScreenshotFullCommand
        {
            get
            {
                return newScreenshotFullCommand;
            }
            set
            {
                if (newScreenshotFullCommand != value)
                {
                    newScreenshotFullCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommandFull pasteCanvasContents;
        public RelayCommandFull PasteCanvasContents
        {
            get
            {
                return pasteCanvasContents;
            }
            set
            {
                if (pasteCanvasContents != value)
                {
                    pasteCanvasContents = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommandFull copyCanvasContents;
        public RelayCommandFull CopyCanvasContents
        {
            get
            {
                return copyCanvasContents;
            }
            set
            {
                if(copyCanvasContents != value)
                {
                    copyCanvasContents = value;
                    OnPropertyChanged();
                }
            }
        }

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
                if(showGridlines != value)
                {
                    showGridlines = value;
                    OnPropertyChanged();
                    ShowGridLinesChanged?.Invoke(this, EventArgs.Empty);
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
                if(selectedTool != value)
                {
                    selectedTool = value;
                    SelectedToolChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();
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
