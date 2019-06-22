using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Models
{

    /// <summary>
    /// Animation, which rotates object around its local axis
    /// specified by property Axis. RotationTime tells, how much
    /// one rotation takes.
    /// </summary>
    class RotationAnimation : MonoBehaviour
    {

        public float RotationTime = 1.0f;
        public Vector3 Axis = Vector3.up;

        void Update()
        {
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Axis * 360.0f * Time.deltaTime / RotationTime);
        }

    }
}
