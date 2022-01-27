//using Autodesk.DesignScript.Runtime;
//using System.Collections.Generic;
//using Autodesk.DesignScript.Geometry;
//using Autodesk.Revit.DB;
//using Revit.GeometryConversion;
//using RevitServices.Persistence;
//using RevitServices.Transactions;

//namespace Rhythm.Revit.Views
//{
//    /// <summary>
//    /// Wrapper class for 3d views.
//    /// </summary>
//    public class View3D
//    {
//        private View3D()
//        { }

//        /// <summary>
//        /// This node will set the given 3d view's section box.
//        /// </summary>
//        /// <param name="view3D">The 3D view to set the section box for.</param>
//        /// <param name="bBox">The bounding box to use.</param>
//        /// <returns name="success">The views that worked.</returns>
//        /// <returns name="failed">The views that failed.</returns>
//        /// <search>
//        /// view, dependent,rhythm
//        /// </search>
//        //this is the node View.ConvertToIndependent
//        [MultiReturn(new[] { "success", "failed" })]
//        public static Dictionary<string, object> SetSectionBox(global::Revit.Elements.Element view3D, BoundingBox bBox)
//        {
//            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
//            Autodesk.Revit.DB.View3D internalView = (Autodesk.Revit.DB.View3D)view3D.InternalElement;

//            BoundingBoxXYZ revitBoxXyz = new BoundingBoxXYZ
//            {
//                Min = bBox.MinPoint.ToRevitType(),
//                Max = bBox.MaxPoint.ToRevitType()
//            };


//            List<object> success = new List<object>();
//            List<object> fail = new List<object>();


//            TransactionManager.Instance.EnsureInTransaction(doc);
//            try
//            {
//                internalView.SetSectionBox(revitBoxXyz);
//                success.Add(view3D);
//            }
//            catch
//            {
//                fail.Add(view3D);
//            }

//            TransactionManager.Instance.TransactionTaskDone();
//            var outInfo = new Dictionary<string, object>
//            {
//                {"success", success},
//                {"failed", fail }
//            };
//            return outInfo;
//        }
//    }
//}
