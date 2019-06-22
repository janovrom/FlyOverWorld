using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui.Effects
{

    /// <summary>
    /// Animation for circle. During its period, it changes 
    /// fill amount of asigned image from 0-1-0.
    /// </summary>
    class FillRotation : MonoBehaviour
    {

        /// <summary>
        /// Assigned image to animate using 360 radial filling.
        /// </summary>
        public Image AnimatedImage;

        /// <summary>
        /// Period of animation from amoutn 0-1-0.
        /// </summary>
        public float MaxFillPeriodSec;

        /// <summary>
        /// Current amount of fill.
        /// </summary>
        private float m_Amount = 0.0f;

        /// <summary>
        /// Whether we fade out or fade in.
        /// </summary>
        private bool m_FadeIn = true;


        /// <summary>
        /// Setup image type to filled, radial 360 and its beggining from top
        /// and animation in clockwise order.
        /// </summary>
        void Start()
        {
            AnimatedImage.type = Image.Type.Filled;
            AnimatedImage.fillMethod = Image.FillMethod.Radial360;
            AnimatedImage.fillOrigin = (int)Image.Origin360.Top;
            AnimatedImage.fillClockwise = true;
            AnimatedImage.fillAmount = 0.0f;
        }

        void Update()
        {
            float mult = -1.0f;
            if (m_FadeIn)
                mult = 1.0f;

            m_Amount += mult * Time.deltaTime / MaxFillPeriodSec;
            // Clamp in min/max range and assign
            m_Amount = Mathf.Clamp(m_Amount, 0.0f, 1.0f);
            AnimatedImage.fillAmount = m_Amount;

            // Change fade in/out
            if (Mathf.Abs(m_Amount - 1.0f) < 0.001f)
            {
                // We reached max, start fade out
                m_FadeIn = false;
                AnimatedImage.fillClockwise = false;
            }
            else if (Mathf.Abs(m_Amount) < 0.001f)
            {
                // We reached min, start fade in
                m_FadeIn = true;
                AnimatedImage.fillClockwise = true;
            }
        }

    }
}
