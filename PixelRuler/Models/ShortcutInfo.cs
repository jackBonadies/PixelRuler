using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PixelRuler
{
    public class ShortcutInfo
    {
        public ShortcutInfo(string name, int hotKeyID, Key key, ModifierKeys modifiers)
        {
            this.CommandName = name;
            this.Key = key;
            this.Modifiers = modifiers;
            this.HotKeyId = hotKeyID;
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

        public Key Key
        {
            get; private set;
        }

        public ModifierKeys Modifiers
        {
            get; private set;
        }
        public int HotKeyId 
        { 
            get; private set; 
        }
    }
}
