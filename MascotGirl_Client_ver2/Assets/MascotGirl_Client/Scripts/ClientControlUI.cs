using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MascotGirlClient
{
    [RequireComponent(typeof(ClientControl))]
    [RequireComponent(typeof(DictationRecognizerControl))]
    public class ClientControlUI : MonoBehaviour
    {
        public GameObject ControlParent;
        public GameObject SettingParent;
        public Button QRCodeButton;
        public TMP_InputField MessageInputField;
        public Button SendMessageButton;
        public Button VoiceInputButton;
        public Button SettingOpenButton;

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
            SettingParent.SetActive(false);
        }

        void Start()
        {
            if (!SceneManager.GetSceneByName("ReadQRCode").IsValid())
            {
                QRCodeButton.gameObject.SetActive(false);
            }
        }

        public void ChangeInteractables(bool val)
        {
            MessageInputField.interactable = val;
            SendMessageButton.interactable = val;
            VoiceInputButton.interactable = val;
            SettingOpenButton.interactable = val;
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
    }
}
