﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PixelRuler.CustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:PixelRuler.CustomControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:PixelRuler.CustomControls;assembly=PixelRuler.CustomControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:TextBoxWithValidation/>
    ///
    /// </summary>
    public class TextBoxWithValidation : Control
    {
        /// <summary>
        /// Property for <see cref="IsDropDownOpen"/>.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(TextBoxWithValidation),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal)
        );

        /// <summary>
        /// Gets or sets the flyout associated with this button.
        /// </summary>
        [Bindable(true)]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        static TextBoxWithValidation()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxWithValidation), new FrameworkPropertyMetadata(typeof(TextBoxWithValidation)));
        }
    }
}
