using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MascotGirlClient
{
    public class CharaQuadSize : MonoBehaviour
    {
        public Camera TargetCamera;

        void Update()
        {
            var scale = Mathf.Tan(TargetCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * (transform.position - TargetCamera.transform.position).magnitude * 2f;
            transform.localScale = scale * new Vector3(1f, 1f, 1f);
        }
    }
}
