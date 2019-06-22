using UnityEngine;
using Assets.Scripts.Utility;

namespace Assets.Scripts.Cameras
{

    /// <summary>
    /// Allows free movement, camera rotation and even more. Enables movement
    /// above terrain in the same altitude using WASD, allows using mouse
    /// scroll to zoom and look towards target using R.
    /// NOTE: It can see in the future and will go straight to hell when deallocated.
    /// </summary>
    public class FreeState : RotationState
    {

        // Some specific defines
        public float flySpeedM = 500.0f;
        public float climbSpeedM = 450.0f;
        public float followDistanceM = 200.0f;
        public float zoomSpeedM = 650.0f;


        public FreeState()
        {
            position = new Vector3(0, 1200.0f, 240.0f);
        }

        /// <summary>
        /// Enables movement
        /// above terrain in the same altitude using WASD, allows using mouse
        /// scroll to zoom and look towards target using R.
        /// </summary>
        /// <param name="cam">PickerCamera which called the update</param>
        public override void Update(PickerCamera cam)
        {
            // share position and rotation with another states
            m_AngleXDeg = cam.transform.rotation.eulerAngles.x;
            if (m_AngleXDeg > 90.0f)
                m_AngleXDeg -= 360.0f;
            m_AngleYDeg = cam.transform.rotation.eulerAngles.y;
            position = cam.transform.position;
            
            // Rotate Camera
            base.Update(cam);

            // Handle movement
            float x = 0;
            float y = 0;
            float up = cam.MoveEnabled && Input.GetKey(KeyCode.Space) ? climbSpeedM : 0;
            if (cam.MoveEnabled)
            {
                x = Input.GetAxis("Horizontal");
                y = Input.GetAxis("Vertical");
                if (Input.GetKey(KeyCode.LeftShift))
                    up = -up;
            }
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (PickerCamera.MouseOverGui() || !cam.MoveEnabled)
                scroll = 0.0f;

            // Move camera in XZ
            if (cam.MoveEnabled)
            {
                Ray ray = cam.Camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                // Change position only in xz
                if (scroll > 0.0f && Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Utility.Constants.LAYER_TERRAIN)))
                {
                    float zoomSpeed = (hit.point - ray.origin).magnitude;
                    zoomSpeed = Mathf.Min(Mathf.Max(350.0f, zoomSpeed), 1000.0f);
                    position += ((cam.transform.right.xz().ToVector3xz() * x + cam.transform.forward.xz().ToVector3xz() * y) * flySpeedM + scroll * (hit.point - ray.origin).normalized * zoomSpeed + cam.transform.up * up) * Time.deltaTime;
                }
                else
                {
                    position += ((cam.transform.right.xz().ToVector3xz() * x + cam.transform.forward.xz().ToVector3xz() * y) * flySpeedM + scroll * cam.transform.forward * zoomSpeedM + cam.transform.up * up) * Time.deltaTime;
                }
            }

            // Rotate towards target
            if (cam.Target != null)
            {
                if (cam.MoveEnabled && Input.GetKeyDown(KeyCode.R))
                {
                    Vector3 newDir = cam.Target.transform.position - position;
                    position = -(newDir).normalized * Mathf.Min(followDistanceM, newDir.magnitude) + cam.Target.transform.position;
                    cam.transform.rotation = Quaternion.LookRotation(newDir);
                    m_AngleXDeg = cam.transform.rotation.eulerAngles.x;
                    m_AngleYDeg = cam.transform.rotation.eulerAngles.y;
                }
            }
            MoveCamera(cam);
        }
    }
}
