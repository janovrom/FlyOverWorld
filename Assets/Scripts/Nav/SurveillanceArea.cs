using Assets.Scripts.Utility;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Nav
{

    /// <summary>
    /// Surveillance area uses orghogonal projector to display blue 
    /// rectangular area on terrain(only). Surveillance area is
    /// defined by its minimum and maximum and label anchor, which
    /// lies in its maximum.
    /// </summary>
    public class SurveillanceArea : Zone
    {

        private Vector3 m_Start;
        private Vector3 m_End;
        private GameObject m_LabelAnchor;


        public Vector3 Max
        {
            get
            {
                return m_End;
            }
        }

        public Vector3 Min
        {
            get
            {
                return m_Start;
            }
        }

        public Vector3 LabelAnchor
        {
            get
            {
                return m_LabelAnchor.transform.position;
            }

            set
            {
                m_LabelAnchor.transform.position = value;
            }
        }


        protected override Color GetColor()
        {
            return Constants.COLOR_BLUE;
        }

        /// <summary>
        /// Creates surveillance area from prefab.
        /// </summary>
        /// <param name="zoneName">name of surveillance area</param>
        /// <param name="start">start of rectangular area</param>
        /// <param name="end">end of rectangular area</param>
        /// <returns>Returns game object with surveillance area.</returns>
        public static GameObject Initialize(string zoneName, Vector3 start, Vector3 end)
        {
            GameObject o = GameObject.Instantiate(ZoneManager.Instance.SurveillanceAreaPrefab);
            SurveillanceArea area = o.GetComponent<SurveillanceArea>();
            area.UpdateArea(zoneName, start, end);
            area.m_LabelAnchor = new GameObject("labelAnchor_" + zoneName);
            area.m_LabelAnchor.transform.position = end;
            // Init label
            area.AddLabel();
            //area.AddBillboard();
            return o;
        }

        /// <summary>
        /// Updates this area, so projector can be resized. Start
        /// and end doesn't have to be minimum and maximum and if not,
        /// their values will be reassigned.
        /// </summary>
        /// <param name="name">new name</param>
        /// <param name="start">new start</param>
        /// <param name="end">new end</param>
        public void UpdateArea(string name, Vector3 start, Vector3 end)
        {
            // Init variables
            m_Start = start.Min(end);
            m_End = start.Max(end);
            // Init game object
            Vector3 size = m_End - m_Start;
            //size.y += 2000.0f;
            gameObject.transform.localPosition = size / 2.0f + m_Start;
            gameObject.isStatic = true;
            this.name = name;
            
            // Initialize projector
            float aspect = size.x / size.z;
            Projector projector = GetComponentInChildren<Projector>();
            projector.aspectRatio = aspect;
            projector.orthographicSize = size.z / 2.0f;
        }

        /// <summary>
        /// When area was already created, but edited. This method is called.
        /// It changes its name, anchor and informs ZoneManager about its change.
        /// </summary>
        /// <param name="oldName">old name of zone</param>
        /// <param name="newName">new name of zone</param>
        /// <param name="anchorPosition">new anchor position</param>
        public void AreaEdited(string oldName, string newName, Vector3 anchorPosition)
        {
            LabelAnchor = anchorPosition;
            gameObject.name = newName;
            m_LabelAnchor.name = "labelAnchor_" + newName;
            ZoneManager.Instance.ZoneRenamed(oldName, this);
        }

        public override string[] Values()
        {
            return new string[] { "Min: " + m_Start.xz(), "Max: " + m_End.xz() };
        }

        public override bool IsDeletable()
        {
            return true;
        }

        [System.Obsolete("No longer uses world billboards. Use AddLabel() for screen space label box.", true)]
        public override void AddBillboard()
        {
            Gui.GuiManager.Instance.AddBillboard(this, 5.0f, name, Color.blue, 
                new UnityEngine.Events.UnityAction(delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(this); }));
        }

        protected override void AddLabel()
        {
            
            m_Label = Instantiate(Gui.LabelManager.Instance.LabelPrefab);
            m_Label.Init(this, Utility.Constants.COLOR_BLUE, Utility.Constants.COLOR_LIGHT, m_LabelAnchor.transform);
        }

        public override void CompassSelected()
        {
        }

        public override void CompassDeselected()
        {
        }

        void OnDestroy()
        {
            if (ZoneManager.Instance)
                ZoneManager.Instance.RemoveZone(this);
            //RemoveBillboard();
            RemoveLabel();
            Destroy(m_LabelAnchor);
        }
    }
}
