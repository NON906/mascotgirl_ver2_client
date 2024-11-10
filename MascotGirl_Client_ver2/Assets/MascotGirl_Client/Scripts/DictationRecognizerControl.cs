using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UnityEngine.Windows.Speech;
#elif UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace MascotGirlClient
{
    [RequireComponent(typeof(ClientControl))]
    [RequireComponent(typeof(ClientControlUI))]
    public class DictationRecognizerControl : MonoBehaviour
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        DictationRecognizer dictationRecognizer_;
#elif UNITY_ANDROID
        AndroidJavaObject libObj_;
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        void dictationRecognizerInit()
        {
            dictationRecognizer_ = new DictationRecognizer(ConfidenceLevel.High);

            //dictationRecognizer_.AutoSilenceTimeoutSeconds = float.MaxValue;

            dictationRecognizer_.DictationResult += (text, confidence) =>
            {
                var client = GetComponent<ClientControl>();
                var clientUI = GetComponent<ClientControlUI>();

                clientUI.MessageInputField.text = text;
                client.StartSendMessage(text, true);

                dictationRecognizer_.Stop();
            };
        }
#endif

        void Start()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            dictationRecognizerInit();
#elif UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }

            using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activityObj = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                libObj_ = new AndroidJavaObject("online.mumeigames.voicerecognizerlibrary.VoiceRecognizerLibrary", activityObj);
            }
#endif
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

#elif UNITY_ANDROID
        IEnumerator startVoiceRecognizerAndroid()
        {
            libObj_.Call("start");

            string result;
            bool isError;
            do
            {
                yield return null;
                result = libObj_.Call<string>("getResult");
                isError = libObj_.Call<bool>("getIsError");
            } while (result == "" && !isError);

            if (isError)
            {
                yield break;
            }

            var client = GetComponent<ClientControl>();
            var clientUI = GetComponent<ClientControlUI>();

            clientUI.MessageInputField.text = result;
            client.StartSendMessage(result, true);
        }
#endif

        public void OnStart()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            dictationRecognizer_.Start();
#elif UNITY_ANDROID
            StartCoroutine(startVoiceRecognizerAndroid());
#endif
        }

        public void OnFinish()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (dictationRecognizer_.Status != SpeechSystemStatus.Running)
            {
                var clientUI = GetComponent<ClientControlUI>();
                clientUI.ChangeInteractables(false);
                dictationRecognizer_.Start();
            }
#elif UNITY_ANDROID
            StartCoroutine(startVoiceRecognizerAndroid());
#endif
        }
    }
}
