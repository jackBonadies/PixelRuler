using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PixelRuler.Models
{
    public class PendingShortcutInfo : INotifyPropertyChanged
    {
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
                if (modifiers != value)
                {
                    modifiers = value;
                    OnPropertyChanged();
                }
            }
        }
        public ShortcutInfo ShortcutInfo { get; set; }
        public PendingShortcutInfo(ShortcutInfo shortcutInfo)
        {
            ShortcutInfo = shortcutInfo;
            Key = shortcutInfo.Key;
            Modifiers = shortcutInfo.Modifiers;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
