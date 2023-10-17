using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class PhotonPlayerNetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject randomPos;
    public string playerEmail { get; set; }

    private void Awake()
    {
        PhotonNetwork.GameVersion = "1";
    }

    private void Start()
    {

    }

    public void startNetwork()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        //������ ������ ���� ����     
        Debug.Log("������ ������ ���� ����  ");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //������ ���� ���� ���� 
        Debug.Log("������ ���� ���� ����");
        PhotonNetwork.ConnectUsingSettings();   //������ 
    }

    /**TO-DO: EMAIL �����ϱ�***/
    public override void OnJoinedLobby()//�κ� ����� �۵�
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.NickName = playerEmail;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //�� ���� ���� 
        Debug.Log("�� ���� ����");
        PhotonNetwork.CreateRoom("Forming Booth", new RoomOptions { MaxPlayers = 20 });
    }

    public override void OnJoinedRoom()
    {
        int r = Random.Range(0, randomPos.transform.childCount);
        //�� ���� ���� 
        Debug.Log("�� ���� ���� " + r);

        Vector3 pos = randomPos.transform.GetChild(r).transform.position;
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity, 0);
        player.name = PhotonNetwork.NickName;
        player.GetComponent<PlayerMoveController>().playerEmail = player.name;

        //PhotonNetwork.PlayerList;
        //���»�� �̸� �������� ���ںٿ��� �����ֱ�
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)//�ٸ� �÷��̾ �濡 ������ �۵�
    {
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)//�÷��̾ �涰������ ȣ��
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject.Find(otherPlayer.NickName).GetComponent<PlayerMoveController>().SetPlayerExist(otherPlayer.NickName);
           // if(GameObject.Find("VoicePro").transform.Find(otherPlayer.NickName) != null)
           if(GameObject.Find("VoicePro").transform.childCount > 0)
                Destroy(GameObject.Find("VoicePro").transform.Find(otherPlayer.NickName));
            Destroy(GameObject.Find(otherPlayer.NickName));
        }
    }

}
