using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesignRuler
{
    public enum ShapeType
    {
        BoundingBox = 0,
    }

    public class BoundingBox
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public Rectangle Shape
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Rectangle boundingBox;
        bool drawingRectangle;

        public MainWindow()
        {
            InitializeComponent();
            this.mainCanvas.MouseWheel += Canvas_MouseWheel;
            this.mainCanvas.PreviewMouseRightButtonDown += MainCanvas_MouseRightButtonDown;
            this.mainCanvas.PreviewMouseRightButtonUp += MainCanvas_MouseRightButtonUp;
            this.mainCanvas.MouseMove += MainCanvas_MouseMove;
            this.mainCanvas.MouseLeftButtonDown += MainCanvas_MouseLeftButtonDown;
            this.mainCanvas.MouseLeftButtonUp += MainCanvas_MouseLeftButtonUp;
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            drawingRectangle = false;
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            drawingRectangle = true;

            boundingBox = new Rectangle();
            boundingBox.Tag = ShapeType.BoundingBox;
            boundingBox.Width = 0;
            boundingBox.Height = 0;
            boundingBox.Stroke = new SolidColorBrush(Colors.Red);
            boundingBox.StrokeThickness = this.getBoundingBoxStrokeThickness();
            this.mainCanvas.Children.Add(boundingBox);
            var roundedPoint = roundToPixel(e.GetPosition(mainCanvas));
            Canvas.SetLeft(boundingBox, roundedPoint.X);
            Canvas.SetTop(boundingBox, roundedPoint.Y);
            Canvas.SetZIndex(boundingBox, 500);
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
            if(isPanning)
            {
                var newPos = System.Windows.Input.Mouse.GetPosition(this);
                var delta = newPos - lastPos;
                lastPos = newPos;
                var tt = TranslateTransform;
 
                totalAmountToMoveX += delta.X;
                totalAmountToMoveY += delta.Y;

                tt.X = (origX + totalAmountToMoveX);
                tt.Y = (origY + totalAmountToMoveY);
                //tt.Y += delta.Y * ScaleTransform.ScaleY;
            }
            else if (drawingRectangle)
            {
                var roundedPoint = roundToPixel(e.GetPosition(mainCanvas));
                boundingBox.Width = Math.Max(roundedPoint.X - Canvas.GetLeft(boundingBox), 0);
                boundingBox.Height = Math.Max(roundedPoint.Y - Canvas.GetTop(boundingBox), 0);
            }
        }

        private bool isPanning = false;

        private void MainCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;

            //var totalAmountScaled = totalAmountToMoveX * ScaleTransform.ScaleX;


            //var tt = TranslateTransform;
            //tt.X += totalAmountScaled;
        }

        public ScaleTransform ScaleTransform
        {
            get
            {
                return (mainCanvas.RenderTransform as TransformGroup).Children.First(it => it is ScaleTransform) as ScaleTransform;
            }
        }

        public TranslateTransform TranslateTransform
        {
            get
            {
                return (mainCanvas.RenderTransform as TransformGroup).Children.First(it => it is TranslateTransform) as TranslateTransform;
            }
        }


        double origX;
        double origY;


        Point lastPos;
        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            mainCanvas.CaptureMouse();

            totalAmountToMoveX = 0;
            totalAmountToMoveY = 0;

            var tt = TranslateTransform;

            origX = tt.X;
            origY = tt.Y;

            //tt.X += 10;
            //tt.Y += 10;
            isPanning = true;
            lastPos = System.Windows.Input.Mouse.GetPosition(this);
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var canvas = sender as Canvas;
            var st = ScaleTransform;

            var pointToKeepAtLocation = System.Windows.Input.Mouse.GetPosition(canvas);

            bool exp = true;
            double zoomDelta = e.Delta > 0 ? .02 : -.02;
            double amountExp = .05;
            double zoomExpDelta = e.Delta > 0 ? 1 + amountExp : 1 - amountExp;

            var oldScale = st.ScaleX;
            if (exp)
            {
                st.ScaleX *= zoomExpDelta;
                st.ScaleY *= zoomExpDelta;
            }
            else
            {
                st.ScaleX += zoomDelta;
                st.ScaleY += zoomDelta;
            }

            st.ScaleX = Math.Max(0.5, st.ScaleX);
            st.ScaleY = Math.Max(0.5, st.ScaleY);

            var newScale = st.ScaleX;

            var tt = TranslateTransform;

            var newX = newScale * pointToKeepAtLocation.X;
            var oldX = oldScale * pointToKeepAtLocation.X;

            var newY = newScale * pointToKeepAtLocation.Y;
            var oldY = oldScale * pointToKeepAtLocation.Y;

            var diffToCorrectX = newX - oldX;
            var diffToCorrectY = newY - oldY;

            tt.X -= diffToCorrectX;
            tt.Y -= diffToCorrectY;

            UpdateForZoomChange();

            e.Handled = true;
        }

        private void UpdateForZoomChange()
        {
            if (boundingBox != null)
            {
                boundingBox.StrokeThickness = getBoundingBoxStrokeThickness();
            }
        }

        private double getBoundingBoxStrokeThickness()
        {
            return 1.0 / ScaleTransform.ScaleX;
        }
    }
}
