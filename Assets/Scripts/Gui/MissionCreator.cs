using Assets.Scripts.Cameras;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{
    /// <summary>
    /// Handles mission creation. For mission, only its
    /// name has to be specified.
    /// </summary>
    class MissionCreator : MonoBehaviour
    {

        public InputField InputText;
        private PickerCamera m_Camera;
        private static int ID = 0;


        public void Start()
        {
            m_Camera = FindObjectOfType<PickerCamera>();
        }

        /// <summary>
        /// Create new mission and new name.
        /// </summary>
        public void CreateMission()
        {
            string name = Agents.Mission.GetMissionName(FindObjectOfType<PickerCamera>().Selection.ToArray());
            if (name != null && name.Length > 0)
            {
                CreateMission(name);
            }
            else
            {
                CreateMission("Mission" + ID++);
            }
        }

        /// <summary>
        /// Create new mission from given name.
        /// </summary>
        /// <param name="missionName">name of mission</param>
        public void CreateMission(string missionName)
        {
            this.gameObject.SetActive(true);
            InputText.text = missionName;
            // Try to get tabshifter, if doesn't exist, focus only name
            TabInputShifter tis = GetComponent<TabInputShifter>();
            if (tis != null)
            {
                tis.Reset();
            }
            else
            {
                InputText.Select();
                InputText.ActivateInputField();
            }
        }

        void Update()
        {
            // Try to finish on enter
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Confirm();
                m_Camera.OnFocusLost("");
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                m_Camera.OnFocusLost("");
                return;
            }
        }

        /// <summary>
        /// Creates mission with specified name if name input is not empty.
        /// When mission is created, all selected objects are assigned to it.
        /// </summary>
        public void Confirm()
        {
            bool check = InputText.text != null && InputText.text.Length > 0;
            // Check on name availability
            check &= Agents.AgentManager.Instance.GetMission(InputText.text) == null;
            if (!check)
            {
                InputText.GetComponent<Outline>().effectColor = Color.red;
                return;
            }
            else
            {
                InputText.GetComponent<Outline>().effectColor = new Color(0,0,0,0);
            }

            if (Agents.AgentManager.Instance.CreateMission(InputText.text, m_Camera.Selection))
            {
                GuiManager.Instance.MissionCreated(InputText.text);
            }
            Close();
        }

        public void Close()
        {
            InputText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            InputText.text = "";
            this.gameObject.SetActive(false);
        }

    }
}
