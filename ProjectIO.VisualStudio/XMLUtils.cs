﻿//------------------------------------------------------------------------------
// <copyright file="XMLUtils.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;
    using System.IO;

    public class XMLUtils
    {
        private readonly System.Xml.XmlDocument _doc;

        internal System.Xml.XmlElement _root;

        internal XMLUtils(string filePath)
        {
            _doc = new System.Xml.XmlDocument();
            _doc.Load(filePath);
            _root = _doc.DocumentElement;
        }

        internal void SelectNodes(System.Xml.XmlElement root, string name, List<System.Xml.XmlElement> list, bool descend = false)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node.GetType() != typeof(System.Xml.XmlElement))
                {
                    continue;
                }

                var element = node as System.Xml.XmlElement;
                if (element.Name == name)
                {
                    list.Add(element);
                }
                else if (descend)
                {
                    SelectNodes(element, name, list);
                }
            }
        }

        internal System.Xml.XmlElement Group(string name, string condition)
        {
            var nodes = new List<System.Xml.XmlElement>();
            SelectNodes(_root, name, nodes);
            foreach (var node in nodes)
            {
                if (node.HasAttribute("Condition") && node.GetAttribute("Condition").Contains(condition))
                {
                    return node;
                }
            }

            return null;
        }

        public List<string> Compiles(string name)
        {
            var list = new List<string>();
            var l1 = new List<System.Xml.XmlElement>();
            SelectNodes(_root, "ItemGroup", l1);
            foreach (var i1 in l1)
            {
                var l2 = new List<System.Xml.XmlElement>();
                SelectNodes(i1, name, l2);
                foreach (var i2 in l2)
                {
                    var link = i2.GetAttribute("Include");
                    list.Add(link);
                }
            }

            return list;
        }

        public Dictionary<string, string> Filters(string name)
        {
            var dict = new Dictionary<string, string>();
            var l1 = new List<System.Xml.XmlElement>();
            SelectNodes(_root, "ItemGroup", l1);
            foreach (var i1 in l1)
            {
                var l2 = new List<System.Xml.XmlElement>();
                SelectNodes(i1, name, l2);
                foreach (var i2 in l2)
                {
                    var link = i2.GetAttribute("Include");
                    dict[link] = string.Empty;

                    var l3 = new List<System.Xml.XmlElement>();
                    SelectNodes(i2, "Filter", l3);
                    foreach (var i3 in l3)
                    {
                        dict[link] = i3.InnerText;
                    }
                }
            }

            return dict;
        }

        private bool NewDotNetCompiles(Proj project, List<string> files, Core.ILogger logger, Core.Paths filePath)
        {
            if (!_root.HasAttribute("Sdk"))
            {
                return false;
            }

            var direc = Path.GetDirectoryName(project.FilePath);
            var removes = new List<string>();
            var l1 = new List<System.Xml.XmlElement>();
            SelectNodes(_root, "ItemGroup", l1);
            foreach (var i1 in l1)
            {
                var l2 = new List<System.Xml.XmlElement>();
                SelectNodes(i1, "Compile", l2);
                foreach (var i2 in l2)
                {
                    var name = i2.GetAttribute("Remove");
                    removes.Add(Path.Combine(direc, name));
                }
            }

            foreach (var name in Directory.GetFiles(direc))
            {
                if (removes.Contains(name) || Path.GetExtension(name) != ".cs")
                {
                    continue;
                }

                files.Add(name);
            }

            return true;
        }

        public void DotNetCompiles(Proj project, List<string> files, Core.ILogger logger, Core.Paths filePath)
        {
            if (NewDotNetCompiles(project, files, logger, filePath))
            {
                return;
            }

            var list = Compiles("Compile");
            var direc = System.IO.Path.GetDirectoryName(project.FilePath);
            filePath.Add("ProjectDir", direc);
            foreach (var link in list)
            {
                var trimmed = link;
                trimmed = trimmed.Replace("$(MSBuildThisFileDirectory)", direc + "\\");
                var file = filePath.RemoveAliases(trimmed);

                if (!System.IO.Path.IsPathRooted(file))
                {
                    file = System.IO.Path.Combine(direc, trimmed);
                }

                if (file.Contains("*"))
                {
                    var directory = System.IO.Path.GetDirectoryName(file);
                    var pattern = System.IO.Path.GetFileName(file);
                    try
                    {
                        foreach (var name in System.IO.Directory.GetFiles(directory, pattern))
                        {
                            logger.Info("Appended \"{0}\"", name);
                            files.Add(name);
                        }
                    }
                    catch
                    {
                    }
                }
                else if (file.Length > 0)
                {
                    if (!System.IO.File.Exists(file))
                    {
                        logger.Warn("Cannot find \"{0}\"", file);
                        continue;
                    }

                    logger.Info("Appended \"{0}\"", file);
                    files.Add(file);
                }
            }

            filePath.Remove("ProjectDir");
        }
    }
}
