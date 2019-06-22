using System;
using System.Collections.Generic;
using System.Collections;
using Assets.Scripts.Utility;
using UnityEngine;


namespace Assets.Scripts.Models
{

    /// <summary>
    /// Creates mesh for fly path given points on it. Fly path can be displayed
    /// as points, simple boxes, boxes ended with cone or points which are connected
    /// with smaller ones.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FlyPathPolygon : MonoBehaviour
    {

        public enum FlyPathType
        {
            POINTS = 0,
            RULER = 1,
            POINTS_FILLED = 2,
            BOX = 3,
        };

        private static float OFFSET = 0.25f;

        // Some specific lists used in creation
        private List<Vector3> m_Positions;
        private List<int> m_Points;
        private List<VertexRing> m_RingVertices;
        private Mesh m_Mesh;


        /// <summary>
        /// Filters positions which are on the same line or too close to each other.
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="filterLines"></param>
        /// <returns></returns>
        private static List<Vector3> FilterPositions(List<Vector3> positions, bool filterLines)
        {
            List<Vector3> filteredPositions = new List<Vector3>();
            // while 3 points on line, make the positions the same
            Vector3 a = positions[0];
            Vector3 b = positions[1];
            Vector3 c = positions[2];

            int next = 3;
            filteredPositions.Add(a);
            while (next < positions.Count)
            {
                while (Vector3.Distance(a, b) < 0.5f)
                {
                    b = c;
                    if (++next >= positions.Count)
                        break;

                    c = positions[next];
                }

                if (filterLines)
                {
                    while (Mathf.Abs(Vector3.Dot((c - b).normalized, (b - a).normalized) - 1.0f) < 0.3f)
                    {
                        b = c;
                        if (++next >= positions.Count)
                            break;

                        c = positions[next];
                    }
                }

                filteredPositions.Add(b);

                a = b;
                b = c;
                if (++next >= positions.Count)
                {
                    filteredPositions.Add(b);
                    break;
                }
                c = positions[next];
            }

            return filteredPositions;
        }

        
        public IEnumerator Initialize(List<Vector3> positions)
        {
            m_RingVertices = new List<VertexRing>();
            m_Positions = positions;
            m_Mesh = CreateMesh(m_Positions);
            m_Mesh.MarkDynamic();
            yield return m_Mesh;
            GetComponent<MeshFilter>().mesh = m_Mesh;
        }

        public void InitializeDynamic()
        {
            m_RingVertices = new List<VertexRing>();
            m_Points = new List<int>();
            m_Positions = new List<Vector3>();
            m_Mesh = new Mesh();
            m_Mesh.MarkDynamic();
            GetComponent<MeshFilter>().mesh = m_Mesh;
        }

        #region MESH GENERATION
        private void AddRing(VertexRing ring, Vector3 up, Vector3 right, Vector3 position)
        {
            ring.m_RingVertices.Add(position - right + up);
            ring.m_RingVertices.Add(position + right + up);
            ring.m_RingVertices.Add(position + right - up);
            ring.m_RingVertices.Add(position - right - up);
            ring.m_RingVertices.Add(position - right + up);
        }

        private void GetVectors(Vector3 dir, out Vector3 right, out Vector3 up)
        {
            // Detect if vertical or horizontal segment
            bool isVertical = IsVertical(dir);
            if (isVertical)
            {
                // if is vertical use square in XZ
                right = new Vector3(dir.z, 0.0f, -dir.y).normalized;
                if (!IsRight(dir, right))
                {
                    right *= -1.0f;
                }
                up = Vector3.Cross(dir, right).normalized;
            }
            else
            {
                // if is horizontal use square along Y
                right = new Vector3(dir.z, 0.0f, -dir.x).normalized;
                if (!IsRight(dir, right))
                {
                    right *= -1.0f;
                }
                up = Vector3.up;
            }

            up *= Settings.FLY_PATH_HEIGHT;
            right *= Settings.FLY_PATH_WIDTH;
        }

        private void AddSegmentBox(Vector3 endPosition, bool drawLines, Color color, bool dropVisibility)
        {
            if (m_Positions.Count > Settings.FLY_PATH_SEGMENT_LIMIT)
                RemoveFirstBox();

            if (m_Positions.Count == 0)
            {
                m_Positions.Add(endPosition);
                return;
            }

            Vector3 startPosition = m_Positions[m_Positions.Count - 1];
            Vector3 dir = (endPosition - startPosition).normalized;

            // pop and rebuild last segment
            //RemoveLastSegment();

            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            VertexRing endRing = new VertexRing();
            VertexRing startRing = new VertexRing();
            Vector3 right;
            Vector3 up;

            GetVectors(dir, out right, out up);

            AddRing(startRing, up, right, startPosition);
            AddRing(endRing, up, right, endPosition);

            // Add box
            for (int i = 0; i < startRing.Count - 1; ++i)
            {
                // First triangle
                verts.Add(startRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i + 1]);
                // Second triangle
                verts.Add(startRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i + 1]);
                verts.Add(startRing.m_RingVertices[i + 1]);
            }

            // Matching triangle
            if (m_RingVertices.Count > 0)
            {
                VertexRing previousRing = m_RingVertices[m_RingVertices.Count - 1];
                Vector3 previousDir = startPosition - m_Positions[m_Positions.Count - 2];
                bool isRight = IsRight(previousDir, dir);
                //if (isVertical)
                //    isRight = !isRight;

                if (isRight)
                {
                    // Filling triangles
                    verts.Add(previousRing.m_RingVertices[0]);
                    verts.Add(startRing.m_RingVertices[0]);
                    verts.Add(startPosition + up);

                    verts.Add(startPosition - up);
                    verts.Add(startRing.m_RingVertices[3]);
                    verts.Add(previousRing.m_RingVertices[3]);

                    // Filling square
                    verts.Add(startRing.m_RingVertices[3]);
                    verts.Add(startRing.m_RingVertices[0]);
                    verts.Add(previousRing.m_RingVertices[0]);

                    verts.Add(previousRing.m_RingVertices[0]);
                    verts.Add(previousRing.m_RingVertices[3]);
                    verts.Add(startRing.m_RingVertices[3]);
                }
                else
                {
                    // Filling triangles
                    verts.Add(startPosition + up);
                    verts.Add(startRing.m_RingVertices[1]);
                    verts.Add(previousRing.m_RingVertices[1]);

                    verts.Add(previousRing.m_RingVertices[2]);
                    verts.Add(startRing.m_RingVertices[2]);
                    verts.Add(startPosition - up);

                    // Filling square
                    verts.Add(previousRing.m_RingVertices[1]);
                    verts.Add(startRing.m_RingVertices[1]);
                    verts.Add(startRing.m_RingVertices[2]);

                    verts.Add(startRing.m_RingVertices[2]);
                    verts.Add(previousRing.m_RingVertices[2]);
                    verts.Add(previousRing.m_RingVertices[1]);
                }
            }

            // Add to lists
            m_RingVertices.Add(startRing);
            m_RingVertices.Add(endRing);
            m_Positions.Add(endPosition);

            int[] triangles = new int[verts.Count];
            Color[] colors = new Color[verts.Count];
            if (dropVisibility)
            {
                int end = 36 * Settings.FLY_PATH_SEGMENT_LIMIT / 2;
                float increment = color.a * 2.0f / (float)Settings.FLY_PATH_SEGMENT_LIMIT;
                Color c = new Color(color.r, color.g, color.b, increment);
                for (int i = 0; i < triangles.Length; ++i)
                {
                    if (i <= end)
                    {
                        for (int j = 0; j < 36 && i < triangles.Length; ++i, ++j)
                        {
                            triangles[i] = i;
                            colors[i] = c;
                        }
                        float a = Mathf.Min(c.a + increment, color.a);
                        c = new Color(c.r, c.g, c.b, a);
                        --i;
                    }
                    else
                    {
                        triangles[i] = i;
                        colors[i] = c;
                    }
                }
            }
            else
            {
                for (int i = 0; i < triangles.Length; ++i)
                {
                    triangles[i] = i;
                    colors[i] = color;
                }
            }

            m_Mesh.vertices = verts.ToArray();
            m_Mesh.triangles = triangles;
            m_Mesh.colors = colors;
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        private void AddConeWithoutCap(Vector3 position, Vector3 up, Vector3 forward, Vector3 right, List<Vector3> verts, List<Color> normals)
        {
            verts.Add(position + up);
            verts.Add(position + forward);
            verts.Add(position + right);
            normals.Add(new Color(up.x, up.y, up.z, 1.0f));
            normals.Add(new Color(forward.x, forward.y, forward.z, 1.0f));
            normals.Add(new Color(right.x, right.y, right.z, 1.0f));

            verts.Add(position + right);
            verts.Add(position + forward);
            verts.Add(position - up);
            normals.Add(new Color(right.x, right.y, right.z, 1.0f));
            normals.Add(new Color(forward.x, forward.y, forward.z, 1.0f));
            normals.Add(new Color(-up.x, -up.y, -up.z, 1.0f));

            verts.Add(position - up);
            verts.Add(position + forward);
            verts.Add(position - right);
            normals.Add(new Color(-up.x, -up.y, -up.z, 1.0f));
            normals.Add(new Color(forward.x, forward.y, forward.z, 1.0f));
            normals.Add(new Color(-right.x, -right.y, -right.z, 1.0f));

            verts.Add(position - right);
            verts.Add(position + forward);
            verts.Add(position + up);
            normals.Add(new Color(-right.x, -right.y, -right.z, 1.0f));
            normals.Add(new Color(forward.x, forward.y, forward.z, 1.0f));
            normals.Add(new Color(up.x, up.y, up.z, 1.0f));
        }

        private void AddPoint(Vector3 position, Vector3 up, Vector3 forward, Vector3 right, List<Vector3> verts, List<Color> normals)
        {
            // front
            AddConeWithoutCap(position, up, forward, right, verts, normals);
            // back
            AddConeWithoutCap(position, up, -forward, -right, verts, normals);

            //verts.Add(position + right);
            //verts.Add(position - forward);
            //verts.Add(position + up);

            //verts.Add(position - up);
            //verts.Add(position - forward);
            //verts.Add(position + right);

            //verts.Add(position - right);
            //verts.Add(position - forward);
            //verts.Add(position - up);

            //verts.Add(position + up);
            //verts.Add(position - forward);
            //verts.Add(position - right);
        }

        private void AddSegmentRuler(Vector3 endPosition, bool drawLines, Color color, bool dropVisibility)
        {
            if (m_Positions.Count > Settings.FLY_PATH_SEGMENT_LIMIT)
                RemoveFirstRuler();

            if (m_Positions.Count == 0)
            {
                m_Positions.Add(endPosition);
                return;
            }

            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            List<Color> norms = new List<Color>();
            norms.AddRange(m_Mesh.colors);
            Vector3 startPosition = m_Positions[m_Positions.Count - 1];
            Vector3 dir = (endPosition - startPosition).normalized;
            Vector3 right;
            Vector3 up;
            GetVectors(dir, out right, out up);
            dir = dir * 1.5f;
            AddConeWithoutCap(startPosition + dir, up - right, -dir, -right - up, verts, norms);

            VertexRing endRing = new VertexRing();
            VertexRing startRing = new VertexRing();
            AddRing(startRing, up, right, startPosition + dir);
            AddRing(endRing, up, right, endPosition - dir);
            m_RingVertices.Add(startRing);
            m_RingVertices.Add(endRing);

            // Add box
            for (int i = 0; i < startRing.Count - 1; ++i)
            {
                // First triangle
                verts.Add(startRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i + 1]);
                norms.Add((startRing.m_RingVertices[i] - startPosition).rgba(1));
                norms.Add((endRing.m_RingVertices[i] - endPosition).rgba(1));
                norms.Add((endRing.m_RingVertices[i + 1] - endPosition).rgba(1));
                // Second triangle
                verts.Add(startRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i + 1]);
                verts.Add(startRing.m_RingVertices[i + 1]);
                norms.Add((startRing.m_RingVertices[i] - startPosition).rgba(1));
                norms.Add((endRing.m_RingVertices[i + 1] - endPosition).rgba(1));
                norms.Add((startRing.m_RingVertices[i + 1] - startPosition).rgba(1));
            }

            AddConeWithoutCap(endPosition - dir, up + right, dir, right - up, verts, norms);

            int[] triangles = new int[verts.Count];
            Color[] colors = new Color[verts.Count];
            if (dropVisibility)
            {
                int end = 48 * Settings.FLY_PATH_SEGMENT_LIMIT / 2;
                float increment = color.a * 2.0f / (float)Settings.FLY_PATH_SEGMENT_LIMIT;
                Color c = new Color(color.r, color.g, color.b, increment);
                for (int i = 0; i < triangles.Length; ++i)
                {
                    if (i <= end)
                    {
                        for (int j = 0; j < 48 && i < triangles.Length; ++i, ++j)
                        {
                            triangles[i] = i;
                            colors[i] = c;
                        }
                        float a = Mathf.Min(c.a + increment, color.a);
                        c = new Color(norms[i].r, norms[i].g, norms[i].b, a);
                        --i;
                    }
                    else
                    {
                        triangles[i] = i;
                        colors[i] = c;
                        colors[i].a = color.a;
                    }
                }
            }
            else
            {
                for (int i = 0; i < triangles.Length; ++i)
                {
                    triangles[i] = i;
                    colors[i] = color;
                }
            }

            m_Mesh.vertices = verts.ToArray();
            m_Mesh.triangles = triangles;
            m_Mesh.colors = colors;
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();

            m_Positions.Add(endPosition);
        }

        private void AddSegmentPointsFilled(Vector3 endPosition, bool drawLines, Color color, bool dropVisibility)
        {
            if (m_Positions.Count > Settings.FLY_PATH_SEGMENT_LIMIT)
                RemoveFirstPointFilled();

            Vector3 startPosition;
            Vector3 dir;
            Vector3 right;
            Vector3 up;

            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            List<Color> norms = new List<Color>();
            norms.AddRange(m_Mesh.colors);
            int count = 1;
            if (m_Positions.Count > 0)
            {
                startPosition = m_Positions[m_Positions.Count - 1];
                dir = (endPosition - startPosition).normalized;
                GetVectors(dir, out right, out up);
                float dist = Vector3.Distance(endPosition - dir * 1.0f, startPosition + dir * 2.0f);
                float offset = 0.0f;
                while (offset <= dist)
                {
                    ++count;
                    AddPoint(startPosition + dir * (2.0f + offset), up / 2.0f, dir * 0.5f, right / 2.0f, verts, norms);
                    offset += 2.0f;
                }
                AddPoint(endPosition, up, dir * 0.5f, right, verts, norms);
            }
            else
            {
                AddPoint(endPosition, Vector3.up, Vector3.forward, Vector3.right, verts, norms);
            }

            m_Points.Add(count);

            int[] triangles = new int[verts.Count];
            Color[] colors = new Color[verts.Count];
            for (int i = 0; i < triangles.Length; ++i)
            {
                triangles[i] = i;
                colors[i] = new Color(norms[i].r, norms[i].g, norms[i].b, 1.0f);
            }

            m_Mesh.vertices = verts.ToArray();
            m_Mesh.triangles = triangles;
            m_Mesh.colors = colors;
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();

            m_Positions.Add(endPosition);
        }

        private void AddSegmentPoints(Vector3 endPosition, bool drawLines, Color color, bool dropVisibility)
        {
            if (m_Positions.Count > Settings.FLY_PATH_SEGMENT_LIMIT)
                RemoveFirstPoint();

            Vector3 startPosition;
            Vector3 dir;
            Vector3 right;
            Vector3 up;

            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            List<Color> norms = new List<Color>();
            norms.AddRange(m_Mesh.colors);
            if (m_Positions.Count > 0)
            {
                startPosition = m_Positions[m_Positions.Count - 1];
                dir = (endPosition - startPosition).normalized;
                // Remove and rebuild last
                //RemoveLastPoint();
                GetVectors(dir, out right, out up);
                AddPoint(endPosition, up, dir * 1.0f, right, verts, norms);
            }
            else
            {
                AddPoint(endPosition, Vector3.up * Settings.FLY_PATH_HEIGHT, Vector3.forward, Vector3.right, verts, norms);
            }

            int[] triangles = new int[verts.Count];
            Color[] colors = new Color[verts.Count];
            if (dropVisibility)
            {
                int end = triangles.Length - 24 * Settings.FLY_PATH_SEGMENT_LIMIT / 2;
                float increment = color.a * 2.0f / (float)Settings.FLY_PATH_SEGMENT_LIMIT;
                Color c = new Color(color.r, color.g, color.b, increment);
                float alpha = increment;
                for (int i = 0; i < triangles.Length; ++i)
                {
                    if (i <= end)
                    {
                        for (int j = 0; j < 24 && i < triangles.Length; ++i, ++j)
                        {
                            triangles[i] = i;
                            colors[i] = norms[i];
                            colors[i].a = alpha;
                        }
                        alpha = Mathf.Min(alpha + increment, color.a);
                        --i;
                    }
                    else
                    {
                        triangles[i] = i;
                        colors[i] = norms[i];
                        colors[i].a = color.a;
                    }
                }
            }
            else
            {
                for (int i = 0; i < triangles.Length; ++i)
                {
                    triangles[i] = i;
                    colors[i] = norms[i];
                }
            }

            m_Mesh.vertices = verts.ToArray();
            m_Mesh.triangles = triangles;
            m_Mesh.colors = colors;
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();

            m_Positions.Add(endPosition);
        }

        public void AddSegment(Vector3 endPosition, bool drawLines, Color color, bool dropVisibility, FlyPathType type)
        {
            switch(type)
            {
                case FlyPathType.POINTS:
                    AddSegmentPoints(endPosition, drawLines, color, dropVisibility);
                    break;
                case FlyPathType.RULER:
                    AddSegmentRuler(endPosition, drawLines, color, dropVisibility);
                    break;
                case FlyPathType.POINTS_FILLED:
                    AddSegmentPointsFilled(endPosition, drawLines, color, dropVisibility);
                    break;
                case FlyPathType.BOX:
                default:
                    AddSegmentBox(endPosition, drawLines, color, dropVisibility);
                    break;
            }

            //if (drawLines)
            //    m_LineRenderer.UpdateLine(m_Id, m_Positions.Select(x => x + transform.parent.position).ToList());
        }
        #endregion

        #region ORIENTATION CHECKS
        private bool IsRight(Vector3 dir, Vector3 x)
        {
            return Mathf.Sign(dir.z * x.x - dir.x * x.z) >= 0;
        }

        private bool IsLeft(Vector3 a, Vector3 b, Vector3 c)
        {
            return Mathf.Sign((a.x - c.x) * (b.z - c.z) - (a.z - c.z) * (b.x - c.x)) < 0.0f;
        }

        private bool IsVertical(Vector3 dir)
        {
            // Atan 6/1 is slightly above 80 degrees ~ 80.537
            return dir.x * dir.x + dir.z * dir.z < 6 * dir.y * dir.y;
        }
        #endregion

        #region UPDATING SEGMENTS
        public void UpdateFirst(Vector3 pos, FlyPathType type)
        {
            switch (type)
            {
                case FlyPathType.POINTS:
                    UpdateFirstPoint(pos);
                    break;
                case FlyPathType.RULER:
                    UpdateFirstRuler(pos);
                    break;
                case FlyPathType.POINTS_FILLED:
                    UpdateFirstPointsFilled(pos);
                    break;
                case FlyPathType.BOX:
                default:
                    UpdateFirstBox(pos);
                    break;
            }

            m_Positions[0] = pos;
        }

        private void UpdateFirstRuler(Vector3 pos)
        {
            if (m_RingVertices.Count < 2)
                return;
            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            // Update first cone
            for (int i = 0; i < 12; ++i)
            {
                verts[i] = verts[i] - m_Positions[0] + pos;
            }
            VertexRing startRing = m_RingVertices[0];
            for (int i = 12; i < (startRing.Count - 1) * 6 + 12; i += 6)
            {
                // First triangle
                verts[i] = verts[i] - m_Positions[0] + pos;
                //verts.Add(startRing.m_RingVertices[i]);
                //verts.Add(endRing.m_RingVertices[i]);
                //verts.Add(endRing.m_RingVertices[i + 1]);

                // Second triangle
                verts[i + 3] = verts[i + 3] - m_Positions[0] + pos;
                verts[i + 5] = verts[i + 5] - m_Positions[0] + pos;
                //verts.Add(startRing.m_RingVertices[i]);
                //verts.Add(endRing.m_RingVertices[i + 1]);
                //verts.Add(startRing.m_RingVertices[i + 1]);
            }

            m_Mesh.vertices = verts.ToArray();
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        private void UpdateFirstPointsFilled(Vector3 pos)
        {
            if (m_Positions.Count < 2)
                return;

            List<Vector3> verts = new List<Vector3>();
            List<Color> norms = new List<Color>();
            // Completely rebuild segment
            Vector3 startPosition = m_Positions[0];
            Vector3 dir = (m_Positions[1] - startPosition).normalized;
            Vector3 right, up;
            GetVectors(dir, out right, out up);
            float dist = Vector3.Distance(m_Positions[1] - dir * 1.0f, startPosition + dir * 2.0f);
            float offset = 0.0f;
            int count = 1;
            while (offset <= dist)
            {
                ++count;
                AddPoint(startPosition + dir * (2.0f + offset), up / 2.0f, dir * 0.5f, right / 2.0f, verts, norms);
                offset += 2.0f;
            }
            AddPoint(m_Positions[1], up, dir * 0.5f, right, verts, norms);
            for (int i = 24 * m_Points[0]; i < m_Mesh.vertexCount; ++i)
            {
                verts.Add(m_Mesh.vertices[i]);
                norms.Add(m_Mesh.colors[i]);
            }
            int[] newIndices = new int[verts.Count];
            for (int i = 0; i < newIndices.Length; ++i)
            {
                newIndices[i] = i;
            }
            m_Points[0] = count;
            m_Mesh.vertices = verts.ToArray();
            m_Mesh.colors = norms.ToArray();
            m_Mesh.triangles = newIndices;
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        private void UpdateFirstPoint(Vector3 pos)
        {
            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            // Change first position
            for (int i = 0; i < 24; ++i)
            {
                verts[i] = verts[i] - m_Positions[0] + pos;
            }
            m_Mesh.vertices = verts.ToArray();
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        private void UpdateFirstBox(Vector3 pos)
        {
            if (m_RingVertices.Count == 0)
                return;

            // Create new ring
            VertexRing startRing = m_RingVertices[0];
            startRing.m_RingVertices.Clear();
            // Get vectors for start ring
            Vector3 dir = m_Positions[1] - pos;
            dir.Normalize();
            Vector3 right, up;
            GetVectors(dir, out right, out up);
            // Update start ring
            AddRing(startRing, up, right, pos);
            VertexRing endRing = m_RingVertices[1];

            for (int i = 0; i < startRing.Count; ++i)
                startRing.m_RingVertices[i] += pos - m_Positions[0];

            List<Vector3> verts = new List<Vector3>();
            for (int i = 0; i < startRing.Count - 1; ++i)
            {
                // First triangle
                verts.Add(startRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i + 1]);
                // Second triangle
                verts.Add(startRing.m_RingVertices[i]);
                verts.Add(endRing.m_RingVertices[i + 1]);
                verts.Add(startRing.m_RingVertices[i + 1]);
            }

            // Add new vertices
            Vector3[] newVerts = m_Mesh.vertices;
            for (int i = 0; i < verts.Count; ++i)
                newVerts[i] = verts[i];

            // Update mesh
            m_Mesh.vertices = newVerts;
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }
        #endregion

        #region REMOVING SEGMENTS
        public void RemoveFirstSegment(int type)
        {
            switch (type)
            {
                case 0:
                    RemoveFirstPoint();
                    break;
                case 1:
                    RemoveFirstRuler();
                    break;
                case 2:
                    RemoveFirstPointFilled();
                    break;
                case 3:
                default:
                    RemoveFirstBox();
                    break;
            }

        }

        private void RemoveFirstBox()
        {
            if (m_RingVertices.Count == 0)
                return;
            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            verts.RemoveRange(0, Math.Min(36, verts.Count));
            // Remove corresponding position
            m_Positions.RemoveAt(0);
            // Remove first two rings
            m_RingVertices.RemoveAt(0);
            m_RingVertices.RemoveAt(0);
            int[] newIndices = new int[verts.Count];

            Color[] colors = new Color[verts.Count];
            Color rgb = new Color(0.2f, 0.2f, 0.8f, 1.0f);
            for (int i = 0; i < newIndices.Length; ++i)
            {
                newIndices[i] = i;
                colors[i] = rgb;
            }
            m_Mesh.triangles = newIndices;
            m_Mesh.vertices = verts.ToArray();
            m_Mesh.colors = colors;
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        public void RemoveLastSegment(int vCount)
        {
            if (m_RingVertices.Count == 0)
                return;
            Vector3[] verts = m_Mesh.vertices;
            Vector3[] newVerts = new Vector3[verts.Length - vCount];
            //m_RingVertexCounts.RemoveAt(m_RingVertexCounts.Count - 1);
            // Remove last ring
            //m_RingVertices.RemoveAt(m_RingVertices.Count - 1);
            int[] indices = m_Mesh.triangles;
            int[] newIndices = new int[newVerts.Length];

            for (int i = 0; i < newVerts.Length; ++i)
            {
                newVerts[i] = verts[i];
                newIndices[i] = indices[i];
            }

            m_Mesh.vertices = newVerts;
            m_Mesh.triangles = newIndices;
        }

        private void RemoveFirstRuler()
        {
            if (m_RingVertices.Count < 2)
                return;
            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            verts.RemoveRange(0, Math.Min(48, verts.Count));
            // Remove corresponding position
            m_Positions.RemoveAt(0);
            // Remove first two rings
            m_RingVertices.RemoveAt(0);
            m_RingVertices.RemoveAt(0);
            int[] newIndices = new int[verts.Count];

            for (int i = 0; i < newIndices.Length; ++i)
            {
                newIndices[i] = i;
            }

            m_Mesh.triangles = newIndices;
            m_Mesh.vertices = verts.ToArray();
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        private void RemoveLastRuler()
        {
            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            verts.RemoveRange(Math.Min(verts.Count - 49, 0), Math.Min(48, verts.Count));
            // Remove corresponding position
            m_Positions.RemoveAt(m_Positions.Count - 1);
            int[] newIndices = new int[verts.Count];

            for (int i = 0; i < newIndices.Length; ++i)
            {
                newIndices[i] = i;
            }

            m_Mesh.triangles = newIndices;
            m_Mesh.vertices = verts.ToArray();
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        private void RemoveFirstPointFilled()
        {
            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            int count = m_Points[0];
            verts.RemoveRange(0, Math.Min(24 * count, verts.Count));
            // Remove corresponding position
            m_Positions.RemoveAt(0);
            m_Points.RemoveAt(0);
            int[] newIndices = new int[verts.Count];

            for (int i = 0; i < newIndices.Length; ++i)
            {
                newIndices[i] = i;
            }

            m_Mesh.triangles = newIndices;
            m_Mesh.vertices = verts.ToArray();
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        private void RemoveFirstPoint()
        {
            List<Vector3> verts = new List<Vector3>();
            List<Color> colors = new List<Color>();
            verts.AddRange(m_Mesh.vertices);
            verts.RemoveRange(0, Math.Min(24, verts.Count));
            colors.AddRange(m_Mesh.colors);
            colors.RemoveRange(0, Math.Min(24, colors.Count));
            // Remove corresponding position
            m_Positions.RemoveAt(0);
            int[] newIndices = new int[verts.Count];

            for (int i = 0; i < newIndices.Length; ++i)
            {
                newIndices[i] = i;
            }

            m_Mesh.triangles = newIndices;
            m_Mesh.vertices = verts.ToArray();
            m_Mesh.colors = colors.ToArray();
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }

        private void RemoveLastPoint()
        {
            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(m_Mesh.vertices);
            verts.RemoveRange(Math.Min(verts.Count - 25, 0), Math.Min(24, verts.Count));
            // Remove corresponding position
            m_Positions.RemoveAt(m_Positions.Count - 1);
            int[] newIndices = new int[verts.Count];

            for (int i = 0; i < newIndices.Length; ++i)
            {
                newIndices[i] = i;
            }

            m_Mesh.triangles = newIndices;
            m_Mesh.vertices = verts.ToArray();
            m_Mesh.RecalculateBounds();
            m_Mesh.RecalculateNormals();
        }
        #endregion

        private Mesh CreateMesh(List<Vector3> positions)
        {
            Mesh mesh = new Mesh();
            //// No path
            //if (positions.Count < 2)
                return mesh;
        }


        class VertexRing
        {
            internal List<Vector3> m_RingVertices = new List<Vector3>();

            internal int Count
            {
                get
                {
                    return m_RingVertices.Count;
                }
            }
        }

    }
}
