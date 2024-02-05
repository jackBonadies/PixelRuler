using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace PixelRuler
{
    public class ShortcutInfo : INotifyPropertyChanged
    {
        public ShortcutInfo(string name, int hotKeyID, Key key, ModifierKeys modifiers, Key defaultKey, ModifierKeys defaultModifiers)
        {
            this.CommandName = name;
            this.Key = key;
            this.Modifiers = modifiers;
            this.HotKeyId = hotKeyID;
        }

        public void Clear()
        {
            this.Key = Key.None;
            this.Modifiers = ModifierKeys.None;
        }

        public bool IsValid
        {
            get
            {
                if (this.Key != Key.None && this.Modifiers != ModifierKeys.None)
                {
                    return true;
                }
                return false;
            }
        }

        public string CommandName
        {
            get; private set;
        }

        private Key key;
        public Key Key
        {
            get
            {
                return key;
            }
            set
            {
                if(key != value)
                {
                    key = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        private ModifierKeys modifiers;
        public ModifierKeys Modifiers
        {
            get
            {
                return modifiers;
            }
            set
            {
                if(modifiers != value)
                {
                    modifiers = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public int HotKeyId 
        { 
            get; private set; 
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
