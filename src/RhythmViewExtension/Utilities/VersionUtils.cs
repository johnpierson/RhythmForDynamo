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

        private const string DynamoRevitMappingUrl = "https://raw.githubusercontent.com/johnpierson/RhythmForDynamo/refs/heads/master/deploy/dynamo_to_revit_mapping.json";

        private static Dictionary<string, List<int>> _mappingCache;

        /// <summary>
        /// Gets the Dynamo to Revit mapping dictionary (cached, loaded from GitHub)
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        private static Dictionary<string, List<int>> GetDynamoRevitMapping()
        {
            if (_mappingCache == null)
            {
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers.Add("User-Agent", "RhythmViewExtension");
                        string json = webClient.DownloadString(DynamoRevitMappingUrl);
                        _mappingCache = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(json);
                    }
                }
                catch (Exception e)
                {
                    string messssss = e.Message;
                    // Return empty dictionary if fetch fails
                    _mappingCache = new Dictionary<string, List<int>>();
                }
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
                
                return new List<int>(){24};
            }
            catch (Exception)
            {
                return new List<int>() { 24 };
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

