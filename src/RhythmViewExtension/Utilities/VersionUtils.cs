using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Newtonsoft.Json;

namespace RhythmViewExtension.Utilities
{
    internal class VersionUtils
    {
        private VersionUtils(){}

        // Embedded Dynamo to Revit version mapping
        private const string DYNAMO_REVIT_MAPPING = @"{
  ""3.6"": [26],
  ""3.5"": [26],
  ""3.4"": [26, 25],
  ""3.3"": [25],
  ""3.2"": [25],
  ""3.0"": [25],
  ""2.19"": [24],
  ""2.18"": [24],
  ""2.17"": [24],
  ""2.16"": [23],
  ""2.15"": [23],
  ""2.13"": [23],
  ""2.12"": [22],
  ""2.11"": [22],
  ""2.10"": [22],
  ""2.8"": [22, 21],
  ""2.7"": [21],
  ""2.6"": [21],
  ""2.5"": [21, 20],
  ""2.4"": [20],
  ""2.3"": [20],
  ""2.2"": [20],
  ""2.1"": [20],
  ""2.0"": [20, 19, 18, 17]
}";

        private static Dictionary<string, List<int>> _mappingCache;

        /// <summary>
        /// Gets the Dynamo to Revit mapping dictionary (cached)
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        private static Dictionary<string, List<int>> GetDynamoRevitMapping()
        {
            if (_mappingCache == null)
            {
                _mappingCache = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(DYNAMO_REVIT_MAPPING);
            }
            return _mappingCache;
        }

        /// <summary>
        /// Gets Revit years that use a specific Dynamo major.minor version
        /// </summary>
        /// <param name="dynamoVersion">Dynamo version in M.m format (e.g., "2.19", "3.0")</param>
        /// <returns>List of Revit years as int (e.g., 24, 25, 26) or null if not found</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<int> GetRevitYearsForDynamo(string dynamoVersion)
        {
            try
            {
                var mapping = GetDynamoRevitMapping();
                
                if (mapping != null && mapping.ContainsKey(dynamoVersion))
                {
                    return mapping[dynamoVersion];
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all Dynamo to Revit mappings
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, List<int>> GetAllDynamoRevitMappings()
        {
            return GetDynamoRevitMapping();
        }

        
        [IsVisibleInDynamoLibrary(false)]
        private static bool CheckForUpdate(string username, string repoName)
        {
            try
            {
                Version currentVersion = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.Contains("RhythmRevit")).GetName().Version;

                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "Unity web player");

                    string address = $"https://api.github.com/repos/{username}/{repoName}/releases/latest";
                    Uri uri = new Uri(address);

                    string releases = webClient.DownloadString(uri);

                    var json = JsonConvert.DeserializeObject<LatestReleaseVersion>(releases);

                    var latest = json.TagName;

                    Version latestVersion = Version.Parse($"{latest}");

                    return currentVersion.CompareTo(latestVersion) <= 0;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        [IsVisibleInDynamoLibrary(false)]
        internal class LatestReleaseVersion
        {
            [JsonProperty("tag_name")]
            public string TagName { get; set; }
        }
    }
}
