using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Models
{

    /// <summary>
    /// Component for drawing OpenGL lines. Needs to be attached to scene camera.
    /// Lines are rendered in screen space or world space. Rendering is done in
    /// OnPostRender. Each line is specified by unique integer id and stored in
    /// dictionaries.
    /// </summary>
    public class DrawLines : MonoBehaviour
    {

        /// <summary>
        /// Dictionary for lines in world space.
        /// </summary>
        private Dictionary<int, List<Vector3>> m_Lines = new Dictionary<int, List<Vector3>>();
        /// <summary>
        /// Dictionary for lines materials.
        /// </summary>
        private Dictionary<int, Material> m_LinesColor = new Dictionary<int, Material>();
        /// <summary>
        /// Dictionary for lines in screen space.
        /// </summary>
        private Dictionary<int, List<Vector3>> m_ScreenLines = new Dictionary<int, List<Vector3>>();
        /// <summary>
        /// Default line material.
        /// </summary>
        public Material LineMaterial;
        private int m_Ids = 0;


        /// <summary>
        /// Removes line from world space.
        /// </summary>
        /// <param name="id">Id of line in world space</param>
        public void RemoveLine(int id)
        {
            m_Lines.Remove(id);
        }

        /// <summary>
        /// Removes line from screen space.
        /// </summary>
        /// <param name="id">Id of line in screen space</param>
        public void RemoveScreenLine(int id)
        {
            m_ScreenLines.Remove(id);
        }

        /// <summary>
        /// Generates new id for line.
        /// </summary>
        /// <returns>Returns new id for line.</returns>
        public int RegisterLine()
        {
            return m_Ids++;
        }

        /// <summary>
        /// Updates line in screen spaces. Doesn't need existing positions.
        /// </summary>
        /// <param name="id">id of line</param>
        /// <param name="line">line segment points</param>
        public void UpdateScreenLine(int id, List<Vector3> line)
        {
            m_ScreenLines[id] = line;
        }

        /// <summary>
        /// Updates line in screen spaces. Takes into account existing positions.
        /// </summary>
        /// <param name="id">id of line</param>
        /// <param name="line">line segment points</param>
        public void UpdateScreenLine(int id, params Vector3[] points)
        {
            for (int i = 0; i < Math.Min(points.Length, m_ScreenLines[id].Count); ++i)
            {
                m_ScreenLines[id][i] = points[i];
            }
        }

        /// <summary>
        /// Sets color for line.
        /// </summary>
        /// <param name="id">id of line</param>
        /// <param name="c">color for this line</param>
        public void SetLineColor(int id, Color c)
        {
            Material m = Instantiate(LineMaterial);
            m.color = c;
            m_LinesColor.Add(id, m);
        }

        /// <summary>
        /// Updates line in world spaces. Doesn't need existing positions.
        /// </summary>
        /// <param name="id">id of line</param>
        /// <param name="line">line segment points</param>
        public void UpdateLine(int id, List<Vector3> line)
        {
            m_Lines[id] = line;
        }

        /// <summary>
        /// Updates line in world spaces. Takes into account existing positions.
        /// </summary>
        /// <param name="id">id of line</param>
        /// <param name="line">line segment points</param>
        public void UpdateLine(int id, params Vector3[] points)
        {
            for (int i = 0; i < Math.Min(points.Length, m_Lines[id].Count); ++i)
            {
                m_Lines[id][i] = points[i];
            }
        }

        void OnPostRender()
        {
            // Render world space
            GL.PushMatrix();
            //LineMaterial.SetPass(0);
            GL.LoadIdentity();
            GL.MultMatrix(transform.parent.localToWorldMatrix);
            GL.MultMatrix(Camera.main.worldToCameraMatrix);
            foreach (int i in m_Lines.Keys)
            {
                List<Vector3> line = m_Lines[i];
                // Get Color if set
                Material m;
                if (m_LinesColor.ContainsKey(i))
                    m = m_LinesColor[i];
                else
                    m = LineMaterial;

                m.SetPass(0);

                GL.Begin(GL.LINES);
                Vector3 prev = line[0];
                foreach (Vector3 point in line)
                {
                    GL.Color(m.color);
                    GL.Vertex(prev);
                    GL.Color(m.color);
                    GL.Vertex(point);
                    prev = point;
                }
                GL.End();
            }
            GL.PopMatrix();

            // Render screen space
            LineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();
            foreach (List<Vector3> line in m_ScreenLines.Values)
            {
                GL.Begin(GL.LINES);
                Vector3 prev = line[0];
                foreach (Vector3 point in line)
                {
                    GL.Color(LineMaterial.color);
                    GL.Vertex(prev);
                    GL.Color(LineMaterial.color);
                    GL.Vertex(point);
                    prev = point;
                }
                GL.End();
            }
            GL.PopMatrix();
        }

    }
}
