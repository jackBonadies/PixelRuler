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

namespace PixelRuler
{
    public class PixelRulerViewModel : INotifyPropertyChanged
    {
        public PixelRulerViewModel()
        {
            BoundsMeasureSelectedCommand = new RelayCommandFull((object? o) => SelectedTool = Tool.BoundingBox, System.Windows.Input.Key.B, System.Windows.Input.ModifierKeys.None, "Bounds Measure");
            ColorPickerSelectedCommand = new RelayCommandFull((object? o) => SelectedTool = Tool.ColorPicker, System.Windows.Input.Key.C, System.Windows.Input.ModifierKeys.None, "Color Picker");
            RulerSelectedCommand = new RelayCommandFull((object? o) => SelectedTool = Tool.Ruler, System.Windows.Input.Key.L, System.Windows.Input.ModifierKeys.None, "Line Measure");
            ZoomInCommand = new RelayCommandFull((object? o) => zoomIn(), System.Windows.Input.Key.OemPlus, System.Windows.Input.ModifierKeys.Control, "Zoom In");
            ZoomOutCommand = new RelayCommandFull((object? o) => zoomOut(), System.Windows.Input.Key.OemMinus, System.Windows.Input.ModifierKeys.Control, "Zoom Out");
            FitWindowCommand = new RelayCommandFull((object? o) => fitWindow(), System.Windows.Input.Key.D0, System.Windows.Input.ModifierKeys.Control, "Fit Window");
            ClearAllMeasureElementsCommand = new RelayCommandFull((object? o) => clearAllMeasureElements(), System.Windows.Input.Key.C, System.Windows.Input.ModifierKeys.Shift, "Clear All");
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

        private void SetZoomSpecial(double newZoomPercent, ZoomBehavior zoomBehavior)
        {
            currentZoomPercent = newZoomPercent;
            ZoomChanged?.Invoke(this, zoomBehavior);
            CurrentZoomPercent = currentZoomPercent;
            OnPropertyChanged(nameof(CurrentZoomPercent));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
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
                imageSource = value;
                ImageSourceChanged?.Invoke(this, EventArgs.Empty);
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


        BoundingBoxLabel? boundingBoxLabel = null;
        public BoundingBoxLabel? BoundingBoxLabel
        {
            get
            {
                return boundingBoxLabel;
            }
            set
            {
                boundingBoxLabel = value;
                boundingBoxLabel.PropertyChanged += BoundingBoxLabel_PropertyChanged;
            }
        }

        private void BoundingBoxLabel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
        }

        private double boundingBoxWidth;
        public double BoundingBoxWidth
        {
            get
            {
                if(boundingBoxLabel != null)
                {
                    return boundingBoxLabel.BoundingBoxWidth;
                }
                else
                {
                    return 0;
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

        private double boundingBoxHeight;
        public double BoundingBoxHeight
        {
            get
            {
                if (boundingBoxLabel != null)
                {
                    return boundingBoxLabel.BoundingBoxHeight;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (boundingBoxHeight != value)
                {
                    boundingBoxHeight = value;
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

        private Tool selectedTool;
        public Tool SelectedTool
        {
            get
            {
                return selectedTool;
            }
            set
            {
                selectedTool = value;
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

        public EventHandler<ZoomBehavior>? ZoomChanged;
        public EventHandler<EventArgs>? ClearAllMeasureElements;
    }

    public enum ZoomBehavior
    {
        KeepMousePositionFixed = 0,
        KeepCenterFixed = 1,
        ResetWindow = 2,
    }

    public enum Tool
    {
        BoundingBox = 0,
        ColorPicker = 1,
        Ruler = 2,
    }
}
