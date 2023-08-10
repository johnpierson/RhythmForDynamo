﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RhythmViewExtension
{
    internal class Global
    {
        internal static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        internal static string PackageBinFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string PackageExtraFolder => PackageBinFolder.Replace("bin", "extra");
        internal static string PackageRoot => PackageBinFolder.Replace("bin", "");
        internal static string PackageJson => Path.Combine(PackageRoot, "pkg.json");
        internal static string RhythmRevitDll => Path.Combine(PackageBinFolder, "RhythmRevit.dll");

        internal static string[] EmbeddedLibraries =
            ExecutingAssembly.GetManifestResourceNames().Where(x => x.EndsWith(".dll")).ToArray();

        internal static string PackageJsonText =>
            "{\r\n  \"license\": \"BSD 3-Clause\",\r\n  \"file_hash\": null,\r\n  \"name\": \"Rhythm\",\r\n  \"version\": \"2023.9.1\",\r\n  \"description\": \"Rhythm is a set of nodes to help your Revit project maintain a good rhythm with Dynamo. Rhythm is fully open-source and authored by John Pierson (@johntpierson) of Design Tech Unraveled (designtechunraveled.com). \\n\\nSupported Revit versions are 2021-2024, with supported Dynamo versions being 2.6.x - 2.18.x\",\r\n  \"group\": \"Breaking CAD\",\r\n  \"keywords\": [\r\n    \"documentation\",\r\n    \"dimension\",\r\n    \"curtainwall\",\r\n    \"open\",\r\n    \"document\",\r\n    \"copy\",\r\n    \"unopened\",\r\n    \"background\",\r\n    \"sketch\"\r\n  ],\r\n  \"dependencies\": [],\r\n  \"host_dependencies\": [],\r\n  \"contents\": \"\",\r\n  \"engine_version\": \"2.12.0.5740\",\r\n  \"engine\": \"dynamo\",\r\n  \"engine_metadata\": \"\",\r\n  \"site_url\": \"https://designtechunraveled.com/\",\r\n  \"repository_url\": \"https://github.com/johnpierson/RhythmForDynamo\",\r\n  \"contains_binaries\": true,\r\n  \"node_libraries\": [\r\n    \"RhythmCore, Version=2022.11.1.0, Culture=neutral, PublicKeyToken=null\",\r\n    \"RhythmRevit, Version=2023.9.1.0, Culture=neutral, PublicKeyToken=null\"\r\n  ]\r\n}\r\n";

        internal static string PackageJsonTextWithoutRevitNodes =>
                "{\r\n  \"license\": \"BSD 3-Clause\",\r\n  \"file_hash\": null,\r\n  \"name\": \"Rhythm\",\r\n  \"version\": \"2023.9.1\",\r\n  \"description\": \"Rhythm is a set of nodes to help your Revit project maintain a good rhythm with Dynamo. Rhythm is fully open-source and authored by John Pierson (@johntpierson) of Design Tech Unraveled (designtechunraveled.com). \\n\\nSupported Revit versions are 2021-2024, with supported Dynamo versions being 2.6.x - 2.18.x\",\r\n  \"group\": \"Breaking CAD\",\r\n  \"keywords\": [\r\n    \"documentation\",\r\n    \"dimension\",\r\n    \"curtainwall\",\r\n    \"open\",\r\n    \"document\",\r\n    \"copy\",\r\n    \"unopened\",\r\n    \"background\",\r\n    \"sketch\"\r\n  ],\r\n  \"dependencies\": [],\r\n  \"host_dependencies\": [],\r\n  \"contents\": \"\",\r\n  \"engine_version\": \"2.12.0.5740\",\r\n  \"engine\": \"dynamo\",\r\n  \"engine_metadata\": \"\",\r\n  \"site_url\": \"https://designtechunraveled.com/\",\r\n  \"repository_url\": \"https://github.com/johnpierson/RhythmForDynamo\",\r\n  \"contains_binaries\": true,\r\n  \"node_libraries\": [\r\n    \"RhythmCore, Version=2022.11.1.0, Culture=neutral, PublicKeyToken=null\"\r\n  ]\r\n}\r\n";
    }
}
