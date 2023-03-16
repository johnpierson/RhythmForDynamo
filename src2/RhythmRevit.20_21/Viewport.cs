using System;
using RhythmRevit_Compatibility;

namespace RhythmRevit_20_21
{
    public class Viewport : IViewport
    {
        public int[] CompatibleVersions => new[]
        {
            2020,
            2021
        };

        public void SetViewTitleLineLength(object viewport, double length)
        {
            throw new Exception("Sorry viewport APIs are only available in Revit 2022+.");
        }

        public void SetViewTitleLocation(object viewport, object location)
        {
            throw new Exception("Sorry viewport APIs are only available in Revit 2022+.");
        }

        public double GetViewTitleLineLength(object viewport)
        {
            throw new Exception("Sorry viewport APIs are only available in Revit 2022+.");
        }

        public object GetViewTitleLocation(object viewport)
        {
            throw new Exception("Sorry viewport APIs are only available in Revit 2022+.");
        }
    }
}
