using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Rhythm.Revit.Views
{
    public class ViewSchedule
    {
        private ViewSchedule(){}

        public static Autodesk.Revit.DB.TableData TableData(global::Revit.Elements.Views.ScheduleView scheduleView)
        {
            Autodesk.Revit.DB.ViewSchedule
                viewSchedule = scheduleView.InternalElement as Autodesk.Revit.DB.ViewSchedule;

            return viewSchedule.GetTableData();
        }
    }

    public class TableData
    {
        private TableData(){}

        public static Autodesk.Revit.DB.TableSectionData TableSectionData(Autodesk.Revit.DB.TableData tableData, string sectionType = "body")
        {
            string lowerSectionType = sectionType.ToLower();

            SectionType actualSectionType = SectionType.None;
            switch (lowerSectionType)
            {
                case "body":
                    actualSectionType = SectionType.Body;
                    break;
                case "footer":
                    actualSectionType = SectionType.Footer;
                    break;
                case "header":
                    actualSectionType = SectionType.Header;
                    break;
                case "summary":
                    actualSectionType = SectionType.Summary;
                    break;
                case "none":
                    actualSectionType = SectionType.None;
                    break;
                default:
                    actualSectionType = SectionType.Body;
                    break;
            }

            return tableData.GetSectionData(actualSectionType);
        }
    }

    public class TableSectionData
    {
        private TableSectionData(){}

        public static void SetColumnWidth(Autodesk.Revit.DB.TableSectionData tableSectionData, int columnNumber, double width)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            tableSectionData.SetColumnWidth(columnNumber, width);
            TransactionManager.Instance.TransactionTaskDone();
        }

        public static double GetColumnWidth(Autodesk.Revit.DB.TableSectionData tableSectionData, int columnNumber)
        {
            return tableSectionData.GetColumnWidth(columnNumber);
        }
    }
}
