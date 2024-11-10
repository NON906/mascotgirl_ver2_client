using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace MascotGirlClient
{
    [RequireComponent(typeof(TMP_Text))]
    public class License : MonoBehaviour
    {
        IEnumerator Start()
        {
            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            using var webRequest = UnityWebRequest.Get(url + "/license");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError(webRequest.error);
                yield break;
            }

            var responseString = webRequest.downloadHandler.text;
            GetComponent<TMP_Text>().text = responseString;
        }
    }
}
