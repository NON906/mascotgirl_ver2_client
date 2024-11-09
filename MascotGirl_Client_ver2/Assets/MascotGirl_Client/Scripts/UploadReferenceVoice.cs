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
    public class UploadReferenceVoice : MonoBehaviour
    {
        public void OnClick()
        {
            SupportedFileType[] supportedFileTypes = {
                new SupportedFileType {
                    Name = "WAV audio",
                    Extension = "wav",
                    MimeType = "audio/wav"
                },
                SupportedFileType.MP3,
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
            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection("file", fileData, fileName, "multipart/form-data"));

            using var webRequest = UnityWebRequest.Post(url + "/upload_reference_voice", formData);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError(webRequest.error);
                yield break;
            }
        }
    }
}
