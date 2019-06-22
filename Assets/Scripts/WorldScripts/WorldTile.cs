using Assets.Scripts.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.WorldScripts
{

    /// <summary>
    /// Component representing tile in scene. It holds 4 created terrains, so they 
    /// can be removed.
    /// </summary>
    public class WorldTile : MonoBehaviour
    {

        /// <summary>
        /// Data for this tile.
        /// </summary>
        private TileData m_TileData;
        private World w;
        public List<GameObject> Terrains = new List<GameObject>();
        public const int HeightmapResolution = 257;
        private bool m_IsDestroyed = false;
        // Vertices, uvs and indices which are always same.
        private static Vector3[] m_Vertices;
        private static Vector2[] m_UVs;
        private static int[] m_Triangles;

        // Varibles that are same for each instance
        static int vertCount = Settings.TEST_MESH_SIZE;
        static float vertDiv = vertCount - 1;
        static bool StaticInit = false;

        /// <summary>
        /// Initializes vertices, uvs and indices that are the same for each
        /// terrain. Only done once.
        /// </summary>
        private void StaticInitialization()
        {
            StaticInit = true;
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            for (int i = 0; i < vertCount; ++i)
            {
                for (int j = 0; j < vertCount; ++j)
                {
                    verts.Add(new Vector3(i / vertDiv, 0, j / vertDiv));
                    uvs.Add(new Vector2(i / vertDiv, j / vertDiv));
                }
            }
            List<int> tris = new List<int>();
            for (int i = 0; i < vertCount-1; ++i)
            {
                for (int j = 0; j < vertCount - 1; ++j)
                {
                    // add square
                    // first tri
                    tris.Add(i * vertCount + j);
                    tris.Add((i + 1) * vertCount + j + 1);
                    tris.Add((i + 1) * vertCount + j);
                    // second tri
                    tris.Add(i * vertCount + j);
                    tris.Add(i * vertCount + j + 1);
                    tris.Add((i + 1) * vertCount + j + 1);
                }
            }

            m_UVs = uvs.ToArray();
            m_Triangles = tris.ToArray();
            m_Vertices = verts.ToArray();
        }

        public TileData Data
        {
            set
            {
                m_TileData = value;
            }

            get
            {
                return m_TileData;
            }
        }

        public bool Destroyed
        {
            get
            {
                return m_IsDestroyed;
            }
        }

        public WorldTile()
        {
        }

        /// <summary>
        /// Creates terrain using Mesh.
        /// </summary>
        /// <param name="baseTex">satellite texture</param>
        /// <param name="Heightmap">heightmap for tile</param>
        /// <param name="size">size of tile</param>
        /// <param name="offset">position of tile</param>
        /// <param name="w">world instance</param>
        public void CreateTerrain(Texture2D baseTex, float[,] Heightmap, float size, Vector2i offset, World w)
        {
            if (!StaticInit)
                StaticInitialization();

            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            GameObject o = Instantiate(w.TerrainPrefab);
            Mesh m = new Mesh();
            int mul = 128 / (vertCount - 1);
            // Create new vertices, since height will be different
            Vector3[] vertices = new Vector3[vertCount* vertCount];
            for (int i = 0; i < vertCount; ++i)
            {
                for (int j = 0; j < vertCount; ++j)
                {
                    vertices[i * vertCount + j] = (new Vector3(m_Vertices[i * vertCount + j].x * size, Heightmap[j* mul, i* mul] * Constants.MAXIMUM_HEIGHT, m_Vertices[i * vertCount + j].z * size));
                }
            }
            // assign values
            m.vertices = vertices;
            m.uv = m_UVs;
            m.triangles = m_Triangles;
            m.RecalculateBounds();
            m.RecalculateNormals();
            o.GetComponent<MeshFilter>().mesh = m;
            o.GetComponent<MeshCollider>().sharedMesh = m;
            MeshRenderer mr = o.GetComponent<MeshRenderer>();
            mr.material = w.TerrainMaterial;
            mr.material.mainTexture = baseTex;
            mr.material.SetFloat("_OffsetX", size * offset.x);
            mr.material.SetFloat("_OffsetY", -size * offset.y);
            o.name = offset.ToString();
            // Move it in -z-axis since it is Mercator's y-axis
            o.transform.position = new Vector3(size * offset.x, 0, -size * offset.y);
            o.layer = LayerMask.NameToLayer("Terrain");
            Terrains.Add(o);
            Test.Log("Terrain=" + s.ElapsedMilliseconds);
            s.Stop();
        }

        /// <summary>
        /// Creates Unity terrain. Usually not used.
        /// </summary>
        /// <param name="baseTex"></param>
        /// <param name="Heightmap"></param>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        /// <param name="subdiv"></param>
        /// <param name="w"></param>
        public void CreateTerrain(Texture2D baseTex, float[,] Heightmap, float size, Vector2i offset, int subdiv, World w)
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();

            TerrainData data = new TerrainData();
            data.heightmapResolution = HeightmapResolution / (subdiv + 1) + 1;
            data.alphamapResolution = 16;
            data.baseMapResolution = 256;
            data.SetHeightsDelayLOD(0, 0, Heightmap);
            data.size = new Vector3(size, Constants.MAXIMUM_HEIGHT, size);
            // Apply texture
            SplatPrototype[] tex = new SplatPrototype[1];
            tex[0] = new SplatPrototype();
            tex[0].texture = baseTex;    //Sets the texture
            tex[0].tileSize = new Vector2(size, size);    //Sets the size of the texture
            data.splatPrototypes = tex;

            GameObject newTerrainGameObject = Terrain.CreateTerrainGameObject(data);
            newTerrainGameObject.name = offset.ToString();
            // Move it in -z-axis since it is Mercator's y-axis
            newTerrainGameObject.transform.position = new Vector3(size * offset.x, 0, -size * offset.y);
            newTerrainGameObject.layer = LayerMask.NameToLayer("Terrain");

            Terrain terrain = newTerrainGameObject.GetComponent<Terrain>();
            terrain.materialType = Terrain.MaterialType.BuiltInLegacyDiffuse;
            terrain.basemapDistance = 0;
            //terrain.heightmapMaximumLOD = 0;
            terrain.drawTreesAndFoliage = false;
            terrain.heightmapPixelError = 5;
            terrain.treeMaximumFullLODCount = 0;
            terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            terrain.ApplyDelayedHeightmapModification();
            terrain.Flush();
            Terrains.Add(terrain.gameObject);
            Test.Log("Terrain=" + s.ElapsedMilliseconds);
            s.Stop();
        }

        /// <summary>
        /// Cleans all data and destroys all game objects.
        /// </summary>
        public void Clean()
        {
            if (m_TileData == null || m_IsDestroyed)
                return;

            //Debug.Log("Cleaning tile" + m_TileData.Position);
            foreach (GameObject o in Terrains)
            {
                Terrain t = o.GetComponent<Terrain>();
                if (t)
                {
                    Destroy(t.terrainData.splatPrototypes[0].texture);
                    t.terrainData.splatPrototypes[0].texture = null;
                    t.terrainData.splatPrototypes = null;
                    Destroy(t.terrainData);
                    t.terrainData = null;
                }
                DestroyImmediate(o);
                //w.TerrainPool.Offer(t);
            }
            Terrains.Clear();
            for (int i = 0; i < m_TileData.Textures.Length; ++i)
            {
                Destroy(m_TileData.Textures[i]);
                m_TileData.Textures[i] = null;
            }
            for (int i = 0; i < transform.childCount; ++i)
            {
                Destroy(transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh);
            }
            Destroy(this.gameObject);
            m_TileData = null;
            m_IsDestroyed = true;
            // Unload assets from memory
            Resources.UnloadUnusedAssets();
        }

    }
}
