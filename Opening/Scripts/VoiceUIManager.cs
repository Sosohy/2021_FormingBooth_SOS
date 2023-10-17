using FrostweepGames.Plugins.Native;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.VoicePro.Examples
{
    public class VoiceUIManager : MonoBehaviour
    {
        private List<ServerSpeakerItem> serverSpeakerItems;

        public Transform parentOfRemoteClients; // 스피커 들을 모든 client들 위치 설정할 값

        public Toggle mySpeakToggle; // 내 목소리 전달할건지 다른 사람한테

        public GameObject remoteClientPrefab; // vcp_speaker_item_prefab 스피커로 만들 프리팹

        public Dropdown microphonesDropdown; // mircophone 선택


        /**필수 옵션*/
        public Recorder recorder;
        public Listener listener;

        private void Start()
        {
            serverSpeakerItems = new List<ServerSpeakerItem>();
            listener.SpeakersUpdatedEvent += SpeakersUpdatedEventHandler;

            Debug.Log("test");
            InitSetting();
        }

        private void Update()
        {
            foreach (var item in serverSpeakerItems)
            {
                item.Update();
            }
        }


        private void InitSetting()
        {
            SettingMicrophones();
            //MuteAllListeners(true); 
            recorder.debugEcho = false;
            SetReliableTransmission(true);
            mySpeakToggle.onValueChanged.AddListener(SpeakToggleValueChanged);
            microphonesDropdown.onValueChanged.AddListener(MicrophoneDropdownOnValueChanged);
        }


        private void SpeakersUpdatedEventHandler(List<Speaker> speakers)
        {

            Debug.Log("test");

            if (serverSpeakerItems.Count > 0)
            {
                foreach (var item in serverSpeakerItems)
                {
                    if (!speakers.Contains(item.Speaker))
                    {
                        item.Dispose();
                    }
                }
            }

            foreach (var speaker in speakers)
            {
                if (serverSpeakerItems.Find(item => item.Speaker == speaker) == null)
                {
                    serverSpeakerItems.Add(new ServerSpeakerItem(parentOfRemoteClients, remoteClientPrefab, speaker, recorder));
                }
            }
        }


        private void SettingMicrophones()
        {
            CustomMicrophone.RequestMicrophonePermission();

            microphonesDropdown.ClearOptions();
            microphonesDropdown.AddOptions(CustomMicrophone.devices.ToList());

            if (CustomMicrophone.HasConnectedMicrophoneDevices())
            {
                recorder.SetMicrophone(CustomMicrophone.devices[0]);
            }
        }

        private void MicrophoneDropdownOnValueChanged(int index)
        {
            if (CustomMicrophone.HasConnectedMicrophoneDevices())
            {
                recorder.SetMicrophone(CustomMicrophone.devices[index]);
            }
        }

        private void SpeakToggleValueChanged(bool status)
        {
            if (status)
            {
                if (!NetworkRouter.Instance.ReadyToTransmit || !recorder.StartRecord())
                {
                    mySpeakToggle.isOn = false;
                }
            }
            else
            {
                recorder.StopRecord();
            }
        }


        private void MuteAllListeners(bool status)
        {
            listener.SetMuteStatus(status);
        }


        private void SetReliableTransmission(bool status)
        {
            recorder.reliableTransmission = status;
        }


        private class ServerSpeakerItem
        {
            private GameObject _selfObject;

            private Text _speakerNameText;

            private Toggle _muteToggle;

            private Toggle _notTalkingToggle;

            public Speaker Speaker { get; private set; }

            public Recorder recorder;

            public ServerSpeakerItem(Transform parent, GameObject prefab, Speaker speaker, Recorder recorder)
            {
                Debug.Log("test");
                Speaker = speaker;
                this.recorder = recorder;
                _selfObject = Instantiate(prefab, parent, false);
                _speakerNameText = _selfObject.transform.Find("Text").GetComponent<Text>();
                _speakerNameText.GetComponent<Button>().onClick.AddListener(OnClickSpeakerText);
                _muteToggle = _selfObject.transform.Find("Remote IsTalking").GetComponent<Toggle>();
                _notTalkingToggle = _selfObject.transform.Find("Remote_NotTalking").GetComponent<Toggle>();

                _speakerNameText.text = Speaker.Name;
                _muteToggle.onValueChanged.AddListener(MuteToggleValueChangedEventHandler);
                _notTalkingToggle.onValueChanged.AddListener(MuteToggleNotTalkingValueChangedEventHandler);
            }

            public void Update()
            {
                if (_speakerNameText.text == "admin1@")
                {
                    _selfObject.SetActive(false);
                }
                else
                {
                    _notTalkingToggle.gameObject.SetActive(!Speaker.Playing);
                    _muteToggle.gameObject.SetActive(Speaker.Playing);
                }
                
            }

            public void Dispose()
            {
                MonoBehaviour.Destroy(_selfObject);
            }

            private void MuteToggleValueChangedEventHandler(bool value)
            {
                if (!_muteToggle.gameObject.activeInHierarchy)
                    return;

                Speaker.IsMute = value;

                _notTalkingToggle.isOn = value;
            }


            private void MuteToggleNotTalkingValueChangedEventHandler(bool value)
            {
                if (!_notTalkingToggle.gameObject.activeInHierarchy)
                    return;

                Speaker.IsMute = value;

                _muteToggle.isOn = value;
            }


            private void OnClickSpeakerText()
            {
                recorder.receivePerson = Speaker.Name;
            }
        }
    }
}