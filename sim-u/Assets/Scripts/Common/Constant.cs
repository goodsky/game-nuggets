﻿namespace Common
{
    /// <summary>
    /// What's that?
    /// You have a hard-coded value but don't want to config it?
    /// You're in the right place.
    /// </summary>
    public static class Constant
    {
        public const string MenuSceneName = "menu";
        public const string GameSceneName = "sim-u";

        /// <summary>
        /// The size of a grid on the map in unity units.
        /// </summary>
        public const float GridSize = 1.0f;

        /// <summary>
        /// The size of a step vertically on the grid in unity units.
        /// </summary>
        public const float GridStepSize = 0.25f;

        /// <summary>
        /// The size of submaterials on the terrain sprite sheet.
        /// </summary>
        public const int SubmaterialGridSize = 64;

        /// <summary>
        /// Epsilon for uv mapping.
        /// </summary>
        public const float uvEpsilon = 1.25e-3f;
    }
}
