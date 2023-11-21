﻿using Autodesk.DesignScript.Runtime;

namespace Rhythm.ShortestWalk.Geometry
{
    [IsVisibleInDynamoLibrary(false)]
    public enum SearchMode
    {
        CurveLength = 1,
        LinearDistance = 3,
        Links = 4,
    }
}