using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MascotGirlClient
{
    public class ChatControl : MonoBehaviour
    {
        List<SendMessageContext> messages_ = new List<SendMessageContext>();

        [TextArea]
        public string SystemMessage = "";

        [Serializable]
        class SendMessageContext
        {
            public string role;
            public string content;
        }

        [Serializable]
        class SendMessageRequest
        {
            public List<SendMessageContext> messages;
        }

        [Serializable]
        class SendMessageResponse
        {
            public bool is_success;
        }

        [Serializable]
        class RecvMessageResponse
        {
            public string eyebrow;
            public string eyes;
            public string message;
            public bool is_finished;
        }

        public IEnumerator Chat(string url, string message)
        {
            if (messages_.Count <= 0)
            {
                messages_.Add(new SendMessageContext { role = "system", content = SystemMessage });
            }
            messages_.Add(new SendMessageContext { role = "user", content = message });

            var request = new SendMessageRequest();
            request.messages = messages_;

            var jsonRequest = JsonUtility.ToJson(request);

            using var webRequest = new UnityWebRequest(url + "/chat_hermes_infer", "POST")
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
            var sendResponse = JsonUtility.FromJson<SendMessageResponse>(responseString);

            if (!sendResponse.is_success)
            {
                UnityEngine.Debug.LogError("Response error.");
                yield break;
            }

            RecvMessageResponse response = null;
            do
            {
                using var webRecvRequest = new UnityWebRequest(url + "/get_chat_hermes_infer", "GET")
                {
                    downloadHandler = new DownloadHandlerBuffer(),
                };

                yield return webRecvRequest.SendWebRequest();

                if (webRecvRequest.result == UnityWebRequest.Result.ConnectionError || webRecvRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    UnityEngine.Debug.LogError(webRecvRequest.error);
                    yield break;
                }

                var recvResponseString = webRecvRequest.downloadHandler.text;
                response = JsonUtility.FromJson<RecvMessageResponse>(recvResponseString);

                if (!response.is_finished)
                {
                    yield return new WaitForSecondsRealtime(0.05f);
                }

            } while (!response.is_finished);

            messages_.Add(new SendMessageContext { role = "assistant", content = response.message });

            // TODO: 音声再生

            // TODO: 表情変更

            // TODO: メッセージ変更
            UnityEngine.Debug.Log(response.message);
        }
    }
}
