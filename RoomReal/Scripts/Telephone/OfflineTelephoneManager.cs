using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using TMPro;
using System.Linq;
using Photon.Pun;

public class OfflineTelephoneManager : MonoBehaviour
{
    GameObject numButtons;
    string[] emotion = {"�̷�", "�Ҿ�", "���", "����", "��ȸ", "�Ƿ�", "¥��", "�����", "�г�", "����"};
    int[] select = new int[4];

    public AudioClip[] numClickSounds = new AudioClip[10];
    public AudioClip[] numSoundsDefault = new AudioClip[10];
    public AudioClip[] numSoundsDuplicate = new AudioClip[10];

    GameObject telephone;
    AudioSource telephoneAudio;
    GameObject panel;

    public TelephoneObjData telephoneData;

    int idx = 0;
    bool isSelectEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        telephone = GameObject.Find("Telephone").gameObject;
        numButtons = GameObject.Find("numButtons").gameObject;

        panel = GameObject.Find("Panel").gameObject;
        
        btnInit();
        resetChoice();
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
    }
    public void startRecord()
    {
        StartCoroutine(DataManager.getDataFromServer("telephone", (raw) =>
        {
            Debug.Log(raw);
            var res = JsonConvert.DeserializeObject<TelephoneObjData>(raw);
            telephoneData.count = res.count;
            telephoneData.numbers = res.numbers;

           // panel.SetActive(true);

            /*string s = "����� " + telephoneData.count + "��° ����� �Դϴ�. �ڽ��� ������ �ִ� ��ȣ�� 4�ڸ� �����ּ���.";
            subtitle.text = s;
            WindowsVoice.speak(s);
            Invoke("panelActiveFalse", 8.0f);*/
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

        idx = 0;
        for (int i = 0; i < select.Length; i++)
            select[i] = -1;

        panel.SetActive(false);
        startRecord();
    }

    public void onClick()
    {
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
        //StartCoroutine(showEmotionSubtitle());
        WindowsVoice.speak(getEmotionText());
        setDuplicateAudio(); //�ߺ� ���� ó��
        yield return new WaitForSeconds(20.0f);


        //subtitle.text = getSoundText();
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

        yield return new WaitForSeconds(numButtons.GetComponent<AudioSource>().clip.length);

        //subtitle.text = "";
        isSelectEnd = true;
        resetChoice();
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
            selectNum += select[i] + " ";
        selectNum += "�� ����ϼ̽��ϴ�. ";

        string numOfSelect = "���� ����� ������ ";
        for (int i = 0; i < select.Length; i++) {
            Debug.Log("btnClick" + select[i]);
            numOfSelect += emotion[select[i]] + " ������ " + telephoneData.numbers[select[i]] + "����, ";
            if (i == 1)
                numOfSelect += "\n";
        }
            
        numOfSelect = numOfSelect.Substring(0, numOfSelect.Length - 2);
        numOfSelect += " �������ϴ�.";

        return selectNum + numOfSelect + " ����� ����� �α׷� ������� ��� ���� ���� ������ ����帮�ڽ��ϴ�.";
    }

    public void touchStartPanel() {
        startRecord();
    }

}

