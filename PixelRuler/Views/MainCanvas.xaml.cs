using PixelRuler.CanvasElements;
using PixelRuler.Common;
using PixelRuler.Views;
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
        public ObservableCollection<MeasurementElementZoomCanvasShape> MeasurementElements { get; private set; } = new();
        public ObservableCollection<IOverlayCanvasShape> OverlayCanvasElements { get; private set; } = new();
        ColorPickElement? colorPickBox;
        bool drawingShape;

        public EventHandler<double> EffectiveZoomChanged;

        ZoomBox zoomBox;

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

            this.overlayCanvas.PreviewMouseDown += OverlayCanvas_MouseDown;
            this.overlayCanvas.PreviewMouseMove += OverlayCanvas_MouseMove;
            this.overlayCanvas.PreviewMouseUp += OverlayCanvas_MouseUp;
            this.overlayCanvas.MouseEnter += OverlayCanvas_MouseEnter;
            this.overlayCanvas.MouseLeave += OverlayCanvas_MouseLeave;




            this.KeyDown += MainCanvas_KeyDown;

            //this.mainCanvas.MouseLeftButtonDown += MainCanvas_MouseLeftButtonDown;
            //this.mainCanvas.MouseLeftButtonUp += MainCanvas_MouseLeftButtonUp;
            MeasurementElements.CollectionChanged += MeasurementElements_CollectionChanged;

            this.LostFocus += MainCanvas_LostFocus;
            this.LostMouseCapture += MainCanvas_LostMouseCapture;

            this.gridLineTop.MainCanvas = this;
            this.gridLineTop.MouseMove += GridLine_MouseMove;
            this.gridLineTop.MouseEnter += GridLine_MouseEnter;
            this.gridLineTop.MouseLeave += GridLine_MouseLeave;
            this.gridLineTop.MouseLeftButtonUp += GridLine_LeftButtonUp;

            this.gridLineLeft.MainCanvas = this;
            this.gridLineLeft.MouseMove += GridLine_MouseMove;
            this.gridLineLeft.MouseEnter += GridLine_MouseEnter;
            this.gridLineLeft.MouseLeave += GridLine_MouseLeave;
            this.gridLineLeft.MouseLeftButtonUp += GridLine_LeftButtonUp;

            this.Loaded += MainCanvas_Loaded;
            this.SizeChanged += MainCanvas_SizeChanged;

        }

        private IntBucket resizeXbucket = new IntBucket();
        private IntBucket resizeYbucket = new IntBucket();

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e.PreviousSize == new Size(0,0))
            {
                return;
            }
            if(this.shouldCenterImage())
            {
                var deltaX = (e.NewSize.Width - e.PreviousSize.Width);
                var deltaY = (e.NewSize.Height - e.PreviousSize.Height);
                var tt = this.innerCanvas.GetTranslateTransform();
                resizeXbucket.Add(deltaX / 2);
                resizeYbucket.Add(deltaY / 2);
                tt.X += resizeXbucket.GetValue();
                tt.Y += resizeYbucket.GetValue();
            }
        }

        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            if (this.ViewModel.ShowGridLines)
            {
                this.gridLineTop.SetZoom(1);
                this.gridLineLeft.SetZoom(1);
            }

            SetupForDpi();

            SetImageLocation(this.innerCanvas, mainImage);
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            SetupForDpi();
            base.OnDpiChanged(oldDpi, newDpi);
        }

        private void SetupForDpi()
        {
            this.gridLineCorner.Width = UiUtils.GetBorderPixelSize(this.GetDpi());
            this.gridLineCorner.Height = UiUtils.GetBorderPixelSize(this.GetDpi()) + 1 / this.GetDpi();
        }

        private void OverlayCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            zoomBox.Hide();
        }

        private void OverlayCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(this.ViewModel.Settings.ZoomBoxQuickZoomKey))
            {
                zoomBox.Show(null, e, ZoomBoxCase.QuickZoom);
            }
        }

        private void GridLine_LeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var isVertical = (sender as Gridline).IsVertical;
            var zoomCanvasCoor = overlayCanvas.TranslatePoint(e.GetPosition(this.overlayCanvas), this.innerCanvas);
            var gridLineCoor = e.GetPosition(this.gridLineTop.canvas);

            var coor = isVertical ? zoomCanvasCoor.Y : zoomCanvasCoor.X;
            var roundCoor = (int)Math.Round(coor);
            var guideLineElement = new GuidelineElement(this, roundCoor, isVertical);
            guideLineElement.UpdateForZoomChange();
            MeasurementElements.Add(guideLineElement);
            guideLineElement.AddToOwnerCanvas();
            // add to gridline view.
            if(isVertical)
            {
                var guidelineTick = new GuidelineTick(gridLineLeft, guideLineElement, GuidelineTick.GridlineTickType.Guideline);
                gridLineLeft.AddTick(guidelineTick);
            }
            else
            {
                var guidelineTick = new GuidelineTick(gridLineTop, guideLineElement, GuidelineTick.GridlineTickType.Guideline);
                gridLineTop.AddTick(guidelineTick);
            }

            //guideLineElement.UpdateForCoordinatesChanged();
        }

        private void GridLine_MouseLeave(object sender, MouseEventArgs e)
        {
            if(sender == gridLineTop)
            {
                verticalPendingLine.Visibility = Visibility.Collapsed;
            }
            else
            {
                horizontalPendingLine.Visibility = Visibility.Collapsed;
            }
        }

        private void GridLine_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void GridLine_MouseMove(object sender, MouseEventArgs e)
        {
            var pt = UiUtils.RoundPoint(e.GetPosition(this.innerCanvas));

            if(sender == gridLineTop)
            {
                verticalPendingLine.X1 = pt.X;
                verticalPendingLine.X2 = pt.X;
                verticalPendingLine.Visibility = Visibility.Visible;
            }
            else
            {
                horizontalPendingLine.Y1 = pt.Y;
                horizontalPendingLine.Y2 = pt.Y;
                horizontalPendingLine.Visibility = Visibility.Visible;
            }
        }

        private void OverlayCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                if(ViewModel.SelectedTool == Tool.ColorPicker)
                {
                    zoomBox.Show(null, e, ZoomBoxCase.ColorPicker);
                }
            }
        }

        private void OverlayCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                if(ViewModel.SelectedTool == Tool.ColorPicker)
                {
                    zoomBox.Hide();
                }
            }
        }

        private void OverlayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            zoomBox.OnOverlayCanvasMouseMove();
            //(zoomBox.Fill as VisualBrush).Viewbox.X = e.GetPosition(innerCanvas).X;
            if (this.ViewModel.ShowGridLines)
            {
                var roundedPoint = UiUtils.RoundPoint(e.GetPosition(this.mainImage));
                gridLineTop.SetCurrentMousePosition(roundedPoint);
                gridLineLeft.SetCurrentMousePosition(roundedPoint);
            }
        }


        private void MainCanvas_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (drawingShape)
            {
                this.EndShape();
            }
            else if(isPanning)
            {
                this.EndPan();
            }
        }

        private void MainCanvas_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void MainCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            int moveX = 0;
            int moveY = 0;
            if (e.Key == Key.Right)
            {
                moveX = 1;
            }
            else if (e.Key == Key.Left)
            {
                moveX = -1;
            }
            else if (e.Key == Key.Up)
            {
                moveY = -1;
            }
            else if (e.Key == Key.Down)
            {
                moveY = 1;
            }

            if (moveX != 0 || moveY != 0)
            {
                SelectedMeasureElements.ForEachExt(it => it.Move(moveX, moveY));
                e.Handled = true;
            }
        }

        public bool CopySelectedData()
        {
            if(this.SelectedMeasureElements.Any())
            {
                var copiedMeasElData = new List<MeasureElementData>();
                foreach (var el in SelectedMeasureElements)
                {
                    // clone for source
                    // clone again to paste
                    //todo or store source data i.e. strart and end (nothing else needed)
                    copiedMeasElData.Add(el.Archive());
                }
                copyCanvasData = new CopyData(copiedMeasElData);
                return true;
            }
            return false;
        }

        public bool PasteCopiedData()
        {
                if(copyCanvasData != null && copyCanvasData.CopiedData.Any())
                {
                    var wasSelected = this.SelectedMeasureElements.ToList();
                    foreach (var item in wasSelected) 
                    {
                        item.Selected = false;
                    }

                    try
                    {
                        selectAll = true;
                        copyCanvasData.NumTimesPasted++;
                        Point offset = new Point(30 * copyCanvasData.NumTimesPasted, 30 * copyCanvasData.NumTimesPasted); //may want shape to provide whether its vert or horz
                        foreach (var data in copyCanvasData.CopiedData)
                        {
                            var newEl = MeasurementElementZoomCanvasShape.FromMeasureElementData(data, innerCanvas);
                            newEl.StartPoint = newEl.StartPoint.Add(offset);
                            newEl.EndPoint = newEl.EndPoint.Add(offset);
                            newEl.FinishedDrawing = true;
                            AddToCanvas(newEl);
                            newEl.SetState();
                            newEl.UpdateForZoomChange();
                            newEl.Selected = true;
                            newEl.SetState();
                        }
                    }
                    finally
                    {
                        selectAll = false;
                    }
                return true;
                }
            return false;
        }

        public class CopyData
        {
            public CopyData(List<MeasureElementData> data)
            {
                CopiedData = data;
                NumTimesPasted = 0;
            }
            public List<MeasureElementData> CopiedData;
            public int NumTimesPasted;
        }

        //move to viewmodel TODO
        CopyData? copyCanvasData = null;

        private IEnumerable<MeasurementElementZoomCanvasShape> SelectedMeasureElements
        {
            get
            {
                return this.MeasurementElements.Where(it => it.Selected);
            }
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
            Bind(true);

            SetClearAllMeasurementsEnabledState();
            SetShowGridLineState();

            zoomBox = new ZoomBox(this, 256, this.ViewModel);
            Canvas.SetZIndex(zoomBox, 1200);
            this.overlayCanvas.Children.Add(zoomBox);
            this.SetCursor();
        }

        public void Bind(bool bind)
        {
            if(bind)
            {
                this.ViewModel.ImageSourceChanged += ViewModel_ImageSourceChanged;
                this.ViewModel.ZoomChanged += SelectedZoomChanged;
                this.ViewModel.SelectedToolChanged += ViewModel_SelectedToolChanged;
                this.ViewModel.ClearAllMeasureElements += ClearAllMeasureElements;
                this.ViewModel.DeleteAllSelectedElements += DeleteAllSelectedMeasureElements;
                this.ViewModel.AllElementsSelected += AllElementsSelected;
                this.ViewModel.ShowGridLinesChanged += ViewModel_ShowGridLinesChanged;
                this.ViewModel.ImageUpdated += ViewModel_ImageUpdated;
            }
            else
            {
                this.ViewModel.ImageSourceChanged -= ViewModel_ImageSourceChanged;
                this.ViewModel.ZoomChanged -= SelectedZoomChanged;
                this.ViewModel.SelectedToolChanged -= ViewModel_SelectedToolChanged;
                this.ViewModel.ClearAllMeasureElements -= ClearAllMeasureElements;
                this.ViewModel.DeleteAllSelectedElements -= DeleteAllSelectedMeasureElements;
                this.ViewModel.AllElementsSelected -= AllElementsSelected;
                this.ViewModel.ShowGridLinesChanged -= ViewModel_ShowGridLinesChanged;
                this.ViewModel.ImageUpdated -= ViewModel_ImageUpdated;
            }
        }

        private void ViewModel_ImageUpdated(object? sender, EventArgs e)
        {
            this.SetImage(this.ViewModel.ImageSource);
        }

        private void ViewModel_SelectedToolChanged(object? sender, EventArgs e)
        {
            this.SetCursor();
        }

        private void SetCursor()
        {
            switch(ViewModel.SelectedTool)
            {
                case Tool.BoundingBox:
                case Tool.Ruler:
                    this.Cursor = Cursors.Cross;
                    break;
                case Tool.ColorPicker:
                    this.Cursor = this.FindResource("EyeDropperCursor") as Cursor;
                    break;
                default:
                    throw new Exception("Unknown Tool");
            }
        }

        private void SetShowGridLineState()
        {
            if(this.ViewModel.ShowGridLines)
            {
                Canvas.SetLeft(this.innerCanvas, UiUtils.GetBorderPixelSize(this.GetDpi())); //TODO dpi changed
                Canvas.SetTop(this.innerCanvas, UiUtils.GetBorderPixelSize(this.GetDpi()));
                this.gridLineCorner.Visibility = Visibility.Visible;
                this.gridLineTop.Visibility = Visibility.Visible;
                this.gridLineLeft.Visibility = Visibility.Visible;

                this.gridLineLeft.SetZoom(this.EffectiveZoom);
                this.gridLineTop.SetZoom(this.EffectiveZoom);

                gridLineTop.UpdateTranslation();
                gridLineTop.UpdateTickmarks();

                gridLineLeft.UpdateTranslation();
                gridLineLeft.UpdateTickmarks();
            }
            else
            {
                Canvas.SetLeft(this.innerCanvas, 0);
                Canvas.SetTop(this.innerCanvas, 0);
                this.gridLineCorner.Visibility = Visibility.Collapsed;
                this.gridLineTop.Visibility = Visibility.Collapsed;
                this.gridLineLeft.Visibility = Visibility.Collapsed;
            }
        }

        private void ViewModel_ShowGridLinesChanged(object? sender, EventArgs e)
        {
            SetShowGridLineState();
        }

        bool selectAll = false;
        private void AllElementsSelected(object? sender, EventArgs e)
        {
            try
            {
                selectAll = true;
                foreach(var measEl in MeasurementElements)
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
            foreach(var measEl in MeasurementElements)
            {
                measEl.Clear();
            }
            MeasurementElements.Clear();
        }

        private void DeleteAllSelectedMeasureElements(object? sender, EventArgs e)
        {
            var selectedItems = MeasurementElements.Where(it => it.Selected).ToList();
            if(selectedItems.Contains(ViewModel.ActiveMeasureElement))
            {
                ViewModel.ActiveMeasureElement = null;
            }
            foreach (var measEl in selectedItems)
            {
                measEl.Clear();
                MeasurementElements.Remove(measEl);
            }
        }

        private void SetClearAllMeasurementsEnabledState()
        {
            bool anyNonEmpty = false;
            foreach (var measEl in MeasurementElements)
            {
                if(!measEl.IsEmpty && measEl.FinishedDrawing)
                {
                    anyNonEmpty = true;
                    break;
                }
            }
        }

        private bool shouldCenterImage()
        {
            // dont need dpi scaled here. already reverse scaled.
            if(this.ActualHeight <= this.ViewModel.Image.Height)
            {
                return false;
            }
            return true;
        }

        private void SetImageLocation(Canvas canvas, Image image)
        {
            // image should be center or if larger than screen bounds then topleft.
            double x = -Canvas.GetLeft(image);
            double y = -Canvas.GetTop(image);

            if(this.IsLoaded)
            {
                if (shouldCenterImage())
                {
                    x += (this.ActualWidth - this.ViewModel.Image.Width) / 2.0;
                    y += (this.ActualHeight - this.ViewModel.Image.Height) / 2.0;
                }
            }
            canvas.GetTranslateTransform().X = (int)Math.Round(x);
            canvas.GetTranslateTransform().Y = (int)Math.Round(y);
        }

        private void ViewModel_ImageSourceChanged(object? sender, EventArgs e)
        {
            // happens before Loaded... cant get this.ActualHeight...
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
            gridLineLeft.HideCurrentPosIndicator();
            gridLineTop.HideCurrentPosIndicator();
        }

        private void MainCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            gridLineLeft.ShowCurrentPosIndicator();
            gridLineTop.ShowCurrentPosIndicator();
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.EndShape();
            }
            else if(e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Right)
            {
                this.EndPan();
            }
        }

        private void EndPan()
        {
            isPanning = false;
            innerCanvas.ReleaseMouseCapture();
        }

        private void EndShape()
        {
            drawingShape = false;
            innerCanvas.Cursor = cursorOld;
            innerCanvas.ReleaseMouseCapture();

            if(currentMeasurementElement != null)
            {
                currentMeasurementElement.FinishedDrawing = true;
                if(currentMeasurementElement.IsEmpty)
                {
                    currentMeasurementElement.Clear();
                    MeasurementElements.Remove(currentMeasurementElement);
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
            if (e.ChangedButton == MouseButton.Left && !ViewModel.IsInWindowSelection())
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
                    //throw new Exception("No View Model on Main Canvas");
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
            var pt = UiUtils.TruncatePoint(e.GetPosition(mainImage));

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
            foreach(var measEl in MeasurementElements)
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

            var roundedPoint = UiUtils.RoundPoint(e.GetPosition(innerCanvas));

            if(!isSticky)
            {
                currentMeasurementElement?.Clear();
            }

            if(ViewModel.SelectedTool == Tool.BoundingBox)
            {
                currentMeasurementElement = new BoundingBoxElement(this.innerCanvas);
            }
            else
            {
                currentMeasurementElement = new RulerElement(this.innerCanvas);
            }


            currentMeasurementElement.StartPoint = roundedPoint;

            AddToCanvas(currentMeasurementElement);

            ViewModel.ActiveMeasureElement = currentMeasurementElement; //TODO why are these even different, it should just be ViewModel.ActiveMeasureElement no second field
        }

        private void AddToCanvas(MeasurementElementZoomCanvasShape el)
        {
            el.AddToOwnerCanvas();
            MeasurementElements.Add(el);
            el.SelectedChanged += CurrentMeasurementElement_SelectedChanged;
            el.Moving += CurrentMeasurementElement_Moving;
            el.StartResize += CurrentMeasurementElement_StartResize;
            el.Resizing += CurrentMeasurementElement_Resizing;
            el.EndResize += CurrentMeasurementElement_EndResize;
        }

        private void CurrentMeasurementElement_EndResize(object? sender, MeasureElementResizeData e)
        {
            zoomBox.Hide();
        }

        private void CurrentMeasurementElement_Resizing(object? sender, MeasureElementResizeData e)
        {
            zoomBox.UpdateForElementResize(sender as MeasurementElementZoomCanvasShape, e);
        }

        private void CurrentMeasurementElement_StartResize(object? sender, MeasureElementResizeData e)
        {
            zoomBox.Show(e, null, ZoomBoxCase.Resizer);
        }

        private void CurrentMeasurementElement_Moving(object? sender, Point e)
        {
            this.SelectedMeasureElements
                .Append(sender as MeasurementElementZoomCanvasShape)
                .Distinct()
                .ForEachExt(it => it.Move((int)e.X, (int)e.Y));
        }

        private void CurrentMeasurementElement_SelectedChanged(object? sender, EventArgs e)
        {
            if(sender is MeasurementElementZoomCanvasShape measEl)
            {
                if(measEl.Selected)
                {
                    ViewModel.ActiveMeasureElement = measEl;
                    if(KeyUtil.IsMultiSelect() || selectAll)
                    {
                        return;
                    }
                    var toDeselect = MeasurementElements.Where(it => it != measEl && it.Selected);
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


        double totalAmountToMoveX;
        double totalAmountToMoveY;

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            ViewModel.CurrentPosition = UiUtils.TruncatePoint(e.GetPosition(mainImage));

            if (ViewModel.SelectedTool == Tool.ColorPicker)
            {
                if(colorPickBox == null)
                {
                    colorPickBox = new ColorPickElement(this.innerCanvas);
                }
                var truncatedPoint = UiUtils.TruncatePoint(e.GetPosition(innerCanvas));
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

                //var prevX = tt.X;
                //var prevY = tt.Y;

                tt.X = Math.Round((origX + totalAmountToMoveX));
                tt.Y = Math.Round((origY + totalAmountToMoveY));

                applyConstraints();

                Panning?.Invoke(this, EventArgs.Empty);

                var ocp = e.GetPosition(this.overlayCanvas);
                var overlayPt = mainImage.TranslatePoint(ocp, this.overlayCanvas);

                if(ViewModel.ShowGridLines)
                {
                    gridLineTop.UpdateTranslation();
                    gridLineTop.UpdateTickmarks();

                    gridLineLeft.UpdateTranslation();
                    gridLineLeft.UpdateTickmarks();
                }


                foreach(var overlayElement in OverlayCanvasElements)
                {
                    overlayElement.UpdateForCoordinatesChanged();
                }



                //var leftCanvasSpace = new Point(Canvas.GetLeft(mainImage), Canvas.GetTop(mainImage));
                //var realSpaceFromCanvasSpace = innerCanvas.TranslatePoint(leftCanvasSpace, this);
                //var minTranslateX = this.ActualWidth * .8;
                //if (realSpaceFromCanvasSpace.X > minTranslateX)
                //{
                //    //var minX = this.TranslatePoint(new Point(minTranslateX, 0), innerCanvas);
                //    //tt.X = minX.X;
                //    //tt.Y = prevY;
                //}

                //tt.Y += delta.Y * ScaleTransform.ScaleY;
            }
            else if (drawingShape)
            {
                var roundedPoint = UiUtils.RoundPoint(e.GetPosition(innerCanvas));
                currentMeasurementElement.SetEndPoint(roundedPoint);
            }
        }

        public event EventHandler<EventArgs> Panning;

        private void applyConstraints()
        {
            if(!this.ViewModel.FullscreenScreenshotMode)
            {
                return;
            }
            var tt = this.innerCanvas.GetTranslateTransform();

            tt.X = Math.Min(tt.X, -10000 * this.CanvasScaleTransform.ScaleX);
            tt.Y = Math.Min(tt.Y, -10000 * this.CanvasScaleTransform.ScaleX);

            tt.X = Math.Max(tt.X, (-10000 * this.CanvasScaleTransform.ScaleX - (this.mainImage.ActualWidth * this.CanvasScaleTransform.ScaleX - this.ActualWidth)));
            tt.Y = Math.Max(tt.Y, (-10000 * this.CanvasScaleTransform.ScaleX - (this.mainImage.ActualHeight * this.CanvasScaleTransform.ScaleX - this.ActualHeight)));
        }

        //private Point applyConstraints(Point mouseEndPoint)
        //{
        //    var roundedEndPoint = UiUtils.RoundPoint(mouseEndPoint);
        //    if(currentMeasurementElement is BoundingBoxElement)
        //    {
        //        var diffX = Math.Abs(currentMeasurementElement.StartPoint.X - roundedEndPoint.X);
        //        var diffY = Math.Abs(currentMeasurementElement.StartPoint.Y - roundedEndPoint.Y);
        //        var newX = currentMeasurementElement.StartPoint.X + diffX;
        //        var newY = currentMeasurementElement.StartPoint.Y + diffY;
        //        return new Point(newX, newY);
        //    }
        //    return roundedEndPoint;
        //}

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

            foreach(var overlayElement in OverlayCanvasElements)
            {
                overlayElement.UpdateForCoordinatesChanged();
            }

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

                tt.X = (int)Math.Round(tt.X);
                tt.Y = (int)Math.Round(tt.Y);
            }

            if(this.ViewModel.ShowGridLines)
            {
                gridLineTop.SetZoom(st.ScaleX);
                gridLineTop.UpdateTranslation();

                gridLineLeft.SetZoom(st.ScaleX);
                gridLineLeft.UpdateTranslation();
            }

            EffectiveZoomChanged?.Invoke(this, st.ScaleX);
            UpdateForZoomChange(); // TODO will the event be lagged?
            applyConstraints();
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
            var minZoomPercent = App.MinZoomPercent;
            if (this.ViewModel.FullscreenScreenshotMode)
            {
                minZoomPercent = 100;
            }
            return Math.Max(Math.Min(scale, App.MaxZoomPercent * .01), minZoomPercent * .01);
        }

        private void UpdateForZoomChange()
        {
            colorPickBox?.UpdateForZoomChange();
            screenshotElement?.UpdateForZoomChange();

            foreach(var measEl in MeasurementElements)
            {
                measEl.UpdateForZoomChange();
            }
        }

        internal void SetImage(BitmapSource? img)
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

        public void ShowZoomBox()
        {
            this.zoomBox.Show(null, null, ViewModel.IsInWindowSelection() ? ZoomBoxCase.ScreenshotBoundSelection : ZoomBoxCase.QuickZoom);
        }

        public void HideZoomBox()
        {
            this.zoomBox.Hide();
        }

        ScreenshotElement? screenshotElement;

        public void SetScreenshotElementPosition(MouseEventArgs e)
        {
            if(screenshotElement == null)
            {
                screenshotElement = new ScreenshotElement(this.innerCanvas);
                screenshotElement.AddToOwnerCanvas();
            }
            screenshotElement.SetPosition(e);
        }

        public void ShowHideScreenshot(bool show)
        {
            if (screenshotElement != null)
            {
                screenshotElement.Visible = show;
            }
        }
    }
}
