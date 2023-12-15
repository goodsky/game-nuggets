import bpy
import os
import sys

# get all arguments after --
arguments = sys.argv[sys.argv.index("--") + 1:]
outputDir = arguments[0]

currentPath = bpy.data.filepath
relativePath = os.path.relpath(currentPath)

filename, extension = os.path.splitext(relativePath)

basename = os.path.basename(relativePath)
sceneName, _ = os.path.splitext(basename)

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
    # Merge all objects into one object before exporting
    for ob in bpy.context.scene.objects:
        if ob.type == 'MESH':
            print('Merging object: ' + ob.name)
            ob.select = True
            bpy.context.scene.objects.active = ob
        else:
            ob.select = False

    bpy.ops.object.join()
    bpy.context.scene.objects.active.name = sceneName
    print("Set object name: " + sceneName)

    # Recenter the thing
    bpy.context.scene.cursor_location = (0.0, 0.0, 0.0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    # Debug - Export the blend
    # originalFilePath, extension = os.path.splitext(currentPath)
    # debugFilePath = originalFilePath + ".debug" + extension
    # bpy.ops.wm.save_as_mainfile(filepath=debugFilePath)
    # print("Saved debug file to \"" + debugFilePath + "\"")

    # Export the FBX
    bpy.ops.export_scene.fbx(filepath=outputPath, axis_forward='Z', axis_up='Y', bake_space_transform=True, global_scale=1, object_types={'MESH'})
    print("*** EXPORTED ***")
