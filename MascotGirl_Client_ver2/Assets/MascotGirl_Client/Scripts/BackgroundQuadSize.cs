using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MascotGirlClient
{
    [RequireComponent(typeof(Renderer))]
    public class BackgroundQuadSize : MonoBehaviour
    {
        public Camera TargetCamera;

        void Update()
        {
            if (GetComponent<Renderer>().material.mainTexture == null)
            {
                return;
            }
            var texture = GetComponent<Renderer>().material.mainTexture;

            var scale = Mathf.Tan(TargetCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * (transform.position - TargetCamera.transform.position).magnitude * 2f;
            var aspect = (float)texture.width / texture.height;
            if (aspect > TargetCamera.aspect)
            {
                transform.localScale = scale * new Vector3(aspect, 1f, 1f);
            }
            else
            {
                transform.localScale = scale * new Vector3(1f, 1f / aspect, 1f);
            }
        }
    }
}
