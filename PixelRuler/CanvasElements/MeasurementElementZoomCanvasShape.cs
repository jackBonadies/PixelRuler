﻿using System;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelRuler.CanvasElements
{
    public abstract class MeasurementElementZoomCanvasShape : AbstractZoomCanvasShape
    {
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

            hitBoxManipulate.Background = new SolidColorBrush(Color.FromArgb(40, 244, 244, 244));

            this.owningCanvas.Children.Add(hitBoxManipulate);
            Canvas.SetZIndex(hitBoxManipulate, App.MANIPULATE_HITBOX_INDEX);
        }

        public override void Clear()
        {
            this.owningCanvas.Children.Remove(hitBoxManipulate);
        }


        protected bool isManipulating = false;

        protected (Point mouseStart, Point shapeStart, Point shapeEnd) MoveStartInfo;

        private void HitBoxManipulate_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Selected = true;
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


        private void HitBoxManipulate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.FinishedDrawing)
            {
                return;
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

        public Point StartPoint { get; protected set; }
        public Point EndPoint { get; protected set; } = new Point(double.NaN, double.NaN);

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
            foreach(var circleSizer in circleSizerControls)
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

        public event EventHandler<System.Windows.Point> Moving;

    }
}