using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;

namespace MascotGirlClient
{
    public class ReadQRCode : MonoBehaviour
    {
        public static bool IsCaptured
        {
            get;
            private set;
        } = false;

        WebCamTexture webCamTexture_ = null;

        void Awake()
        {
#if UNITY_ANDROID
            Permission.RequestUserPermission(Permission.Camera);
#endif
        }

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Application.Quit();
                return;
            }

            if (webCamTexture_ == null)
            {
                var width = Screen.width;
                var height = Screen.height;
                webCamTexture_ = new WebCamTexture(height, width);
                webCamTexture_.Play();
                GetComponent<Renderer>().material.mainTexture = webCamTexture_;
                transform.localScale = new Vector3(2f, width * 2f / height, 2f);
            }
            else
            {
                var reader = new BarcodeReader();
                var rawRGB = webCamTexture_.GetPixels32();
                var width = webCamTexture_.width;
                var height = webCamTexture_.height;
                var result = reader.Decode(rawRGB, width, height);

                if (result != null && result.Text != null && result.Text.StartsWith("mascotgirl2://"))
                {
                    PlayerPrefs.SetString("mascotgirl_httpUrl", result.Text.Remove(0, "mascotgirl2://".Length));
                    PlayerPrefs.Save();

                    IsCaptured = true;
                    SceneManager.LoadScene("Client");
                }
            }
        } 
    }
}
