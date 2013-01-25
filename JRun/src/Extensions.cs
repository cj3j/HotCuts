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
using System.Xml;

namespace JRun
{
    static class Extensions
    {
        public static IEnumerable<XmlNode> SelectXmlNodes(this System.Xml.XmlNode node, string query)
        {
            foreach (var obj in node.SelectNodes(query))
            {
                var subNode = obj as XmlNode;

                if (subNode != null)
                {
                    yield return subNode;
                }
            }
        }

        public static string GetAttributeValue(this XmlNode node, string attribName)
        {
            if (node.Attributes != null)
            {
                var attrib = node.Attributes[attribName];

                if (attrib != null)
                {
                    return attrib.Value;
                }
            }

            return null;
        }

        public static void SetAttributeValue(this XmlNode node, string attribName, string attribValue)
        {
            if (node.Attributes != null)
            {
                var attrib = node.Attributes[attribName];

                if (attrib == null)
                {
                    attrib = node.OwnerDocument.CreateAttribute(attribName);
                    node.Attributes.Append(attrib);
                }

                attrib.Value = attribValue;
            }
        }

        public static string GetDebugName(this XmlNode node)
        {
            var name = node.GetAttributeValue("name");

            if (name == null)
            {
                name = node.Name;
            }

            return name;
        }

        public static void AppendLineFormat(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendLine(String.Format(format, args));
        }
    }
}
