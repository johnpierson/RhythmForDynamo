using System;
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
