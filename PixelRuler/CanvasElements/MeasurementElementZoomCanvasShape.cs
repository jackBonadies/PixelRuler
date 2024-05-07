using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelRuler.CanvasElements
{
    /// <summary>
    /// A class that manages its canvas shapes - adding / removing from canvas,
    ///   updating stroke to be UI units, handling resize events.
    /// </summary>
    public abstract class MeasurementElementZoomCanvasShape : AbstractZoomCanvasShape, INotifyPropertyChanged
    {
        public event EventHandler<MeasureElementResizeData>? Resizing;
        public event EventHandler<MeasureElementResizeData>? StartResize;
        public event EventHandler<MeasureElementResizeData>? EndResize;

        protected void OnResizing(object tag)
        {
            Resizing?.Invoke(this, new MeasureElementResizeData(this, tag));
        }

        protected void OnStartResize(object tag)
        {
            StartResize?.Invoke(this, new MeasureElementResizeData(this, tag));
        }

        protected void OnEndResize(object tag)
        {
            EndResize?.Invoke(this, new MeasureElementResizeData(this, tag));
        }

        protected System.Collections.Generic.List<CircleSizerControl> circleSizerControls = new System.Collections.Generic.List<CircleSizerControl>();

        protected MeasurementElementZoomCanvasShape(Canvas owningCanvas) : base(owningCanvas)
        {
            hitBoxManipulate = new Canvas();
            hitBoxManipulate.Focusable = true;
            hitBoxManipulate.Cursor = Cursors.SizeAll;
            hitBoxManipulate.Background = new SolidColorBrush(Colors.Transparent);
            hitBoxManipulate.FocusVisualStyle = null;

            hitBoxManipulate.MouseLeftButtonDown += HitBoxManipulate_MouseDown;
            hitBoxManipulate.MouseMove += HitBoxManipulate_MouseMove;
            hitBoxManipulate.MouseLeftButtonUp += HitBoxManipulate_MouseUp;
            hitBoxManipulate.LostMouseCapture += HitBoxManipulate_LostMouseCapture;

            hitBoxManipulate.Background = new SolidColorBrush(Color.FromArgb(40, 244, 244, 244));

            Canvas.SetZIndex(hitBoxManipulate, App.MANIPULATE_HITBOX_INDEX);
        }

        public virtual MeasureElementData Archive()
        {
            return new MeasureElementData()
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                ElementType = this is BoundingBoxElement ? Tool.BoundingBox : Tool.Ruler
            };
        }

        public static MeasurementElementZoomCanvasShape FromMeasureElementData(MeasureElementData data, Canvas owningCanvas)
        {
            MeasurementElementZoomCanvasShape el = null;
            switch (data.ElementType)
            {
                case Tool.Ruler:
                    el = new RulerElement(owningCanvas);
                    break;
                case Tool.BoundingBox:
                    el = new BoundingBoxElement(owningCanvas);
                    break;
                default:
                    return null;
            }
            el.StartPoint = data.StartPoint;
            el.EndPoint = data.EndPoint;
            return el;
        }

        private void HitBoxManipulate_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (isManipulating)
            {
                EndManipulate(false);
            }
        }

        public override void AddToOwnerCanvas()
        {
            this.owningCanvas.Children.Add(hitBoxManipulate);
        }

        public override void Clear()
        {
            this.owningCanvas.Children.Remove(hitBoxManipulate);
            this.hitBoxManipulate.Children.Clear();
        }


        protected bool isManipulating = false;

        protected (Point mouseStart, Point shapeStart, Point shapeEnd) MoveStartInfo;

        private void HitBoxManipulate_MouseUp(object sender, MouseButtonEventArgs e)
        {
            EndManipulate();
        }

        private void EndManipulate(bool shouldSelect = true)
        {
            this.Selected = shouldSelect;
            isManipulating = false;
            this.hitBoxManipulate.ReleaseMouseCapture();
        }

        private void HitBoxManipulate_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isManipulating)
            {
                return;
            }
            var newPos = e.GetPosition(this.owningCanvas);
            var delta = MoveStartInfo.mouseStart - newPos;
            System.Diagnostics.Trace.WriteLine($"x: {delta.X} y: {delta.Y}");

            int xMove = -(int)delta.X;
            int yMove = -(int)delta.Y;

            MoveStartInfo.mouseStart = MoveStartInfo.mouseStart.Add(new Point(xMove, yMove));

            OnMoving(new Point(xMove, yMove));

            //this.StartPoint = new Point(MoveStartInfo.shapeStart.X + xMove, MoveStartInfo.shapeStart.Y + yMove);
            //this.EndPoint = new Point(MoveStartInfo.shapeEnd.X + xMove, MoveStartInfo.shapeEnd.Y + yMove);
        }

        private readonly bool select_on_mouse_down = true;

        private void HitBoxManipulate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.FinishedDrawing)
            {
                return;
            }

            if (select_on_mouse_down)
            {
                this.Selected = true;
            }


            isManipulating = true;
            this.hitBoxManipulate.Focus();
            MoveStartInfo = (e.GetPosition(this.owningCanvas), StartPoint, EndPoint);
            this.hitBoxManipulate.CaptureMouse();
            e.Handled = true;
        }

        protected Canvas hitBoxManipulate;

        public abstract void SetEndPoint(System.Windows.Point roundedPoint);

        public abstract bool IsEmpty { get; }

        public abstract bool FinishedDrawing { get; set; }

        public event EventHandler<EventArgs>? SelectedChanged;

        private bool selected = false;
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                if (value != selected)
                {
                    selected = value;
                    SetSelectedState();
                    SelectedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private Point startPoint;
        public Point StartPoint
        {
            get
            {
                return startPoint;
            }
            set
            {
                if (startPoint != value)
                {
                    startPoint = value;
                    ShapeSizeChanged();
                }
            }
        }

        private Point endPoint = new Point(double.NaN, double.NaN);
        public Point EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                if (endPoint != value)
                {
                    endPoint = value;
                    ShapeSizeChanged();
                }
            }
        }

        public void ShapeSizeChanged()
        {
            OnPropertyChanged("ShapeWidth");
            OnPropertyChanged("ShapeHeight");
        }

        public virtual double ShapeWidth
        {
            get
            {
                return Math.Abs(this.endPoint.X - this.startPoint.X);
            }
        }

        public virtual double ShapeHeight
        {
            get
            {
                return Math.Abs(this.endPoint.Y - this.startPoint.Y);
            }
        }

        public abstract void SetState();

        public void Move(int deltaX, int deltaY)
        {
            // this can just be the base method and non virtual TODO
            this.StartPoint = new Point(this.StartPoint.X + deltaX, this.StartPoint.Y + deltaY);
            this.EndPoint = new Point(this.EndPoint.X + deltaX, this.EndPoint.Y + deltaY);
            this.SetState();
        }

        public void MoveStartPoint(int deltaX, int deltaY)
        {
            this.StartPoint = new Point(this.StartPoint.X + deltaX, this.StartPoint.Y + deltaY);
            this.SetState();
        }

        public void MoveEndPoint(int deltaX, int deltaY)
        {
            this.EndPoint = new Point(this.EndPoint.X + deltaX, this.EndPoint.Y + deltaY);
            this.SetState();
        }

        public virtual void SetSelectedState()
        {
            foreach (var circleSizer in circleSizerControls)
            {
                circleSizer.Visibility = this.Selected ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public override void UpdateForZoomChange()
        {
            foreach (var circleSizer in circleSizerControls)
            {
                var st = circleSizer.LayoutTransform as ScaleTransform;
                st.ScaleX = 1.0 / this.owningCanvas.GetScaleTransform().ScaleX;
                st.ScaleY = 1.0 / this.owningCanvas.GetScaleTransform().ScaleY;
            }
            SetState();
        }

        public void OnMoving(System.Windows.Point pt)
        {
            Moving?.Invoke(this, pt);
        }

        public event EventHandler<System.Windows.Point>? Moving;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract System.Collections.Generic.List<UIElement> GetZoomCanvasElements();

        internal void SetOwner(MainCanvas mainCanvas)
        {
            this.Clear();
            this.owningCanvas = mainCanvas.innerCanvas;
            this.AddToOwnerCanvas();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class MeasureElementData
    {
        public Point StartPoint;
        public Point EndPoint;
        public Tool ElementType;
    }

    public class MeasureElementResizeData
    {
        public MeasureElementResizeData(MeasurementElementZoomCanvasShape measurementElementZoomCanvasShape, object tag)
        {
            MeasEl = measurementElementZoomCanvasShape;
            Tag = tag;
        }

        public MeasurementElementZoomCanvasShape MeasEl { get; set; }
        public object Tag { get; set; }

    }
}