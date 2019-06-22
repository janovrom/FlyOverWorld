using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{
    /// <summary>
    /// Changes color tint of image based on selected UI theme.
    /// The color can be substractively/additively darkened/brightened for light theme or 
    /// it can use color assigned to text.
    /// </summary>
    //[ExecuteInEditMode]
    class ColorChangableImage : MonoBehaviour
    {

        /// <summary>
        /// Assigned image to change color.
        /// </summary>
        public Image ColorImage;

        /// <summary>
        /// Use color of text, which is reversed color of theme.
        /// </summary>
        public bool UseTextColor;

        /// <summary>
        /// Darken (brighten for light theme) the color.
        /// </summary>
        public bool UseDarkening;

        /// <summary>
        /// Color used for substracive darkening or additive brightening.
        /// </summary>
        private static readonly Color m_DarkeningColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        void Start()
        {
            ChangeColor();
        }

        void OnEnable()
        {
            ChangeColor();
        }

        /// <summary>
        /// Changes image color tint, but preserves previous color alpha.
        /// </summary>
        public void ChangeColor()
        {
            if (!enabled)
                return;

            Color c;
            if (UseTextColor)
                c = Utility.Settings.TEXT_COLOR;
            else
                c = Utility.Settings.BACKGROUND_COLOR;

            if (UseDarkening)
            {
                if (UseTextColor)
                {
                    float r = Mathf.Min(1.0f, (c.r + m_DarkeningColor.r));
                    float g = Mathf.Min(1.0f, (c.g + m_DarkeningColor.g));
                    float b = Mathf.Min(1.0f, (c.b + m_DarkeningColor.b));
                    ColorImage.color = new Color(r, g, b, ColorImage.color.a);
                }
                else
                {
                    float r = Mathf.Max(0.0f, (c.r - m_DarkeningColor.r));
                    float g = Mathf.Max(0.0f, (c.g - m_DarkeningColor.g));
                    float b = Mathf.Max(0.0f, (c.b - m_DarkeningColor.b));
                    ColorImage.color = new Color(r, g, b, ColorImage.color.a);
                }
            }
            else
                ColorImage.color = new Color(c.r, c.g, c.b, ColorImage.color.a);
        }

    }
}
