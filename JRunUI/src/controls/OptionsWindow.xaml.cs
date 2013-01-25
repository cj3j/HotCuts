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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;

namespace JRunUI.controls
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        int _numKeysPressed;
        ModifierKeys _modKey;
        Key _regKey;
        string hotkeyString;

        public OptionsWindow()
        {
            InitializeComponent();

            HotKeyString.FromHotKeyString(JRunUI.Properties.Settings.Default.HotKeyString, out _modKey, out _regKey);

            RebuildHotkeyText();
        }

        private void XmlFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xml";
            var result = dlg.ShowDialog();

            if (result == true)
            {
                JRunUI.Properties.Settings.Default.XmlFile = dlg.FileName;
            }
        }

        private void SelectorBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();

            if (result == true)
            {
                JRunUI.Properties.Settings.Default.ProfileSelector = dlg.FileName;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
			bool bStartWithWindows = JRunUI.Properties.Settings.Default.bStartWithWindows;

			var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

			if (bStartWithWindows)
			{
				registryKey.SetValue("ApplicationName", System.Reflection.Assembly.GetExecutingAssembly().Location);
			}
			else
			{
				registryKey.DeleteValue("ApplicationName");
			}

			JRunUI.Properties.Settings.Default.HotKeyString = hotkeyString;
			JRunUI.Properties.Settings.Default.Save();

            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            JRunUI.Properties.Settings.Default.Reload();
            Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ++_numKeysPressed;

            var tempModKey = HotKeyString.GetModifierKey(e.SystemKey, e.Key);

            if (tempModKey != ModifierKeys.None)
            {
                _modKey = tempModKey;
            }
            else
            {
                if (e.Key == Key.System)
                {
                    _regKey = e.SystemKey;
                }
                else
                {
                    _regKey = e.Key;
                }
            }

            RebuildHotkeyText();

            e.Handled = true;
        }

        void RebuildHotkeyText()
        {
            hotkeyString = HotKeyString.GetHotKeyString(_modKey, _regKey);

            TextHotKey.Text = hotkeyString;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            --_numKeysPressed;

            if (_numKeysPressed == 0)
            {
                _regKey = Key.None;
                _modKey = ModifierKeys.None;
            }

            e.Handled = true;
        }
    }
}
