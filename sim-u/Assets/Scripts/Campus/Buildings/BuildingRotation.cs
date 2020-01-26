using Common;
using GameData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Campus
{
    public enum BuildingRotation
    {
        deg0 = 0,
        deg90 = 1,
        deg180 = 2,
        deg270 = 3
    }

    public static class BuildingRotationUtils
    {
        private static readonly Dictionary<BuildingRotation, (int dx, int dz)> _toUnitVector = new Dictionary<BuildingRotation, (int dx, int dz)>()
        {
            { BuildingRotation.deg0, (0, -1) },
            { BuildingRotation.deg90, (-1, 0) },
            { BuildingRotation.deg180, (0, 1) },
            { BuildingRotation.deg270, (1, 0) },
        };

        private static readonly Dictionary<BuildingRotation, Quaternion> _toQuaternion = new Dictionary<BuildingRotation, Quaternion>()
        {
            { BuildingRotation.deg0, Quaternion.Euler(0f, 0f, 0f) },
            { BuildingRotation.deg90, Quaternion.Euler(0f, 90f, 0f) },
            { BuildingRotation.deg180, Quaternion.Euler(0f, 180f, 0f) },
            { BuildingRotation.deg270, Quaternion.Euler(0f, 270f, 0f) },
        };

        public static BuildingRotation Add(BuildingRotation rot1, BuildingRotation rot2)
        {
            int rotInt = (int)rot1 + (int)rot2;
            return (BuildingRotation)(rotInt % 4);
        }

        /// <summary>
        /// Rotates the grid cursor's footprint and returns back the new footprint along with the new footprint origin location.
        /// </summary>
        /// <param name="building">The grid cursosrs.</param>
        /// <param name="location">The location to place the center of the building in the terrain.</param>
        /// <param name="rotation">The rotation to apply to the footprint.</param>
        /// <returns>The new terrain location along with the rotated footprint.</returns>
        public static (Point3 location, GridCursor[,] footprint) RotateGridCursors(GridCursor[,] cursors, Point3 location, BuildingRotation rotation)
        {
            return Rotate(cursors, location, rotation);
        }

        /// <summary>
        /// Rotates the building's footprint and returns back the new footprint along with the new footprint origin location.
        /// </summary>
        /// <param name="building">The building's data.</param>
        /// <param name="location">The location to place the center of the building in the terrain.</param>
        /// <param name="rotation">The rotation to apply to the footprint.</param>
        /// <returns>The new terrain location along with the rotated footprint.</returns>
        public static (Point3 location, bool[,] footprint) RotateFootprint(BuildingData building, Point3 location, BuildingRotation rotation)
        {
            return Rotate(building.Footprint, location, rotation);
        }

        /// <summary>
        /// Rotates the building's entrances.
        /// </summary>
        /// <param name="building">The building's data.</param>
        /// <param name="rotation">The rotation to apply to the building's entrances.</param>
        /// <returns>The rotated building entrances.</returns>
        public static BuildingEntry[] RotateBuildingEntries(BuildingEntry[] buildingEntries, int xWidth, int yWidth, BuildingRotation rotation)
        {
            int entryCount = buildingEntries.Length;
            var rotatedEntries = new BuildingEntry[entryCount];
            for (int i = 0; i < entryCount; ++i)
            {
                BuildingEntry entry = buildingEntries[i];

                switch (rotation)
                {
                    case BuildingRotation.deg0:
                        rotatedEntries[i] = entry;
                        break;

                    case BuildingRotation.deg90:
                        rotatedEntries[i] = new BuildingEntry { X = entry.Y, Y = xWidth - entry.X - 1, Rotation = Add(entry.Rotation, rotation) };
                        break;

                    case BuildingRotation.deg180:
                        rotatedEntries[i] = new BuildingEntry { X = xWidth - entry.X - 1, Y = yWidth - entry.Y - 1, Rotation = Add(entry.Rotation, rotation) };
                        break;

                    case BuildingRotation.deg270:
                        rotatedEntries[i] = new BuildingEntry { X = yWidth - entry.Y - 1, Y = entry.X, Rotation = Add(entry.Rotation, rotation) };
                        break;

                    default:
                        throw new InvalidOperationException($"Unexpected rotation value! Roation = {rotation}");
                }
            }

            return rotatedEntries;
        }

        /// <summary>
        /// Rotates the building's entrances and then calculates the points
        /// on the map that you can enter the building from.
        /// </summary>
        /// <param name="building">The building's data</param>
        /// <param name="footprintOrigin">The origin of the building's footprint.</param>
        /// <param name="rotation">The rotation to apply to the building entrances.</param>
        /// <returns></returns>
        public static Point2[] CalculateBuildingEntries(BuildingData building, Point3 footprintOrigin, BuildingRotation rotation)
        {
            int entryCount = building.BuildingEntries.Length;
            int xWidth = building.Footprint.GetLength(0);
            int yWidth = building.Footprint.GetLength(1);

            BuildingEntry[] entries = RotateBuildingEntries(building.BuildingEntries, xWidth, yWidth, rotation);
            Point2[] entryPoints = new Point2[entryCount];
            for (int i = 0; i < entryCount; ++i)
            {
                BuildingEntry entry = entries[i];
                (int dx, int dz) = _toUnitVector[entry.Rotation];
                entryPoints[i] = new Point2(footprintOrigin.x + entry.X + dx, footprintOrigin.z + entry.Y + dz);
            }

            return entryPoints;
        }

        /// <summary>
        /// Rotates the building and returns back the terrain position along with the world rotation value.
        /// </summary>
        /// <param name="building">The building's data.</param>
        /// <param name="location">The location to place the center of the building in the terrain.</param>
        /// <param name="rotation">The rotation to apply to the footprint.</param>
        /// <returns>The new terrain location along with the world rotation.</returns>
        public static (Point3 location, Quaternion rotation) RotateBuilding(BuildingData building, Point3 location, BuildingRotation rotation)
        {
            int xWidth = building.Footprint.GetLength(0);
            int zWidth = building.Footprint.GetLength(1);

            // Draw out where the origin of the building needs to be after a rotation.
            // This assumes that buildings always start at the model origin.
            // Make sure you're using the export.py script from blender for this to work.
            int xBuildingCenter;
            int zBuildingCenter;
            switch (rotation)
            {
                case BuildingRotation.deg0:
                    xBuildingCenter = (xWidth - 1) / 2;
                    zBuildingCenter = (zWidth - 1) / 2;
                    break;

                case BuildingRotation.deg90:
                    xBuildingCenter = (zWidth - 1) / 2;
                    zBuildingCenter = -(xWidth - 1) / 2 - 1;
                    break;

                case BuildingRotation.deg180:
                    xBuildingCenter = -(xWidth - 1) / 2 - 1;
                    zBuildingCenter = -(zWidth - 1) / 2 - 1;
                    break;

                case BuildingRotation.deg270:
                    xBuildingCenter = -(zWidth - 1) / 2 - 1;
                    zBuildingCenter = (xWidth - 1) / 2;
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected rotation value! Roation = {rotation}");
            }

            return (new Point3(location.x - xBuildingCenter, location.y, location.z - zBuildingCenter), _toQuaternion[rotation]);
        }

        /// <summary>
        /// This could be done more elegantly. But this is my algorithm. I wrote it myself.
        /// </summary>
        private static (Point3 location, T[,] vals) Rotate<T>(T[,] vals, Point3 location, BuildingRotation rotation)
        {
            int xWidth = vals.GetLength(0);
            int zWidth = vals.GetLength(1);

            // Rotate the footprint matrix using a double for loop.
            // Draw out where the origin of the footprint matrix should be.
            // This allows us to use the rotated matrix in the terrain without
            // the terrain manager needing to know about the black magic happening here.
            T[,] newVals;
            int xFootprintCenter;
            int zFootprintCenter;
            switch (rotation)
            {
                case BuildingRotation.deg0:
                    xFootprintCenter = (xWidth - 1) / 2;
                    zFootprintCenter = (zWidth - 1) / 2;
                    newVals = vals;
                    break;

                case BuildingRotation.deg90:
                    xFootprintCenter = (zWidth - 1) / 2;
                    zFootprintCenter = xWidth / 2;

                    newVals = new T[zWidth, xWidth];
                    for (int x = 0; x < xWidth; ++x)
                        for (int z = 0; z < zWidth; ++z)
                            newVals[z, xWidth - x - 1] = vals[x, z];
                    break;

                case BuildingRotation.deg180:
                    xFootprintCenter = xWidth / 2;
                    zFootprintCenter = zWidth / 2;

                    newVals = new T[xWidth, zWidth];
                    for (int x = 0; x < xWidth; ++x)
                        for (int z = 0; z < zWidth; ++z)
                            newVals[xWidth - x - 1, zWidth - z - 1] = vals[x, z];
                    break;

                case BuildingRotation.deg270:
                    xFootprintCenter = zWidth / 2;
                    zFootprintCenter = (xWidth - 1) / 2;

                    newVals = new T[zWidth, xWidth];
                    for (int x = 0; x < xWidth; ++x)
                        for (int z = 0; z < zWidth; ++z)
                            newVals[zWidth - z - 1, x] = vals[x, z];
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected rotation value! Roation = {rotation}");
            }

            return (new Point3(location.x - xFootprintCenter, location.y, location.z - zFootprintCenter), newVals);
        }
    }
}
