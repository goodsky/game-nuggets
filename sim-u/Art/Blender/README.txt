# ART!
While editing the file in Blender keep the default settings (Z-up and 1m units to match 1 grid).
In order for a grid square to count as part of a footprint in sim-u, the center of the grid must be ocluded.

# Exporting to FBX

## Export Script
Run the export.ps1 script. It will automatically export any .blend file from below this directory to the /Assets/Resources/Models directory in the Unity project.

The script has been written so as to not overwrite any file.

## Manually
Export these blender files to .FBX using the following instructions:
  *  Scale: 0.01
  *  Forward: Z Forward
  *  Up: Y Up
  *  Apply Transform

