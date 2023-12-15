using System;

namespace Campus.GridTerrain
{
    /// <summary>
    /// Stores pointers into the mesh to locate a grid square.
    /// </summary>
    [Serializable]
    public class GridData
    {
        /// <summary>
        /// Index to the start of the grid in the <see cref="GridMesh"/> vertices array.
        /// NB: This property should not be serialized so it can be set by the runtime.
        /// </summary>
        [NonSerialized]
        public int VertexIndex;

        /// <summary>Current submaterial.</summary>
        public int SubmaterialIndex;

        /// <summary>Rotation to the current submaterial.</summary>
        public SubmaterialRotation Rotation;

        /// <summary>Inversion on the current submaterial.s</summary>
        public SubmaterialInversion Inversion;
    }
}
