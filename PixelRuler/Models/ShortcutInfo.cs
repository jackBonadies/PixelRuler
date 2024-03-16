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
            this.DefaultKey = defaultKey;
            this.DefaultModifiers = defaultModifiers;
        }

        public bool IsGlobal
        {
            get
            {
                return true;
            }
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
                return KeyboardHelper.IsShortcutValid(this.Key, this.Modifiers);
            }
        }

        public bool IsDefault
        {
            get
            {
                if (this.Key == this.DefaultKey && this.Modifiers == this.DefaultModifiers)
                {
                    return true;
                }
                return false;
            }
        }

        public Key DefaultKey
        {
            get; private set;
        }

        public ModifierKeys DefaultModifiers
        {
            get; private set;
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
                    OnPropertyChanged(nameof(IsDefault));
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
                    OnPropertyChanged(nameof(IsDefault));
                }
            }
        }

        private RegistrationStatus status;
        public RegistrationStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged();
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

    public enum RegistrationStatus
    {
        Unregistered = 0,
        SuccessfulRegistration = 1,
        FailedRegistration = 2,
    }
}
