using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Models
{
    /// <summary>
    /// Component for creation of mesh from vertices and indices.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    class BuildingPolygon : MonoBehaviour
    {

        public void Initialize(Vector3[] verts, int[] indices)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = indices;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().mesh = mesh;
            gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_BUILDINGS);
        }

    }
}
