import clr
clr.AddReference("RevitAPI")

#import the specific Revit API sections we need, you can also just do import* if you want
from Autodesk.Revit.DB import FilteredElementCollector,BuiltInCategory,Viewport, View

clr.AddReference('RevitServices')
import RevitServices
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument

viewports = FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Viewports).WhereElementIsNotElementType().ToElements()

views = []

for viewport in viewports:
	viewId = viewport.ViewId
	views.append(doc.GetElement(viewId))


OUT = viewports,views
