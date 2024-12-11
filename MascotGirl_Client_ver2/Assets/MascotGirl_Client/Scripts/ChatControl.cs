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
        List<SendMessageContent> messages_ = new List<SendMessageContent>();

        [TextArea]
        public string SystemMessage = "";

        [Serializable]
        class SendMessageContent
        {
            public string role;
            public string content;
            public string tool_calls;
        }

        [Serializable]
        class SendMessageRequest
        {
            public List<SendMessageContent> messages;
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
            public SendMessageContent[] history;
            public bool is_finished;
        }

        [Serializable]
        class SetSystemMessageRequest
        {
            public string message;
        }

        [Serializable]
        class GetSystemMessageResponse
        {
            public string message;
        }

        IEnumerator Start()
        {
            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            using var webRecvRequest = new UnityWebRequest(url + "/get_system_message", "GET")
            {
                downloadHandler = new DownloadHandlerBuffer(),
            };

            yield return webRecvRequest.SendWebRequest();

            if (webRecvRequest.result == UnityWebRequest.Result.ConnectionError || webRecvRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError(webRecvRequest.error);
                FindObjectOfType<ClientControlUI>().SystemMessageInputField.text = SystemMessage;
                yield break;
            }

            var recvResponseString = webRecvRequest.downloadHandler.text;
            var response = JsonUtility.FromJson<GetSystemMessageResponse>(recvResponseString);

            SystemMessage = response.message;

            FindObjectOfType<ClientControlUI>().SystemMessageInputField.text = SystemMessage;
        }

        public void SetSystemMessage(string message)
        {
            SystemMessage = message;
            if (messages_.Count > 0 && messages_[0].role == "system")
            {
                messages_[0].content = SystemMessage;
            }

            StartCoroutine(setSystemMessageCoroutine(message));
        }

        IEnumerator setSystemMessageCoroutine(string message)
        {
            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            var request = new SetSystemMessageRequest();
            request.message = message;

            var jsonRequest = JsonUtility.ToJson(request);

            using var webRequest = new UnityWebRequest(url + "/set_system_message", "POST")
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
        }

        public IEnumerator Chat(string url, string message)
        {
            if (messages_.Count <= 0)
            {
                messages_.Add(new SendMessageContent { role = "system", content = SystemMessage });
            }

            if (messages_.Count > 0 && messages_[messages_.Count - 1].role == "tool" && messages_[messages_.Count - 1].content == "<Š®—¹>")
            {
                messages_[messages_.Count - 1].content = "user‚Ì”­Œ¾:\n" + message;
            }
            else
            {
                messages_.Add(new SendMessageContent { role = "user", content = message });
            }

            RecvMessageResponse response = null;
            do
            {
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
                        yield return new WaitForSecondsRealtime(0.5f);
                    }
                    //UnityEngine.Debug.Log(response.message);

                } while (!response.is_finished);

            } while (string.IsNullOrEmpty(response.message));

            messages_.AddRange(response.history);

            var voiceManager = FindObjectOfType<VoiceManager>();
            voiceManager.Clear();
            yield return voiceManager.AddCoroutine(response.message);

            FindObjectOfType<CharaImage>().Change(response.eyebrow, response.eyes);
        }
    }
}
