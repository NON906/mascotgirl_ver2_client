using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MascotGirlClient
{
    [RequireComponent(typeof(Renderer))]
    public class BackgroundImage : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(getImage());
        }

        public void Restart()
        {
            StartCoroutine(getImage());
        }

        IEnumerator getImage()
        {
            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url + "/get_background_image");

            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError(uwr.error);
                yield break;
            }

            GetComponent<Renderer>().material.mainTexture = DownloadHandlerTexture.GetContent(uwr);
        }
    }
}
