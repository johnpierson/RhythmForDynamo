
# import common language runtime 
import clr
# Import RevitAPI
clr.AddReference('RevitAPI')
# Import the appropriate classes we need
from Autodesk.Revit.DB import *

def IsFamDoc(doc):
    return doc.IsFamilyDocument()
    
docs = IN[0]

#return the results
if isinstance(IN[0], list): OUT = [IsFamDoc(x) for x in docs]
else: OUT = IsFamDoc(docs)