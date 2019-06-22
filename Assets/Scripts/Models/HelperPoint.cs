using UnityEngine;

namespace Assets.Scripts.Models
{

    /// <summary>
    /// Object (probably spherical) on screen used for visualization 
    /// mouse position projected on terrain. Helper point is scaled
    /// with distance from camera between minimum and maximum size.
    /// </summary>
    class HelperPoint : MonoBehaviour
    {

        public float MinSize = 0.5f;
        public float MaxSize = 25.0f;
        /// <summary>
        /// Distance on which is point scaled from minimum to maximum.
        /// </summary>
        private const float m_ResizableDistanceM = 1000.0f;

        void Update()
        {
            float dist = Vector3.Distance(Camera.main.transform.parent.position, transform.position);
            float mix = dist / m_ResizableDistanceM;
            float scale = Mathf.Min(MinSize + (MaxSize - MinSize) * mix, MaxSize);
            if (transform.parent != null)
                scale /= transform.parent.lossyScale.x;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
