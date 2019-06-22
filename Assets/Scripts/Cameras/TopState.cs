using UnityEngine;
using System.Collections;
using Assets.Scripts.Cameras;

namespace Assets.Scripts.Cameras
{

    /// <summary>
    /// Perspective look from above. It allows to move above ground in
    /// the same altitude and zoom.
    /// </summary>
    public class TopState : State
    {

        public float flySpeedM = 500.0f;
        public float zoomSpeedM = 250.0f;
        private Quaternion m_LookDown;

        public TopState()
        {
            // Orient to look down
            m_LookDown = Quaternion.Euler(90.0f, 0, 0);
            position = new Vector3(0, 1200.0f, 0);
        }

        /// <summary>
        /// Handles move in XZ and zoom also resets camera to always look down.
        /// </summary>
        /// <param name="cam"></param>
        public override void Update(PickerCamera cam)
        {
            base.Update(cam);
            float x = 0;
            float y = 0;
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (cam.MoveEnabled)
            {
                x = Input.GetAxis("Horizontal");
                y = Input.GetAxis("Vertical");
            }
            // share position with another states
            position = cam.transform.position;

            if (PickerCamera.MouseOverGui() || !cam.MoveEnabled)
                scroll = 0.0f;
            if (cam.MoveEnabled)
                position += ((cam.transform.right * x + cam.transform.up * y) * flySpeedM + scroll * cam.transform.forward * zoomSpeedM) * Time.deltaTime;

            // Target on - move camera above it
            if (cam.Target != null)
            {
                if (cam.MoveEnabled && Input.GetKeyDown(KeyCode.R))
                {
                    position = cam.Target.transform.position + Vector3.up * 500.0f;
                }
            }
            MoveCamera(cam);
            // Look down to be sure
            cam.transform.rotation = m_LookDown;
            cam.Compass.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
}
