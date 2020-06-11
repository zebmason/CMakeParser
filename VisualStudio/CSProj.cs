﻿//------------------------------------------------------------------------------
// <copyright file="CSProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    internal class CSProj : NetProj
    {
        public CSProj(string path, Core.Paths paths)
            : base(path, paths)
        {
        }

        public override List<string> Dependencies()
        {
            var list = base.Dependencies();
            var l1 = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(_xml._root, "Import", l1);
            foreach (var i1 in l1)
            {
                if (!i1.HasAttribute("Label"))
                    continue;

                if (i1.GetAttribute("Label") != "Shared")
                    continue;

                var link = i1.GetAttribute("Project");
                link = _paths.Path(link);
                list.Add(link);
            }

            return list;
        }

        public static void Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects, Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping)
        {
            var solutionPath = paths.Value("SolutionDir");
            var proj = new CSProj(filePath, paths);

            projects[proj.Name] = new Core.CSharp();
            dependencies[projects[proj.Name]] = proj.Dependencies();
            proj.Compiles(projects[proj.Name].FilePaths, logger, paths);
            mapping[filePath] = proj.Name;
        }
    }
}
