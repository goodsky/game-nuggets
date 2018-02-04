# Unity Terrain Construction Engine

> A "tycoon" grid style terrain editor that allows for runtime modification of the terrain

## Getting Started

This tool requires a Unity GameObject with the Terrain and TerrainCollider behaviors configured in specific ways. Ensure the Terrain Settings follow these rules:

* Heightmap Resolution must correspond to the Terrain Width and Terrain Height
  * For Example: If you want 10x10 grids in world units you can set the Heightmap Resolution to 33 (effectively 32x32) and then the Terrain Width and Height to 320x320.
* Terrain Height must be a power of 2 (32, 64, 128, 256) and the Grid Height Step must divide into it evenly
  * For Example: If you want 20 world unit steps in your terrain that are each 4 units high you can set the Terrain Height to 64 and the GridHeightSize to 4.0.

Once the terrain is configured correctly, simply add an EditableTerrain behavior on it and go wild!

