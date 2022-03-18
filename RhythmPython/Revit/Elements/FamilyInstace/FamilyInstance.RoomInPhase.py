__author__ = 'john pierson - sixtysecondrevit@gmail.com'
__twitter__ = '@60secondrevit'
__github__ = '@johnpierson'
__version__ ='1.0.0'
__license__ = 'BSD 3-Clause - https://tldrlegal.com/license/bsd-3-clause-license-(revised)'

# import common language runtime 
import clr

# Import RevitAPI
clr.AddReference("RevitAPI")
# import all classes from Revit DB
from Autodesk.Revit.DB import *

# import Revit dynamo libraries
clr.AddReference("RevitNodes")
import Revit
clr.ImportExtensions(Revit.Elements)
clr.ImportExtensions(Revit.GeometryConversion)

# custom definition for getting room that works with single items or lists
def GetRoomInPhase(item,phase):
	return item.Room[phase]

# the family instances
items = UnwrapElement(IN[0])
phase = UnwrapElement(IN[1])

# return the results
if isinstance(IN[0], list): OUT = [GetRoomInPhase(x,phase) for x in items]
else: OUT = GetRoomInPhase(items,phase)
