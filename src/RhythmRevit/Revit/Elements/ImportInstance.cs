using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;
using Autodesk.Revit.DB;
using ProtoCore.AST.AssociativeAST;

namespace Rhythm.Revit.Elements
{
    public class ImportInstance
    {
        private ImportInstance(){}

        public static List<GeometryInstance> GetInstanceBlocks(global::Revit.Elements.ImportInstance importInstance)
        {
            Autodesk.Revit.DB.ImportInstance
                import = importInstance.InternalElement as Autodesk.Revit.DB.ImportInstance;

            var totalTransform = import.GetTotalTransform();

            List<GeometryInstance> blocks = new List<GeometryInstance>();

            foreach (GeometryObject geometryObject in import.get_Geometry(new Options()))
            {
                if (geometryObject is GeometryInstance)
                {
                    GeometryInstance dwgInstance = geometryObject as GeometryInstance;

                    foreach (GeometryObject blockObject in dwgInstance.SymbolGeometry)
                    {
                        if (blockObject is GeometryInstance blockInstance)
                        {
                            var id = blockInstance.GetSymbolGeometryId().SymbolId;

                            var name = import.Document.GetElement(id).Name;

                            Transform transform = blockInstance.Transform;
                            Transform fullTrans = transform * totalTransform; //full transform of both

                            blocks.Add(blockInstance);
                        }
                    }
                }
            }

            return blocks;

        }
    }
}
