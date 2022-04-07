__author__ = 'john pierson - sixtysecondrevit@gmail.com'
__twitter__ = '@60secondrevit'
__github__ = '@johnpierson'
__version__ ='1.0.0'
__license__ = 'BSD 3-Clause - https://tldrlegal.com/license/bsd-3-clause-license-(revised)'
__madefor__ = 'Dynamo 2.13 / CPython3'

# Load the Python Standard and DesignScript Libraries
import clr
clr.AddReference('ProtoGeometry')
from Autodesk.DesignScript.Geometry import *

# import Revit API
clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import GraphicsStyle,ElementId,Element,BuiltInParameter

# import Dynamo Revit Services
clr.AddReference('RevitServices')
import RevitServices
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument

# custom def for setting workset
def SetWorkset(element,workset):
    TransactionManager.Instance.EnsureInTransaction(doc)
    param = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
    
    # set based on type
    try:
        if workset.GetType() is GraphicsStyle:
            param.Set(workset.Id)
        elif workset.GetType() is ElementId:
            param.Set(workset)
    # if we were given an int, try to set that way
    except:
         param.Set(ElementId(workset))
   
    TransactionManager.Instance.TransactionTaskDone()
    return element

# inputs
elements = UnwrapElement(IN[0])
workset = UnwrapElement(IN[1])

# return the results
if isinstance(IN[0], list): OUT = [SetWorkset(x,workset) for x in elements]
else: OUT = SetWorkset(elements,workset)
