using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Changes color of assigned text. Color is reversed for used color theme.
    /// </summary>
    class ColorChangableText : MonoBehaviour
    {

        public Text ColorText;

        
        void Start()
        {
            ChangeColor();
        }

        void OnEnable()
        {
            ChangeColor();
        }

        public void ChangeColor()
        {
            if (!enabled)
                return;

            ColorText.color = Utility.Settings.TEXT_COLOR;
        }

    }
}
