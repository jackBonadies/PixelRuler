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

namespace PixelRuler
{
    public class PixelRulerViewModel : INotifyPropertyChanged
    {
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

        private double zoom = 100;
        public double CurrentZoom
        {
            get
            {
                return zoom;
            }
            set
            {
                if (zoom != value)
                {
                    zoom = value;
                    OnPropertyChanged();
                }
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
    }

    public enum Tool
    {
        BoundingBox = 0,
        ColorPicker = 1,
    }
}
