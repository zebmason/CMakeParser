﻿//------------------------------------------------------------------------------
// <copyright file="VBProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    internal class VBProj : NetProj
    {
        public VBProj(string path, Core.Paths paths)
            : base(path, paths)
        {
        }

        public static void Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects, Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping)
        {
            var solutionPath = paths.Value("SolutionDir");
            var proj = new VBProj(filePath, paths);

            projects[proj.Name] = new Core.VBasic();
            dependencies[projects[proj.Name]] = proj.Dependencies();
            proj.Compiles(projects[proj.Name].FilePaths, logger, paths);
            mapping[filePath] = proj.Name;
        }
    }
}
