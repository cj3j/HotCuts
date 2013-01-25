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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using JRun;

namespace JRunUI.controls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WpfApplicationHotKey.HotKey _keyHook;
        ShortcutWindow _shortcutWindow;
       
        public MainWindow()
        {
            InitializeComponent();

            JRunUI.Properties.Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);

            trayIcon.Icon = JRunUI.Properties.Resources.DefaultTrayIcon1;

            RebuildHotkey();

            OpenShortcutWindow();
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // rebuild the hotkey shortcut when the hotkey string changes
            if (e.PropertyName == "HotKeyString")
            {
                RebuildHotkey();
            }
        }

        /**
         * Reads the hotkey string from settings and registers the hotkey combo with windows
         */
        void RebuildHotkey()
        {
            if (_keyHook != null)
            {
                _keyHook.UnregisterHotKey();
                _keyHook = null;
            }

            var hotKeyString = JRunUI.Properties.Settings.Default.HotKeyString;
            ModifierKeys modKey;
            Key regKey;

            if (HotKeyString.FromHotKeyString(hotKeyString, out modKey, out regKey))
            {
                try
                {
                    _keyHook = new WpfApplicationHotKey.HotKey(modKey, regKey, this);
                    _keyHook.HotKeyPressed += new Action<WpfApplicationHotKey.HotKey>(_keyHook2_HotKeyPressed);
                }
                catch (Exception ex)
                {
                    var button = MessageBoxButton.OK;
                    var icon = MessageBoxImage.Warning;
                    MessageBox.Show(this, String.Format("Could not register hotkey '{0}'. {1}", hotKeyString, ex.Message), "Error", button, icon);
                }
            }
            else
            {
                var button = MessageBoxButton.OK;
                var icon = MessageBoxImage.Warning;
                MessageBox.Show(this, String.Format("Could not load hotkey '{0}'.", hotKeyString), "Error", button, icon);
            }
        }

        void _keyHook2_HotKeyPressed(WpfApplicationHotKey.HotKey obj)
        {
            OpenShortcutWindow();
        }

        public void OpenShortcutWindow()
        {
            CloseShortcutWindow();

            Activate();
            Show();

            if (_shortcutWindow == null)
            {
                _shortcutWindow = new ShortcutWindow();
                _shortcutWindow.Closed += new EventHandler(_shortcutWindow_Closed);
                _shortcutWindow.Show();
                _shortcutWindow.Activate();
                _shortcutWindow.Owner = this;
            }
        }

        void _shortcutWindow_Closed(object sender, EventArgs e)
        {
            _shortcutWindow = null;
            Hide();
        }

        void CloseShortcutWindow()
        {
            Hide();
            if (_shortcutWindow != null)
            {
                _shortcutWindow.Close();
                _shortcutWindow = null;
            }
        }

		private void ContextMenuHelp_Click(object sender, RoutedEventArgs e)
		{
			OpenShortcutWindow();

			_shortcutWindow.ContextMenuHelp_Click(sender, e);
		}

		private void ContextMenuEditShortcuts_Click(object sender, RoutedEventArgs e)
		{
			OpenShortcutWindow();

			_shortcutWindow.ContextMenuEditShortcuts_Click(sender, e);
		}

        private void ContextMenuOptions_Click(object sender, RoutedEventArgs e)
        {
            OpenShortcutWindow();

            _shortcutWindow.ContextMenuOptions_Click( sender, e );
        }

        private void ContextMenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ContextMenuMove_Click(object sender, RoutedEventArgs e)
        {
            OpenShortcutWindow();

            _shortcutWindow.ContextMenuMove_Click(sender, e);
        }

        private void ContextMenuResize_Click(object sender, RoutedEventArgs e)
        {
            OpenShortcutWindow();

            _shortcutWindow.ContextMenuResize_Click(sender, e);
        }

        private void ContextMenuReset_Click(object sender, RoutedEventArgs e)
        {
            JRunUI.Properties.Settings.Default.Reset();
            JRunUI.Properties.Settings.Default.Save();
        }

        void ContextMenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Show();
            aboutWindow.Owner = Owner;
        }
    }

    public class ActivateCommand : ICommand
    {
        public void Execute(object parameter)
        {
            ((MainWindow)parameter).OpenShortcutWindow();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}
