using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace PixelRuler
{
    /// <summary>
    /// Custom version of WPF UI DropDownButtom allowing for custom placement of menu
    /// </summary>
    public class DropDownButtonCustom : Wpf.Ui.Controls.Button
    {
        private ContextMenu? _contextMenu;

        /// <summary>
        /// Property for <see cref="Flyout"/>.
        /// </summary>
        public static readonly DependencyProperty FlyoutProperty = DependencyProperty.Register(
            nameof(Flyout),
            typeof(object),
            typeof(DropDownButtonCustom),
            new PropertyMetadata(null, OnFlyoutChangedCallback)
        );

        /// <summary>
        /// Property for <see cref="IsDropDownOpen"/>.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(DropDownButtonCustom),
            new PropertyMetadata(false)
        );

        /// <summary>
        /// Property for <see cref="IsDropDownOpen"/>.
        /// </summary>
        public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.Register(
            nameof(PlacementTarget),
            typeof(FrameworkElement),
            typeof(DropDownButtonCustom),
            new PropertyMetadata(null)
        );

        /// <summary>
        /// Gets or sets the flyout associated with this button.
        /// </summary>
        [Bindable(true)]
        public object? Flyout
        {
            get => GetValue(FlyoutProperty);
            set => SetValue(FlyoutProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the drop-down for a button is currently open.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if the drop-down is open; otherwise, <see langword="false" />. The default is <see langword="false" />.</returns>
        [Bindable(true)]
        [Browsable(false)]
        [Category("Appearance")]
        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the drop-down for a button is currently open.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if the drop-down is open; otherwise, <see langword="false" />. The default is <see langword="false" />.</returns>
        [Bindable(true)]
        public FrameworkElement PlacementTarget
        {
            get => GetValue(PlacementTargetProperty) as FrameworkElement;
            set => SetValue(PlacementTargetProperty, value);
        }

        private static void OnFlyoutChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DropDownButtonCustom dropDownButton)
            {
                dropDownButton.OnFlyoutChangedCallback(e.NewValue);
            }
        }

        protected virtual void OnFlyoutChangedCallback(object value)
        {
            if (value is ContextMenu contextMenu)
            {
                _contextMenu = contextMenu;
                _contextMenu.Opened += OnContextMenuOpened;
                _contextMenu.Closed += OnContextMenuClosed;
            }
        }

        protected virtual void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }

        protected virtual void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
        }

        protected virtual void OnIsDropDownOpenChanged(bool currentValue) { }

        protected override void OnClick()
        {
            base.OnClick();

            if (_contextMenu is null)
            {
                return;
            }

            _contextMenu.SetCurrentValue(MinWidthProperty, ActualWidth);
            //_contextMenu.SetCurrentValue(ContextMenu.PlacementTargetProperty, this);
            _contextMenu.Measure(new Size(double.MaxValue, double.MaxValue));
            var desiredWidth = _contextMenu.DesiredSize.Width;
            _contextMenu.PlacementRectangle = new Rect(PlacementTarget.ActualWidth - desiredWidth, PlacementTarget.ActualHeight, 0, 0);// new Rect(-desiredWidth + this.ActualWidth, this.ActualHeight, 0, 0);
            _contextMenu.Placement = PlacementMode.RelativePoint;
            _contextMenu.PlacementTarget = PlacementTarget;
            _contextMenu.SetCurrentValue(ContextMenu.IsOpenProperty, true);
        }
    }
}
