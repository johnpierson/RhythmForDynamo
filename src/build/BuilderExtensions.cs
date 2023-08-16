using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

namespace _build;

public static class BuilderExtensions
{
    
    /// <summary>
    /// Get project from project name
    /// </summary>
    /// <param name="solution">solution</param>
    /// <param name="projectName">project name</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static Project GetProject(this Solution solution, string projectName) =>
        solution.GetProject(projectName) ?? throw new NullReferenceException($"Can't find project \"{projectName}\"");

    
    /// <summary>
    /// return bin folder of project
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static AbsolutePath GetBinDirectory(this Project project) => project.Directory / "bin";
    
    
    /// <summary>
    /// return configuration from pattern filter name
    /// </summary>
    /// <param name="solution"></param>
    /// <param name="startPatterns"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static List<string> GetConfigurations(this Solution solution, params string[] startPatterns)
    {
        var configurations = solution.Configurations
            .Select(pair => pair.Key)
            .Where(s => startPatterns.Any(s.StartsWith))
            .Select(s =>
            {
                var platformIndex = s.LastIndexOf('|');
                return s.Remove(platformIndex);
            })
            .ToList();
        if (configurations.Count == 0)
            throw new Exception(
                $"Can't find configurations in the solution by patterns: {string.Join(" | ", startPatterns)}.");
        return configurations;
    }
    
}