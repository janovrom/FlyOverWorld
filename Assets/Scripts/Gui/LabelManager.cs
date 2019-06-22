using Assets.Scripts.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Manages labels on screen. Based on force-based graph layout using 
    /// repelling and attracting forces. Labels are marked as on screen/off screen
    /// so that only labels on screen are positioned. When object is off screen, 
    /// its label is held on edge of screen based on its position.
    /// </summary>
    class LabelManager : MonoBehaviour
    {

        private static LabelManager m_Instance;
        // List of object on/off screen
        private List<Label> m_PickablesOnScreen = new List<Label>();
        private List<Label> m_PickablesOffScreen = new List<Label>();
        /// <summary>
        /// Computed forces
        /// </summary>
        private List<Vector3> m_Forces = new List<Vector3>();
        /// <summary>
        /// Previous locations of endpoints to which points are drawn.
        /// </summary>
        private List<Vector3> m_PreviousLocations = new List<Vector3>();
        /// <summary>
        /// Current positions of endpoints, so gui update can be prevented.
        /// </summary>
        private List<Vector3> m_Endpoints = new List<Vector3>();
        // public properties
        public Label LabelPrefab;
        public RectTransform LabelPanel;
        // Modifiers for each applied force.
        /// <summary>
        /// Modifier for attractive force between endpoint and its anchor.
        /// </summary>
        public Vector2 W1 = new Vector2(0.17f, 0.27f);
        /// <summary>
        /// Modifier for repulsive force between endpoints.
        /// </summary>
        public Vector2 W2 = new Vector2(20000.0f, 12000.0f);
        /// <summary>
        /// Modifier for repulsive force between enpoint and each anchor.
        /// </summary>
        public Vector2 W3 = new Vector2(6000.0f, 3959.5f);
        /// <summary>
        /// Modifier for attracting for to center of screen.
        /// </summary>
        public Vector2 W4 = new Vector2(0.86f, 0.86f);
        /// <summary>
        /// Modifier for attracting force for endpoint's previous location.
        /// </summary>
        public Vector2 W5 = new Vector2(0.83f, 0.83f);
        /// <summary>
        /// Modifier for attracting force to mouse position.
        /// </summary>
        public Vector2 W6 = new Vector2(0.27f, 0.27f);


        private LabelManager()
        {
        }

        public static LabelManager Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = FindObjectOfType(typeof(LabelManager)) as LabelManager;

                    if (!m_Instance)
                    {
                        Debug.LogError("There needs to be one active LabelManager script on a GameObject in your scene.");
                    }
                    else
                    {
                        m_Instance.Init();
                    }
                }
                return m_Instance;
            }
        }

        private void Init()
        {

        }

        void Update()
        {
            m_Forces.Clear();
            // We have some location, try not to move from it
            m_PreviousLocations.Clear();
            m_Endpoints.Clear();

            for (int i = 0; i < m_PickablesOnScreen.Count; ++i)
            {
                m_Forces.Add(new Vector3());
                m_PreviousLocations.Add(m_PickablesOnScreen[i].Endpoint);
                m_Endpoints.Add(m_PickablesOnScreen[i].Endpoint);
            }

            // Handle objects on screen
            for (int cycle = 0; cycle < 10; ++cycle)
            {
                // Compute forces
                for (int i = 0; i < m_PickablesOnScreen.Count; ++i)
                {
                    // Label selected
                    if (m_PickablesOnScreen[i].IsSelected)
                    {
                        continue;
                    }
                    // Keep point on its previous location
                    m_Forces[i] = AttractingForce(W5, m_PreviousLocations[i], m_Endpoints[i]);
                    // Compute endpoint to anchor force
                    m_Forces[i] += (AttractingForce(W1, m_PickablesOnScreen[i].Anchor, m_Endpoints[i]));
                    // Attract to center
                    m_Forces[i] += (AttractingForce(W4, new Vector3(Screen.width / 2, Screen.height / 2, 0.0f), m_Endpoints[i]));
                    // Attract point to mouse location but as -RepulsiveForce, since we want attract only the closest points
                    m_Forces[i] += AttractingForceWithDistance(W6, Input.mousePosition, m_Endpoints[i]);
                    //UnityEditor.EditorApplication.isPaused = true;
                    for (int j = 0; j < m_PickablesOnScreen.Count; ++j)
                    {
                        if (i != j)
                        {
                            // Compute repulsive forces between endpoints
                            //Debug.Log(RepulsiveForce(W2, m_PickablesOnScreen[j].Endpoint, m_PickablesOnScreen[i].Endpoint));
                            m_Forces[i] += RepulsiveForce(W2, m_Endpoints[j], m_Endpoints[i]);
                        }
                        // Compute repulsive forces between endpoint and each anchor
                        m_Forces[i] += RepulsiveForce(W3, m_PickablesOnScreen[j].Anchor, m_Endpoints[i]);
                    }
                }

                // Apply forces
                for (int i = 0; i < m_PickablesOnScreen.Count; ++i)
                {
                    m_Endpoints[i] = m_Endpoints[i] + m_Forces[i];
                }

            }

            // Assign endpoints
            for (int i = 0; i < m_PickablesOnScreen.Count; ++i)
            {
                m_PickablesOnScreen[i].Endpoint = m_Endpoints[i];
            }

            m_PreviousLocations.Clear();
            m_Forces.Clear();
            m_Endpoints.Clear();

            for (int i = 0; i < m_PickablesOffScreen.Count; ++i)
            {
                m_Forces.Add(new Vector3());
                m_PreviousLocations.Add(m_PickablesOffScreen[i].Endpoint);
                m_Endpoints.Add(m_PickablesOffScreen[i].Endpoint);
            }

            // Keep objects off screen on edge of screen
            for (int cycle = 0; cycle < 10; ++cycle)
            {
                // Handle objects off screen - keep them on sides
                for (int i = 0; i < m_PickablesOffScreen.Count; ++i)
                {
                    // Compute endpoint to anchor force
                    Vector3 a = m_PickablesOffScreen[i].AnchorZ;
                    // Unwrap horizontal from middle - it can be behind me and in that case it is "on screen" but with -z
                    if (a.z < 0)
                    {
                        a.x = 0.0f;
                        a.y = 0.0f;
                    } 
                    a.z = 0.0f;
                    m_Forces[i] = (AttractingForce(W4, a, m_Endpoints[i]));
                }
                for (int i = 0; i < m_PickablesOffScreen.Count; ++i)
                {
                    Vector3 a = m_Endpoints[i] + m_Forces[i];
                    a.x = Mathf.Max(Mathf.Min(a.x, Screen.width - 40.0f), 40.0f);
                    a.y = Mathf.Max(Mathf.Min(a.y, Screen.height - 13.0f), 13.0f);
                    m_Endpoints[i] = a;
                }
            }
            // Assign enpoints
            for (int i = 0; i < m_PickablesOffScreen.Count; ++i)
            {
                m_PickablesOffScreen[i].Endpoint = m_Endpoints[i];
            }
        }

        /// <summary>
        /// Modeled as Hooke's law, but divided by its distance, so that
        /// closer we get, the higher is the attracting force.
        /// </summary>
        /// <param name="w">modifier</param>
        /// <param name="to">attractor</param>
        /// <param name="p">attractee</param>
        /// <returns>Returns force or 0 if closer than 5 units.</returns>
        private Vector3 AttractingForceWithDistance(Vector2 w, Vector3 to, Vector3 p)
        {
            Vector3 ret = (to - p);
            float d = ret.magnitude;
            if (d > 80.0f)
                return Vector3.zero;
            //d *= d;
            ret.x *= w.x;
            ret.y *= w.y;

            return ret;
        }

        /// <summary>
        /// Attracting force modeled as Hooke's law.
        /// </summary>
        /// <param name="w">modifier for force</param>
        /// <param name="to">attractor</param>
        /// <param name="p">attractee</param>
        /// <returns>Returns attracting force.</returns>
        private Vector3 AttractingForce(Vector2 w, Vector3 to, Vector3 p)
        {
            Vector3 ret = (to - p);
            ret.x *= w.x;
            ret.y *= w.y;
            return ret;
        }

        /// <summary>
        /// Models repulsive force based on COulomb's law.
        /// </summary>
        /// <param name="w">modifier</param>
        /// <param name="from">repulsor</param>
        /// <param name="p">repulsed point</param>
        /// <returns>Returns repulsive force.</returns>
        private Vector3 RepulsiveForce(Vector2 w, Vector3 from, Vector3 p)
        {
            Vector3 d = from.DistancePerCoordinate(p).Max(Vector3.one * 0.1f);
            Vector3 ret = (p - from).normalized.Div(d);
            ret.x *= w.x;
            ret.y *= w.y;
            return ret;
        }

        /// <summary>
        /// When pickable is on screen, remove it from off screen list 
        /// and add it to on screen list.
        /// </summary>
        /// <param name="p"></param>
        public void PickableOnScreen(Pickable p)
        {
            if (!m_PickablesOnScreen.Contains(p.GuiLabel) && p.GuiLabel != null)
            {
                m_PickablesOffScreen.Remove(p.GuiLabel);
                m_PickablesOnScreen.Add(p.GuiLabel);
            }
        }

        /// <summary>
        /// When pickable is off screen, remove it from on screen list 
        /// and add it to off screen list.
        /// </summary>
        /// <param name="p"></param>
        public void PickableOffScreen(Pickable p)
        {
            if (!m_PickablesOffScreen.Contains(p.GuiLabel) && p.GuiLabel != null)
            {
                m_PickablesOnScreen.Remove(p.GuiLabel);
                m_PickablesOffScreen.Add(p.GuiLabel);
            }
        }

        /// <summary>
        /// Removes pickable from both lists.
        /// </summary>
        /// <param name="p">object to remove</param>
        public void RemovePickable(Pickable p)
        {
            m_PickablesOnScreen.Remove(p.GuiLabel);
            m_PickablesOffScreen.Remove(p.GuiLabel);
        }

        /// <summary>
        /// Adds label to the graph.
        /// </summary>
        /// <param name="l">label to add</param>
        public void AddLabel(Label l)
        {
            l.transform.SetParent(LabelPanel);
        }

    }
}
