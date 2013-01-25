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
using System.Xml;
using System.Text.RegularExpressions;
using JLib;

namespace JRun
{
    /**
     * Represents errors of invalid use of the shortcuts xml document
     */
    class JRunXmlException : JLib.JException
    {
        public XmlNode Node;

        public JRunXmlException(XmlNode node, string message, params object[] args)
            : base(message, args)
        {
            Node = node;
        }

        public JRunXmlException(XmlNode node, Exception innerException, string message, params object[] args)
            : base(innerException, message, args)
        {
            Node = node;
        }
    }

    /**
     * A shortcut derived from a shorcuts xml file. This may be an explicit shortcut, or
     * one derived from an inherited list (hence the virtual).
     */
    class XmlVirtualShortcut
    {
        public string Name;
        public XmlNode ShortcutNode;
        public XmlNode ContextNode;
        public XmlMacroCollection Macros;

        public XmlVirtualShortcut( string inName, XmlNode inContextNode, XmlNode inShortcutNode, XmlMacroCollection inMacros )
        {
            JArgumentEmptyException.Check(inName, "inName");
            JArgumentNullException.Check(inContextNode, "inContextNode");
            JArgumentNullException.Check(inShortcutNode, "inShortcutNode");
            JArgumentNullException.Check(inMacros, "inMacros");

            Name = inName;
            ContextNode = inContextNode;
            ShortcutNode = inShortcutNode;
            Macros = inMacros;
        }
    }

    /**
     * Use to load a shortcuts xml document in order to retrieve one or more shortcuts
     */
    class XmlShortcutFile
    {
        XmlDocument _doc;
        XmlMacroCollection _globalMacros;

        public XmlShortcutFile(string filePath)
        {
            _doc = new XmlDocument();
            _doc.Load(filePath);

            _globalMacros = new XmlMacroCollection();

            _globalMacros.AddMacros(_doc.DocumentElement);
        }

        /**
         * Retrieve the names of all shortcuts in the given profile.
         * Quicker than GetAllShortcuts if all you need is a list of names
         */
        public IEnumerable<string> GetAllShortcutNames(string profileName)
        {
            foreach (var vShortcut in GetAllVirtualShortcuts(profileName))
            {
                yield return vShortcut.Name;
            }
        }

        /**
         * Retrieves all shortcuts from the given xml file and profile name.
         * Useful for auto-complete or exporting.
         */
        public IEnumerable<Shortcut> GetAllShortcuts(string profileName)
        {
            foreach (var vShortcut in GetAllVirtualShortcuts( profileName))
            {
                yield return CreateShortcut(vShortcut);
            }
        }

        /**
         * Retrieves the named shortcut from the given xml file and profile name.
         */
        public Shortcut GetShortcut(string profileName, string shortcutName)
        {
            foreach (var vShortcut in GetAllVirtualShortcuts(profileName))
            {
                if ( StringComparer.OrdinalIgnoreCase.Equals( vShortcut.Name, shortcutName ) )
                {
                    return CreateShortcut(vShortcut);
                }
            }

            return null;
        }

        /**
        * Retrieves all shortcut xml nodes from the given xml file and profile name.
        */
        public IEnumerable<XmlVirtualShortcut> GetAllVirtualShortcuts(string profileName)
        {
            List<XmlVirtualShortcut> shortCuts = new List<XmlVirtualShortcut>();

            foreach (var profile in _doc.DocumentElement.SelectXmlNodes("Profile"))
            {
                if (String.IsNullOrEmpty(profileName) || AttributeValueMatches(profile, _globalMacros, "name", profileName))
                {
                    var profileMacros = new XmlMacroCollection(_globalMacros);
                    AddTemplateMacros(profile, profileMacros);

                    // get all shortcut nodes defined directly in the profile
                    foreach (var shortcutNode in GetAllShortcutNodes(profile, profileMacros))
                    {
                        var vShortcut = CreateVirtualShortcut(shortcutNode, shortcutNode, profileMacros);

                        if (vShortcut != null)
                        {
                            shortCuts.Add(vShortcut);
                        }
                    }

                    // get all shortcut nodes defined in lists
                    foreach (var profileTemplate in NodeTemplateTree(profile, profileMacros))
                    {
                        foreach (var listNode in profileTemplate.SelectXmlNodes("List"))
                        {
                            var listMacros = new XmlMacroCollection(profileMacros);
                            AddTemplateMacros(listNode, listMacros);

                            foreach (var shortcutNode in GetAllShortcutNodes(listNode, listMacros))
                            {
                                var vShortcut = CreateVirtualShortcut(shortcutNode, shortcutNode, listMacros);

                                if (vShortcut != null)
                                {
                                    shortCuts.Add(vShortcut);
                                }
                            }
                        }
                    }

                    break;
                }
            }

            return shortCuts;
        }

        /**
         * Creates a virtual shortcut for the given context and node
         */
        XmlVirtualShortcut CreateVirtualShortcut(XmlNode context, XmlNode shortcutNode, XmlMacroCollection outerMacros)
        {
            var nodeName = shortcutNode.GetAttributeValue("name");

            if (!String.IsNullOrEmpty(nodeName))
            {
                var realName = outerMacros.GetExpandedValue(nodeName);

                if (!String.IsNullOrEmpty(realName))
                {
                    var newMacros = new XmlMacroCollection(outerMacros);

                    AddTemplateMacros(shortcutNode, newMacros);

                    return new XmlVirtualShortcut(realName, context, shortcutNode, newMacros);
                }
            }

            return null;
        }

        /**
         * Returns all shortcut nodes that are in the given node and the node's templates.
         */
        IEnumerable<XmlNode> GetAllShortcutNodes(XmlNode listNode, XmlMacroCollection macros)
        {
            foreach (var template in NodeTemplateTree(listNode, macros))
            {
                foreach (var node in template.SelectXmlNodes("Shortcut"))
                {
                    yield return node;
                }
            }
        }

        /**
         * Constructs a new Shortcut object from the information in the given node
         */
        Shortcut CreateShortcut(XmlVirtualShortcut vShortcut)
        {
            string filePath, processParams;

            GetPropertyValue(vShortcut.ShortcutNode, vShortcut.Macros, "Path", out filePath);
            GetPropertyValue(vShortcut.ShortcutNode, vShortcut.Macros, "Params", out processParams);

            if (string.IsNullOrEmpty(filePath))
            {
                throw new JRunXmlException(vShortcut.ContextNode, "No executable path was specified");
            }

            var shortcut = new Shortcut();
            shortcut.Executable = filePath;
            shortcut.Params = processParams;

            return shortcut;
        }

        /**
         * Gets the macro-expanded value of the named attribute and returns true
         * if it matches the given value check (ignores case)
         */
        bool AttributeValueMatches(XmlNode node, XmlMacroCollection macros, string attribName, string valueCheck)
        {
            string nodeValue;
            if (GetAttributeValue(node, macros, attribName, out nodeValue))
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(nodeValue, valueCheck))
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * Calculates the macro-expanded value of an attribute on the node.
         * Returns true if the attribute is defined.
         */
        bool GetAttributeValue(XmlNode node, XmlMacroCollection macros, string attribName, out string value)
        {
            value = node.GetAttributeValue(attribName);

            if (value != null)
            {
                try
                {
                    value = macros.GetExpandedValue(value);
                }
                catch (Exception ex)
                {
                    throw new JRunXmlException(node, ex, "Could not expand macro value for attribute \"{0}\"", attribName);
                }
            }

            return value != null;
        }

        /**
         * Retrieve the macro-expanded text value of a child of an xml node.
         * Properties are inheritable from templates.
         * Returns true if the property is defined.
         */
        bool GetPropertyValue(XmlNode node, XmlMacroCollection macros, string propertyName, out string value)
        {
            value = null;

            // check if we define this, or any of our templates
            foreach (var templateNode in NodeTemplateTree(node, macros))
            {
                var childNode = templateNode.SelectSingleNode(propertyName);

                if (childNode != null)
                {
                    value = childNode.InnerText;
                    break;
                }
            }

            if (value != null)
            {
                try
                {
                    value = macros.GetExpandedValue(value);
                }
                catch (Exception ex)
                {
                    throw new JRunXmlException(node, ex, "Could not expand macro value for property \"{0}\"", propertyName);
                }
            }

            return value != null;
        }

        /**
         * Recursively add the node and his template's macros
         */
        void AddTemplateMacros(XmlNode node, XmlMacroCollection macros)
        {
            var templateNode = GetNodeTemplate(node, macros);

            if (templateNode != null)
            {
                AddTemplateMacros(templateNode, macros);
            }

            macros.AddMacros(node);
        }

        /**
         * Enumerates through the given node and upwards through his template hierarchy
         */
        IEnumerable<XmlNode> NodeTemplateTree(XmlNode node, XmlMacroCollection macros)
        {
            // check if we define this, or any of our templates
            for (var templateNode = node; templateNode != null; templateNode = GetNodeTemplate(templateNode, macros))
            {
                yield return templateNode;
            }
        }

        /**
         * Gets the node the given node inherits from, if it exists.
         */
        XmlNode GetNodeTemplate(XmlNode node, XmlMacroCollection macros)
        {
            string templateName;

            if (GetAttributeValue(node, macros, "inherits", out templateName))
            {
                if (!String.IsNullOrEmpty(templateName))
                {
                    var templateNode = FindTemplate(node.ParentNode, macros, templateName);

                    if (templateNode != null)
                    {
                        return templateNode;
                    }
                }

                throw new JRunXmlException(node, "Could not find template \"{0}\"", templateName);
            }

            return null;
        }

        /**
         * Searches up the parent node tree to find a template by the given name
         */
        XmlNode FindTemplate(XmlNode start, XmlMacroCollection macros, string templateName)
        {
            foreach (var nodeObj in start.ChildNodes)
            {
                var node = nodeObj as XmlNode;

                if (node != null)
                {
                    if (AttributeValueMatches(node, macros, "name", templateName))
                    {
                        return node;
                    }
                }
            }

            if (start.ParentNode != null)
            {
                return FindTemplate(start.ParentNode, macros, templateName);
            }

            return null;
        }
    }
}
