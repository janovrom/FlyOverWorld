using UnityEngine;

namespace Assets.Scripts.Nav
{
    /// <summary>
    /// Parent class for all zones displayed in scene - Surveillance area,
    /// no-fly zone and waypoints.
    /// </summary>
    public abstract class Zone : Gui.Pickable
    {

        protected int m_LineId;

        [System.Obsolete("No longer uses world billboards. Use RemoveLabel() for screen space label box.", true)]
        public override void RemoveBillboard()
        {
            if (Gui.GuiManager.Instance)
                Gui.GuiManager.Instance.RemoveBillboard(name);
        }

        public int LineId
        {
            get
            {
                return m_LineId;
            }
        }

    }
}
