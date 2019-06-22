using Assets.Scripts.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Models
{

    /// <summary>
    /// Holds vertices, height and position for building. Building is
    /// generated using EarCut algorithm.
    /// </summary>
    public class BuildingHolder
    {
        public Vector3 center { get; set; }
        public float Rotation { get; set; }
        public bool IsModelCreated;
        private List<Vector3> _verts;
        private float m_Height;
        private bool m_IsVolumePolygon;
        private Vector3[] m_Vertices;
        private int[] m_Indices;


        /// <summary>
        /// Creates vertices and indices for building.
        /// </summary>
        public void Initialize()
        {
            Triangulator tris = new Triangulator(_verts.Select(x => x.xz()).ToArray());
            // Top vertices
            List<Vector3> vertices = _verts.Select(x => new Vector3(x.x, x.y + m_Height, x.z)).ToList();
            List<int> indices;
            if (m_IsVolumePolygon)
            {
                indices = tris.Triangulate().ToList();
            }
            else
            {
                indices = new List<int>();
            }
            // Clockwise - visible, CCW - invisible
            int n = vertices.Count;
            for (int index = 0; index < n; index++)
            {
                Vector3 v = vertices[index];
                // Bottom vertices - go 2 meters under ground - solid base needed
                vertices.Add(new Vector3(v.x, v.y - 2.0f - m_Height, v.z));
            }

            // Add side faces
            for (int i = 0; i < n - 1; i++)
            {
                indices.Add(i);
                indices.Add(i + n);
                indices.Add(i + n + 1);

                indices.Add(i);
                indices.Add(i + n + 1);
                indices.Add(i + 1);
            }

            // Wrap the last side without modulo
            indices.Add(n - 1);
            indices.Add(n);
            indices.Add(0);

            indices.Add(n - 1);
            indices.Add(n + n - 1);
            indices.Add(n);

            // Duplicate vertices for edges - needed for hard edges and correct lightning
            List<Vector3> correctVertices = new List<Vector3>();
            List<int> correctIndices = new List<int>();
            for (int i = 0; i < indices.Count; ++i)
            {
                correctVertices.Add(vertices[indices[i]]);
                correctIndices.Add(i);
            }

            if (!m_IsVolumePolygon)
            {
                // It is some kind of wall, duplicate faces
                // correction for z-fighting might be needed
                int c = correctVertices.Count;
                for (int i = 0; i < c; ++i)
                {
                    correctIndices.Add(c - i - 1);
                }
            }

            m_Vertices = correctVertices.ToArray();
            m_Indices = correctIndices.ToArray();
        }

        /// <summary>
        /// Initialize building holder.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="verts"></param>
        /// <param name="height"></param>
        /// <param name="volumePolygon"></param>
        public BuildingHolder(Vector3 c, List<Vector3> verts, float height, bool volumePolygon)
        {
            IsModelCreated = false;
            center = c;
            _verts = verts;
            m_Height = height;
            m_IsVolumePolygon = volumePolygon;
        }

        /// <summary>
        /// Creates model from already created vertices and indices. Returns null if
        /// these were not yet created.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="name"></param>
        /// <returns>Returns null if vertices were not yet created, or generated mesh.</returns>
        public GameObject CreateModel(WorldScripts.WorldTile tile, string name)
        {
            if (IsModelCreated)
                return null;

            BuildingPolygon m = new GameObject(name).AddComponent<BuildingPolygon>();
            m.Initialize(m_Vertices, m_Indices);
            m.gameObject.isStatic = true;
            m.name = name;
            m.transform.parent = tile.transform;
            Ray ray = new Ray();
            // Consider highest point available - 8900 meters
            ray.origin = new Vector3(center.x, 10000.0f, center.z);
            // Terrain should be below - for Earth at least
            ray.direction = new Vector3(0, -1, 0);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                center = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                m.gameObject.transform.position = center;
                m.GetComponent<MeshRenderer>().sharedMaterial = GameObject.FindObjectOfType<World>().buildingMaterial;
                IsModelCreated = true;
                return m.gameObject;
            }
            else
            {
                // Center outside, at least 1 corner should be in
                foreach (Vector3 v in _verts)
                {
                    // Consider highest point available - 8900 meters
                    ray.origin = new Vector3(v.x + center.x, 10000.0f, v.z + center.z);
                    if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
                    {
                        center = new Vector3(hit.point.x - v.x, hit.point.y, hit.point.z - v.z);
                        m.gameObject.transform.position = center;
                        m.GetComponent<MeshRenderer>().sharedMaterial = GameObject.FindObjectOfType<World>().buildingMaterial;
                        IsModelCreated = true;
                        return m.gameObject;
                    }
                }

                IsModelCreated = true;
                GameObject.Destroy(m.gameObject);
                return null;
            }
        }

    }
}
