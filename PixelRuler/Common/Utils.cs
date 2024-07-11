using PixelRuler.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace PixelRuler
{
    public static class DisplayKeysHelper
    {
        public static List<string> GetDisplayKeys(Key key, ModifierKeys modifierKeys, bool pendingCase = false)
        {
            if (!pendingCase && (key == Key.None || (modifierKeys == ModifierKeys.None && key != Key.PrintScreen)))
            {
                return new List<string>() { "Not Set" };
            }
            else
            {
                var keys = new List<string>();
                if (modifierKeys.HasFlag(ModifierKeys.Windows))
                {
                    keys.Add("Win");
                }
                if (modifierKeys.HasFlag(ModifierKeys.Control))
                {
                    keys.Add("Ctrl");
                }
                if (modifierKeys.HasFlag(ModifierKeys.Alt))
                {
                    keys.Add("Alt");
                }
                if (modifierKeys.HasFlag(ModifierKeys.Shift))
                {
                    keys.Add("Shift");
                }

                if (key != Key.None)
                {
                    keys.Add(KeyboardHelper.GetFriendlyName(key));
                }

                return keys;
            }
        }
    }

    public class RelayCommandFull : RelayCommand
    {
        /// <summary>
        /// Bound shortcut (in case keys are rebound)
        /// </summary>
        private ShortcutInfo? shortcutInfo;

        public RelayCommandFull(Action<object?> action, Key key, ModifierKeys modifiers, string toolTipText) : base(action)
        {
            this.key = key;
            this.modifiers = modifiers;
            this.toolTipTextBase = toolTipText;
        }

        /// <summary>
        /// Relay command bound to the ShortcutInfo (for rebinding keys)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="shortcutInfo"></param>
        /// <param name="toolTipText"></param>
        public RelayCommandFull(Action<object?> action, ShortcutInfo shortcutInfo, string toolTipText = null) : base(action)
        {
            this.shortcutInfo = shortcutInfo;
            this.shortcutInfo.PropertyChanged += ShortcutInfo_PropertyChanged;
            this.key = shortcutInfo.Key;
            this.modifiers = shortcutInfo.Modifiers;
            this.toolTipTextBase = toolTipText ?? shortcutInfo.CommandName;
        }

        private void ShortcutInfo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(shortcutInfo);
            this.Key = shortcutInfo.Key;
            this.Modifiers = shortcutInfo.Modifiers;
        }

        private Key key;
        public Key Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
                OnPropertyChanged();
                OnPropertyChanged("ToolTipTextShortcut");
                OnPropertyChanged("ToolTipTextFull");
            }
        }

        private ModifierKeys modifiers;
        public ModifierKeys Modifiers
        {
            get
            {
                return this.modifiers;
            }
            set
            {
                this.modifiers = value;
                OnPropertyChanged();
                OnPropertyChanged("ToolTipTextShortcut");
                OnPropertyChanged("ToolTipTextFull");
            }
        }

        private string toolTipTextBase;
        public string ToolTipTextBase
        {
            get
            {
                return this.toolTipTextBase;
            }
            set
            {
                this.toolTipTextBase = value;
                OnPropertyChanged();
                OnPropertyChanged("ToolTipTextShortcut");
                OnPropertyChanged("ToolTipTextFull");
            }
        }

        public string ToolTipTextFull
        {
            get
            {
                if (key != Key.None)
                {
                    var shortcut = KeyboardHelper.GetShortcutLabel(modifiers, key);
                    return $"{toolTipTextBase} ({shortcut})";
                }
                return toolTipTextBase;
            }
        }

        public string ToolTipTextShortcut
        {
            get
            {
                if (this.shortcutInfo != null)
                {
                    if (shortcutInfo.Status == RegistrationStatus.Unregistered)
                    {
                        return "None";
                    }
                    else if (shortcutInfo.Status == RegistrationStatus.FailedRegistration)
                    {
                        return "None";
                    }
                }
                if (key != Key.None)
                {
                    var shortcut = KeyboardHelper.GetShortcutLabel(modifiers, key);
                    return shortcut;
                }
                return string.Empty;
            }
        }
    }

    public class RelayCommand : ICommand
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Action<object?> action;
        public RelayCommand(Action<object?> action)
        {
            this.action = action;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return canExecute;
        }

        private bool canExecute = true;

        public void SetCanExecute(bool value)
        {
            if (canExecute != value)
            {
                canExecute = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Execute(object? parameter)
        {
            action.Invoke(parameter);
        }

        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class KeyUtil
    {
        public static bool IsShiftDown()
        {
            return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        }

        public static bool IsCtrlDown()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }

        public static bool IsMultiSelect()
        {
            return IsShiftDown() || IsCtrlDown();
        }
    }

    public static class CardUtils
    {
        public static readonly DependencyProperty SupressMouseUpProperty = DependencyProperty.RegisterAttached(
            "SupressMouseUp",
            typeof(bool),
            typeof(CardUtils),
            new PropertyMetadata(false, SuppressMouseUp_Changed));

        public static bool GetSuppressMouseUp(DependencyObject obj) => (bool)obj.GetValue(SupressMouseUpProperty);
        public static void SetSuppressMouseUp(DependencyObject obj, bool value) => obj.SetValue(SupressMouseUpProperty, value);

        private static void SuppressMouseUp_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CardExpander cardExpander)
            {
                if ((bool)e.NewValue)
                {
                    cardExpander.MouseUp += CardExpanderContent_MouseUp;
                }
                else
                {
                    cardExpander.MouseUp -= CardExpanderContent_MouseUp;
                }
            }
        }

        private static void CardExpanderContent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // do not close on mouse up
            e.Handled = true;
        }

        public static readonly DependencyProperty ExpandOnMouseUpProperty = DependencyProperty.RegisterAttached(
            "ExpandOnMouseUp",
            typeof(bool),
            typeof(CardUtils),
            new PropertyMetadata(false, ExpandOnMouseUp_Changed));

        public static bool GetExpandOnMouseUp(DependencyObject obj) => (bool)obj.GetValue(ExpandOnMouseUpProperty);
        public static void SetExpandOnMouseUp(DependencyObject obj, bool value) => obj.SetValue(ExpandOnMouseUpProperty, value);

        private static void ExpandOnMouseUp_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CardExpander cardExpander)
            {
                if ((bool)e.NewValue)
                {
                    cardExpander.MouseLeftButtonUp += CardExpander_MouseLeftButtonUp;
                }
                else
                {
                    cardExpander.MouseLeftButtonUp -= CardExpander_MouseLeftButtonUp;
                }
            }
        }

        private static void CardExpander_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as Expander).IsExpanded = !(sender as Expander).IsExpanded;
        }

        public static readonly DependencyProperty RemoveChevronProperty = DependencyProperty.RegisterAttached(
            "RemoveChevron",
            typeof(bool),
            typeof(CardUtils),
            new PropertyMetadata(false, RemoveChevron_Changed));

        public static bool GetRemoveChevron(DependencyObject obj) => (bool)obj.GetValue(RemoveChevronProperty);
        public static void SetRemoveChevron(DependencyObject obj, bool value) => obj.SetValue(RemoveChevronProperty, value);

        private static void RemoveChevron_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CardExpander cardExpander)
            {
                if ((bool)e.NewValue)
                {
                    cardExpander.Loaded += CardExpander_Loaded;
                }
                else
                {
                    cardExpander.Loaded -= CardExpander_Loaded;
                }
            }
        }

        private static void CardExpander_Loaded(object sender, RoutedEventArgs e)
        {
            var cardExpander = sender as CardExpander;
            var toggleButton = UiUtils.FindChild<ToggleButton>(cardExpander, "ExpanderToggleButton");
            _ = toggleButton ?? throw new NullReferenceException("ToggleButton not found in CardExpander");
            BindingOperations.ClearBinding(toggleButton, ToggleButton.IsCheckedProperty);
            toggleButton.IsChecked = true;
            var chev = UiUtils.FindChild<Grid>(toggleButton, "ChevronGrid");
            _ = chev ?? throw new NullReferenceException("ChevronGrid not found in CardExpander");
            chev.Visibility = Visibility.Collapsed;
        }
    }

    public static class KeyboardHelper
    {
        public static bool IsShortcutValid(Key key, ModifierKeys modifiers)
        {
            if (key == Key.PrintScreen)
            {
                return true;
            }
            return key != Key.None && modifiers != ModifierKeys.None;
        }

        public static string GetShortcutLabel(ModifierKeys modifiers, Key key)
        {
            var modifiersText = KeyboardHelper.GetModifierKeyName(modifiers);
            var keyName = KeyboardHelper.GetFriendlyName(key);
            return $"{modifiersText}{keyName}";
        }

        public static string GetModifierKeyName(ModifierKeys modifiers)
        {
            string modString = "";
            if (modifiers.HasFlag(ModifierKeys.Windows))
            {
                modString += "Win+";
            }
            if (modifiers.HasFlag(ModifierKeys.Control))
            {
                modString += "Ctrl+";
            }
            if (modifiers.HasFlag(ModifierKeys.Alt))
            {
                modString += "Alt+";
            }
            if (modifiers.HasFlag(ModifierKeys.Shift))
            {
                modString += "Shift+";
            }
            return modString;
        }

        public static string GetFriendlyName(Key key)
        {
            switch (key)
            {
                case Key.OemSemicolon: //oem1
                    return ";";
                case Key.OemQuestion:  //oem2
                    return "?";
                case Key.Oem3:     //oem3
                    return "~";
                case Key.OemOpenBrackets:  //oem4
                    return "[";
                case Key.OemPipe:  //oem5
                    return "\\";
                case Key.OemCloseBrackets:    //oem6
                    return "]";
                case Key.OemQuotes:        //oem7
                    return "'";
                case Key.OemBackslash: //oem102
                    return "/";
                case Key.OemPlus:
                    return "=";
                case Key.OemMinus:
                    return "-";
                case Key.OemComma:
                    return ",";
                case Key.OemPeriod:
                    return ".";

                case Key.Subtract:
                    return "NumPad -";
                case Key.Add:
                    return "NumPad +";
                case Key.Divide:
                    return "NumPad /";
                case Key.Multiply:
                    return "NumPad *";
                case Key.Decimal: // if shift pressed the key is Delete TODO
                    return "NumPad .";

                //digits
                case Key.D0:
                    return "0";
                case Key.NumPad0: // if shift pressed the key is Ins TODO
                    return "NumPad 0";
                case Key.D1:
                    return "1";
                case Key.NumPad1:
                    return "NumPad 1";
                case Key.D2:
                    return "2";
                case Key.NumPad2:
                    return "NumPad 2";
                case Key.D3:
                    return "3";
                case Key.NumPad3:
                    return "NumPad 3";
                case Key.D4:
                    return "4";
                case Key.NumPad4:
                    return "NumPad 4";
                case Key.D5:
                    return "5";
                case Key.NumPad5:
                    return "NumPad 5";
                case Key.D6:
                    return "6";
                case Key.NumPad6:
                    return "NumPad 6";
                case Key.D7:
                    return "7";
                case Key.NumPad7:
                    return "NumPad 7";
                case Key.D8:
                    return "8";
                case Key.NumPad8:
                    return "NumPad 8";
                case Key.D9:
                    return "9";
                case Key.NumPad9:
                    return "NumPad 9";

                case Key.CapsLock:
                    return "CapsLock";
                case Key.Back:
                    return "Backspace";

                case Key.Next:
                    return "PageDown";

                case Key.PrintScreen:
                    return "PrtScr";

                default:
                    return key.ToString();

            }
        }
    }

    public static class EnumUtil
    {
        public static T CycleEnum<T>(T enu) where T : Enum
        {
            var cnt = Enum.GetValues(typeof(T)).Length;
            var curEnum = Convert.ToInt32(enu);
            var nextEnum = (curEnum + 1) % cnt;
            Enum enumValue = (Enum)Enum.ToObject(typeof(T), nextEnum);
            return (T)enumValue;
        }


    }

    public static class ImageCommon
    {
        public static ImageFormat GetImageFormatFromFilename(string fileName)
        {
            var extIndex = fileName.LastIndexOf('.');
            var ext = fileName.Substring(extIndex + 1).ToLower();
            switch (ext)
            {
                case "jpg":
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "png":
                    return ImageFormat.Png;
                case "bmp":
                    return ImageFormat.Bmp;
                case "gif":
                    return ImageFormat.Gif;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void SaveImage(string fileName, Bitmap image)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
            image.Save(fileName, GetImageFormatFromFilename(fileName));
        }
    }
}

