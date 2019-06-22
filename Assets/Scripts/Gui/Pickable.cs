using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Class representing objects in scene, that can be picked, 
    /// created, edited, have labels or are in gui. Provides interface 
    /// for getting values to display or behaviour when object ise
    /// selected. Zones and agents are one of those.
    /// </summary>
    public abstract class Pickable : MonoBehaviour
    {

        /// <summary>
        /// Images to recolor, when pickable is selected.
        /// </summary>
        private List<Image> m_GuiElements = new List<Image>();
        /// <summary>
        /// Label assigned to this pickable.
        /// </summary>
        protected Label m_Label;
        protected float m_PreviousOutline = 0.0f;

        /// <summary>
        /// Values are used for displaying specific information
        /// on selected object. Each string in array is one
        /// info.
        /// </summary>
        /// <returns>Returns formated information in an array.</returns>
        public abstract string[] Values();

        /// <summary>
        /// Whether object can be deleted.
        /// </summary>
        /// <returns>Returns true iff object can be deleted in scene.</returns>
        public abstract bool IsDeletable();

        // Adding billboards, which floats above the pickable.
        [System.Obsolete]
        public abstract void AddBillboard();
        [System.Obsolete]
        public abstract void RemoveBillboard();

        /// <summary>
        /// Adds label in screen space, which is connected to anchor 
        /// of this pickable by black line.
        /// </summary>
        protected abstract void AddLabel();

        /// <summary>
        /// Returns color assigned to this pickable.
        /// </summary>
        /// <returns>Returns color assigned to this pickable.</returns>
        protected abstract Color GetColor();

        /// <summary>
        /// Changes object state, when compass camera is selected.
        /// </summary>
        public abstract void CompassSelected();

        // <summary>
        /// Changes object state, when compass camera is deselected.
        /// </summary>
        public abstract void CompassDeselected();


        /// <summary>
        /// Removes label from LabelManager and from the scene.
        /// </summary>
        protected void RemoveLabel()
        {
            if (LabelManager.Instance)
                LabelManager.Instance.RemovePickable(this);
            if (m_Label != null)
                Destroy(m_Label.gameObject);
            m_Label = null;
        }

        /// <summary>
        /// Adds images, that should be highlighted when selected.
        /// Images are automatically deleted, when their respective
        /// game objects are destroyed. It also sets default color
        /// per class type.
        /// </summary>
        /// <param name="i">Image whose color can be changed.</param>
        public void AddImage(Image i)
        {
            m_GuiElements.Add(i);
            i.color = GetColor();
        }

        /// <summary>
        /// Each registered image is recolored to show it was selected.
        /// Automatically removes no longer existing images.
        /// </summary>
        public void Select()
        {
            for (int i = 0; i < m_GuiElements.Count; ++i)
            {
                if (m_GuiElements[i] == null)
                {
                    m_GuiElements.RemoveAt(i);
                    --i;
                }
                else
                {
                    m_GuiElements[i].color = Utility.Constants.COLOR_BLAND_GREEN;
                }
            }

            // Inform label about selection
            m_Label.Select();
        }

        /// <summary>
        /// Each registered image is recolored to show it was deselected.
        /// Automatically removes no longer existing images.
        /// </summary>
        public void Deselect()
        {
            for (int i = 0; i < m_GuiElements.Count; ++i)
            {
                if (m_GuiElements[i] == null)
                {
                    m_GuiElements.RemoveAt(i);
                    --i;
                }
                else
                {
                    m_GuiElements[i].color = GetColor();
                }
            }

            // Inform label about deselection
            m_Label.Deselect();
        }

        /// <summary>
        /// Handles state change, when object is on screen.
        /// </summary>
        protected virtual void OnScreen()
        {
            LabelManager.Instance.PickableOnScreen(this);
        }

        /// <summary>
        /// Handles state change, when object is off screen.
        /// </summary>
        protected virtual void OffScreen()
        {
            LabelManager.Instance.PickableOffScreen(this);
        }

        /// <summary>
        /// When mouse is first over the object, its outline is 
        /// changed to bright green color and made larger to see.
        /// </summary>
        protected virtual void OnMouseEnter()
        {
            if (this is Nav.SurveillanceArea)
                return;

            MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
            mr.material.SetColor("_OutlineColor", Utility.Constants.COLOR_GREEN);
            m_PreviousOutline = mr.material.GetFloat("_Outline");
            float scale = 0.3f / Mathf.Max(1.0f, transform.localScale.x);
            // Base range of sizes for outline on distance from camera
            float rangeScale = Mathf.Log(Vector3.Distance(this.transform.position, Camera.main.transform.position));
            // Min value 0.03f
            scale = Mathf.Max(scale, 0.01f * rangeScale);
            // Max value of 0.5f
            scale = Mathf.Min(scale, 0.15f * rangeScale);
            mr.material.SetFloat("_Outline", scale);
        }

        /// <summary>
        /// When mouse is first out of object, its outline is
        /// changed back to its previous state.
        /// </summary>
        void OnMouseExit()
        {
            if (this is Nav.SurveillanceArea)
                return;

            MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
            mr.material.SetColor("_OutlineColor", Color.black);
            mr.material.SetFloat("_Outline", m_PreviousOutline);
        }

        /// <summary>
        /// Checks whether object is on screen.
        /// </summary>
        void LateUpdate()
        {
            // Check whether object is on screen
            Vector3 screen = Camera.main.WorldToScreenPoint(transform.position);
            if (screen.x < 0 || screen.y < 0 || screen.x > Display.main.renderingWidth || screen.y > Display.main.renderingHeight || screen.z < 0)
            {
                OffScreen();
            }
            else
            {
                OnScreen();
            }
        }

        /// <summary>
        /// Returns label assigned to this pickable. Readonly.
        /// </summary>
        public Label GuiLabel
        {
            get
            {
                return m_Label;
            }
        }

    }
}
