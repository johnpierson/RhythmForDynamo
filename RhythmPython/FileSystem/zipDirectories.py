'''
ZIP Directories
'''

__author__ = 'john pierson - sixtysecondrevit@gmail.com'
__twitter__ = '@60secondrevit'
__github__ = '@johnpierson'
__version__ ='1.0.0'
__license__ = 'GNU General Public License v3.0' 'https://choosealicense.com/licenses/gpl-3.0/'

# import common language runtime and system
import clr
import sys
# Reference the filesystem
clr.AddReference("System.IO.Compression.FileSystem")
# Import the appropriate classes we need
from System.IO.Compression import ZipFile

# custom definition to zip directories given a path
def ZipDirectory(dPath):
	zipName = dPath+".zip"
	try:
		ZipFile.CreateFromDirectory(dPath,zipName)
		return zipName + " was created"
	except Exception, e:
		return e
    
#the directory name or names
directoryName = IN[0]

#return the results
if isinstance(IN[0], list): OUT = [ZipDirectory(x) for x in directoryName]
else: OUT = ZipDirectory(directoryName)
