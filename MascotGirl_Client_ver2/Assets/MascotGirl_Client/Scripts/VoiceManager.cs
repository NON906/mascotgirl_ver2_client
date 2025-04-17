using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace MascotGirlClient
{
    public class VoiceManager : MonoBehaviour
    {
        class Voice
        {
            public string message;
            public AudioClip audioClip;
        }

        [Serializable]
        class VoiceInferRequest
        {
            public string text;
            public string format_ext;
        }

        [Serializable]
        class VoiceInferResponse
        {
            public bool is_success;
        }

        public AudioSource TargetAudioSource;
        public TMP_Text MessageText;

        List<Voice> voices_ = new List<Voice>();
        Coroutine addCoroutineObj_ = null;
        int playIndex_ = -1;

        public void Add(string voiceMessage)
        {
            addCoroutineObj_ = StartCoroutine(AddCoroutine(voiceMessage));
        }

        public IEnumerator AddCoroutine(string voiceMessage)
        {
            var voice = new Voice();
            voice.message = voiceMessage;

            var request = new VoiceInferRequest();
            request.text = voiceMessage;

#if UNITY_EDITOR || UNITY_STANDALONE
            var audioType = AudioType.WAV;
            request.format_ext = "wav";
#else
            var audioType = AudioType.OGGVORBIS;
            request.format_ext = "ogg";
#endif

            var client = FindObjectOfType<ClientControl>();
            var downloadUrl = client.HttpUrl;

            var jsonRequest = JsonUtility.ToJson(request);

            using var webRequest = new UnityWebRequest(downloadUrl + "/voice_infer", "POST")
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
            var sendResponse = JsonUtility.FromJson<VoiceInferResponse>(responseString);

            if (!sendResponse.is_success)
            {
                UnityEngine.Debug.LogError("Response error.");
                yield break;
            }

            using var webRequestForAudio = UnityWebRequestMultimedia.GetAudioClip(downloadUrl + "/get_voice_infer", audioType);

            yield return webRequestForAudio.SendWebRequest();

            if (webRequestForAudio.result == UnityWebRequest.Result.ConnectionError || webRequestForAudio.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError(webRequestForAudio.error);
                yield break;
            }

            voice.audioClip = DownloadHandlerAudioClip.GetContent(webRequestForAudio);

            voices_.Add(voice);
        }

        public void Clear()
        {
            voices_.Clear();
            playIndex_ = -1;
            TargetAudioSource.Stop();
            if (addCoroutineObj_ != null)
            {
                StopCoroutine(addCoroutineObj_);
            }
        }

        void Update()
        {
            if (!TargetAudioSource.isPlaying)
            {
                var newText = "";
                for (int loopIdx = 0; loopIdx <= playIndex_; loopIdx++)
                {
                    newText += voices_[loopIdx].message;
                }
                MessageText.text = newText;

                if (voices_.Count > playIndex_ + 1)
                {
                    playIndex_++;
                    TargetAudioSource.clip = voices_[playIndex_].audioClip;
                    TargetAudioSource.Play();
                }
            }
            else
            {
                var newText = "";
                for (int loopIdx = 0; loopIdx < playIndex_; loopIdx++)
                {
                    newText += voices_[loopIdx].message;
                }
                var stringInfo = new StringInfo(voices_[playIndex_].message);
                var fullLength = stringInfo.LengthInTextElements;
                var playProcess = TargetAudioSource.time / TargetAudioSource.clip.length;
                var showLength = (int)(fullLength * playProcess);
                newText += stringInfo.SubstringByTextElements(0, showLength);
                MessageText.text = newText;
            }
        }
    }
}
