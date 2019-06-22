using UnityEngine;

namespace Assets.Scripts.Cameras
{

    /// <summary>
    /// State handling moving on ground from persons field of view.
    /// Person can move and look around, but only to 90 degrees up and down.
    /// </summary>
    class WalkerState : RotationState
    {

        private float m_WalkingSpeed = 10.5f;

        public WalkerState()
        {
            m_MovementDragLocked = true;
        }

        /// <summary>
        /// Rotates camera towards target.
        /// </summary>
        /// <param name="target">target to look at</param>
        /// <param name="center">center from which to look</param>
        public void LookAt(Vector3 target, Vector3 center)
        {
            position = center;
            Quaternion q = Quaternion.LookRotation(target - center, Vector3.up);
            m_AngleXDeg = q.eulerAngles.x;
            m_AngleYDeg = q.eulerAngles.y;
        }

        /// <summary>
        /// Resets look to zero angles on X,Y axis and positions
        /// itself to center.
        /// </summary>
        /// <param name="center">position of camera</param>
        public void ResetLook(Vector3 center)
        {
            position = center;
            m_AngleXDeg = 0.0f;
            m_AngleYDeg = 0.0f;
        }

        /// <summary>
        /// Handles moving on the ground.
        /// </summary>
        /// <param name="cam">PickerCamera which called this update</param>
        public override void Update(PickerCamera cam)
        {
            base.Update(cam);
            float x = 0;
            float y = 0;
            x = Input.GetAxis("Horizontal");
            y = Input.GetAxis("Vertical");
            Vector3 right = cam.transform.right;
            Vector3 forward = cam.transform.forward;
            right.y = 0;
            forward.y = 0;
            if (cam.MoveEnabled)
                position += (right * x + forward * y) * m_WalkingSpeed * Time.deltaTime;
            MoveCamera(cam);
        }

    }
}
