using System;

namespace Campus.GridTerrain
{
    /// <summary>
    /// Inversion of a submaterial.
    /// Used when setting a submaterial on <see cref="GridMesh"/>.
    /// </summary>
    [Flags]
    public enum SubmaterialInversion
    {
        None = 0,
        InvertX = 1,
        InvertZ = 2,
    }
}
