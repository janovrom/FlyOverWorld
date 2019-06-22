using UnityEngine;

namespace Assets.Scripts.Models
{

    /// <summary>
    /// Scales model in 3 different lods if scaling is enabled. Otherwise
    /// size of model is set to 1. First lod is real size, second lod is 
    /// transforming between minimum and maximum size and last is log10
    /// increment.
    /// </summary>
    class ModelScaler : MonoBehaviour
    {

        /// <summary>
        /// Keeps the size, which was in this distance, constant.
        /// </summary>
        private const float DistanceOfRealSizeM = 10.0f;
        private const float DefaultDistanceM = 500.0f;

        private const float LODDistance0M = 50.0f;
        private const float LODDistance1M = 500.0f;
        private const float MaxSizeLOD1 = 20.0f;
        private const float MaxSizeLOD2 = 30.0f;


        void Update()
        {
            if (Utility.Settings.SCALING_ENABLED)
            {
                float dist = Vector3.Distance(Camera.main.transform.parent.position, transform.position);
                if (Utility.Settings.SCALING_MIN_MAX_SIZE)
                {
                    // Until LODDistance0M keep real size
                    // From LODDistance0M scale linearly until MaxSizeLOD
                    // From LODDistance1M scale log10
                    if (dist < LODDistance0M)
                    {
                        transform.localScale = Vector3.one;
                    }
                    else if (dist < LODDistance1M)
                    {
                        float scale = MaxSizeLOD1 * (dist - LODDistance0M) / (LODDistance1M - LODDistance0M) + 1.0f;
                        transform.localScale = new Vector3(scale, scale, scale);
                    }
                    else
                    {
                        float scale = Mathf.Min(Mathf.Log10(dist - LODDistance1M) + MaxSizeLOD1, MaxSizeLOD2);
                        transform.localScale = new Vector3(scale, scale, scale);
                    }
                }
                else
                {
                    float scale = Mathf.Max(dist / DistanceOfRealSizeM, 1.0f);
                    transform.localScale = new Vector3(scale, scale, scale);
                }
            }
            else
            {
                // It could be switched off during play
                transform.localScale = Vector3.one;
            }
        }

    }
}
