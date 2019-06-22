using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui.Effects
{
    /// <summary>
    /// Blinking outline around image or text. Blinking speed is defined
    /// by its period when outline's alpha goes from 0-1-0.
    /// </summary>
    [RequireComponent(typeof(Outline))]
    class BlinkOutline : MonoBehaviour
    {

        /// <summary>
        /// Maximum value of alpha for this outline.
        /// </summary>
        public float MaxAlpha;

        /// <summary>
        /// Blinking speed defined as period 0-max-0.
        /// </summary>
        public float MaxBlinkPeriodSec;

        /// <summary>
        /// Is vizualization using dark or light theme.
        /// </summary>
        public bool IsDarkTheme;

        /// <summary>
        /// Color of outline.
        /// </summary>
        private Color m_Color;
        private Outline m_Outline;

        /// <summary>
        /// Current value of alpha.
        /// </summary>
        private float m_Alpha = 0.0f;

        /// <summary>
        /// Whether it fades out or fades i.
        /// </summary>
        private bool m_FadeIn = true;


        void Start()
        {
            m_Outline = GetComponent<Outline>();
            m_Color = Utility.Constants.COLOR_OUTLINE_LIGHT;
            m_Color.a = m_Alpha;
            m_Outline.effectColor = m_Color;
        }

        void OnEnable()
        {
            IsDarkTheme = Utility.Settings.BACKGROUND_COLOR.Equals(Utility.Constants.COLOR_DARK);
            m_Color.a = m_Alpha;
            ChangeColor();
        }

        public void ChangeColor()
        {
            if (IsDarkTheme)
            {
                m_Color.r = Utility.Constants.COLOR_OUTLINE_DARK.r;
                m_Color.g = Utility.Constants.COLOR_OUTLINE_DARK.g;
                m_Color.b = Utility.Constants.COLOR_OUTLINE_DARK.b;
            }
            else
            {
                m_Color.r = Utility.Constants.COLOR_OUTLINE_LIGHT.r;
                m_Color.g = Utility.Constants.COLOR_OUTLINE_LIGHT.g;
                m_Color.b = Utility.Constants.COLOR_OUTLINE_LIGHT.b;
            }
        }

        void Update()
        {
            float mult = -1.0f;
            if (m_FadeIn)
                mult = 1.0f;

            m_Alpha += mult * MaxAlpha * Time.deltaTime / MaxBlinkPeriodSec;
            // Clamp color in min/max range and assign to outline
            m_Alpha = Mathf.Clamp(m_Alpha, 0.0f, MaxAlpha);
            m_Color.a = m_Alpha / 255.0f;
            m_Outline.effectColor = m_Color;

            // Change fade in/out
            if (Mathf.Abs(m_Alpha - MaxAlpha) < 0.001f)
            {
                // We reached max, start fade out
                m_FadeIn = false;
            }
            else if(Mathf.Abs(m_Alpha) < 0.001f)
            {
                // We reached min, start fade in
                m_FadeIn = true;
            }
        }

    }
}
