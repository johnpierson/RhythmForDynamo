using System;
using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Rhythm.Revit.Views
{
    /// <summary>
    /// Wrapper class for view.
    /// </summary>
    public class View
    {
        public static bool IsFilterEnabled(global::Revit.Elements.Views.View view,global::Revit.Elements.Element viewFilter)
        {
            try
            {
                Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
                return internalView.GetIsFilterEnabled(viewFilter.InternalElement.Id);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public static void ToggleFilterInView(global::Revit.Elements.Views.View view, global::Revit.Elements.Element viewFilter, bool toggle)
        {
            try
            {
                Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
                internalView.SetIsFilterEnabled(viewFilter.InternalElement.Id,toggle);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static List<global::Revit.Elements.Element> GetOrderedFilters(global::Revit.Elements.Views.View view)
        {
            Autodesk.Revit.DB.View internalView = view.InternalElement as Autodesk.Revit.DB.View;
            Document doc = internalView.Document;
            
            return internalView.GetOrderedFilters().Select(f => doc.GetElement(f).ToDSType(true)).ToList();
        }

        //public static object GetLoadedAssemblies(string one, string two)
        //{
        //    return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("OrchidRB"));
        //}
    }
}
