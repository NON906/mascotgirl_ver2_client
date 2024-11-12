using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MascotGirlClient
{
    [RequireComponent(typeof(ClientControl))]
    [RequireComponent(typeof(DictationRecognizerControl))]
    public class ClientControlUI : MonoBehaviour
    {
        [Serializable]
        class Settings
        {
            public int llm_api = 0;
            public string llm_repo_name = "";
            public string llm_file_name = "";
        }

        public GameObject ControlParent;
        public GameObject SettingParent;
        public Button QRCodeButton;
        public TMP_InputField MessageInputField;
        public Button SendMessageButton;
        public Button VoiceInputButton;
        public Button SettingOpenButton;
        public TMP_InputField SystemMessageInputField;
        public GameObject LicenseParent;
        public TMP_Dropdown LlmApiDropdown;
        public TMP_InputField LlmRepoNameInputField;
        public TMP_InputField LlmFileNameInputField;

        List<Selectable> settingUIs_ = new List<Selectable>();

        public void OnClickBackPanel()
        {
            ControlParent.SetActive(!ControlParent.activeSelf);
        }

        public void OnClickSettingOpenButton()
        {
            SettingParent.SetActive(true);
        }

        public void OnClickSettingPanel()
        {
            if (SettingOpenButton.interactable)
            {
                SettingParent.SetActive(false);
            }
        }

        void Start()
        {
            QRCodeButton.gameObject.SetActive(!ClientControl.StartLocal);

            StartCoroutine(getSettings());
        }

        IEnumerator getSettings()
        {
            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            using var webRecvRequest = new UnityWebRequest(url + "/get_settings", "GET")
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
            var response = JsonUtility.FromJson<Settings>(recvResponseString);

            LlmApiDropdown.value = response.llm_api;
            LlmRepoNameInputField.text = response.llm_repo_name;
            LlmRepoNameInputField.gameObject.SetActive(LlmApiDropdown.value == 0);
            LlmFileNameInputField.text = response.llm_file_name;
            LlmFileNameInputField.gameObject.SetActive(LlmApiDropdown.value == 0);
        }

        IEnumerator setSettingCoroutine(string json)
        {
            var client = FindObjectOfType<ClientControl>();
            var url = client.HttpUrl;

            using var webRequest = new UnityWebRequest(url + "/set_setting", "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
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

        [Serializable]
        class SettingValueString
        {
            public string name;
            public string value;
        }

        public void SetSetting(string name, string val)
        {
            var request = new SettingValueString();
            request.name = name;
            request.value = val;

            var jsonRequest = JsonUtility.ToJson(request);

            StartCoroutine(setSettingCoroutine(jsonRequest));
        }

        [Serializable]
        class SettingValueFloat
        {
            public string name;
            public float value;
        }

        public void SetSetting(string name, float val)
        {
            var request = new SettingValueFloat();
            request.name = name;
            request.value = val;

            var jsonRequest = JsonUtility.ToJson(request);

            StartCoroutine(setSettingCoroutine(jsonRequest));
        }

        [Serializable]
        class SettingValueInt
        {
            public string name;
            public int value;
        }

        public void SetSetting(string name, int val)
        {
            var request = new SettingValueInt();
            request.name = name;
            request.value = val;

            var jsonRequest = JsonUtility.ToJson(request);

            StartCoroutine(setSettingCoroutine(jsonRequest));
        }

        [Serializable]
        class SettingValueBool
        {
            public string name;
            public bool value;
        }

        public void SetSetting(string name, bool val)
        {
            var request = new SettingValueBool();
            request.name = name;
            request.value = val;

            var jsonRequest = JsonUtility.ToJson(request);

            StartCoroutine(setSettingCoroutine(jsonRequest));
        }

        public void ChangeInteractables(bool val)
        {
            MessageInputField.interactable = val;
            SendMessageButton.interactable = val;
            VoiceInputButton.interactable = val;
            SettingOpenButton.interactable = val;

            if (!val)
            {
                Selectable[] selectables = SettingParent.GetComponentsInChildren<Selectable>();
                foreach (var selectable in selectables)
                {
                    if (selectable.interactable)
                    {
                        settingUIs_.Add(selectable);
                        selectable.interactable = false;
                    }
                }
            }
            else
            {
                foreach (var selectable in settingUIs_)
                {
                    selectable.interactable = true;
                }
                settingUIs_.Clear();
            }
        }

        public void OnClickSendMessageButton()
        {
            var client = GetComponent<ClientControl>();
            client.StartSendMessage(MessageInputField.text, false);
        }

        public void OnClickVoiceInputButton()
        {
            ChangeInteractables(false);

            var drControl = GetComponent<DictationRecognizerControl>();
            drControl.OnStart();
        }

        public void OnClickedReadQRCodeButton()
        {
            PlayerPrefs.DeleteKey("mascotgirl_httpUrl");
            PlayerPrefs.Save();

            Reconnect.CanConnect = false;

            SceneManager.LoadScene("ReadQRCode");
        }

        public void OnClickLicenseButton()
        {
            LicenseParent.SetActive(true);
        }

        public void OnClickLicensePanel()
        {
            LicenseParent.SetActive(false);
        }

        public void OnChangedAnime4KToggle(bool val)
        {
            PlayerPrefs.SetInt("mascotgirl_anime4K", val ? 1 : 0);
            PlayerPrefs.Save();

            FindObjectOfType<CharaImage>().ExecuteAnime4K();
        }

        public void OnChangeLlmRepoNameInputField(string val)
        {
            SetSetting("llm_repo_name", val);
        }

        public void OnChangeLlmFileNameInputField(string val)
        {
            SetSetting("llm_file_name", val);
        }

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Application.Quit();
                return;
            }
        }
    }
}
