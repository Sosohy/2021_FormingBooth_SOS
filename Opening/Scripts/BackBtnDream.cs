using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class BackBtnDream : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("RoomCloset"))
            GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "";
        else
            GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[♬ 따뜻한 분위기의 음악이 나오고 있다.]";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickGoDream() {

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (!player.GetComponent<PlayerMoveController>().pv.IsMine) return;

        player.GetComponent<PlayerMoveController>().loadingLocation = new Vector3(0, 0, 0); //(-35.3009f, 7.432f, -15.51192f);
        player.GetComponent<PlayerMoveController>().isLoading = true;

        if (SceneManager.GetActiveScene().name.Equals("RoomCloset"))
        {
            player.GetComponent<PlayerMoveController>().isClosetStart = false;
            player.GetComponent<PlayerMoveController>().isClosetEnd = true;
        }
        else {
            GameObject.Find("DreamSound").GetComponent<DreamRoomSound>().ChangeAtticSound(false);
        }
        LoadingSceneManager.LoadScene("RoomDreamScene", "LoadingScene");
    }

    public void OnClickAirballoonBack() {

        GameObject.Find("AirBalloon_Make").GetComponent<AirBalloonManage>().OnClickBackBtn();
    }

    public void OnClickTelephoneBack() {
        GameObject.Find("telephoneZone").GetComponent<TelephoneManager>().resetChoice();
    }
}
