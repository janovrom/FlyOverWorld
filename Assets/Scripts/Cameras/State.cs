using UnityEngine;

namespace Assets.Scripts.Cameras
{
    /// <summary>
    /// Abstract class, which contains basic functionality for camera, which
    /// is panning and selecting objects and moving camera using its 
    /// character controller.
    /// </summary>
    public abstract class State
    {

        protected Vector3 position = new Vector3();
        private Vector3 m_PreviousPosition;
        protected bool m_MovementDragLocked = false;
        private bool m_DragStarted = false;
        private Vector3 m_StartPosition;
        private Vector3 m_StartCameraPosition;
        private Vector3 m_StartCameraRight;
        private Vector3 m_StartCameraUp;
        private float m_PreviousHitDistance = -1.0f;

        
        /// <summary>
        /// Used for camera pan. If we have input point over the terrain,
        /// we can get distance and how big should approximate distance 
        /// for moving camera be.
        /// </summary>
        /// <param name="cam">PickerCamera for which we are getting results</param>
        /// <param name="dir">inout parameter, since we multiply it by distance</param>
        /// <param name="aspect">aspect of rendering camera</param>
        /// <returns>Returns direction of movement sized to correspond with terrain.</returns>
        private Vector3 GetPerspectiveSize(PickerCamera cam, Vector3 dir, float aspect)
        {
            if (m_PreviousHitDistance < 0.0f)
            {
                // first wasn't raycast hit
                dir.x *= 500.0f * aspect;
                dir.y *= 500.0f;
            }
            else
            {
                // first was raycast hit
                dir.x *= m_PreviousHitDistance * Mathf.Tan(Mathf.Deg2Rad * cam.Camera.fieldOfView / 2.0f) * aspect;
                dir.y *= m_PreviousHitDistance * Mathf.Tan(Mathf.Deg2Rad * cam.Camera.fieldOfView / 2.0f);
            }

            return dir;
        }

        /// <summary>
        /// Moves camera using its character controller. Also checks, if we didn't move
        /// under the ground.
        /// </summary>
        /// <param name="cam">PickerCamera to move</param>
        protected void MoveCamera(PickerCamera cam)
        {
            // make sure that position is above ground
            Ray ray = new Ray();
            ray.origin = new Vector3(position.x, 10000.0f, position.z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Utility.Constants.LAYER_TERRAIN)))
            {
                if (position.y < hit.point.y + 1.8f)
                    position.y = hit.point.y + 1.8f;
                // Max distance of camera is 3kms from ground
                if (position.y - hit.point.y > 3000.0f)
                    position.y = hit.point.y + 3000.0f;
            }
            cam.Character.Move(position - cam.transform.position);
        }

        /// <summary>
        /// Updates state of State. During this update, panning and selection 
        /// is done.
        /// </summary>
        /// <param name="cam">PickerCamera to update</param>
        public virtual void Update(PickerCamera cam)
        {
            // Left mouse picking
            if (cam.PhysicsPick())
            {
                Ray ray = cam.Module.GetRay(cam.Camera);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Utility.Constants.LAYER_PICKABLE)))
                {
                    //Debug.DrawRay(ray.origin, ray.direction, Color.blue, 20.0f);
                    cam.SelectObject(hit.transform.gameObject.GetComponent<Gui.Pickable>());
                } // else: Never deselect on miss ray, it will apply even for gui
                else
                {
                    // We didn't hit anything worth it, like pickable, hit terrain
                    // Try hit terrain, since we are using projectors which don't have any bounding box
                    if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Utility.Constants.LAYER_TERRAIN)))
                    {
                        Nav.ZoneManager.Instance.SelectZone(hit.point);
                    }
                }
            }

            // Middle button pressed down: lock original position
            if (!m_MovementDragLocked && cam.Module.PanningInput() == InputModule.InputStart)
            {
                m_DragStarted = true;
                m_StartPosition = Input.mousePosition;
                m_StartCameraPosition = position;
                m_StartCameraUp = cam.Camera.transform.up;
                m_StartCameraRight = cam.Camera.transform.right;
                // Get distance from ray hit (if any) and decide whether use constant or distance based movement
                Ray ray = cam.Module.GetRay(cam.Camera);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Utility.Constants.LAYER_TERRAIN)))
                {
                    m_PreviousHitDistance = Vector3.Distance(ray.origin, hit.point);
                }
                else
                {
                    m_PreviousHitDistance = -1.0f;
                }
            }

            // Record movement iff drag was initiated by clicking down
            if (m_DragStarted && !m_MovementDragLocked && cam.Module.PanningInput() == InputModule.InputNow)
            {
                Vector3 pos = Input.mousePosition;
                Display d = Display.displays[cam.Camera.targetDisplay];
                Vector3 dir = pos - m_StartPosition;
                dir.x /= d.renderingWidth;
                dir.y /= d.renderingHeight;
                float aspect = (float)d.renderingWidth / (float)d.renderingHeight;
                if (cam.Camera.orthographic)
                {
                    dir.x *= 2.0f * cam.Camera.orthographicSize * aspect;
                    dir.y *= 2.0f * cam.Camera.orthographicSize;
                }
                else
                {
                    dir = GetPerspectiveSize(cam, dir, aspect);
                }

                position = m_StartCameraPosition - m_StartCameraUp * dir.y - m_StartCameraRight * dir.x;
                MoveCamera(cam);
            }

            // Movement stopped
            if (!m_MovementDragLocked && cam.Module.PanningInput() == InputModule.InputEnd)
            {
                m_PreviousHitDistance = -1.0f;
                m_DragStarted = false;
            }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

    }
}
