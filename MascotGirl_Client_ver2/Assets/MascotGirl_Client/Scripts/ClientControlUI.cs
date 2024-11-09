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
        public TMP_InputField SystemMessageInputField;

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
    }
}
