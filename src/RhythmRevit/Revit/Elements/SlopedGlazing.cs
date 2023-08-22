using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;

namespace Rhythm.Revit.Elements
{
    /// <summary>
    /// Wrapper class for sloped glazing
    /// </summary>
    public class SlopedGlazing
    {
        private SlopedGlazing(){}
        /// <summary>
        /// Set the offset parameters
        /// </summary>
        /// <param name="slopedGlazing"></param>
        /// <param name="angle1">Value between -89 and 89</param>
        /// <param name="angle2">Value between -89 and 89</param>
        /// <param name="offset1">Value to offset side 1 by</param>
        /// <param name="offset2">Value to offset side 2 by</param>
        /// <returns></returns>
        public static global::Revit.Elements.Roof SetAnglesAndOffsets(global::Revit.Elements.Roof slopedGlazing, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] double? angle1, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] double? angle2, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] double? offset1, [DefaultArgument("Rhythm.Utilities.MiscUtils.GetNull()")] double? offset2)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            FootPrintRoof footPrintRoof = slopedGlazing.InternalElement as FootPrintRoof;

            var angle1Parameter = footPrintRoof.get_Parameter(BuiltInParameter.CURTAINGRID_ANGLE_1);
            var offset1Parameter = footPrintRoof.get_Parameter(BuiltInParameter.CURTAINGRID_ORIGIN_1);

            var angle2Parameter = footPrintRoof.get_Parameter(BuiltInParameter.CURTAINGRID_ANGLE_2);
            var offset2Parameter = footPrintRoof.get_Parameter(BuiltInParameter.CURTAINGRID_ORIGIN_2);

            TransactionManager.Instance.EnsureInTransaction(doc);

            if (angle1 != null)
            {
                var angleRad = angle1.Value * global::System.Math.PI / 180;
                angle1Parameter.Set(angleRad);
            }
            if (offset1 != null)
            {
                offset1Parameter.Set(offset1.Value);
            }
            if (angle2 != null)
            {
                var angleRad = angle2.Value * global::System.Math.PI / 180;
                angle2Parameter.Set(angleRad);
            }
            if (offset2 != null)
            {
                offset2Parameter.Set(offset2.Value);
            }

            TransactionManager.Instance.TransactionTaskDone();

            return slopedGlazing;
        }
    }
}
