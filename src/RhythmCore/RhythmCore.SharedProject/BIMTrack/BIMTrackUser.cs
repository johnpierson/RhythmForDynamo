using ProtoCore;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Autodesk.DesignScript.Runtime;

namespace RhythmCore.SharedProject.BIMTrack
{
    [IsVisibleInDynamoLibrary(false)]
    public class BIMTrackUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class BIMTrackHubUser
    {
        public BIMTrackUser User { get; set; }
        public string Role { get; set; }
        public DateTime LastWebLogin { get; set; }
        public DateTime LastAddinLogin { get; set; }
        public DateTime LastHubProjectActivity { get; set; }
        public bool IsActivated { get; set; }
    }
}
