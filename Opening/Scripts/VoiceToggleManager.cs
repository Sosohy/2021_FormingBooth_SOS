using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceToggleManager : MonoBehaviour
{
    FrostweepGames.VoicePro.Examples.VoiceUIManager voiceUI;
    public bool isClicked = false;

    void Start()
    {
        voiceUI = GameObject.Find("PR_DefaultUICanvas").GetComponent<FrostweepGames.VoicePro.Examples.VoiceUIManager>();

        voiceUI.mySpeakToggle.isOn = false;
        isClicked = false;
        this.transform.Find("Wave").gameObject.SetActive(false);
        this.transform.Find("Icon").GetComponent<Animator>().enabled = false;
    }

    public void onClickVoiceImg()
    {
        if (!isClicked)
        {
            voiceUI.mySpeakToggle.isOn = true;
            isClicked = true;
            this.transform.Find("Wave").gameObject.SetActive(true);
            this.transform.Find("Icon").GetComponent<Animator>().enabled = true;
        }
        else
        {
            voiceUI.mySpeakToggle.isOn = false;
            isClicked = false;
            this.transform.Find("Wave").gameObject.SetActive(false);
            this.transform.Find("Icon").GetComponent<Animator>().enabled = false;
        }
    }

}
