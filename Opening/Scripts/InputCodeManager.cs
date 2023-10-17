using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;

public class InputCodeManager : MonoBehaviour
{
    private PlayerData playerData;

    public InputField inputField;
    public Text NotificationText;
    public Text TimeText;
    //public string playerTmp = "";
    int playerExist = -1;

    //ivate bool isHost = false;
    private string[] admins = { "server@", "admin1@", "admin2@", "admin3@"};
    private string[] kinectAccount = { "bubble@", "wave@", "closet@" };
    private string[] test = { "test1@", "test2@", "test3@" };

    private void Start()
    {
        NotificationText.text = "";
    }

    private void Update()
    {
        TimeText.text = DateTime.Now.ToString("d") + " 남은 시간 " + (60 - DateTime.Now.Minute) + ":" + (60 - DateTime.Now.Second);

        if (DataManager.GetCurrentRound() == 5 && DateTime.Now.ToString("d").Equals("2022-02-07"))
        {
            TimeText.text = DateTime.Now.ToString("d") + " 남은 시간 " + (30 - DateTime.Now.Minute) + ":" + (60 - DateTime.Now.Second);
        }
    }
    public void EnterBtnClick()
    {
        string inputEmail = inputField.text;

        if (admins.Contains(inputEmail) || kinectAccount.Contains(inputEmail) || test.Contains(inputEmail)) {
            Debug.Log("contains");
            playerExist = 1;
        }
        else
            GetPlayerData(inputEmail);

        Invoke("NotifyPlayerExist", 1.0f);
    }

    void NotifyPlayerExist() {
        switch (playerExist)
        {
            case 0:
                Debug.Log("등록되지 않은 관객입니다. \n 날짜와 회차, 해당 이메일이 맞는지 확인 후에 다시 시도해주세요.");
                NotificationText.text = "등록되지 않은 관객입니다. \n날짜와 회차, 해당 이메일이 맞는지 확인 후에 다시 시도해주세요.";
                break;
            case 1:
                Debug.Log("입장하기 성공");
                //SendCurrentScene(); // TO-DO : Player 안에 있는 scene을 변경하도록 수정해야 함.\
                GameObject.Find("PhotonNetworkManager").GetComponent<PhotonPlayerNetworkManager>().startNetwork();
                NotificationText.text = "입장 중입니다. 조금만 기다려주세요.";
                GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[샤라랑, 입장을 알리는 종소리]";
                //GameObject.Find("NetworkManager").GetComponent<Mirror.NetworkManagerHUD>().StartButtons(isHost);
                //LoadingSceneManager.LoadScene("RoomDreamScene", "LoadingScene");//LoadingSceneManager.LoadScene("NewScene", "LoadingScene");
                break;
            case 2:
                Debug.Log("다른 PC에서 해당 이메일로 접속중입니다.");
                NotificationText.text = "다른 PC에서 해당 이메일로 접속중입니다.";
                break;
            case -1:
                Debug.Log("네트워크 상황이 원활하지 않습니다. 잠시후 다시 시도해주세요.");
                break;
        }
    }

    public void GetPlayerData(string email)
    {
        playerData = new PlayerData();
        playerData.email = email;
        playerData.date = DateTime.Now.ToString("d");
        playerData.round = DataManager.GetCurrentRound();

        var req = JsonConvert.SerializeObject(playerData);
        Debug.Log(req);
        StartCoroutine(DataManager.sendDataToServer("player", req, (raw) =>
        {
            Debug.Log(raw);
            playerExist = Convert.ToInt32(raw);
        }));
    }
}
