//
// Copyright (c) 2012-2013 C. Jared Cone jared.cone@gmail.com
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace JRunUI
{
    /**
     * Used to pull modifier keys from a key set,
     * and to convert to and from hotkey strings
     */
    public static class HotKeyString
    {
        static Dictionary<Key, ModifierKeys> _key2modifiers;
        static string _delimiter = " + ";

        static HotKeyString()
        {
            _key2modifiers = new Dictionary<Key, ModifierKeys>();

            _key2modifiers.Add(Key.LeftAlt, ModifierKeys.Alt);
            _key2modifiers.Add(Key.RightAlt, ModifierKeys.Alt);

            _key2modifiers.Add(Key.LeftCtrl, ModifierKeys.Control);
            _key2modifiers.Add(Key.RightCtrl, ModifierKeys.Control);

            _key2modifiers.Add(Key.LeftShift, ModifierKeys.Shift);
            _key2modifiers.Add(Key.RightShift, ModifierKeys.Shift);

            _key2modifiers.Add(Key.LWin, ModifierKeys.Windows);
            _key2modifiers.Add(Key.RWin, ModifierKeys.Windows);
        }

        /**
         * If any of the two keys represent a modifier key, return the value of the modifier key.
         * Otherwise return ModifierKeys.None
         */
        public static ModifierKeys GetModifierKey(Key sysKey, Key regKey)
        {
            ModifierKeys modKey;

            if (_key2modifiers.TryGetValue(sysKey, out modKey))
            {
                return modKey;
            }

            if (_key2modifiers.TryGetValue(regKey, out modKey))
            {
                return modKey;
            }

            return ModifierKeys.None;
        }

        /**
         * Convert a hotkey pair into a string format, which can be deconverted back into keys later
         */
        public static string GetHotKeyString(ModifierKeys modKey, Key regKey)
        {
            if (modKey != ModifierKeys.None && regKey != Key.None)
            {
                return new ModifierKeysConverter().ConvertToString(modKey) + _delimiter + new KeyConverter().ConvertToString(regKey);
            }
            else if (modKey != ModifierKeys.None)
            {
                return new ModifierKeysConverter().ConvertToString(modKey);
            }
            else if (regKey != Key.None)
            {
                return new KeyConverter().ConvertToString(regKey);
            }
            else
            {
                return "";
            }
        }

        /**
         * Convert a hotkey string back into a modifier key and regular key.
         * Return true if successful
         */
        public static bool FromHotKeyString(string str, out ModifierKeys modKey, out Key regKey)
        {
            modKey = ModifierKeys.None;
            regKey = Key.None;

            if (String.IsNullOrEmpty(str))
            {
                return false;
            }

            int index = str.IndexOf(_delimiter);

            if (index > 0)
            {
                string modStr = str.Substring(0, index);
                string regStr = str.Substring(index + _delimiter.Length);

                modKey = (ModifierKeys)new ModifierKeysConverter().ConvertFromString(modStr);
                regKey = (Key)new KeyConverter().ConvertFromString(regStr);
                return true;
            }

            modKey = (ModifierKeys)new ModifierKeysConverter().ConvertFromString(str);

            if (modKey != ModifierKeys.None)
            {
                return true;
            }

            regKey = (Key)new KeyConverter().ConvertFromString(str);

            if (regKey != Key.None)
            {
                return true;
            }

            return false;
        }
    }
}
