using Assets.Scripts.Cameras;
using UnityEngine;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Gui class representing radial menu. Menu has items around its center position.
    /// </summary>
    class Menu : MonoBehaviour
    {

        // Creators for each object
        public MissionCreator MissionCreator;
        public NoFlightZoneCreator NFZCreator;
        public SurveillanceAreaCreator SACreator;
        public WaypointsCreator WaypointsCreator;
        // Additional buttons
        public GameObject RemoveButton;
        public GameObject EditButton;
        private PickerCamera m_Camera;


        public void Awake()
        {
            m_Camera = FindObjectOfType<PickerCamera>();
        }

        /// <summary>
        /// When shown display remove and edit buttons if some selection which
        /// is deletable or editable is selected.
        /// </summary>
        void OnEnable()
        {
            // Set remove and edit button to invisible and disabled, they can be shown later
            RemoveButton.SetActive(false);
            EditButton.SetActive(false);
            // Detect if i can remove
            foreach (Pickable o in m_Camera.Selection)
            {
                if (o.IsDeletable())
                {
                    RemoveButton.SetActive(true);
                    break;
                }
            }
            // Detect if I can edit
            for (int i = 0; i < m_Camera.Selection.Count; ++i)
            {
                // I like this block, it's good looking, isn't it?
                if (    m_Camera.Selection[i] is Nav.TargetWaypoint 
                    ||  m_Camera.Selection[i] is Nav.NoFlyZone
                    ||  m_Camera.Selection[i] is Nav.SurveillanceArea)
                {
                    EditButton.SetActive(true);
                    break;
                }
            }
            // Bring to front.
            transform.SetAsLastSibling();
        }

        /// <summary>
        /// Stops and resets each creator and then hides menu popup window.
        /// </summary>
        private void StopCreation()
        {
            MissionCreator.Close();
            NFZCreator.Close();
            SACreator.Close();
            WaypointsCreator.Close();
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Takes user selection and finds TargetWaypoint, NoFlightZone 
        /// or SurveillanceArea and makes it editable. Only one object 
        /// can be edited at time.
        /// </summary>
        public void Edit()
        {
            StopCreation();
            // Only one object can be edited
            for (int i = 0; i < m_Camera.Selection.Count; ++i)
            {
                if (m_Camera.Selection[i] is Nav.TargetWaypoint)
                {
                    WaypointsCreator.Edit(m_Camera.Selection[i] as Nav.TargetWaypoint);
                    break;
                }
                else if (m_Camera.Selection[i] is Nav.NoFlyZone)
                {
                    NFZCreator.Edit(m_Camera.Selection[i] as Nav.NoFlyZone);
                    break;
                }
                else if (m_Camera.Selection[i] is Nav.SurveillanceArea)
                {
                    SACreator.Edit(m_Camera.Selection[i] as Nav.SurveillanceArea);
                    break;
                }
            }
            this.gameObject.SetActive(false);
        }

        #region CREATORS Calls to each respective creator. Disable picking.
        public void CreateWaypoints()
        {
            StopCreation();
            m_Camera.PickingEnabled = false;
            WaypointsCreator.CreateWaypoints();
        }

        public void CreateMission()
        {
            StopCreation();
            MissionCreator.CreateMission();
        }

        public void CreateMission(string missionName)
        {
            StopCreation();
            MissionCreator.CreateMission(missionName);
        }

        public void CreateNFZ()
        {
            StopCreation();
            m_Camera.PickingEnabled = false;
            NFZCreator.CreateNFZ();
        }

        public void CreateSurveillanceArea()
        {
            StopCreation();
            m_Camera.PickingEnabled = false;
            SACreator.CreateArea();
        }

        public void Remove()
        {
            StopCreation();
            m_Camera.RemoveObjects();
        }
        #endregion
    }
}
