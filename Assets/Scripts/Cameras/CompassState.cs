using UnityEngine;

namespace Assets.Scripts.Cameras
{

    /// <summary>
    /// State representing gimbal camera. It can rotate and moves with object.
    /// </summary>
    public class CompassState : RotationState
    {

        
        public float flySpeedM = 10.0f;
        public float climbSpeedM = 1.0f;
        public float followDistanceM = 50.0f;


        public CompassState()
        {
            m_MovementDragLocked = true;
        }

        /// <summary>
        /// Handles following the target. During its computation disables character collider,
        /// to remove bumping.
        /// </summary>
        /// <param name="cam">PickerCamera which calls this update</param>
        public override void Update(PickerCamera cam)
        {
            Collider c = cam.Target.GetComponent<Collider>();
            // Ignore collision for target on CharacterController.Move() or for assigning rotation
            if (c != null)
                c.enabled = false;

            Quaternion lookRotation = Quaternion.LookRotation(cam.Target.transform.forward, Vector3.up);
            // Rotate Camera
            m_AngleXAdditionDeg = lookRotation.eulerAngles.x;
            m_AngleYAdditionDeg = lookRotation.eulerAngles.y;
            base.Update(cam);
            // Rotate forward
            if (cam.MoveEnabled && Input.GetKey(KeyCode.E))
            {
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, lookRotation, Time.deltaTime * 5.0f);
                m_AngleXDeg = 0.0f;
                m_AngleYDeg = 0.0f;
            }
            position = cam.Target.transform.position + (cam.Target.transform.forward - Vector3.up) * 0.25f;
            
            MoveCamera(cam);

            // Enable collision for others
            if (c != null)
                c.enabled = true;

        }
    }
}
