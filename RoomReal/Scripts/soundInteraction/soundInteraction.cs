using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Photon.Pun;

public class soundInteraction : MonoBehaviour
{

    List<string> soundObj = new List<string>() { "desk", "mirror", "postcard", "lamp"};
    public AudioClip[] audioClips = new AudioClip[2];
    AudioSource objAudio;

    public SoundData soundData;
    bool tact = false;
    bool interaction = false;

    int idx = 0;
    bool getData = false;

    TextMeshProUGUI subtitle;
    string[] interactionTexts = { "[여러 사람의 고뇌가 담긴 소리]", "[들숨날숨 숨소리]", "[촤아악, 파도소리]", "[쿵쿵-쿵쿵-, 심장박동 소리]" };

    // Start is called before the first frame update
    void Start()
    {
        objAudio = this.GetComponent<AudioSource>();
        idx = soundObj.IndexOf(this.gameObject.name);
        subtitle = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
        initSound();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider collision)
    {   
       if (collision.CompareTag("Player"))
        {
            if (!collision.transform.gameObject.GetComponent<PlayerMoveController>().pv.IsMine)
                return;

            if (this.gameObject.name != "lamp")
            {
                getData = true;
                // DB상태 변경
                changeSoundInfo("sound/untact", true);

                StartCoroutine(getSoundData());
            }else
                subtitle.text = interactionTexts[idx];

            objAudio.volume = 1.0f;
        }
    }

    IEnumerator getSoundData()
    {
        while (getData) {
            changeSoundInfo("sound/untact/getData", true);
            yield return new WaitForSeconds(2.0f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.transform.gameObject.GetComponent<PlayerMoveController>().pv.IsMine)
            return;

        if (other.CompareTag("Player"))
        {
            if (this.gameObject.name != "lamp")
            {
                getData = false;
                //DB상태 변경
                changeSoundInfo("sound/untact", false);
                StopCoroutine(getSoundData());
            }
            objAudio.volume = 0;
            subtitle.text = "";
        }
    }

    void changeSoundInfo(String api, bool state)
    {
        soundData.id = idx;
        soundData.state = state;

        var req = JsonConvert.SerializeObject(soundData);
        Debug.Log(req);
        StartCoroutine(DataManager.sendDataToServer(api, req,  (raw) =>
        {
            Debug.Log("raw " + raw);
            tact = (raw == "1") ? true : false;

            if (getData && tact && !interaction)
            {
                interaction = true;
                changeSound(1);
            }
            else if((!getData || !tact) && interaction){
                interaction = false;
                changeSound(0, state);
            }else if(getData && !interaction)
                subtitle.text = interactionTexts[idx];

        }));
    }

    void initSound()
    {
        objAudio.clip = audioClips[0];
        objAudio.playOnAwake = true;
        objAudio.loop = true;
        objAudio.volume = 0;
        objAudio.Play();
    }

    void changeSound(int i, bool state = true)
    {
        if (state) {
            if (i == 0)
                subtitle.text = interactionTexts[idx];
            else //인터랙션
                subtitle.text = "[지지직-, 불협의 소리]";
        }
        objAudio.clip = audioClips[i];
        objAudio.Play();
    }
}
