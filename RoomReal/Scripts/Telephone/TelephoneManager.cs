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
    string[] emotion = {"�̷�", "�Ҿ�", "���", "����", "��ȸ", "�Ƿ�", "¥��", "����", "�г�", "����"};
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
    /*    �̸�ĵ���� �ڸ�
     *    string[] emotionSoundTexts = { "�˶˶� �Ƿ��� �Ҹ�", "������ ������ �ǹ� �Ǳ� �Ҹ�", "���ڰ��ִ� ���̽� �Ҹ�",
                                       "¸¸�� ������ �ǹ� �Ǳ� �Ҹ�", "���԰� �ִ� ȣ�� �Ҹ�", "������ �ִ� ������ �Ҹ�", 
                                       "���� �������� �ǹ� �Ǳ� �Ҹ�", "�ε巯�� ������ �÷�Ʈ �Ҹ�", "������ ������ ÿ�� �Ҹ�", "�ε巯�� ������ ���� �Ҹ�" };*/

    string[] emotionSoundTexts = {
        "�˶˶� �Ƿ��� �Ҹ�",
        "������ ������ �ǹ� �Ǳ� �Ҹ�",
        "���ڰ��ִ� ���̽� �Ҹ�",
        "������ ������ ÿ�� �Ҹ�",
        "�ε巯�� ������ �÷�Ʈ �Ҹ�",
         "�ε巯�� ������ ���� �Ҹ�",
        "¸¸�� ������ �ǹ� �Ǳ� �Ҹ�",
        "���԰� �ִ� ȣ�� �Ҹ�",
        "������ �ִ� ������ �Ҹ�",
        "���� �������� �ǹ� �Ǳ� �Ҹ�",
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
        subtitle.text = "[�ݾ�ο� �������� ������ �귯���´�.]";
        
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

        //WindowsVoice.speak("�׽�Ʈ�Դϴ�");
        StartCoroutine(DataManager.getDataFromServer("telephone", (raw) =>
        {
            Debug.Log(raw);
            var res = JsonConvert.DeserializeObject<TelephoneObjData>(raw);
            telephoneData.count = res.count;
            telephoneData.numbers = res.numbers;

            panel.SetActive(true);

            string s = "����� " + telephoneData.count + "��° ����� �Դϴ�. �ڽ��� ������ �ִ� ��ư�� 4�ڸ� �����ּ���.";
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
            
        callText.GetComponent<TextMeshProUGUI>().text = "spacebar�� ���� ��ȭ�� �޾��ּ���";
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
            subtitle.text = "[��-��-��-��-]";

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
        setDuplicateAudio(); //�ߺ� ���� ó��
        yield return new WaitForSeconds(25.0f);

        if (telephonePlayer.IsMine)
            GameObject.Find("Room").GetComponent<AudioSource>().volume = 0f;

        subtitle.text = getSoundText();
        //���� ��ε�
        GameObject.Find("numButtons").gameObject.GetComponent<AudioSource>().Play();

        //���� ����
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

        callText.GetComponent<TextMeshProUGUI>().text = "spacebar�� ���� ��ȭ�� ���� �� �ֽ��ϴ�.";
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
        selectNum += "�� ����ϼ̽��ϴ�. ";

        string numOfSelect = "���� ����� ������ ";
        for (int i = 0; i < select.Length; i++) {
            numOfSelect += emotion[select[i]] + " ������ " + telephoneData.numbers[select[i]] + "����, ";
            if (i == 1)
                numOfSelect += "\n";
        }
            
        numOfSelect = numOfSelect.Substring(0, numOfSelect.Length - 2);
        numOfSelect += " �������ϴ�.";

        return selectNum + numOfSelect + " ����� ����� �α׷� ������� ��� ���� ���� ������ ����帮�ڽ��ϴ�.";
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
        string soundText = "��";
        for (int i = 0; i < select.Length; i++) {
            soundText += emotionSoundTexts[select[i]] + ", ";
            if (i == 1)
                soundText += "\n";
        }
        soundText = soundText.Substring(0, soundText.Length-2);
        
        return soundText + "�� ������ ��ε� ���´�.";
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
                subtitle.text = "[������, ��ȭ�� �Ҹ�]";
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

