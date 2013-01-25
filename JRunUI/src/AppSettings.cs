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

namespace JRunUI
{
    /**
     * Application settings - loaded and saved from user preferences,
     * and overridden with commandline arguments
     */
    public class AppSettings
    {
        // path to the shortcuts xml file
        public string XmlFile { get; private set; }

        // the name of the profile to use inside of the shortcuts xml file
        public string ProfileName { get; private set; }

        // batch or executable for selecting a profile
        public string ProfileSelector { get; private set; }

        // if true, reset the application settings on load
        public bool bResetAppSettings { get; private set; }

        // if true, log debug information
        public bool bLog { get; set; }

        // the current version
        public string Version
        {
            get
            {
                return "1.0 Beta 1";
            }
        }

		public string HelpURL
		{
			get
			{
				return "http://code.google.com/p/hotcuts/wiki/Intro";
			}
		}

        internal AppSettings(string[] args, JRunUI.Properties.Settings defaults )
        {
            LoadFromAppSettings(defaults);

            defaults.SettingsSaving += new System.Configuration.SettingsSavingEventHandler(defaults_SettingsSaving);

            var options = new NDesk.Options.OptionSet() {
                    { "f=|file=", "File to load shortcuts from. Default is 'Shortcuts.xml'.", s => XmlFile = s },
                    { "p=|profile'", "Profile to execute the shortcut in. Default is the first profile.", s => ProfileName = s },
                    { "s=|selector=", "Program to execute to determine which profile to use.", s => ProfileSelector = s },
                    { "r|reset", "Reset application settings to default", s => bResetAppSettings = s != null },
                    { "log", "output debug information to a log file", s => bLog = s != null },
                };

            options.Parse(args);
        }

        void defaults_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadFromAppSettings((JRunUI.Properties.Settings)sender);
        }

        void LoadFromAppSettings(JRunUI.Properties.Settings defaults)
        {
            XmlFile = defaults.XmlFile;
            ProfileName = defaults.ProfileName;
            ProfileSelector = defaults.ProfileSelector;
        }
    }
}
