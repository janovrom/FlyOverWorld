using Assets.Scripts.Models;
using Assets.Scripts.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{
    /// <summary>
    /// Label for pickable objects in scene - Drones, ground targets or zones.
    /// Label consists of rectangular label and leader lines.
    /// </summary>
    public class Label : MonoBehaviour
    {

        /// <summary>
        /// Anchor to which leading line is connected.
        /// </summary>
        private Transform m_Anchor;
        private RectTransform m_Rect;
        // For drawing OpenGL lines.
        private DrawLines m_LineRenderer;
        private int m_LineId;

        /// <summary>
        /// If object associated with this label is in compass state.
        /// </summary>
        private bool m_CompassSelected = false;

        /// <summary>
        /// If label is selected.
        /// </summary>
        private bool m_Selected = false;
        private Color m_LabelColor;


        /// <summary>
        /// Initialize new label with anchor as pickables center.
        /// </summary>
        /// <param name="p">labeled object</param>
        /// <param name="labelColor">label color</param>
        /// <param name="textColor">text color in label</param>
        public void Init(Pickable p, Color labelColor, Color textColor)
        {
            Init(p, labelColor, textColor, p.transform);
        }

        /// <summary>
        /// Initialize new label with specific anchor position.
        /// </summary>
        /// <param name="p">labeled object</param>
        /// <param name="labelColor">label color</param>
        /// <param name="textColor">text color in label</param>
        /// <param name="anchor">anchor position</param>
        public void Init(Pickable p, Color labelColor, Color textColor, Transform anchor)
        {
            m_LabelColor = labelColor;
            m_Anchor = anchor;
            name = p.name;
            m_Rect = GetComponent<RectTransform>();
            Endpoint = Camera.main.WorldToScreenPoint(m_Anchor.position);
            GetComponent<Image>().color = labelColor;
            GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(p); }));
            Text t = GetComponentInChildren<Text>();
            t.text = p.name;
            t.color = textColor;
            LabelManager.Instance.AddLabel(this);

            // Add leader line
            m_LineRenderer = Camera.main.gameObject.GetComponent<DrawLines>();
            m_LineId = m_LineRenderer.RegisterLine();
            m_LineRenderer.UpdateScreenLine(m_LineId, new List<Vector3>() { Anchor, Endpoint });

            // Enable dragging
            DragItem di = gameObject.AddComponent<DragItem>();
            di.DraggedObject = p;
        }

        /// <summary>
        /// Label selected.
        /// </summary>
        public void Select()
        {
            GetComponent<Image>().color = Constants.COLOR_BLAND_GREEN;
            m_Selected = true;
        }

        /// <summary>
        /// Label deselected. Also deselecting compass.
        /// </summary>
        public void Deselect()
        {
            GetComponent<Image>().color = m_LabelColor;
            m_CompassSelected = false;
            m_Selected = false;
        }

        /// <summary>
        /// Pickable is in compass state.
        /// </summary>
        public void CompassSelected()
        {
            m_CompassSelected = true;
        }


        /// <summary>
        /// Each update is anchor checked to see if it is on screen.
        /// </summary>
        void Update()
        {
            // Update leader line
            // Check whether object is on screen
            Vector3 screen = Camera.main.WorldToScreenPoint(m_Anchor.position);
            if (m_CompassSelected || screen.x < 0 || screen.y < 0 || screen.x > Display.main.renderingWidth || screen.y > Display.main.renderingHeight || screen.z < 0)
            {
                // Off screen or selected
                m_LineRenderer.UpdateScreenLine(m_LineId, Vector3.zero, Vector3.zero);
            }
            else
            {
                // On screen
                m_LineRenderer.UpdateScreenLine(m_LineId, Anchor, Endpoint);
            }
        }

        void OnDestroy()
        {
            m_LineRenderer.RemoveScreenLine(m_LineId);
        }

        public string Name
        {
            set
            {
                name = value;
                GetComponentInChildren<Text>().text = value;
            }
        }

        /// <summary>
        /// Anchor position with 0 z-value.
        /// </summary>
        public Vector3 Anchor
        {
            get
            {
                Vector3 pos = Camera.main.WorldToScreenPoint(m_Anchor.position);
                return new Vector3(pos.x, pos.y, 0);
            }
        }

        /// <summary>
        /// Anchor position.
        /// </summary>
        public Vector3 AnchorZ
        {
            get
            {
                Vector3 pos = Camera.main.WorldToScreenPoint(m_Anchor.position);
                return pos;
            }
        }

        /// <summary>
        /// Endpoint position with 0 z-value.
        /// </summary>
        public Vector3 Endpoint
        {
            get
            {
                return new Vector3(m_Rect.position.x, m_Rect.position.y, 0);
            }

            set
            {
                m_Rect.position = new Vector3(value.x, value.y, 0);
            }
        }

        public bool IsSelected
        {
            get
            {
                return m_Selected;
            }
        }

        public bool IsCompassSelected
        {
            get
            {
                return m_CompassSelected;
            }
        }

    }
}
