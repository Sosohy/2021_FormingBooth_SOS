using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using System;
using TMPro;

public class DefaultUIManager : MonoBehaviour
{
    bool onSubtitle = false;
    GameObject subtitle;
    GameObject info;
    public Sprite explain;
    public Sprite minigameExplain;
    public Sprite[] dreamExplain = new Sprite[4]; // 0: default 1:airballoon 2: closet 3: attic

    bool isOpening = true;
    // Start is called before the first frame update
    void Start()
    {
        InitSetting();
    }

    // Update is called once per frame
    void Update()
    {
        if ((SceneManager.GetActiveScene().name != "OpeningScene" && SceneManager.GetActiveScene().name != "LoadingScene" && SceneManager.GetActiveScene().name != "BackLoadingScene") && isOpening)
        {
            isOpening = false;
            this.transform.Find("Info").gameObject.SetActive(true);
            this.transform.Find("VoiceChat").gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            onSubtitle = (onSubtitle == false) ? true : false;
            subtitle.SetActive(onSubtitle);
        }
    }

    public void InitSetting()
    {
        subtitle = GameObject.Find("Subtitle").gameObject;
        subtitle.GetComponentInChildren<TextMeshProUGUI>().text = "[¢›¿‹¿‹«— ∫–¿ß±‚¿« ¿Ωæ«¿Ã »Í∑Ø≥™ø¬¥Ÿ.]";
        subtitle.SetActive(false);
        info = GameObject.Find("InfoImage").gameObject;
        info.GetComponent<Image>().sprite = explain;

        GameObject.Find("Info").SetActive(false);
        GameObject.Find("VoiceChat").SetActive(false);

    }

    public void OnClickInfoBtn()
    {
        if (info.activeSelf)
        {
            info.SetActive(false);
            if(SceneManager.GetActiveScene().name == "MiniGameScene")
                GameManager.isExplainEnd = true;
        }
        else
            info.SetActive(true);
    }

    public void setInfoImage(int index = 0)
    {
        //int idx = Array.FindIndex(explain, x => x.ToString() == SceneManager.GetActiveScene().name);

        if (SceneManager.GetActiveScene().name == "RoomRealScene")
            info.GetComponent<Image>().sprite = explain;
        else if(SceneManager.GetActiveScene().name == "MiniGameScene")
            info.GetComponent<Image>().sprite = minigameExplain;
        else
            info.GetComponent<Image>().sprite = dreamExplain[index];
    }

    public static void ActiveInfoImage(int idx = 0)
    {
        GameObject.Find("PR_DefaultUICanvas").GetComponent<DefaultUIManager>().setInfoImage(idx);
        GameObject.Find("PR_DefaultUICanvas").transform.Find("Info").GetChild(0).gameObject.SetActive(true);
    }

    public void OnClickStartDefaultBubble() {
        if (SceneManager.GetActiveScene().name == "RoomDreamScene" && GameObject.Find("admin1@").GetComponent<PlayerMoveController>().currentScene == "RoomDreamScene")
            GameObject.Find("PR_OnlineBalloonGame").GetComponent<CreateBubble>().BubbleDefaultStart();
    }
}
