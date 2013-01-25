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
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using JRun;

namespace JRunUI.controls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ShortcutWindow : Window
    {
        /**
         * Result of an asynchronous shortcut execution
         */
        class ShortcutResult
        {
            public string Shortcut;
            public Exception ThrownException;
            public bool bSuccess;
        }

        /**
         * Result of an asynchronous autocomplete execution
         */
        class AutoCompleteResult
        {
            public String InputText, OutputText;
        }

        // polls mouse position
        DispatcherTimer mouseTimer;

        public ShortcutWindow()
        {
            InitializeComponent();

            // we need to close the window when it gets deactivated,
            // so it will properly get focus when the user pressed the shortcut keys
            Application.Current.Deactivated += new EventHandler(Current_Deactivated);

            txtShortcut.KeyDown += new KeyEventHandler(txtShortcut_KeyDown);
            txtShortcut.TextChanged += new TextChangedEventHandler(txtShortcut_TextChanged);
            txtShortcut.Focus();
        }

        void Current_Deactivated(object sender, EventArgs e)
        {
            Close();
        }

        /**
         * Populate the parameters to pass to JRun
         */
		public ShortcutParams InstanceParams()
        {
			var parms = new ShortcutParams();

            if (!String.IsNullOrEmpty(App.Current.Settings.XmlFile))
            {
                parms.SetFile(App.Current.Settings.XmlFile);
            }

            if (!String.IsNullOrEmpty(App.Current.Settings.ProfileSelector))
            {
                parms.SetProfile(ShortcutExecutor.SelectProfile(App.Current.Settings.ProfileSelector));
            }
            else if (!String.IsNullOrEmpty(App.Current.Settings.ProfileName))
            {
                parms.SetProfile(App.Current.Settings.ProfileName);
            }

            return parms;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            txtShortcut.Focus();
        }

        /**
         * Update auto complete or execute a shortcut when the user maniplates the text box
         */
        void txtShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var text = ((TextBox)sender).Text;

                if (!String.IsNullOrEmpty(text))
                {
                    var result = new ShortcutResult();
                    result.Shortcut = text;

                    var worker = new System.ComponentModel.BackgroundWorker();
                    worker.DoWork += (s, args) => ExecuteShortcut(result);
                    worker.RunWorkerCompleted += (s, args) => HandleShortcutResult(result);
                    worker.RunWorkerAsync();
                }

                ((TextBox)sender).Text = "";
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                var text = ((TextBox)sender).Text;

                if (!String.IsNullOrEmpty(text))
                {
                    ((TextBox)sender).SelectionLength = 0;
                    ((TextBox)sender).CaretIndex = text.Length;

                    UpdateAutoComplete((TextBox)sender);
                }
            }
        }

        /**
         * Update auto complete when the textbox text changes
         */
        void txtShortcut_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;

            // only update autocomplete if text was added, so we don't fight the user
            foreach (var change in e.Changes)
            {
                if (change.AddedLength > 0)
                {
                    UpdateAutoComplete(box);
                    break;
                }
            }
        }

        /**
         * Look thru the shortcuts xml file for shortcuts that partially match the current textbox text
         */
        void UpdateAutoComplete(TextBox textbox)
        {
            var text = textbox.Text;

            if (!String.IsNullOrEmpty(text))
            {
                var result = new AutoCompleteResult();
                result.InputText = text;

                var worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += (s, args) => UpdateAutoComplete(result);
                worker.RunWorkerCompleted += (s, args) => HandleAutoCompleteResult(textbox, result);
                worker.RunWorkerAsync();
            }
        }

        /**
         * Asynchronously execute a named shortcut
         */
        void ExecuteShortcut(ShortcutResult result)
        {
            try
            {
                // try a file system reference first
                if (ShortcutExecutor.ExecuteFileSystem(result.Shortcut, ""))
                {
                    result.bSuccess = true;
                }
                // if no luck there, try a shortcut
                else
                {
                    result.bSuccess = ShortcutExecutor.ExecuteShortcut(InstanceParams().SetShortcut(result.Shortcut));
                }
            }
            catch (Exception ex)
            {
                result.ThrownException = ex;
            }
        }

        /**
         * Handle the result of an executed shortcut
         */
        void HandleShortcutResult(ShortcutResult result)
        {
            // shortcut executed successfully
            if (result.bSuccess)
            {
                if (JRunUI.Properties.Settings.Default.History == null)
                {
                    JRunUI.Properties.Settings.Default.History = new System.Collections.Specialized.StringCollection();
                }
                JRunUI.Properties.Settings.Default.History.Remove(result.Shortcut);
                JRunUI.Properties.Settings.Default.History.Insert(0,result.Shortcut);
                Visibility = System.Windows.Visibility.Hidden;
                Close();
            }

            // shortcut was not found
            else if (result.ThrownException == null)
            {
                // do error flash
                var storyboard = FindResource("ErrorAnimation") as Storyboard;
                var anim1 = storyboard.Children[0];
                var anim2 = storyboard.Children[1];
                Storyboard.SetTargetName(anim1, MainBorder.Name);
                Storyboard.SetTargetName(anim2, txtShortcut.Name);
                storyboard.Begin(this);
            }

            // something went wrong while trying to execute the shortcut
            else
            {
                ShowErrorDialog(result.ThrownException);
            }
        }

        /**
         * Show an error dialog to the user when something is wrong with the shortcuts xml file
         */
        void ShowErrorDialog(Exception thrownException)
        {
            StringBuilder sb = new StringBuilder();

            for (var ex = thrownException; ex != null; ex = ex.InnerException)
            {
                sb.AppendLine(ex.Message);
            }

            var button = MessageBoxButton.OK;
            var icon = MessageBoxImage.Warning;
            MessageBox.Show(this, sb.ToString(), "Error", button, icon);
        }

        /**
         * Try to find the best filesystem path or shortcut that matches the input text
         */
        void UpdateAutoComplete(AutoCompleteResult result)
        {
            try
            {
                var shortcutNames = new List<string>();

                // build the list of valid shortcuts
                foreach (var shortcutName in ShortcutExecutor.GetAllShortcutNames(InstanceParams()))
                {
                    if (shortcutName.StartsWith(result.InputText, StringComparison.OrdinalIgnoreCase))
                    {
                        shortcutNames.Add(shortcutName);
                    }
                }

                // check history first for a match (but only if it stll exists as a valid shortcut)
                if (JRunUI.Properties.Settings.Default.History != null)
                {
                    foreach (var historyName in JRunUI.Properties.Settings.Default.History)
                    {
                        if (historyName.StartsWith(result.InputText, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var shortcutName in shortcutNames)
                            {
                                if (StringComparer.OrdinalIgnoreCase.Equals(shortcutName, historyName))
                                {
                                    result.OutputText = historyName;
                                    return;
                                }
                            }
                        }
                    }
                }

                // if not found in history, pick the first available one
                if (shortcutNames.Count > 0)
                {
                    result.OutputText = shortcutNames[0];
                }

                // if no shortcut found, try completing directories and files
                else
                {
                    var drives = System.IO.DriveInfo.GetDrives();

                    foreach (var drive in drives)
                    {
                        if (AutoCompletDirectory(drive.RootDirectory, result))
                        {
                            break;
                        }
                    }
                }
            }
            catch
            {
                // not a huge deal if auto-complete doesn't work
            }
        }

        /**
         * Search down the filesystem to see if the user is inputting a filesystem path
         */
        bool AutoCompletDirectory(System.IO.DirectoryInfo dir, AutoCompleteResult result)
        {
            if (dir.FullName.StartsWith(result.InputText, StringComparison.OrdinalIgnoreCase))
            {
                result.OutputText = dir.FullName.TrimEnd('\\') + '\\';
            }

            if (result.InputText.StartsWith(dir.FullName, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var subFile in dir.GetFiles())
                {
                    if (subFile.FullName.StartsWith(result.InputText, StringComparison.OrdinalIgnoreCase))
                    {
                        result.OutputText = subFile.FullName;
                        return true;
                    }
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    if (AutoCompletDirectory(subDir, result))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Handle the results of an autocomplete query.
         * Will update the textbox's text value
         */
        void HandleAutoCompleteResult(TextBox textbox, AutoCompleteResult result)
        {
            if (!String.IsNullOrEmpty(result.OutputText))
            {
                var currentText = GetUnHighlightedText(textbox);

                // don't update the textbox if it's changed since autocomplete started
                if (currentText.Equals(result.InputText))
                {
                    textbox.Text = result.OutputText;

                    // select/highlight the autocompleted part so the user can easily skip or delete it
                    textbox.Select(result.InputText.Length, result.OutputText.Length - result.InputText.Length);
                }
            }
        }

        /**
         * Get the part of the text that is not highlighted.
         * This part is what the user has actually typed, the rest is guess
         */
        string GetUnHighlightedText(TextBox textbox)
        {
            if (!String.IsNullOrEmpty(textbox.SelectedText))
            {
                if (textbox.Text.EndsWith(textbox.SelectedText))
                {
                    return textbox.Text.Substring(0, textbox.Text.LastIndexOf(textbox.SelectedText));
                }
            }

            return textbox.Text;
        }

        void ContextMenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Show();
            aboutWindow.Owner = Owner;
        }

		public void ContextMenuHelp_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(App.Current.Settings.HelpURL, "");
			}
			catch (System.Exception ex)
			{
				ShowErrorDialog(ex);
			}
		}

		public void ContextMenuEditShortcuts_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ShortcutExecutor.ExecuteFileSystem(InstanceParams().File, "");
			}
			catch (System.Exception ex)
			{
				ShowErrorDialog(ex);
			}
		}

        public void ContextMenuOptions_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new OptionsWindow();
            optionsWindow.Show();
        }

        public void ContextMenuExit_Click(object sender, RoutedEventArgs e)
        {
            Owner.Close();
        }

        public void ContextMenuMove_Click(object sender, RoutedEventArgs e)
        {
            WatchMouseEvents(true);
        }

        public void ContextMenuResize_Click(object sender, RoutedEventArgs e)
        {
            WatchMouseEvents(false);
        }

        public void ContextMenuReset_Click(object sender, RoutedEventArgs e)
        {
            JRunUI.Properties.Settings.Default.Reset();
            JRunUI.Properties.Settings.Default.Save();
        }

        void WatchMouseEvents(bool bDragging)
        {
            this.CaptureMouse();
            this.MouseDown += MainWindow_MouseDown;

            if ( mouseTimer == null )
            {
                mouseTimer = new DispatcherTimer();
                mouseTimer.Interval = TimeSpan.FromMilliseconds(16);
                mouseTimer.Start();
            }

            if (bDragging)
            {
                mouseTimer.Tick += MainWindow_DragMouseMove;
            }
            else
            {
                mouseTimer.Tick += MainWindow_ResizeMouseMove;
            }
        }

        void IgnoreMouseEvents()
        {
            this.MouseDown -= MainWindow_MouseDown;
            this.mouseTimer.Stop();
            this.mouseTimer = null;
            this.ReleaseMouseCapture();
            JRunUI.Properties.Settings.Default.Save();
        }

        void MainWindow_DragMouseMove(object sender, EventArgs e)
        {
            var point = WinAPI.GetCursporPos();

            Top = point.Y - (Height / 2);
            Left = point.X - (Width / 2);
        }

        void MainWindow_ResizeMouseMove(object sender, EventArgs e)
        {
            var topLeftPoint = PointToScreen(new Point(0, 0));
            var mousePoint = WinAPI.GetCursporPos();

            Width = Math.Max(10, mousePoint.X - topLeftPoint.X) + 5;
            Height = Math.Max(10, mousePoint.Y - topLeftPoint.Y) + 5;
        }

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IgnoreMouseEvents();
        }

        void MainWindow_ResizeMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            Width = position.X + 25;
            Height = position.Y + 25;
            e.Handled = true;
        }
    }

    /**
     * Routes bindings to our app settings
     */
    public class SettingsBinding : Binding
    {
        public SettingsBinding()
        {
            Initialize();
        }

        public SettingsBinding(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = JRunUI.Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
