using UnityEngine;
using UnityEngine.Rendering;

namespace GridTerrain
{
    /// <summary>
    /// Fits like a glove.
    /// A custom cursor that floats over the terrain.
    /// </summary>
    public class GridCursor : MonoBehaviour
    {
        // Height above the mesh to float.
        private static readonly float FloatAmount = 0.05f;

        /// <summary>The grid terrain to curse over.</summary>
        public IGridTerrain Terrain;

        /// <summary>The material for this cursor.</summary>
        public Material CursorMaterial;

        private Mesh _mesh;
        private MeshRenderer _renderer;
        private Vector3[] _vertices;
        private Vector2[] _uv;
        private int[] _triangles;

        /// <summary>Gets a value indicated if the cursor game object is active.</summary>
        public bool IsActive { get { return gameObject.activeSelf; } }

        /// <summary>
        /// Static factory to correctly create a cursor game object.
        /// </summary>
        /// <param name="terrain">The terrain to be a cursor over.</param>
        /// <param name="material">The material to use on the cursor.</param>
        /// <param name="parent">The parent game object of the cursor.</param>
        /// <returns>The newly minted cursor.</returns>
        public static GridCursor Create(IGridTerrain terrain, Material material, Transform parent)
        {
            GameObject cursorObject = new GameObject("GridCursor");
            cursorObject.transform.parent = parent;

            var cursor = cursorObject.AddComponent<GridCursor>();
            cursor.Terrain = terrain;
            cursor.CursorMaterial = material;
            cursor.Initialize();

            return cursor;
        }

        /// <summary>
        /// A required initialize step to setup the cursor.
        /// </summary>
        protected void Initialize()
        {
            var filter = gameObject.AddComponent<MeshFilter>();

            _mesh = filter.mesh = new Mesh();
            _renderer = gameObject.AddComponent<MeshRenderer>();
            _renderer.receiveShadows = false;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;

            _vertices = new Vector3[]
            {
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(Terrain.Size, 0.0f, 0.0f),
                new Vector3(Terrain.Size, 0.0f, Terrain.Size),
                new Vector3(0.0f, 0.0f, Terrain.Size),
                new Vector3(Terrain.Size / 2.0f, 0.0f, Terrain.Size / 2.0f)
            };

            _uv = new Vector2[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(0.5f, 0.5f),
            };

            _triangles = new int[]
            {
                0, 4, 1,
                1, 4, 2,
                2, 4, 3,
                3, 4, 0,
            };

            _mesh.name = "grid-cursor";
            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;

            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();

            _renderer.material = CursorMaterial;
        }

        /// <summary>
        /// Activate the Unity game object.
        /// </summary>
        public void Activate()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivate the Unity game object.
        /// </summary>
        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Place the cursor at the requested grid position.
        /// </summary>
        /// <param name="x">X coordinate on the grid.</param>
        /// <param name="z">Z coordinate on the grid.</param>
        public void Place(int x, int z)
        {
            transform.position = new Vector3(x * Terrain.Size, 0.0f /* Should be terrain height? */, z * Terrain.Size);

            _vertices[0].y = Terrain.GetPointWorldHeight(x, z) + FloatAmount;
            _vertices[1].y = Terrain.GetPointWorldHeight(x + 1, z) + FloatAmount;
            _vertices[2].y = Terrain.GetPointWorldHeight(x + 1, z + 1) + FloatAmount;
            _vertices[3].y = Terrain.GetPointWorldHeight(x, z + 1) + FloatAmount;
            _vertices[4].y = Terrain.GetSquareWorldHeight(x, z) + FloatAmount;

            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;

            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
        }
    }
}
