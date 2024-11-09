using Keiwando.NFSO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Windows.Forms;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace MascotGirlClient
{
    public class UploadBackgroundImage : MonoBehaviour
    {
        [Serializable]
        class Response
        {
            public bool is_success;
        }

        public void OnClick()
        {
            SupportedFileType[] supportedFileTypes = {
                SupportedFileType.PNG,
                SupportedFileType.JPEG,
                SupportedFileType.Any
            };

            NativeFileSO.shared.OpenFile(supportedFileTypes,
                delegate (bool fileWasOpened, OpenedFile file) {
                    if (fileWasOpened)
                    {
                        StartCoroutine(upload(file.Data, file.Name));
                    }
                });
        }

        IEnumerator upload(byte[] fileData, string fileName)
        {
            var clientUI = FindObjectOfType<ClientControlUI>();
            clientUI.ChangeInteractables(false);

            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection("file", fileData, fileName, "multipart/form-data"));

            using var webRequest = UnityWebRequest.Post(url + "/upload_background_image", formData);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError(webRequest.error);
                clientUI.ChangeInteractables(true);
                yield break;
            }

            var responseString = webRequest.downloadHandler.text;
            var response = JsonUtility.FromJson<Response>(responseString);

            if (!response.is_success)
            {
                UnityEngine.Debug.LogError("Response error.");
                clientUI.ChangeInteractables(true);
                yield break;
            }

            FindObjectOfType<BackgroundImage>().Restart();
            clientUI.ChangeInteractables(true);
        }
    }
}
