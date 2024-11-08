using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityCommandLineParser;
using System;

namespace MascotGirlClient
{
    [RequireComponent(typeof(ClientControlUI))]
    [RequireComponent(typeof(DictationRecognizerControl))]
    [RequireComponent(typeof(ChatControl))]
    public class ClientControl : MonoBehaviour
    {
        public const string HTTP_URL_DEFAULT = "http://localhost:55007";

        public string HttpUrl = HTTP_URL_DEFAULT;

        static bool startLocal_ = false;

        [CommandLineCommand("start_local")]
        private static void startLocalStatic()
        {
            startLocal_ = true;

            var obj = FindObjectOfType<ClientControl>();
            if (obj != null)
            {
                obj.HttpUrl = HTTP_URL_DEFAULT;
            }
        }

        public void StartSendMessage(string message, bool isVoiceInput)
        {
            StartCoroutine(sendMessage(message, isVoiceInput));
        }

        IEnumerator sendMessage(string message, bool isVoiceInput)
        {
            var clientUI = GetComponent<ClientControlUI>();
            clientUI.ChangeInteractables(false);

            string url = HttpUrl;
            if (string.IsNullOrEmpty(url))
            {
                url = HTTP_URL_DEFAULT;
            }
            if (url[url.Length - 1] == '/')
            {
                url = url.Remove(url.Length - 1);
            }

            var chatControl = GetComponent<ChatControl>();
            yield return chatControl.Chat(url, message);

            clientUI.MessageInputField.text = "";
            if (!isVoiceInput)
            {
                clientUI.ChangeInteractables(true);
            }
            else
            {
                var drControl = GetComponent<DictationRecognizerControl>();
                drControl.OnFinish();
            }
        }
    }
}
