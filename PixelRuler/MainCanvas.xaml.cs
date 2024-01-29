using PixelRuler.CanvasElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.PointOfService;

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for MainCanvas.xaml
    /// </summary>
    public partial class MainCanvas : UserControl
    {
        MeasurementElementZoomCanvasShape? currentMeasurementElement;
        ObservableCollection<MeasurementElementZoomCanvasShape> measurementElements = new ObservableCollection<MeasurementElementZoomCanvasShape>();
        ColorPickElement? colorPickBox;
        bool drawingShape;

        public EventHandler<double> EffectiveZoomChanged;

        public MainCanvas()
        {
            InitializeComponent();

            //this.ViewModel.ImageSourceChanged += imageSourceChanged;
            this.DataContextChanged += MainCanvas_DataContextChanged;

            this.mainImage.SourceUpdated += MainCanvas_SourceUpdated;
            this.innerCanvas.MouseWheel += Canvas_MouseWheel;

            this.EffectiveZoomChanged += OnEffectiveZoomChanged;
            //this.mainCanvas.PreviewMouseRightButtonDown += MainCanvas_MouseRightButtonDown;
            //this.mainCanvas.PreviewMouseRightButtonUp += MainCanvas_MouseRightButtonUp;

            this.innerCanvas.MouseEnter += MainCanvas_MouseEnter;
            this.innerCanvas.MouseLeave += MainCanvas_MouseLeave;
            this.innerCanvas.MouseMove += MainCanvas_MouseMove;
            this.innerCanvas.MouseDown += MainCanvas_MouseDown;
            this.innerCanvas.MouseUp += MainCanvas_MouseUp;

            //this.mainCanvas.MouseLeftButtonDown += MainCanvas_MouseLeftButtonDown;
            //this.mainCanvas.MouseLeftButtonUp += MainCanvas_MouseLeftButtonUp;
            measurementElements.CollectionChanged += MeasurementElements_CollectionChanged;

        }

        private void MeasurementElements_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetClearAllMeasurementsEnabledState();
        }

        Point lastCenterPoint;
        double lastDistance;
        private void MainCanvas_TouchDown(object? sender, TouchEventArgs e)
        {
            //if (e.TouchDevice.GetIntermediateTouchPoints(innerCanvas).Count == 2)
            //{
            //    e.Handled = true;
            //    var points = e.TouchDevice.GetIntermediateTouchPoints(innerCanvas);
            //    lastCenterPoint = new Point(
            //        (points[0].Position.X + points[1].Position.X) / 2,
            //        (points[0].Position.Y + points[1].Position.Y) / 2);
            //    lastDistance = GetDistance(points[0].Position, points[1].Position);
            //}
        }

        private double GetDistance(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        private void MainCanvas_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ViewModel.ImageSourceChanged += ViewModel_ImageSourceChanged;
            this.ViewModel.ZoomChanged += SelectedZoomChanged;
            this.ViewModel.ClearAllMeasureElements += ClearAllMeasureElements;
            this.ViewModel.DeleteAllSelectedElements += DeleteAllSelectedMeasureElements;
            this.ViewModel.AllElementsSelected += AllElementsSelected;
            SetClearAllMeasurementsEnabledState();
        }

        bool selectAll = false;
        private void AllElementsSelected(object? sender, EventArgs e)
        {
            try
            {
                selectAll = true;
                foreach(var measEl in measurementElements)
                {
                    measEl.Selected = true;
                }
            }
            finally
            {
                selectAll = false;
            }
        }

        private void ClearAllMeasureElements(object? sender, EventArgs e)
        {
            foreach(var measEl in measurementElements)
            {
                measEl.Clear();
            }
            measurementElements.Clear();
        }

        private void DeleteAllSelectedMeasureElements(object? sender, EventArgs e)
        {
            var selectedItems = measurementElements.Where(it => it.Selected).ToList();
            foreach (var measEl in selectedItems)
            {
                measEl.Clear();
                measurementElements.Remove(measEl);
            }
        }

        private void SetClearAllMeasurementsEnabledState()
        {
            bool anyNonEmpty = false;
            foreach (var measEl in measurementElements)
            {
                if(!measEl.IsEmpty && measEl.FinishedDrawing)
                {
                    anyNonEmpty = true;
                    break;
                }
            }
            this.ViewModel.ClearAllMeasureElementsCommand.SetCanExecute(anyNonEmpty);
        }

        private static void SetImageLocation(Canvas canvas, Image image)
        {
            // image should be center or if larger than screen bounds then topleft.
            canvas.GetTranslateTransform().X = -Canvas.GetLeft(image);
            canvas.GetTranslateTransform().Y = -Canvas.GetTop(image);
        }

        private void ViewModel_ImageSourceChanged(object? sender, EventArgs e)
        {
            SetImageLocation(innerCanvas, mainImage);    
        }


        /// <summary>
        /// Canvas was updated, update ViewModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnEffectiveZoomChanged(object? sender, double e)
        {
            this.ViewModel.CurrentZoomPercent = (e * 100);
        }


        /// <summary>
        /// ViewModel was updated, update canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keepMousePositionAtLocation"></param>
        public void SelectedZoomChanged(object? sender, ZoomBehavior zoomBehavior)
        {
            bool force = zoomBehavior == ZoomBehavior.ResetWindow;
            if (this.EffectiveZoom != this.ViewModel.CurrentZoom || force)
            {
                var centerPt = getPointToKeepAtLocation(zoomBehavior);
                this.Zoom(this.EffectiveZoom, this.ViewModel.CurrentZoom, centerPt, zoomBehavior);
            }
        }

        private Point getPointToKeepAtLocation(ZoomBehavior zoomBehavior)
        {
            bool isWithinBounds = true;
            bool keepMousePositionAtLocation = zoomBehavior == ZoomBehavior.KeepMousePositionFixed;
            if (keepMousePositionAtLocation)
            {
                var mousePos = Mouse.GetPosition(this);
                if(mousePos.X < 0 || 
                   mousePos.Y < 0 || 
                   mousePos.X >= this.ActualWidth || 
                   mousePos.Y >= this.ActualHeight)
                {
                    isWithinBounds = false;
                }
            }

            if(keepMousePositionAtLocation && isWithinBounds)
            {
                return Mouse.GetPosition(this.innerCanvas);
            }
            else
            {
                var startPt = this.innerCanvas.RenderTransform.Inverse.Transform(new System.Windows.Point(0, 0));
                var endPt = this.innerCanvas.RenderTransform.Inverse.Transform(new System.Windows.Point(this.ActualWidth, this.ActualHeight));
                var centerPtX = (endPt.X + startPt.X) / 2;
                var centerPtY = (endPt.Y + startPt.Y) / 2;
                return new System.Windows.Point(centerPtX, centerPtY);
            }
        }

        private void imageSourceChanged(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MainCanvas_SourceUpdated(object? sender, DataTransferEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void MainCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            switch(ViewModel.SelectedTool)
            {
                case Tool.BoundingBox:
                    this.Cursor = Cursors.Cross;
                    break;
                case Tool.ColorPicker:
                    this.Cursor = Cursors.Arrow;
                    break;
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.EndShape(e);
            }
            else if(e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Right)
            {
                this.EndPan(e);
            }
        }

        private void EndPan(MouseButtonEventArgs e)
        {
            innerCanvas.ReleaseMouseCapture();

            isPanning = false;
        }

        private void EndShape(MouseButtonEventArgs e)
        {
            innerCanvas.Cursor = cursorOld;
            innerCanvas.ReleaseMouseCapture();
            drawingShape = false;

            if(currentMeasurementElement != null)
            {
                currentMeasurementElement.FinishedDrawing = true;
                if(currentMeasurementElement.IsEmpty)
                {
                    currentMeasurementElement.Clear();
                    measurementElements.Remove(currentMeasurementElement);
                }
            }
            SetClearAllMeasurementsEnabledState();
        }

        private void ToolDown(MouseButtonEventArgs e)
        {
            switch(ViewModel.SelectedTool)
            {
                case Tool.BoundingBox:
                case Tool.Ruler:
                    StartMeasureElement(e);
                    break;
                case Tool.ColorPicker:
                    ColorPickerMouseDown(e);
                    break;
            }
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            this.mainImage.Focus();
            if (e.ChangedButton == MouseButton.Left)
            {
                this.ToolDown(e);
            }
            else if (e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Right)
            {
                this.StartPan(e);
            }
        }

        private void StartPan(MouseButtonEventArgs e)
        {
            innerCanvas.CaptureMouse();

            totalAmountToMoveX = 0;
            totalAmountToMoveY = 0;

            var tt = this.innerCanvas.GetTranslateTransform();

            origX = tt.X;
            origY = tt.Y;

            //tt.X += 10;
            //tt.Y += 10;
            isPanning = true;
            lastPos = System.Windows.Input.Mouse.GetPosition(this);
        }

        private PixelRulerViewModel ViewModel
        {
            get
            {
                var prvm = DataContext as PixelRulerViewModel;
                if (prvm == null)
                {
                    throw new Exception("No View Model on Main Canvas");
                }
                return prvm;
            }
        }

        private void ColorPickerMouseDown(MouseButtonEventArgs e)
        {
            SetColorUnderMouse(e);
        }

        private void SetColorUnderMouse(MouseEventArgs e)
        {
            var pt = truncate(e.GetPosition(mainImage));

            if (this.ViewModel.Image == null ||
                this.ViewModel.Image.Width <= pt.X ||
                this.ViewModel.Image.Height <= pt.Y ||
                pt.X < 0 ||
                pt.Y < 0)
            {
                return;
            }
            else
            {
                this.ViewModel.Color = this.ViewModel.Image.GetPixel((int)pt.X, (int)pt.Y);
            }
        }

        private bool isSticky = true;

        private void UnselectAll()
        {
            foreach(var measEl in measurementElements)
            {
                measEl.Selected = false;
            }
        }


        private void StartMeasureElement(MouseButtonEventArgs e)
        {
            UnselectAll();
            innerCanvas.CaptureMouse();

            drawingShape = true;
            cursorOld = innerCanvas.Cursor;
            //innerCanvas.Cursor = Cursors.None;

            var roundedPoint = roundToPixel(e.GetPosition(innerCanvas));

            if(!isSticky)
            {
                currentMeasurementElement?.Clear();
            }

            if(ViewModel.SelectedTool == Tool.BoundingBox)
            {
                currentMeasurementElement = new BoundingBoxElement(this.innerCanvas, roundedPoint);
                if(currentMeasurementElement is BoundingBoxElement b)
                {
                    ViewModel.BoundingBoxLabel = b.BoundingBoxLabel;
                }
            }
            else
            {
                currentMeasurementElement = new RulerElement(this.innerCanvas, roundedPoint);
            }

            measurementElements.Add(currentMeasurementElement);
            currentMeasurementElement.SelectedChanged += CurrentMeasurementElement_SelectedChanged;
        }

        private void CurrentMeasurementElement_SelectedChanged(object? sender, EventArgs e)
        {
            if(sender is MeasurementElementZoomCanvasShape measEl)
            {
                if(measEl.Selected)
                {
                    if(KeyUtil.IsCtrlDown() || KeyUtil.IsShiftDown() || selectAll)
                    {
                        return;
                    }
                    var toDeselect = measurementElements.Where(it => it != measEl && it.Selected);
                    foreach(var measElToDeselect in toDeselect)
                    {
                        measElToDeselect.Selected = false;
                    }
                }
            }
            else
            {
                throw new Exception("Unexpected type - Selected Changed");
            }
        }

        private Cursor? cursorOld;

        private Point truncate(Point mousePos)
        {
            var roundX = (int)(mousePos.X);
            var roundY = (int)(mousePos.Y);
            return new Point(roundX, roundY);
        }

        private Point roundToPixel(Point mousePos)
        {
            var roundX = Math.Round(mousePos.X);
            var roundY = Math.Round(mousePos.Y);
            return new Point(roundX, roundY);
        }

        double totalAmountToMoveX;
        double totalAmountToMoveY;

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel.SelectedTool == Tool.ColorPicker)
            {
                if(colorPickBox == null)
                {
                    colorPickBox = new ColorPickElement(this.innerCanvas);
                }
                var truncatedPoint = truncate(e.GetPosition(innerCanvas));
                colorPickBox.SetPosition(truncatedPoint);
                if(System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    SetColorUnderMouse(e);
                }
            }


            if (isPanning)
            {
                var newPos = System.Windows.Input.Mouse.GetPosition(this);
                var delta = newPos - lastPos;
                lastPos = newPos;
                var tt = this.innerCanvas.GetTranslateTransform();

                totalAmountToMoveX += delta.X;
                totalAmountToMoveY += delta.Y;

                tt.X = Math.Round((origX + totalAmountToMoveX));
                tt.Y = Math.Round((origY + totalAmountToMoveY));
                //tt.Y += delta.Y * ScaleTransform.ScaleY;
            }
            else if (drawingShape)
            {
                var roundedPoint = roundToPixel(e.GetPosition(innerCanvas));
                currentMeasurementElement.SetEndPoint(roundedPoint);
            }
        }

        private bool isPanning = false;

        double origX;
        double origY;


        private Point constrainToImage(Point pt)
        {
            double left = Canvas.GetLeft(mainImage);
            double right = left + mainImage.ActualWidth;
            double top = Canvas.GetTop(mainImage);
            double bottom = top + mainImage.ActualHeight;

            var x = Math.Min(Math.Max(left, pt.X), right);
            var y = Math.Min(Math.Max(top, pt.Y), bottom);

            return new Point(x, y);
        }


        Point lastPos;

        int mouseWheelAmountAccum = 0;

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var canvas = sender as Canvas;
            var st = this.innerCanvas.GetScaleTransform();

            var mousePos = System.Windows.Input.Mouse.GetPosition(canvas);
            var pointToKeepAtLocation = constrainToImage(mousePos);

            if((mouseWheelAmountAccum > 0 && e.Delta < 0) ||
               (mouseWheelAmountAccum < 0 && e.Delta > 0))
            {
                mouseWheelAmountAccum = 0;
            }
            mouseWheelAmountAccum += e.Delta;

            int thresholdAmount = 30;
            if(mouseWheelAmountAccum >= thresholdAmount)
            {
                mouseWheelAmountAccum %= thresholdAmount;
            }
            else if(mouseWheelAmountAccum <= -thresholdAmount)
            {
                mouseWheelAmountAccum %= thresholdAmount;
            }
            else
            {
                return;
            }

            double zoomExpDelta = e.Delta > 0 ? 2 : .5;

            var oldScale = st.ScaleX;
            var newScale = zoomExpDelta * oldScale;

            Zoom(oldScale, newScale, pointToKeepAtLocation);

            e.Handled = true;
        }

        public void Zoom(double oldScale, double newScale, Point pointToKeepAtLocation, ZoomBehavior zoomBehavior = ZoomBehavior.KeepMousePositionFixed)
        {
            var st = this.innerCanvas.GetScaleTransform();

            newScale = clampScale(newScale);

            st.ScaleX = newScale;
            st.ScaleY = newScale;

            if(zoomBehavior == ZoomBehavior.ResetWindow)
            {
                SetImageLocation(this.innerCanvas, this.mainImage);
            }
            else
            {
                var tt = this.innerCanvas.GetTranslateTransform();

                var newX = newScale * pointToKeepAtLocation.X;
                var oldX = oldScale * pointToKeepAtLocation.X;

                var newY = newScale * pointToKeepAtLocation.Y;
                var oldY = oldScale * pointToKeepAtLocation.Y;

                var diffToCorrectX = newX - oldX;
                var diffToCorrectY = newY - oldY;

                tt.X -= diffToCorrectX;
                tt.Y -= diffToCorrectY;
            }

            EffectiveZoomChanged?.Invoke(this, st.ScaleX);
            UpdateForZoomChange(); // TODO will the event be lagged?
        }

        public double EffectiveZoomPercent
        {
            get
            {
                return EffectiveZoom * 100;
            }
        }

        public double EffectiveZoom
        {
            get
            {
                var st = this.innerCanvas.GetScaleTransform();
                return st.ScaleX;
            }
        }

        private double clampScale(double scale)
        {
            return Math.Max(Math.Min(scale, App.MaxZoomPercent * .01), App.MinZoomPercent * .01);
        }

        private void UpdateForZoomChange()
        {
            if(colorPickBox != null)
            {
                colorPickBox.UpdateForZoomChange();
            }

            foreach(var measEl in measurementElements)
            {
                measEl.UpdateForZoomChange();
            }
        }

        internal void SetImage(BitmapSource img)
        {
            this.ViewModel.CurrentZoomPercent = 100;
            this.mainImage.Source = img;
            SetImageLocation(this.innerCanvas, this.mainImage);
        }

        private void mainImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void mainImage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void innerCanvas_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        private void innerCanvas_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var exp = e.DeltaManipulation.Expansion;
            var trans = e.DeltaManipulation.Translation;
            var rot = e.DeltaManipulation.Rotation;
            var scale = e.DeltaManipulation.Scale;


            e.Handled = true;
        }
    }
}
