using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RhythmCore.SharedProject.BIMTrack
{
    public class BIMTrack
    {
        private BIMTrack()
        {
        }

        public static List<BIMTrackHubUser> ParseHubUser(string json)
        {
            return JsonConvert.DeserializeObject<List<BIMTrackHubUser>>(json);
        }
    }
}
