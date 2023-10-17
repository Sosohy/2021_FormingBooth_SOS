using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using TMPro;
using System.Linq;
using Photon.Pun;

public class TelephoneManager : MonoBehaviour
{
    TextMeshProUGUI subtitle;
    GameObject numButtons;
    string[] emotion = {"미련", "불안", "우울", "슬픔", "후회", "피로", "짜증", "무력", "분노", "걱정"};
    int[] select = new int[4];

    public AudioClip[] numClickSounds = new AudioClip[10];
    public AudioClip[] numSoundsDefault = new AudioClip[10];
    public AudioClip[] numSoundsDuplicate = new AudioClip[10];
    public AudioClip[] telephoneSounds = new AudioClip[2];

    GameObject telephone;
    AudioSource telephoneAudio;
    GameObject callText;
    GameObject panel;

    public TelephoneObjData telephoneData;
    /*    미리캔버스 자막
     *    string[] emotionSoundTexts = { "똥똥똥 실로폰 소리", "구슬픈 느낌의 건반 악기 소리", "박자감있는 베이스 소리",
                                       "쨍쨍한 느낌의 건반 악기 소리", "무게감 있는 호른 소리", "분위기 있는 오르간 소리", 
                                       "딱딱 끊어지는 건반 악기 소리", "부드러운 선율의 플루트 소리", "서글픈 느낌의 첼로 소리", "부드러운 느낌의 하프 소리" };*/

    string[] emotionSoundTexts = {
        "똥똥똥 실로폰 소리",
        "구슬픈 느낌의 건반 악기 소리",
        "박자감있는 베이스 소리",
        "서글픈 느낌의 첼로 소리",
        "부드러운 선율의 플루트 소리",
         "부드러운 느낌의 하프 소리",
        "쨍쨍한 느낌의 건반 악기 소리",
        "무게감 있는 호른 소리",
        "분위기 있는 오르간 소리",
        "딱딱 끊어지는 건반 악기 소리",
        };


    int idx = 0;
    bool isCall = false;
    bool isSelectEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        telephoneAudio = this.GetComponent<AudioSource>();

        telephone = GameObject.Find("Telephone").gameObject;
        numButtons = GameObject.Find("numButtons").gameObject;

        callText = GameObject.Find("callText").gameObject;
        panel = GameObject.Find("Panel").gameObject;

        subtitle = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
        subtitle.text = "[♬어두운 분위기의 음악이 흘러나온다.]";
        
        btnInit();
        resetChoice();
        DefaultUIManager.ActiveInfoImage(0);
    }

    void btnInit()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject.Find("num" + i).GetComponent<AudioSource>().clip = numSoundsDefault[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isCall && Input.GetKeyDown(KeyCode.Space))
        {
            callText.SetActive(false);
            startRecord();
        }

        if (isSelectEnd && Input.GetKeyDown(KeyCode.Space))
        {
            subtitle.text = "";
            telephoneAudio.Stop();
            resetChoice();
        }
    }
    public void startRecord()
    {
        telephoneAudio.Stop();
        isCall = false;
        telephone.SetActive(true);

        if (telephonePlayer.IsMine)
            FadeSound.telephoneFade = true;

        //WindowsVoice.speak("테스트입니다");
        StartCoroutine(DataManager.getDataFromServer("telephone", (raw) =>
        {
            Debug.Log(raw);
            var res = JsonConvert.DeserializeObject<TelephoneObjData>(raw);
            telephoneData.count = res.count;
            telephoneData.numbers = res.numbers;

            panel.SetActive(true);

            string s = "당신은 " + telephoneData.count + "번째 기록자 입니다. 자신의 감정이 있는 버튼을 4자리 눌러주세요.";
            subtitle.text = s;
            WindowsVoice.speak(s);
            Invoke("panelActiveFalse", 8.0f);
        }));
    }

    void panelActiveFalse() {
        panel.SetActive(false);
    }

    public void resetChoice()
    {
        isSelectEnd = false;

        for (int i = 0; i < 4; i++) { 
            if(select[i] != -1)
                GameObject.Find("num" + select[i]).GetComponent<AudioSource>().clip = numSoundsDefault[select[i]];
        }
            
        callText.GetComponent<TextMeshProUGUI>().text = "spacebar를 눌러 전화를 받아주세요";
        callText.SetActive(false);
        telephone.SetActive(false);

        idx = 0;
        for (int i = 0; i < select.Length; i++)
            select[i] = -1;

        if (telephonePlayer != null && telephonePlayer.IsMine)
            GameObject.Find("Room").GetComponent<AudioSource>().volume = 1f;

        panel.SetActive(false);
        telephoneAudio.clip = telephoneSounds[0];
    }

    public void onClick()
    {
        if (telephonePlayer != null && telephonePlayer.IsMine)
            subtitle.text = "[삑-삑-삑-삑-]";

        string btnClick = EventSystem.current.currentSelectedGameObject.name;
        select[idx++] = btnClick[btnClick.Length - 1] - '0';
        telephone.gameObject.GetComponent<AudioSource>().clip = numClickSounds[select[idx-1]];
        telephone.gameObject.GetComponent<AudioSource>().Play();

        if (idx == 4)
        {
            panel.SetActive(true);
            StartCoroutine(finishSelect());
        }
    }

    IEnumerator finishSelect()
    {
        StartCoroutine(showEmotionSubtitle());
        WindowsVoice.speak(getEmotionText());
        setDuplicateAudio(); //중복 사운드 처리
        yield return new WaitForSeconds(25.0f);

        if (telephonePlayer.IsMine)
            GameObject.Find("Room").GetComponent<AudioSource>().volume = 0f;

        subtitle.text = getSoundText();
        //메인 멜로디
        GameObject.Find("numButtons").gameObject.GetComponent<AudioSource>().Play();

        //선택 감정
        for (int i = 0; i < select.Length; i++)
        {
            telephoneData.numbers[select[i]]++;
            GameObject.Find("num" + select[i]).gameObject.GetComponent<AudioSource>().Play();
        }

        var req = JsonConvert.SerializeObject(telephoneData.numbers);
        Debug.Log(req);
        StartCoroutine(DataManager.sendDataToServer("telephone/update", req, (raw) =>
         {
             Debug.Log(raw);
         }));

        Debug.Log(numButtons.GetComponent<AudioSource>().clip.length);
        yield return new WaitForSeconds(numButtons.GetComponent<AudioSource>().clip.length + 1.0f);

        callText.GetComponent<TextMeshProUGUI>().text = "spacebar를 눌러 전화를 끊을 수 있습니다.";
        callText.SetActive(true);
        subtitle.text = "";
        isSelectEnd = true;

        telephoneAudio.clip = telephoneSounds[1];
        telephoneAudio.Play();
    }

    void setDuplicateAudio()
    {
        for (int i = 0; i < select.Length; i++)
        {
            var d = select.Count(x => x == select[i]);
            if (d != 1)
                GameObject.Find("num" + select[i]).gameObject.GetComponent<AudioSource>().clip = numSoundsDuplicate[select[i]];
        }
    }

    string getEmotionText()
    {
        string selectNum = "";
        for (int i = 0; i < select.Length; i++)
            selectNum += emotion[select[i]] + " "; 
        selectNum += "을 기록하셨습니다. ";

        string numOfSelect = "오늘 당신이 선택한 ";
        for (int i = 0; i < select.Length; i++) {
            numOfSelect += emotion[select[i]] + " 감정은 " + telephoneData.numbers[select[i]] + "명이, ";
            if (i == 1)
                numOfSelect += "\n";
        }
            
        numOfSelect = numOfSelect.Substring(0, numOfSelect.Length - 2);
        numOfSelect += " 느꼈습니다.";

        return selectNum + numOfSelect + " 당신이 기록한 로그로 만들어진 당신 만을 위한 음악을 들려드리겠습니다.";
    }

    IEnumerator showEmotionSubtitle() {
        string[] sub = getEmotionText().Split('.');
        float[] time = { 3.0f, 11.0f, 5.0f };
        for (int i = 0; i < 3; i++) {
            subtitle.text = sub[i];
            yield return new WaitForSeconds(time[i]);
        }
    }

    string getSoundText() {
        string soundText = "♬";
        for (int i = 0; i < select.Length; i++) {
            soundText += emotionSoundTexts[select[i]] + ", ";
            if (i == 1)
                soundText += "\n";
        }
        soundText = soundText.Substring(0, soundText.Length-2);
        
        return soundText + "가 합쳐져 멜로디가 나온다.";
    }

    PhotonView telephonePlayer;
    void OnTriggerEnter(Collider collision)
    {
        Debug.Log(collision.transform.tag);
        if (collision.transform.tag == "Player")
        {
            telephonePlayer = collision.transform.gameObject.GetComponent<PlayerMoveController>().pv;
            if (telephone.activeSelf == false && telephonePlayer.IsMine)
            {
                callText.SetActive(true);
                subtitle.text = "[따르릉, 전화벨 소리]";
                telephoneAudio.Play();
                isCall = true;
            }
        }
    }

    void OnTriggerExit(Collider collision)
    {
        Debug.Log(collision.transform.tag);
        if (collision.transform.tag == "Player")
        {
            telephonePlayer = collision.transform.gameObject.GetComponent<PlayerMoveController>().pv;
            if (isCall && telephonePlayer.IsMine)
            {
                subtitle.text = "";
                callText.SetActive(false);
                telephoneAudio.Stop();
                isCall = false;
            }
        }
    }
}

