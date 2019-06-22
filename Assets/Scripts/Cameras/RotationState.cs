using UnityEngine;

namespace Assets.Scripts.Cameras
{

    /// <summary>
    /// Abstract class, which is superclass for every state, that can rotate
    /// itself. It is specifed by rotation around X and Y.
    /// </summary>
    public abstract class RotationState : State
    {

        protected float m_AngleXDeg = 0.0f;
        protected float m_AngleYDeg = 0.0f;
        protected float m_RotationSpeedDeg = 1.5f;

        protected float m_AngleXAdditionDeg = 0.0f;
        protected float m_AngleYAdditionDeg = 0.0f;

        /// <summary>
        /// Handles rotation of camera using mouse input.
        /// </summary>
        /// <param name="cam"></param>
        public override void Update(PickerCamera cam)
        {
            base.Update(cam);
            // Rotation camera
            if (Input.GetMouseButton(1))
            {
                // Right mouse button rotation
                float x = Input.GetAxis("Mouse X");
                float y = Input.GetAxis("Mouse Y");
                m_AngleXDeg -= y * m_RotationSpeedDeg;
                m_AngleYDeg += x * m_RotationSpeedDeg;
                m_AngleXDeg = Mathf.Clamp(m_AngleXDeg, -90, 90);
            }

            cam.Compass.rotation = Quaternion.Euler(Vector3.forward * m_AngleYDeg);
            cam.transform.rotation = Quaternion.Euler(m_AngleXDeg + m_AngleXAdditionDeg, m_AngleYDeg + m_AngleYAdditionDeg, 0);
        }

    }
}
