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
using JLib;

namespace JRun
{
	/**
	 * Command-line utility for JRun
	 */
    class Program
    {
        static void Main(string[] args)
        {
            DateTime appStart = DateTime.Now;

            string filePath = "Shortcuts.xml";
            string profile = "";
            string exportPath = null;
            string cmd = null;
            bool bReportTime = false;

            var options = new NDesk.Options.OptionSet() {
                { "cmd:|shortcut", "The name of the shortcut to execute.", s => cmd = s },
                { "f:|file:", "File to load shortcuts from. Default is 'Shortcuts.xml'.", s => filePath = s },
                { "p:|profile:", "Profile to execute the shortcut in. Default is the first profile.", s => profile = s },
                //{ "e:|export:", "The SlickRun (.qrs) file to export all of the profile's shortcuts to.", s => exportPath = s },
                { "timed", "Prints how long it takes to find and execute the shortcut.", s => bReportTime = s != null },
            };

            options.Parse(args);

            if (cmd == null)
            {
                if (args.Length > 0 && args[0].Length > 0 && args[0][0] != '-')
                {
                    cmd = args[0];
                }
            }

            if (cmd != null)
            {
                try
                {
                    DateTime executeStart = DateTime.Now;

                    var jparams = new ShortcutParams().SetFile(filePath).SetProfile(profile).SetShortcut(cmd);
                    var bSuccess = ShortcutExecutor.ExecuteShortcut(jparams);

                    if (!bSuccess)
                    {
                        Console.WriteLine("Could not find shortcut \"{0}\" for profile \"{1}\" in xml file \"{2}\"", cmd, profile, filePath);
                    }

                    if (bReportTime)
                    {
                        DateTime End = DateTime.Now;
                        TimeSpan appTime = End - appStart;
                        TimeSpan executeTime = End - executeStart;

                        Console.WriteLine("App Time: {0}, Execute Time: {1}", appTime.TotalMilliseconds, executeTime.TotalMilliseconds);
                        Console.ReadKey();
                    }
                }
                catch (Exception ex)
                {
                    for (var innerEx = ex; innerEx != null; innerEx = innerEx.InnerException)
                    {
                        Console.WriteLine(innerEx.Message);
                    }
                    Console.ReadKey();
                }
            }
            else if (exportPath != null)
            {
                // TODO support exporting shortcuts to other formats
            }
        }
    }
}
