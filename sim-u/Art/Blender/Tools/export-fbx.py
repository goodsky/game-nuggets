import bpy
import os
import sys

# get all arguments after --
arguments = sys.argv[sys.argv.index("--") + 1:]
outputDir = arguments[0]

currentPath = bpy.data.filepath
relativePath = os.path.relpath(currentPath)

filename, extension = os.path.splitext(relativePath)

outputfile = filename + ".fbx"
outputPath = os.path.join(outputDir, outputfile)

print("Current Path: " + currentPath + ".")
print("Relative Path: " + bpy.data.filepath + ".")
print("File Name: " + filename + ".")
print("File Ext: " + extension + ".")
print("Output Path: " + outputPath + ".")

if (os.path.exists(outputPath)):
    print("*** FILE ALREADY EXISTS - SKIPPING ***")
else:
    # Export the FBX
    bpy.ops.export_scene.fbx(filepath=outputPath, axis_forward='Z', axis_up='Y', bake_space_transform=True, global_scale=1, object_types={'MESH'})
    print("*** EXPORTED ***")
