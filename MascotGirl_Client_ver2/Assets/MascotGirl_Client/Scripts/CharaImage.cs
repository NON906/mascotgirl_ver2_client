using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using uLipSync;
using UnityEngine;
using UnityEngine.Networking;

namespace MascotGirlClient
{
    public class CharaImage : MonoBehaviour
    {
        [Serializable]
        class CopyImagesRequest
        {
            public string path;
        }

        [Serializable]
        class CopyImagesResponse
        {
            public bool is_success;
        }

        static readonly string[] eyebrowOptions_ = new[] { "normal", "troubled", "angry", "happy", "serious" };
        static readonly string[] eyeOptions_ = new[] { "normal", "half", "closed", "happy_closed", "relaxed_closed", "surprized", "wink" };
        static readonly string[] mouthOptions_ = new[] { "normal", "aaa", "iii", "uuu", "eee", "ooo" };

        List<Texture2D> charaTextures_ = new List<Texture2D>();
        int eyebrowIndex_ = 0;
        int eyeIndex_ = 0;
        int mouthIndex_ = 0;
        bool isFinishedStart_ = false;

        IEnumerator Start()
        {
            var request = new CopyImagesRequest();
            request.path = Path.Combine(Application.temporaryCachePath, "mascotgirl/chara_images");

            var jsonRequest = JsonUtility.ToJson(request);

            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            using var webRequest = new UnityWebRequest(url + "/copy_images", "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRequest)),
                downloadHandler = new DownloadHandlerBuffer(),
            };

            webRequest.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError(webRequest.error);
                yield break;
            }

            var responseString = webRequest.downloadHandler.text;
            var sendResponse = JsonUtility.FromJson<CopyImagesResponse>(responseString);

            if (!sendResponse.is_success)
            {
                UnityEngine.Debug.LogError("Response error.");
                yield break;
            }

            foreach (var eyebrow in eyebrowOptions_)
            {
                foreach (var eye in eyeOptions_)
                {
                    foreach (var mouth in mouthOptions_)
                    {
                        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture($"file://{request.path}/{eyebrow}_{eye}_{mouth}.png");

                        yield return uwr.SendWebRequest();

                        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                        {
                            UnityEngine.Debug.LogError(uwr.error);
                            yield break;
                        }

                        charaTextures_.Add(DownloadHandlerTexture.GetContent(uwr));
                    }
                }
            }

            isFinishedStart_ = true;
        }

        void Update()
        {
            if (!isFinishedStart_)
            {
                return;
            }

            GetComponent<Renderer>().material.mainTexture = charaTextures_[eyebrowIndex_ * eyeOptions_.Length * mouthOptions_.Length + eyeIndex_ * mouthOptions_.Length + mouthIndex_];
        }

        public void OnLipSyncUpdate(LipSyncInfo info)
        {
            if (info.volume >= Mathf.Epsilon)
            {
                switch (info.phoneme)
                {
                    case "A":
                        mouthIndex_ = 1;
                        break;
                    case "I":
                        mouthIndex_ = 2;
                        break;
                    case "U":
                        mouthIndex_ = 3;
                        break;
                    case "E":
                        mouthIndex_ = 4;
                        break;
                    case "O":
                        mouthIndex_ = 5;
                        break;
                    default:
                        mouthIndex_ = 0;
                        break;
                }
            }
            else
            {
                mouthIndex_ = 0;
            }
        }
    }
}
