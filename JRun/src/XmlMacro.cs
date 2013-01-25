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
using System.Text.RegularExpressions;
using System.Xml;
using JLib;

namespace JRun
{
    /**
     * The value of an xml-defined macro
     */
    class XmlMacroValue
    {
        // The string value - cannot be null.
        // Once this gets set manually, the macro value is considered
        // to be finaized and cannot be changed again,
        public string Value
        {
            get { return _value; }
            set
            {
                JArgumentNullException.Check(value, "value");

                if (_bValueFinalized)
                {
                    throw new JInvalidOperationException("Cannot change a macro's value after it has been finalized");
                }

                _value = value;
                _bValueFinalized = true;
            }
        }

        // true if the string value of this macro has itself been macro-expanded.
        public bool bValueFinalized
        {
            get { return _bValueFinalized; }
        }

        string _value;
        bool _bValueFinalized;

        public XmlMacroValue(string initialValue)
        {
            _value = initialValue;
        }
    }

    /**
     * Holds a collection of xml macros pulled from multiple nodes
     */
    class XmlMacroCollection
    {
        // list of xml nodes we have parsed, to prevent adding nodes more than once
        Dictionary<XmlNode,bool> _nodes;

        // hash of macro names to value
        Dictionary<string, XmlMacroValue> _macros;

        public XmlMacroCollection()
        {
        }

        /**
         * Construct a new macro collection that copies from an existing one
         */
        public XmlMacroCollection(XmlMacroCollection CopyFrom)
        {
            if (CopyFrom._nodes != null)
            {
                _nodes = new Dictionary<XmlNode, bool>(CopyFrom._nodes);
            }

            if (CopyFrom._macros != null)
            {
                _macros = new Dictionary<string, XmlMacroValue>(CopyFrom._macros);
            }
        }

        /**
         * Iterate over all the Define nodes in this node and add them as macros
         */
        public void AddMacros(XmlNode node)
        {
            if (_nodes == null)
            {
                _nodes = new Dictionary<XmlNode, bool>();
            }

            try
            {
                _nodes.Add(node, true);
            }
            catch (System.ArgumentException)
            {
                throw new JRunXmlException(node, "Attempted to add a node's macros twice");
            }

            foreach (var defNode in node.SelectXmlNodes("Define"))
            {
                var name = defNode.GetAttributeValue("name");

                if (name != null && name.Length > 0)
                {
                    var value = defNode.GetAttributeValue("value");

                    if (value != null)
                    {
                        if (_macros == null)
                        {
                            _macros = new Dictionary<string, XmlMacroValue>(StringComparer.OrdinalIgnoreCase);
                        }

                        // overwrite any macros that are already there
                        _macros.Remove(name);
                        _macros.Add(name, new XmlMacroValue(value));
                    }
                    else
                    {
                        throw new JRunXmlException(defNode, "Define nodes must have a value");
                    }
                }
                else
                {
                    throw new JRunXmlException(defNode, "Define nodes must have a name");
                }
            }
        }

        /**
         * Gets the macro-expanded value of a string
         */
        public string GetExpandedValue(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = Regex.Replace(value, "\\{\\w+\\}", p => MacroReplace(p, true));
                value = Regex.Replace(value, "\\[\\w+\\]", p => MacroReplace(p, false));
            }

            return value;
        }

		/**
         * Replaces a macro used in a value with the value of the macro.
		 * Recursively evaluates macros (in case you use a macro in a macro).
         */
        string MacroReplace(Match match, bool bRequired)
        {
            string name = match.Value.Substring(1, match.Value.Length - 2);
            string value = null;

            XmlMacroValue macroValue = null;

            if (_macros.TryGetValue(name, out macroValue))
            {
                if (!macroValue.bValueFinalized)
                {
                    // TODO handle infinite recursion (macro depends on another macro)
                    macroValue.Value = GetExpandedValue(macroValue.Value);
                }

                value = macroValue.Value;
            }
            else
            {
                if (bRequired)
                {
                    throw new JLib.JException("Macro \"{0}\" is undefined", name);
                }

                value = "";
            }

            return value;
        }
    }
}
