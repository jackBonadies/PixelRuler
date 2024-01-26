using System;
using System.Collections.Generic;
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

namespace PixelRuler
{
    /// <summary>
    /// Interaction logic for MainCanvas.xaml
    /// </summary>
    public partial class MainCanvas : UserControl
    {
        BoundingBoxElement boundingBox;
        ColorPickElement colorPickBox;
        bool drawingRectangle;

        public EventHandler<double> ScaleChanged;

        public MainCanvas()
        {
            InitializeComponent();

            //this.ViewModel.ImageSourceChanged += imageSourceChanged;
            this.DataContextChanged += MainCanvas_DataContextChanged;

            this.mainImage.SourceUpdated += MainCanvas_SourceUpdated;
            this.mainCanvas.MouseWheel += Canvas_MouseWheel;

            //this.mainCanvas.PreviewMouseRightButtonDown += MainCanvas_MouseRightButtonDown;
            //this.mainCanvas.PreviewMouseRightButtonUp += MainCanvas_MouseRightButtonUp;

            this.mainCanvas.MouseEnter += MainCanvas_MouseEnter;
            this.mainCanvas.MouseLeave += MainCanvas_MouseLeave;
            this.mainCanvas.MouseMove += MainCanvas_MouseMove;
            this.mainCanvas.MouseDown += MainCanvas_MouseDown;
            this.mainCanvas.MouseUp += MainCanvas_MouseUp;

            //this.mainCanvas.MouseLeftButtonDown += MainCanvas_MouseLeftButtonDown;
            //this.mainCanvas.MouseLeftButtonUp += MainCanvas_MouseLeftButtonUp;

        }

        private void MainCanvas_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ViewModel.ImageSourceChanged += ViewModel_ImageSourceChanged;
        }
        private static void SetImageLocation(Canvas canvas, Image image)
        {
            // image should be center or if larger than screen bounds then topleft.
            canvas.GetTranslateTransform().X = -Canvas.GetLeft(image);
            canvas.GetTranslateTransform().Y = -Canvas.GetTop(image);
        }

        private void ViewModel_ImageSourceChanged(object? sender, EventArgs e)
        {
            SetImageLocation(mainCanvas, mainImage);    
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
                this.EndBoundingBox(e);
            }
            else if(e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Right)
            {
                this.EndPan(e);
            }
        }

        private void EndPan(MouseButtonEventArgs e)
        {
            mainCanvas.ReleaseMouseCapture();

            isPanning = false;
        }

        private void EndBoundingBox(MouseButtonEventArgs e)
        {
            mainCanvas.Cursor = cursorOld;
            mainCanvas.ReleaseMouseCapture();
            drawingRectangle = false;

            if(boundingBox != null)
            {
                if(boundingBox.Width == 0 && boundingBox.Height == 0)
                {
                    boundingBox.Clear();
                }
            }
        }

        private void ToolDown(MouseButtonEventArgs e)
        {
            switch(ViewModel.SelectedTool)
            {
                case Tool.BoundingBox:
                    StartBoundingBox(e);
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
            mainCanvas.CaptureMouse();

            totalAmountToMoveX = 0;
            totalAmountToMoveY = 0;

            var tt = this.mainCanvas.GetTranslateTransform();

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

        private void StartBoundingBox(MouseButtonEventArgs e)
        {
            mainCanvas.CaptureMouse();

            drawingRectangle = true;
            cursorOld = mainCanvas.Cursor;
            mainCanvas.Cursor = Cursors.None;

            var roundedPoint = roundToPixel(e.GetPosition(mainCanvas));

            boundingBox?.Clear();
            boundingBox = new BoundingBoxElement(this.mainCanvas, roundedPoint);
            ViewModel.BoundingBoxLabel = boundingBox.BoundingBoxLabel;
        }

        private Cursor cursorOld;

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
                    colorPickBox = new ColorPickElement(this.mainCanvas);
                }
                var truncatedPoint = truncate(e.GetPosition(mainCanvas));
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
                var tt = this.mainCanvas.GetTranslateTransform();

                totalAmountToMoveX += delta.X;
                totalAmountToMoveY += delta.Y;

                tt.X = Math.Round((origX + totalAmountToMoveX));
                tt.Y = Math.Round((origY + totalAmountToMoveY));
                //tt.Y += delta.Y * ScaleTransform.ScaleY;
            }
            else if (drawingRectangle)
            {
                var roundedPoint = roundToPixel(e.GetPosition(mainCanvas));
                boundingBox.EndPoint = roundedPoint;
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

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var canvas = sender as Canvas;
            var st = this.mainCanvas.GetScaleTransform();

            var mousePos = System.Windows.Input.Mouse.GetPosition(canvas);
            var pointToKeepAtLocation = constrainToImage(mousePos);

            bool exp = true;
            double zoomExpDelta = e.Delta > 0 ? 2 : .5;
            var oldScale = st.ScaleX;

            st.ScaleX *= zoomExpDelta;
            st.ScaleY *= zoomExpDelta;

            st.ScaleX = clampScale(st.ScaleX);
            st.ScaleY = clampScale(st.ScaleY);

            var newScale = st.ScaleX;

            var tt = this.mainCanvas.GetTranslateTransform();

            var newX = newScale * pointToKeepAtLocation.X;
            var oldX = oldScale * pointToKeepAtLocation.X;

            var newY = newScale * pointToKeepAtLocation.Y;
            var oldY = oldScale * pointToKeepAtLocation.Y;

            var diffToCorrectX = newX - oldX;
            var diffToCorrectY = newY - oldY;

            tt.X -= diffToCorrectX;
            tt.Y -= diffToCorrectY;


            ScaleChanged?.Invoke(this, st.ScaleX);
            UpdateForZoomChange(); // TODO will the event be lagged?

            e.Handled = true;
        }

        private double clampScale(double scale)
        {
            return Math.Max(Math.Min(scale, App.MaxZoom * .01), App.MinZoom * .01);
        }

        private void UpdateForZoomChange()
        {
            if(colorPickBox != null)
            {
                colorPickBox.UpdateForZoomChange();
            }
            //var items = this.mainCanvas.Children.OfType<IZoomCanvasShape>();
            //foreach (IZoomCanvasShape item in items)
            //{
            //    item.UpdateForZoomChange();
            //}
            if (boundingBox != null)
            {
                boundingBox.UpdateForZoomChange();
            }
        }

        internal void SetImage(BitmapSource img)
        {
            this.mainImage.Source = img;
            SetImageLocation(this.mainCanvas, this.mainImage);
        }

        private void mainImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void mainImage_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
