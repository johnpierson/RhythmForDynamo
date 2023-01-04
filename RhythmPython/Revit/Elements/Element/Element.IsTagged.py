__author__ = 'john pierson - designtechunraveled@gmail.com'
__twitter__ = '@johntpierson'
__github__ = '@johnpierson'
__version__ ='1.0.0'
__license__ = 'BSD 3-Clause - https://tldrlegal.com/license/bsd-3-clause-license-(revised)'
__madefor__ = 'Dynamo 2.16 / CPython3'

import clr

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *
from Autodesk.Revit.DB.Structure import *

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

def IsTagged(element):
    tags = []
    elementFilter = ElementClassFilter(IndependentTag)
    
    # find the tags dependent on the element (Note: this will show all tags dependent on the element. Eg. Select a wall, this will return a door tag if there is a door hosted on the wall with a tag.)
    allDependentTags = UnwrapElement(element).GetDependentElements(elementFilter)
    
    # no tags at all, return false
    if allDependentTags.count == 0:
        return False
        
    # here is where we check if the element itself is tagged (if it had dependent tags)
    for t in allDependentTags:
        taggedItems = doc.GetElement(t).GetTaggedElementIds()
        for i in taggedItems:
            if i.HostElementId == element.Id:
                return True
    # if we found no tags on the element return false
    return False

#Preparing input from dynamo to revit
elements = UnwrapElement(IN[0])

if isinstance(IN[0], list): OUT = [IsTagged(x) for x in elements]
else: OUT = IsTagged(elements)
