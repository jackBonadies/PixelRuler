using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler
{
    public class PixelRulerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
                    return 123;
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


        private string testing = "100";
        public string Testing
        {
            get
            {
                return testing;
            }
            set
            {
                if (testing != value)
                {
                    testing = value;
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
                    return 123;
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


        public double[] AvailableZooms
        {
            get;
            set;
        } = App.ZoomSelections;

        private double zoom;
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


    }
}
