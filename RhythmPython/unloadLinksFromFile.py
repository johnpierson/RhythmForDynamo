'''
UNLOAD LINKS IN FILE BY PATH
USING TRANSMISSION DATA
'''

__author__ = 'john pierson - sixtysecondrevit@gmail.com'
__twitter__ = '@60secondrevit'
__github__ = '@johnpierson'
__version__ ='1.0.0'
__license__ = 'GNU General Public License v3.0' 'https://choosealicense.com/licenses/gpl-3.0/'

# import common language runtime 
import clr
# Import RevitAPI
clr.AddReference('RevitAPI')
# Import the appropriate classes we need
from Autodesk.Revit.DB import TransmissionData,FilePath,ExternalFileReferenceType

# custom definition to unload links using Transmission Data
def UnloadRevitLinks(path):
    try:
        mPath = FilePath(path)
        tData = TransmissionData.ReadTransmissionData(mPath)
        #collect all (immediate) external references in the model
        externalReferences = tData.GetAllExternalFileReferenceIds()
    
        for refId in externalReferences:
            extRef = tData.GetLastSavedReferenceData(refId)
            if extRef.ExternalFileReferenceType  == ExternalFileReferenceType .RevitLink:
                tData.SetDesiredReferenceData(refId, extRef.GetPath(), extRef.PathType, False)
        #make sure the IsTransmitted property is set 
        tData.IsTransmitted = True
        #modified transmission data must be saved back to the model
        TransmissionData.WriteTransmissionData(mPath, tData);
        return path
    except:
        return "failed to transmit"
    
#the model paths
modelPaths = UnwrapElement(IN[0])

#return the results
if isinstance(IN[0], list): OUT = [UnloadRevitLinks(x) for x in modelPaths]
else: OUT = UnloadRevitLinks(modelPaths)