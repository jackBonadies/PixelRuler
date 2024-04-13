using PixelRuler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelRuler
{
    public static class DisplayKeysHelper
    {
        public static List<string> GetDisplayKeys(Key key, ModifierKeys modifierKeys, bool pendingCase = false)
        {
            if (!pendingCase && (key == Key.None || modifierKeys == ModifierKeys.None))
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
            _ = shortcutInfo ?? throw new ArgumentNullException(nameof(shortcutInfo));
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
                    var modifiersText = KeyboardHelper.GetModifierKeyName(modifiers);
                    var keyName = KeyboardHelper.GetFriendlyName(key);
                    return $"{toolTipTextBase} ({modifiersText}{keyName})";
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
                    else if(shortcutInfo.Status == RegistrationStatus.FailedRegistration)
                    {
                        return "None";
                    }
                }
                if (key != Key.None)
                {
                    var modifiersText = KeyboardHelper.GetModifierKeyName(modifiers);
                    var keyName = KeyboardHelper.GetFriendlyName(key);
                    return $"{modifiersText}{keyName}";
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

    //public static class ImageBehavior
    //{
    //    public static readonly DependencyProperty UpdatePositioningOnSourceChangeProperty = DependencyProperty.RegisterAttached(
    //        "UpdatePositioningOnSourceChange",
    //        typeof(bool),
    //        typeof(ImageBehavior),
    //        new PropertyMetadata(false, OnUpdatePositioningOnSourceChanged_Changed));

    //    public static bool GetUpdatePositioningOnSourceChange(DependencyObject obj) => (bool)obj.GetValue(UpdatePositioningOnSourceChangeProperty);
    //    public static void SetUpdatePositioningOnSourceChange(DependencyObject obj, bool value) => obj.SetValue(UpdatePositioningOnSourceChangeProperty, value);

    //    private static void OnUpdatePositioningOnSourceChanged_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (d is Image image)
    //        {
    //            if ((bool)e.NewValue)
    //            {
    //                image.SourceUpdated += OnImageSourceUpdated;
    //            }
    //            else
    //            {
    //                image.SourceUpdated -= OnImageSourceUpdated;
    //            }
    //        }
    //    }

    //    private static void OnImageSourceUpdated(object sender, DataTransferEventArgs e)
    //    {
    //        if (sender is Image image && image.Source != null)
    //        {
    //            SetImageLocation(image.Parent as Canvas, image);
    //        }
    //    }



    //}

    public static class KeyboardHelper
    {
        public static bool IsShortcutValid(Key key, ModifierKeys modifiers)
        {
            // todo may want to make non modifierkeys valid too such as just "PrtScn"
            //   in case someone really loves this program.
            return key != Key.None && modifiers != ModifierKeys.None;
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

    public static class ImageCommon
    {
        public static ImageFormat GetImageFormatFromFilename(string fileName)
        {
            var extIndex = fileName.LastIndexOf('.');
            var ext = fileName.Substring(extIndex+1).ToLower();
            switch(ext)
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

