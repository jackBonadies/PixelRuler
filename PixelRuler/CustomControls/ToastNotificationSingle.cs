using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PixelRuler.CustomControls
{
    public class ToastNotificationSingle : ContentControl
    {
        FrameworkElement? parent;
        public void Show(FrameworkElement parent)
        {
            this.parent = parent;
            this.RenderTransform = new TranslateTransform() { Y = 10 };
            this.Measure(new Size(double.MaxValue, double.MaxValue));
            if(parent is Canvas canvas)
            {
                canvas.Children.Add(this);
                canvas.SizeChanged += Canvas_SizeChanged;
                Canvas.SetBottom(this, 18);
                Canvas.SetLeft(this, canvas.ActualWidth / 2 - this.DesiredSize.Width / 2);
            }
            else if(parent is Grid grid)
            {
                grid.Children.Add(this);
                grid.SizeChanged += Canvas_SizeChanged;
                this.VerticalAlignment = VerticalAlignment.Bottom;
                this.HorizontalAlignment = HorizontalAlignment.Center;
                this.Margin = new Thickness(0, 0, 0, 18);
            }

            double durationSeconds = .2;
            double translationExtent = 80;
            Storyboard s = new Storyboard();
            var d1 = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(durationSeconds),
                From = translationExtent,
                To = 0,
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
            };
            Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.Y"));
            Storyboard.SetTarget(d1, this);
            s.Children.Add(d1);

            var d2 = new DoubleAnimation()
            {
                BeginTime = TimeSpan.FromSeconds(2.4),
                Duration = TimeSpan.FromSeconds(durationSeconds),
                From = 0,
                To = translationExtent,
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 2 },
            };
            Storyboard.SetTargetProperty(d2, new PropertyPath("RenderTransform.Y"));
            Storyboard.SetTarget(d2, this);
            s.Children.Add(d2);

            s.Begin();
            s.Completed += S_Completed;
        }

        private void S_Completed(object? sender, EventArgs e)
        {
            if (this.parent is Panel p)
            {
                p.Children.Remove(this);
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }

    public class ToastNotifTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BoldToastNotifStringTemplate { get; set; }
        public DataTemplate? DefaultTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(BoldToastNotifStringTemplate == null || DefaultTemplate == null)
            {
                throw new Exception("Failed to initialize ToastTemplate");
            }
            if(item is string)
            {
                return BoldToastNotifStringTemplate;
            }
            return DefaultTemplate;
        }

    }
}
