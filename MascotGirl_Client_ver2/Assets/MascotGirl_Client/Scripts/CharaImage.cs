using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using uLipSync;
using UnityEngine;
using UnityEngine.Networking;

namespace MascotGirlClient
{
    [RequireComponent(typeof(Renderer))]
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
        Coroutine changeCoroutine_;

        void Start()
        {
            StartCoroutine(startProcess());
        }

        public void Restart()
        {
            StartCoroutine(startProcess());
        }

        IEnumerator startProcess()
        {
            isFinishedStart_ = false;

#if UNITY_EDITOR || UNITY_STANDALONE
            var request = new CopyImagesRequest();
            request.path = Path.Combine(Application.temporaryCachePath, "mascotgirl", "chara_images");

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

            charaTextures_.Clear();
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
#else
            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            charaTextures_.Clear();
            foreach (var eyebrow in eyebrowOptions_)
            {
                foreach (var eye in eyeOptions_)
                {
                    foreach (var mouth in mouthOptions_)
                    {
                        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture($"{url}/get_image?id={eyebrow}_{eye}_{mouth}");

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
#endif

            isFinishedStart_ = true;

            Change("normal", "normal");
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
            if (!isFinishedStart_)
            {
                return;
            }

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

        public void Change(string eyebrow, string eye)
        {
            if (!isFinishedStart_)
            {
                return;
            }

            for (int loop = 0; loop < eyebrowOptions_.Length; loop++)
            {
                if (eyebrowOptions_[loop] == eyebrow)
                {
                    eyebrowIndex_ = loop;
                }
            }
            for (int loop = 0; loop < eyeOptions_.Length; loop++)
            {
                if (eyeOptions_[loop] == eye)
                {
                    eyeIndex_ = loop;
                }
            }

            if (changeCoroutine_ != null)
            {
                StopCoroutine(changeCoroutine_);
            }
            changeCoroutine_ = StartCoroutine(blink(eyeIndex_));
        }

        IEnumerator blink(int startEyeIndex)
        {
            float startTime = Time.time;
            float normalTime = UnityEngine.Random.Range(4f, 6f);
            float closeTime = 0.08f;
            float halfTime = 0.08f;

            while (eyeOptions_[startEyeIndex] == "normal" || eyeOptions_[startEyeIndex] == "surprized")
            {
                float time = Time.time - startTime;
                if (time < normalTime)
                {
                    eyeIndex_ = startEyeIndex;
                }
                else if (time < normalTime + closeTime)
                {
                    eyeIndex_ = 2;
                }
                else if (time < normalTime + closeTime + halfTime)
                {
                    eyeIndex_ = 1;
                }
                else
                {
                    eyeIndex_ = startEyeIndex;
                    startTime = Time.time;
                }
                yield return null;
            }
        }
    }
}
