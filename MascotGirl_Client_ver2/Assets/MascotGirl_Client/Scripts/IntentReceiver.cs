using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MascotGirlClient
{
    public class IntentReceiver : MonoBehaviour
    {
        public static bool IsRecieved
        {
            get;
            private set;
        } = false;

        void Start()
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
            using (var uri = intent.Call<AndroidJavaObject>("getData"))
            {
                if (uri != null)
                {
                    string uriStr = uri.Call<string>("toString");
                    if (uriStr.StartsWith("mascotgirl://"))
                    {
                        PlayerPrefs.SetString("mascotgirl_httpUrl", uriStr.Remove(0, "mascotgirl://".Length));
                        PlayerPrefs.Save();

                        IsRecieved = true;
                        SceneManager.LoadScene("Client");
                    }
                }
            }
        }
    }
}
