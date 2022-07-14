import clr

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import Viewport, XYZ

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


def CenterViewTitle(viewport, offset):
    TransactionManager.Instance.EnsureInTransaction(doc)
    # set label line length to 0 to get approximate length by bounding box
    viewport.LabelOffset = XYZ.Zero
    viewport.LabelLineLength = 0
    labelOutline = viewport.GetLabelOutline()
    width = labelOutline.MaximumPoint.X - labelOutline.MinimumPoint.X
    # set the label line to the length
    viewport.LabelLineLength = width
    
    #viewport box center for X value
    boxCenter = viewport.GetBoxCenter()
    bottomLeft = viewport.GetBoxOutline().MinimumPoint
    #build new location
    calculatedCenter = boxCenter.Subtract(bottomLeft)
    newLocation = XYZ(calculatedCenter.X,offset,0)
    viewport.LabelOffset = newLocation
    TransactionManager.Instance.TransactionTaskDone()
    
    return viewport
#Preparing input from dynamo to revit
items = UnwrapElement(IN[0])
offset = IN[1]
# return the results
if isinstance(IN[0], list): OUT = [CenterViewTitle(x,offset) for x in items]
else: OUT = CenterViewTitle(items,offset)
