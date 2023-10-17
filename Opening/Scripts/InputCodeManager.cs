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
        TimeText.text = DateTime.Now.ToString("d") + " ���� �ð� " + (60 - DateTime.Now.Minute) + ":" + (60 - DateTime.Now.Second);

        if (DataManager.GetCurrentRound() == 5 && DateTime.Now.ToString("d").Equals("2022-02-07"))
        {
            TimeText.text = DateTime.Now.ToString("d") + " ���� �ð� " + (30 - DateTime.Now.Minute) + ":" + (60 - DateTime.Now.Second);
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
                Debug.Log("��ϵ��� ���� �����Դϴ�. \n ��¥�� ȸ��, �ش� �̸����� �´��� Ȯ�� �Ŀ� �ٽ� �õ����ּ���.");
                NotificationText.text = "��ϵ��� ���� �����Դϴ�. \n��¥�� ȸ��, �ش� �̸����� �´��� Ȯ�� �Ŀ� �ٽ� �õ����ּ���.";
                break;
            case 1:
                Debug.Log("�����ϱ� ����");
                //SendCurrentScene(); // TO-DO : Player �ȿ� �ִ� scene�� �����ϵ��� �����ؾ� ��.\
                GameObject.Find("PhotonNetworkManager").GetComponent<PhotonPlayerNetworkManager>().startNetwork();
                NotificationText.text = "���� ���Դϴ�. ���ݸ� ��ٷ��ּ���.";
                GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[�����, ������ �˸��� ���Ҹ�]";
                //GameObject.Find("NetworkManager").GetComponent<Mirror.NetworkManagerHUD>().StartButtons(isHost);
                //LoadingSceneManager.LoadScene("RoomDreamScene", "LoadingScene");//LoadingSceneManager.LoadScene("NewScene", "LoadingScene");
                break;
            case 2:
                Debug.Log("�ٸ� PC���� �ش� �̸��Ϸ� �������Դϴ�.");
                NotificationText.text = "�ٸ� PC���� �ش� �̸��Ϸ� �������Դϴ�.";
                break;
            case -1:
                Debug.Log("��Ʈ��ũ ��Ȳ�� ��Ȱ���� �ʽ��ϴ�. ����� �ٽ� �õ����ּ���.");
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
