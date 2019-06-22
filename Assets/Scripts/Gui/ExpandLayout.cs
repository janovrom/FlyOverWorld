using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Assets.Scripts.Gui
{
    
    /// <summary>
    /// Layout which handles adding of panels, buttons and texts.
    /// </summary>
    //[ExecuteInEditMode]
    public class ExpandLayout : MonoBehaviour
    {

        void Start()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                (child as RectTransform).anchorMax = new Vector2(0, 1);
                (child as RectTransform).anchorMin = new Vector2(0, 1);
            }
        }

        /// <summary>
        /// Get child specified by name or null if it doesn't exist.
        /// </summary>
        /// <param name="name">inquired name</param>
        /// <returns>returns child transform specifed by its name.</returns>
        private Transform GetChild(string name)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).name == name)
                    return transform.GetChild(i);
            }

            return null;
        }

        /// <summary>
        /// Contains child specified by name?
        /// </summary>
        /// <param name="name">inquired name</param>
        /// <returns>returns true iff there is child transform with specified name</returns>
        public bool Contains(string name)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).name == name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Add simple button without any defined actions.
        /// </summary>
        /// <param name="name">name of button</param>
        /// <param name="prefab">prefab for this button</param>
        public void AddButton(string name, RectTransform prefab)
        {
            if (!Contains(name))
            {
                AddElement(Instantiate(prefab).GetComponent<RectTransform>(), name);
            }
        }

        /// <summary>
        /// Add button to specifed pickable and enable dragging. Also assings
        /// default actions for picking and delete.
        /// </summary>
        /// <param name="pickable">pickable for which button is created</param>
        /// <param name="addDrag">can item be dragged to mission gui</param>
        public void AddButton(Pickable pickable, bool addDrag)
        {
            AddButton(pickable,
                new UnityAction(delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(pickable); }),
                new UnityAction(delegate () { FindObjectOfType<Cameras.PickerCamera>().RemoveObject(pickable); }),
                addDrag);
        }

        /// <summary>
        /// Adds DragItem to specifed RectTransform.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="pickable"></param>
        private void AddDrag(RectTransform rect, Pickable pickable)
        {
            DragItem di = rect.gameObject.AddComponent<DragItem>();
            di.DraggedObject = pickable;
        }

        /// <summary>
        /// Adds deletable button to mission. When this button is deleted, it won't destroy its 
        /// referenced object in scene.
        /// </summary>
        /// <param name="pickable">object to reference</param>
        /// <param name="select">action called when selected</param>
        /// <param name="remove">action called when removed</param>
        /// <param name="addDrag">is object draggable</param>
        /// <param name="missionName">name of mission to which it is assigned</param>
        public void AddMissionDeletableButton(Pickable pickable, UnityAction select, UnityAction remove, bool addDrag, string missionName = null)
        {
            // Deletable are in mission so they can be added multiple times based on mission preferences
            RectTransform rect = Instantiate(GuiManager.Instance.DeletableElementPrefab).GetComponent<RectTransform>();
            rect.transform.SetParent(transform);
            rect.name = pickable.name;
            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            // Register for selection
            rect.GetChild(0).GetComponent<ColorChangableImage>().enabled = false;
            pickable.AddImage(rect.GetChild(0).GetComponent<Image>());
            // can be switched?
            if (missionName != null)
                rect.GetChild(0).gameObject.AddComponent<DragMoveBefore>().MissionName = missionName;
            // Add DragItem
            if (addDrag)
                AddDrag(rect, pickable);
            // Assign button action and its name
            rect.GetChild(0).GetComponentInChildren<Text>().text = pickable.name;
            if (select != null)
                rect.GetChild(0).GetComponent<Button>().onClick.AddListener(select);
            // Assign remove action
            if (remove != null)
                rect.GetChild(1).GetComponent<Button>().onClick.AddListener(remove);
        }

        /// <summary>
        /// Add button with specified selection and remove actions. It can be specified 
        /// if it should be dragged.
        /// </summary>
        /// <param name="pickable">object to reference</param>
        /// <param name="select">action called when selected</param>
        /// <param name="remove">action called when removed</param>
        /// <param name="addDrag">is object draggable</param>
        public void AddButton(Pickable pickable, UnityAction select, UnityAction remove, bool addDrag)
        {
            if (!Contains(pickable.gameObject.name))
            {
                if (select != null)
                {
                    RectTransform rect = Instantiate(GuiManager.Instance.ElementPrefab).GetComponent<RectTransform>();
                    // Register for selection
                    rect.GetComponent<ColorChangableImage>().enabled = false;
                    pickable.AddImage(rect.GetComponent<Image>());
                    // Add DragItem
                    if (addDrag)
                        AddDrag(rect, pickable);
                    AddElement(rect, pickable.name, new UnityAction(select));
                }
            }
        }

        /// <summary>
        /// Adds text to layout.
        /// </summary>
        /// <param name="name">name displayed in text</param>
        /// <param name="fontSize">font size of text</param>
        public void AddText(string name, int fontSize)
        {
            if (!Contains(name))
            {
                //m_Elements.Add(name);
                GameObject o = Instantiate(GuiManager.Instance.ElementPrefab);
                o.GetComponentInChildren<Text>().fontSize = fontSize;
                AddElement(o.GetComponent<RectTransform>(), name);
            }
        }

        /// <summary>
        /// Adds text to layout.
        /// </summary>
        /// <param name="name">name displayed in text</param>
        public void AddText(string name)
        {
            if (!Contains(name))
            {
                AddElement(Instantiate(GuiManager.Instance.ElementPrefab).GetComponent<RectTransform>(), name);
            }
        }

        /// <summary>
        /// Adds new ExpandLayout as child. If panel already exists, it is returned
        /// and if not, it is created.
        /// </summary>
        /// <param name="name">name of new ExpandLayout</param>
        /// <returns>Returns object of ExpandLayout specified by name.</returns>
        public ExpandLayout AddPanel(string name)
        {
            if (Contains(name))
                return Get(name).GetComponent<ExpandLayout>();

            GameObject o = Instantiate(GuiManager.Instance.PanelPrefab);
            RectTransform rect = o.GetComponent<RectTransform>();
            rect.name = name;
            rect.transform.SetParent(transform);
            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            return o.GetComponent<ExpandLayout>();
        }

        /// <summary>
        /// Adds arbitrary transform to this layout.
        /// </summary>
        /// <param name="panel">transform to add</param>
        /// <param name="name">name of the panel</param>
        public void Add(Transform panel, string name)
        {
            panel.name = name;
            panel.SetParent(transform);
        }

        /// <summary>
        /// Returns child transform specified by name.
        /// </summary>
        /// <param name="name">inquired name of child</param>
        /// <returns>Returns child transform specified by name.</returns>
        public Transform Get(string name)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).name == name)
                    return transform.GetChild(i);
            }
            return null;
        }

        /// <summary>
        /// Removes child transform specified by its name.
        /// </summary>
        /// <param name="name">name of child transform</param>
        public void Remove(string name)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).name == name)
                    DestroyImmediate(transform.GetChild(i--).gameObject);
            }
        }

        /// <summary>
        /// Removes all children.
        /// </summary>
        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; --i)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Assigns existing RectTransform and gives it name.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="name"></param>
        private void AddElement(RectTransform rect, string name)
        {
            rect.name = name;
            rect.transform.SetParent(transform);
            rect.GetComponentInChildren<Text>().text = name;
            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
        }

        /// <summary>
        /// Assigns existing RectTransform and gives it name and some action
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="name"></param>
        /// <param name="action"></param>
        private void AddElement(RectTransform rect, string name, UnityAction action)
        {
            Button b = rect.gameObject.AddComponent<Button>();
            b.onClick.AddListener(action);
            rect.name = name;
            rect.transform.SetParent(transform);
            rect.GetComponentInChildren<Text>().text = name;
            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
        }

        /// <summary>
        /// Remove first child of layout.
        /// </summary>
        public void RemoveFirst()
        {
            Remove(transform.GetChild(0).name);
        }

        /// <summary>
        /// Number of children.
        /// </summary>
        public int Count
        {
            get
            {
                return transform.childCount;
            }
        }
    }
}
