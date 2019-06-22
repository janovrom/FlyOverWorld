using UnityEngine;
using System;

namespace Assets.Scripts.Cameras
{

    [Obsolete]
    /// <summary>
    /// Unused.
    /// </summary>
    public class GimbalState : State
    {

        public float flySpeedM = 10.0f;
        public float zoomSpeedM = 5.0f;
        private Quaternion m_LookDown;

        // Update is called once per frame
        public override void Update(PickerCamera cam)
        {
            base.Update(cam);
            cam.transform.position = cam.Target.transform.position;
            cam.transform.rotation = cam.Target.transform.rotation;
        }
    }
}
