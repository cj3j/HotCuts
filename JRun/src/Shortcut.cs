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
using System.Text;

namespace JRun
{
    /**
     * Represents a shortcut to an executable.
     * Holds the necessary process information.
     */
    class Shortcut
    {
        public string Executable { get; set; }
        public string Params { get; set; }

        /**
         * Start the process
         */
        public bool Execute()
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(Executable);
            process.StartInfo.FileName = Executable;
            process.StartInfo.Arguments = Params;

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                throw new JLib.JException(ex, "Could not start process \"{0}\"", Executable);
            }

            return true;
        }
    }
}
