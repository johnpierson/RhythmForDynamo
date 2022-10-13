import clr
clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

clr.AddReference("RevitNodes")
import Revit
clr.ImportExtensions(Revit.Elements)

def GetNestedFamilies(fi):
	# this allows us to dig down
	internalFamilies = []
	
	# add each family to this list to work down
	internalFamilies.append(fi)
	
	# for us to output (this can be adapted to be nested further)
	nestedList = []
	
	# for us to step through the families
	flag = 0
	
	# fwiw, this could also be an int
	stopString = "keepGoing"

	while stopString == "keepGoing":
		if flag != internalFamilies.Count:
			#go through all the subcomponents (while adding them to the overall list, to then iterate through)
			for subComponent in internalFamilies[flag].GetSubComponentIds():
				
				famDoc = internalFamilies[flag].Document
	
				internalFamily = famDoc.GetElement(subComponent)
				# this enables the further digging
				internalFamilies.append(internalFamily)
				
				nestedList.append(internalFamily.ToDSType(True))
			# increment the flag
			flag = flag + 1
			stopString = "keepGoing"
		else:
			stopString = "stahp it"

	return nestedList
	
def tolist(obj1):
	if hasattr(obj1,"__iter__"): return obj1
	else: return [obj1]

familyInstances = tolist(UnwrapElement(IN[0]))

subLists = []

# iterate through the given instances
for fi in familyInstances:
	subLists.append(GetNestedFamilies(fi))

		
OUT = subLists
