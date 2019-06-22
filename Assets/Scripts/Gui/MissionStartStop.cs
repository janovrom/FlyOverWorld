using Assets.Scripts.Agents;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Handles mission controls. Allows user to stop, start or remove mission.
    /// Moreover mission content can be hidden or shown to show less space.
    /// It requires created mission and thus should be instantiated only
    /// after valid mission is in AgentManager.
    /// </summary>
    class MissionStartStop : MonoBehaviour
    {

        public Text MissionName;
        public Image ButtonImage;
        // Sprites changing based on state.
        public Sprite PlaySprite;
        public Sprite StopSprite;
        public ExpandLayout Content;
        

        void Start()
        {
            // Don't have to check on m!=null since it should be called after instantiating where is check
            Mission m = AgentManager.Instance.GetMission(name);
            Content = transform.parent.GetComponent<ExpandLayout>().AddPanel(MissionName.text + "_content");
            ExpandLayout drones = Content.AddPanel(name + "_drones");
            ExpandLayout targets = Content.AddPanel(name + "_targets");
            ExpandLayout areas = Content.AddPanel(name + "_areas");
            ExpandLayout wps = Content.AddPanel(name + "_waypoints");
            Content.Add(Instantiate(GuiManager.Instance.DividingLinePrefab).transform, name + "_divline");
            // Add DropZone for each expand layout
            drones.gameObject.AddComponent<DropZone>().MissionNameStr = MissionName.text;
            targets.gameObject.AddComponent<DropZone>().MissionNameStr = MissionName.text;
            areas.gameObject.AddComponent<DropZone>().MissionNameStr = MissionName.text;
            wps.gameObject.AddComponent<DropZone>().MissionNameStr = MissionName.text;
            GuiManager.Instance.AddMissionContent(m, drones, targets, areas, wps);
        }

        /// <summary>
        /// Finds assigned mission (has to be present in AgentManager) and 
        /// assigns resources to each respective content panel.
        /// </summary>
        public void AddContent()
        {
            Mission m = AgentManager.Instance.GetMission(name);
            ExpandLayout drones = GuiManager.Instance.GetChild(Content.transform, name + "_drones").GetComponent<ExpandLayout>();
            ExpandLayout targets = GuiManager.Instance.GetChild(Content.transform, name + "_targets").GetComponent<ExpandLayout>();
            ExpandLayout areas = GuiManager.Instance.GetChild(Content.transform, name + "_areas").GetComponent<ExpandLayout>();
            ExpandLayout wps = GuiManager.Instance.GetChild(Content.transform, name + "_waypoints").GetComponent<ExpandLayout>();
            drones.Clear();
            targets.Clear();
            areas.Clear();
            wps.Clear();
            GuiManager.Instance.AddMissionContent(m, drones, targets, areas, wps);
        }

        /// <summary>
        /// Hides or shows content of mission to which this component is assigned.
        /// </summary>
        public void HideShowContent()
        {
            if (Content.gameObject.activeInHierarchy)
            {
                Content.gameObject.SetActive(false);
            }
            else
            {
                Content.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Removes mission assigned to this component and removes this button
        /// and its content from gui.
        /// </summary>
        public void Remove()
        {
            Mission m = AgentManager.Instance.GetMission(MissionName.text);
            // Remove and stop mission
            AgentManager.Instance.RemoveMission(m);
            GuiManager.Instance.RemoveChild(Content.transform, name + "_drones");
            GuiManager.Instance.RemoveChild(Content.transform, name + "_targets");
            GuiManager.Instance.RemoveChild(Content.transform, name + "_areas");
            GuiManager.Instance.RemoveChild(Content.transform, name + "_waypoints");
            GuiManager.Instance.RemoveChild(Content.transform, name + "_divline");
            GuiManager.Instance.RemoveChild(Content.transform.parent, Content.name);
        }

        /// <summary>
        /// Starts or stops the mission based on whether the mission is running or not.
        /// </summary>
        public void StartStop()
        {
            Mission m = AgentManager.Instance.GetMission(MissionName.text);
            if (m.IsPlaying)
            {
                Stop(m);
            }
            else
            {
                Play(m);
            }
        }

        /// <summary>
        /// Changes the sprite to play sprite, since next possible action will be play.
        /// </summary>
        public void Stop()
        {
            // Change state
            ButtonImage.sprite = PlaySprite;
        }

        /// <summary>
        /// Changes the sprite to stop sprite, since next possible action will be stop.
        /// </summary>
        public void Play()
        {
            // Change state
            ButtonImage.sprite = StopSprite;
        }

        /// <summary>
        /// Stops the mission and changes the sprite to play sprite, since next possible action will be play.
        /// </summary>
        /// <param name="m">Mission to stop.</param>
        private void Stop(Mission m)
        {
            m.Stop();
            // Change state
            ButtonImage.sprite = PlaySprite;
        }

        /// <summary>
        /// Starts the mission and changes the sprite to stop sprite, since next possible action will be stop.
        /// </summary>
        /// <param name="m">Mission to start.</param>
        private void Play(Mission m)
        {
            m.Start();
            // Change state
            ButtonImage.sprite = StopSprite;
        }

    }
}
