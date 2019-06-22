using Assets.Scripts.Gui;
using System;
using UnityEngine;

namespace Assets.Scripts.Agents
{
    public abstract class Agent : Pickable
    {

        private const float VerticalDistance = 1.5f;

        [Obsolete("No longer uses world billboards. Use AddLabel() for screen space label box.", true)]
        public override void AddBillboard()
        {
            Gui.GuiManager.Instance.AddBillboard(this, VerticalDistance, name, Color.green, 
                new UnityEngine.Events.UnityAction(delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(this); }));
        }

        [Obsolete("No longer uses world billboards. Use RemoveLabel() for screen space label box.", true)]
        public override void RemoveBillboard()
        {
            Gui.GuiManager.Instance.RemoveBillboard(name);
        }

        /// <summary>
        /// Adds green label in screen space, which is connected to anchor 
        /// of this pickable by black line.
        /// </summary>
        protected override void AddLabel()
        {
            m_Label = Instantiate(LabelManager.Instance.LabelPrefab);
            m_Label.Init(this, GetColor(), Utility.Constants.COLOR_DARK);
        }

        /// <summary>
        /// Handles state change, when object is on screen. That includes
        /// changing its state in agents gui window.
        /// </summary>
        protected override void OnScreen()
        {
            base.OnScreen();
            Gui.GuiManager.Instance.AgentOnScreen(this);
        }

        /// <summary>
        /// Handles state change, when object is off screen. That includes
        /// changing its state in agents gui window.
        /// </summary>
        protected override void OffScreen()
        {
            base.OffScreen();
            Gui.GuiManager.Instance.AgentOffScreen(this);
        }

    }
}
