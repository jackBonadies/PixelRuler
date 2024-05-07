using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PixelRuler.CustomControls
{
    public class ToastNotificationSingle : ContentControl
    {
        public ToastAnimationStyle AnimationType { get; set; } = ToastAnimationStyle.MoveInOut;
        public double AnimationDurationSeconds { get; set; } = .2;
        public double ToastDurationSeconds { get; set; } = 2.4;


        FrameworkElement? parent;
        public void Show(FrameworkElement parent)
        {
            this.parent = parent;
            this.Measure(new Size(double.MaxValue, double.MaxValue));
            if (parent is Canvas canvas)
            {
                canvas.Children.Add(this);
                canvas.SizeChanged += Canvas_SizeChanged;
                Canvas.SetBottom(this, 18);
                Canvas.SetLeft(this, canvas.ActualWidth / 2 - this.DesiredSize.Width / 2);
            }
            else if (parent is Grid grid)
            {
                grid.Children.Add(this);
                grid.SizeChanged += Canvas_SizeChanged;
                this.Margin = new Thickness(0, 0, 0, 18);
            }

            Storyboard s = new Storyboard();
            if (AnimationType == ToastAnimationStyle.MoveInOut)
            {
                this.RenderTransform = new TranslateTransform() { Y = 10 };
                double translationExtent = 80;

                var d1 = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromSeconds(AnimationDurationSeconds),
                    From = translationExtent,
                    To = 0,
                    EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                };
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.Y"));
                Storyboard.SetTarget(d1, this);
                s.Children.Add(d1);

                var d2 = new DoubleAnimation()
                {
                    BeginTime = TimeSpan.FromSeconds(ToastDurationSeconds),
                    Duration = TimeSpan.FromSeconds(AnimationDurationSeconds),
                    From = 0,
                    To = translationExtent,
                    EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                };
                Storyboard.SetTargetProperty(d2, new PropertyPath("RenderTransform.Y"));
                Storyboard.SetTarget(d2, this);
                s.Children.Add(d2);
            }
            else
            {
                var d1 = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromSeconds(AnimationDurationSeconds),
                    From = 0,
                    To = Opacity,
                    EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                };
                Storyboard.SetTargetProperty(d1, new PropertyPath("Opacity"));
                Storyboard.SetTarget(d1, this);
                s.Children.Add(d1);

                var d2 = new DoubleAnimation()
                {
                    BeginTime = TimeSpan.FromSeconds(ToastDurationSeconds),
                    Duration = TimeSpan.FromSeconds(AnimationDurationSeconds),
                    From = Opacity,
                    To = 0,
                    EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
                };
                Storyboard.SetTargetProperty(d2, new PropertyPath("Opacity"));
                Storyboard.SetTarget(d2, this);
                s.Children.Add(d2);
            }

            s.Completed += S_Completed;
            s.Begin();
        }

        private void S_Completed(object? sender, EventArgs e)
        {
            if (this.parent is Panel p)
            {
                p.Children.Remove(this);
            }
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        public event EventHandler? Closed;
    }

    public class ToastNotifTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BoldToastNotifStringTemplate { get; set; }
        public DataTemplate? DefaultTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (BoldToastNotifStringTemplate == null || DefaultTemplate == null)
            {
                throw new Exception("Failed to initialize ToastTemplate");
            }
            if (item is string)
            {
                return BoldToastNotifStringTemplate;
            }
            return DefaultTemplate;
        }
    }

    public enum ToastAnimationStyle
    {
        MoveInOut = 0,
        FadeInOut = 1,
    }
}
