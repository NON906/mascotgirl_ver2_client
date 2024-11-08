using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace MascotGirlClient
{
    public class Reconnect : MonoBehaviour
    {
        public static bool CanConnect
        {
            get;
            set;
        } = false;

        IEnumerator Start()
        {
            string httpUrl = PlayerPrefs.GetString("mascotgirl_httpUrl", "");
            if (string.IsNullOrEmpty(httpUrl))
            {
                yield break;
            }
            if (httpUrl[httpUrl.Length - 1] == '/')
            {
                httpUrl = httpUrl.Remove(httpUrl.Length - 1);
            }

            using var webRequest = new UnityWebRequest(httpUrl + "/health", "GET")
            {
                downloadHandler = new DownloadHandlerBuffer(),
            };

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                yield break;
            }

            CanConnect = true;
            SceneManager.LoadScene("Client");
        }
    }
}
