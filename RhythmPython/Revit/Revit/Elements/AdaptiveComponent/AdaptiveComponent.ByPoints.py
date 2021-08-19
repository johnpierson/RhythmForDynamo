import clr

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *
from Autodesk.Revit.DB.Structure import *

clr.AddReference('RevitAPIUI')
from Autodesk.Revit.UI import *

clr.AddReference('RevitNodes')
import Revit
clr.ImportExtensions(Revit.GeometryConversion)
clr.ImportExtensions(Revit.Elements)

clr.AddReference('RevitServices')
import RevitServices
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument
uidoc=DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument


def PlaceComponent(document,symbol,points):
	instance = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(document, symbol);

	placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(instance)

	for i,j in zip(placePointIds,points):
		point = document.GetElement(i)
		point.Position = j.ToRevitType(False)
	return instance
		
pointList = IN[0]
symbol = UnwrapElement(IN[1])
toggle = IN[2]

newFamilyInstances = []
TransactionManager.Instance.EnsureInTransaction(doc)

if IN[2] == True:
	for i in pointList:		
		newFam = PlaceComponent(doc,symbol,i)
		newFamilyInstances.append(newFam)
# Assign your output to the OUT variable.
OUT = newFamilyInstances