﻿using PixelRuler.Common;
using PixelRuler.CustomControls;
using PixelRuler.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PixelRuler.Views
{
    /// <summary>
    /// Interaction logic for ScreenshotSelectionPerScreenPanel.xaml
    /// </summary>
    public partial class ScreenshotSelectionPerScreenPanel : UserControl
    {
        Storyboard enterAnimationPanel;
        Storyboard leaveAnimationPanel;
        Storyboard enterAnimationHelp;
        Storyboard leaveAnimationHelp;
        Storyboard enterHelpAreaAnimation;
        Storyboard leaveHelpAreaAnimation;

        public ScreenshotSelectionPerScreenPanel(double scaleFactor)
        {
            InitializeComponent();
            enterAnimationPanel = this.Resources["enterAnimationPanel"] as Storyboard ?? throw new NullReferenceException("Missing Enter Transform Storyboard");
            leaveAnimationPanel = this.Resources["leaveAnimationPanel"] as Storyboard ?? throw new NullReferenceException("Missing Leave Transform Storyboard");
            enterAnimationHelp = this.Resources["enterAnimationHelp"] as Storyboard ?? throw new NullReferenceException("Missing Enter Transform Storyboard");
            leaveAnimationHelp = this.Resources["leaveAnimationHelp"] as Storyboard ?? throw new NullReferenceException("Missing Leave Transform Storyboard");

            enterHelpAreaAnimation = this.Resources["enterHelpAreaAnimation"] as Storyboard ?? throw new NullReferenceException("Missing Leave Transform Storyboard");
            leaveHelpAreaAnimation = this.Resources["leaveHelpAreaAnimation"] as Storyboard ?? throw new NullReferenceException("Missing Leave Transform Storyboard");

            ScaleFactor = scaleFactor;

            this.DataContextChanged += ScreenshotSelectionPerScreenPanel_DataContextChanged;
        }

        private void storeOriginalHelpBounds(double margin)
        {
            var start = helpPanel.TranslatePoint(new Point(-margin, -margin), this);
            var end = helpPanel.TranslatePoint(new Point(helpPanel.ActualWidth + margin, helpPanel.ActualHeight + margin), this);
            this.originalHelpBounds = new Rect(start, end);
        }

        private Rect? originalHelpBounds;

        private PixelRulerViewModel ViewModel
        {
            get
            {
                var vm = (this.DataContext as PixelRulerViewModel);
                ArgumentNullException.ThrowIfNull(vm);
                return vm;
            }
        }

        private void ScreenshotSelectionPerScreenPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel.Settings.ScreenshotSelectionViewModel.ScreenshotHelpOnChanged += ScreenshotSelectionViewModel_ScreenshotHelpOnChanged;
            ViewModel.ColorCopied += ViewModel_ColorCopied;
            ViewModel.ColorSelected += ViewModel_ColorSelected;
            ViewModel.ColorStartSelect += ViewModel_ColorStartSelect;
        }

        private void ViewModel_ColorStartSelect(object? sender, EventArgs e)
        {
            //var sb = (this.gridTopLevel.Resources["colorPanelTemplate"] as ControlTemplate).Resources["colorPanelDownAnimation"] as Storyboard;
            //sb.Begin();
        }

        private void ViewModel_ColorSelected(object? sender, EventArgs e)
        {

        }

        private void ViewModel_ColorCopied(object? sender, EventArgs e)
        {
            if (this.IsMouseEnteredVirtual)
            {
                var mode = (this.ViewModel.Settings.QuickColorMode);
                if (mode == QuickColorMode.AutoCopyMany || mode == QuickColorMode.AutoCopyAndClose)
                {
                    var tns = new ToastNotificationSingle()
                    {
                        DataContext = new ToastNotifColorViewModel(
                            this.ViewModel.Color,
                            UiUtils.FormatColor(this.ViewModel.Color, this.ViewModel.Settings.ColorFormatMode)),
                        Style = Application.Current.Resources["toastColorStyle"] as Style,
                    };
                    if (mode == QuickColorMode.AutoCopyMany)
                    {
                        tns.Show(this.gridTopLevel);
                    }
                    else
                    {
                        var popupHost = new PopupHost(this.Bounds);
                        popupHost.IsOpen = true;
                        tns.Show(popupHost.RootGrid);
                        tns.Closed += (object? sender, EventArgs e) =>
                        {
                            popupHost.IsOpen = false;
                        };
                    }
                }
            }
        }

        private void ScreenshotSelectionViewModel_ScreenshotHelpOnChanged(object? sender, bool e)
        {
            if (IsMouseEnteredVirtual)
            {
                if (ViewModel.Settings.ScreenshotSelectionViewModel.ScreenshotHelpOn)
                {
                    enterHelpAnimation();
                }
                else
                {
                    leaveHelpAnimation();
                }
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        public double ScaleFactor { get; private set; }

        public bool IsMouseEnteredVirtual
        {
            get; set;
        }

        public Rect Bounds { get; set; }

        private void enterPanelAnimation()
        {
            this.enterAnimationPanel.Begin();
        }

        private void enterHelpAnimation()
        {
            this.enterAnimationHelp.Begin();
        }

        private void leavePanelAnimation()
        {
            this.leaveAnimationPanel.Begin();
        }

        private void leaveHelpAnimation()
        {
            this.leaveAnimationHelp.Begin();
        }

        private bool isWithinOriginalHelpArea = false;

        private bool isMouseWithinBoundsIgnoreTranslation(MouseEventArgs e, FrameworkElement element, TranslateTransform t, double margin)
        {
            bool isWithin = e.GetPosition(element).X >= -margin - t.X &&
                e.GetPosition(element).X < element.ActualWidth + margin - t.X &&
                e.GetPosition(element).Y >= -margin - t.Y &&
                e.GetPosition(element).Y < element.ActualHeight + margin - t.Y;
            return isWithin;
        }

        internal void HandleMouse(MouseEventArgs e, Point pos)
        {
            bool inside = Bounds.Contains(pos);

            if (!isWithinOriginalHelpArea)
            {
                // RenderTransform may not affect layout but it DOES affect GetPosition
                var hitHelp = isMouseWithinBoundsIgnoreTranslation(e, this.helpPanel, this.helpPanel.RenderTransform as TranslateTransform, 20);
                if (hitHelp)
                {
                    isWithinOriginalHelpArea = true;
                    storeOriginalHelpBounds(120);
                    (enterHelpAreaAnimation.Children[0] as DoubleAnimation).To = this.Height - this.helpPanel.ActualHeight - 20;
                    enterHelpAreaAnimation.Begin();
                    this.ViewModel.Test = !this.ViewModel.Test;
                }
            }
            else
            {
                var hitHelp = isMouseWithinBoundsIgnoreTranslation(e, this.helpPanel, this.helpPanel.RenderTransform as TranslateTransform, 120);
                //var hitHelp = this.helpPanel.IsMouseWithinBounds(e, 40);
                if (!hitHelp)
                {
                    isWithinOriginalHelpArea = false;
                    (leaveHelpAreaAnimation.Children[0] as DoubleAnimation).From = this.Height - this.helpPanel.ActualHeight - 20;
                    (leaveHelpAreaAnimation.Children[0] as DoubleAnimation).To = 0;
                    leaveHelpAreaAnimation.Begin();
                }
            }

            if (!IsMouseEnteredVirtual && inside)
            {
                IsMouseEnteredVirtual = true;
                enterPanelAnimation();
                if (ViewModel.Settings.ScreenshotSelectionViewModel.ScreenshotHelpOn)
                {
                    enterHelpAnimation();
                }
            }
            else if (IsMouseEnteredVirtual && !inside)
            {
                IsMouseEnteredVirtual = false;
                leavePanelAnimation();
                if (ViewModel.Settings.ScreenshotSelectionViewModel.ScreenshotHelpOn)
                {
                    leaveHelpAnimation();
                }
            }
        }
    }
}
