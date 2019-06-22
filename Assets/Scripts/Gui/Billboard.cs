using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Billboard created using world ui canvas and buttons. Billboard is always
    /// facing towards camera hovering above its target. It is connected with its
    /// target using LineRenderer. It is scaled proportianally with distance.
    /// </summary>
    [Obsolete]
    public class Billboard : MonoBehaviour
    {

        /// <summary>
        /// Center of anchor for which billboard is created.
        /// </summary>
        private Transform m_Center;

        /// <summary>
        /// Vertical distance from anchor.
        /// </summary>
        private float m_VerticalDistance;

        /// <summary>
        /// Distance, in which the size of billboard is fixed.
        /// </summary>
        private const float DefaultDistance = 500.0f;
        private LineRenderer m_LineRenderer;
        private Vector3[] m_Positions;


        /// <summary>
        /// Sets up billboard and line renderer connecting billboard and its anchor.
        /// </summary>
        void Start()
        {
            m_LineRenderer = gameObject.AddComponent<LineRenderer>();
            m_LineRenderer.receiveShadows = false;
            // Disable shadows for billboard.
            m_LineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_LineRenderer.SetVertexCount(2);
            m_LineRenderer.SetWidth(4.0f, 1.0f);
            m_LineRenderer.SetColors(Color.black, Color.black);
            m_LineRenderer.material = GuiManager.Instance.GuiMaterial;
            m_Positions = new Vector3[2];
        }

        /// <summary>
        /// Rotates billboard towards camera, scales it and based on scale moves it from anchor.
        /// </summary>
        void Update()
        {
            transform.position = m_Center.position + Vector3.up * (m_VerticalDistance * m_Center.localScale.y);
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);

            float dist = Vector3.Distance(Camera.main.transform.position, transform.position);
            float scale = dist / DefaultDistance;
            transform.localScale = new Vector3(scale, scale, 1.0f);

            m_Positions[0] = transform.position;
            m_Positions[1] = m_Center.position;
            m_LineRenderer.SetWidth(Utility.Settings.BILLBOARD_LABEL_LINE_START_WIDTH * scale, Utility.Settings.BILLBOARD_LABEL_LINE_END_WIDTH * scale);
            m_LineRenderer.SetPositions(m_Positions);
        }


        /// <summary>
        /// Initialize billboard with center, distance from the center, billboard name and its color.
        /// </summary>
        /// <param name="center">center of anchor for billboard</param>
        /// <param name="dist">distance from center of anchor</param>
        /// <param name="name">name displayed on billboard</param>
        /// <param name="color">color of billboard</param>
        public void Init(Pickable center, float dist, string name, Color color)
        {
            m_VerticalDistance = dist;
            m_Center = center.transform;
            // Allow to drag to mission
            DragItem di = gameObject.AddComponent<DragItem>();
            di.DraggedObject = center;
            // Change name and color
            gameObject.GetComponentInChildren<Text>().text = name;
            gameObject.GetComponent<Image>().color = color;
        }

    }
}
