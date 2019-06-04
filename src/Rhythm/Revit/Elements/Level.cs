using System;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for levels. We make this plural because ya know Dynamo has a crashing issue with namespaces pre 2.1.0
    /// </summary>
    public class Levels
    {
        private Levels()
        {
        }

        /// <summary>
        /// Check to see if the level has a view created for it.😎 
        /// </summary>
        /// <param name="level">The level to check.</param>
        /// <returns name="result">Does it have a level?</returns>
        /// <search>
        /// Level.HasView
        /// </search>
        public static bool HasView(global::Revit.Elements.Level level)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector fec = new FilteredElementCollector(doc);

            //nested this in a try catch because I want false if this fails for any reason, instead of nulls. 
            try
            {   
                return fec.OfClass(typeof(ViewPlan))
                    .Cast<ViewPlan>().Where(v => !v.IsTemplate).Any(v => v.GenLevel.Id.IntegerValue.Equals(level.Id));
            }
            catch (Exception e )
            {
                return false;
            }
        }

    }
}
