import clr

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

clr.AddReference('RevitNodes')
import Revit
clr.ImportExtensions(Revit.GeometryConversion)

clr.AddReference('RevitServices')
import RevitServices
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument
uidoc=DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument

#function to set LeaderElbow for spatial element tags. Would probably work for regular tags, too.
def SetSpatialTagElbow(spatialTag,point):
	TransactionManager.Instance.EnsureInTransaction(doc)
	spatialTag.LeaderElbow = point.ToRevitType(True) #cast point to revit type
	TransactionManager.Instance.TransactionTaskDone()
	return spatialTag

#Preparing input from dynamo to revit
spatialTags = UnwrapElement(IN[0])
locationPoints = UnwrapElement(IN[1])

# return the results
if isinstance(IN[0], list): OUT = [SetSpatialTagElbow(t,p) for t,p in zip(spatialTags,locationPoints)]
else: OUT = SetSpatialTagElbow(spatialTags,locationPoints)
