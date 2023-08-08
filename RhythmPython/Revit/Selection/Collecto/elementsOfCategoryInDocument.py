__author__ = 'john pierson - sup@johnp.lol'
__twitter__ = '@johntpierson'
__github__ = '@johnpierson'
__version__='1.0.0'
__pythonversion__ ='IronPython 2.7'
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


# the family instances
linkDoc = UnwrapElement(IN[0])
category = UnwrapElement(IN[1])

filter = ElementCategoryFilter(category.Id);

elements = FilteredElementCollector(linkDoc).WhereElementIsNotElementType().WherePasses(filter).ToElements()


# return the results
OUT = elements
