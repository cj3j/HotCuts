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
using JLib;

namespace JRun
{
    /**
     * Parameters passed to various ShortcutExecutor functions
     */
    public class ShortcutParams
    {
        public string File, Profile, Shortcut, Args;

		public ShortcutParams()
        {
            File = "Shortcuts.xml";
            Profile = "";
            Shortcut = "";
            Args = "";
        }

		public ShortcutParams SetShortcut(string shortcut) { Shortcut = shortcut; return this; }
		public ShortcutParams SetFile(string file) { File = file; return this; }
		public ShortcutParams SetProfile(string profile) { Profile = profile; return this; }
		public ShortcutParams SetArgs(string args) { Args = args; return this; }
    }

    /**
     * Provides helper functions for using an XmlShortcutFile
     */
	public static class ShortcutExecutor
    {
        /**
         * Attempts to execute the process at the given path.
         * The string that the process writes to standard output
         * will be used as the profile name to pull shortcuts from.
         */
        public static string SelectProfile(string selectorPath)
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo(selectorPath);
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                var process = System.Diagnostics.Process.Start(startInfo);
                process.WaitForExit();

                return process.StandardOutput.ReadLine();
            }

            catch (Exception ex)
            {
                throw new JRunException(ex, "Could not execute profile selector {0}", selectorPath);
            }
        }

        /**
         * Open/execute a file or folder using whatever the shell wants to open it.
         */
        public static bool ExecuteFileSystem(string filePath, string args)
        {
            if (System.IO.File.Exists(filePath) || System.IO.Directory.Exists(filePath))
            {
                var process = System.Diagnostics.Process.Start(filePath, args);
                return true;
            }

            return false;
        }

        /**
         * Execute a shortcut in an xml shorcuts file from the given parameters
         */
        public static bool ExecuteShortcut(ShortcutParams parameters)
        {
			JArgumentNullException.Check(parameters, "parameters");

            try
            {
				var profile = parameters.Profile;
				var xmlFile = new XmlShortcutFile(parameters.File);
				var shortcut = xmlFile.GetShortcut(parameters.Profile, parameters.Shortcut);

                if (shortcut != null)
                {
					shortcut.Params += " " + parameters.Args;
                    return shortcut.Execute();
                }

                return false;
            }
            catch (JRunXmlException ex)
            {
                throw CreateXmlException(ex);
            }
            catch (JRunException)
            {
                throw;
            }
            catch (Exception ex)
            {
				throw new JException(ex, "Could not execute shortcut \"{0}\" for profile \"{1}\" in xml file \"{2}\"", parameters.Shortcut, parameters.Profile, parameters.File);
            }
        }

        /**
         * Enumerates the names of all shortcuts (explicit and virtual) in the xml shortcuts file.
         * Useful for auto-complete.
         */
        public static IEnumerable<string> GetAllShortcutNames()
        {
            return GetAllShortcutNames(new ShortcutParams());
        }

        /**
         * Enumerates the names of all shortcuts (explicit and virtual) in the xml shortcuts file.
         * Useful for auto-complete.
         */
		public static IEnumerable<string> GetAllShortcutNames(ShortcutParams parameters)
        {
            try
            {
				var xmlFile = new XmlShortcutFile(parameters.File);

				return xmlFile.GetAllShortcutNames(parameters.Profile);
            }
            catch (JRunXmlException ex)
            {
                throw CreateXmlException(ex);
            }
            catch (Exception ex)
            {
				throw new JException(ex, "Could not get all shortcut names for profile \"{0}\" in xml file \"{1}\"", parameters.Profile, parameters.File);
            }
        }

        static JRunException CreateXmlException(JRunXmlException ex)
        {
            string nodePath = ex.Node.GetDebugName();

            for (var node = ex.Node.ParentNode; node != null; node = node.ParentNode)
            {
                nodePath = node.GetDebugName() + "." + nodePath;
            }

            return new JRunException(ex, "Error parsing XML Node \"{0}\"", nodePath);
        }
    }
}
