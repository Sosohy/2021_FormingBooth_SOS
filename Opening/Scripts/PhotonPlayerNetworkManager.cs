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
        //마스터 서버에 접속 성공     
        Debug.Log("마스터 서버에 접속 성공  ");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //마스터 서버 접속 실패 
        Debug.Log("마스터 서버 접속 실패");
        PhotonNetwork.ConnectUsingSettings();   //재접속 
    }

    /**TO-DO: EMAIL 수정하기***/
    public override void OnJoinedLobby()//로비에 연결시 작동
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.NickName = playerEmail;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //방 참가 실패 
        Debug.Log("방 참가 실패");
        PhotonNetwork.CreateRoom("Forming Booth", new RoomOptions { MaxPlayers = 20 });
    }

    public override void OnJoinedRoom()
    {
        int r = Random.Range(0, randomPos.transform.childCount);
        //방 참가 성공 
        Debug.Log("방 참가 성공 " + r);

        Vector3 pos = randomPos.transform.GetChild(r).transform.position;
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity, 0);
        player.name = PhotonNetwork.NickName;
        player.GetComponent<PlayerMoveController>().playerEmail = player.name;

        //PhotonNetwork.PlayerList;
        //들어온사람 이름 랜덤으로 숫자붙여서 정해주기
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)//다른 플레이어가 방에 들어오면 작동
    {
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)//플레이어가 방떠났을때 호출
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
