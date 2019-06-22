using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Component assigned to scrollbar, which keeps it down, 
    /// so when new object is added, it will display it. Only
    /// kept down, when close to bottom.
    /// </summary>
    class ScrollbarDown : MonoBehaviour
    {

        public Scrollbar Scrollbar;
        private bool m_KeepDown = true;

        void Update()
        {
            if (m_KeepDown)
                Scrollbar.value = 0.0f;
        }

        public void KeepDown()
        {
            m_KeepDown = Scrollbar.value < 0.01;
        }

        public void DontMove()
        {
            m_KeepDown = false;
        }

    }
}
