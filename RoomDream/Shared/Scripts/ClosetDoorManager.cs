using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class ClosetDoorManager : MonoBehaviour, IPunObservable
{
    public bool isPossible = false;
    public bool isOnce = true;
    public bool isPersonIn = false;
    GameObject player;
    public string playerName = "";

    private Animator doorAnimator;
    TextMeshProUGUI notice;

    private void Start()
    {
        notice = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        doorAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {

        if (Input.GetKey(KeyCode.Space) && isPossible)
            if (isOnce)
            {
                if (this.name.Equals("Door_1_5m") && !isPersonIn)
                {
                    if (player.GetComponent<PlayerMoveController>().pv.IsMine)
                    {
                        doorAnimator.SetTrigger("doorclose");
                        player.GetComponent<PlayerMoveController>().isClosetStart = true;
                        player.GetComponent<PlayerMoveController>().loadingLocation = new Vector3(-35.19f, 0.6f, -12.55f);
                        player.GetComponent<PlayerMoveController>().isLoading = true;
                        LoadingSceneManager.LoadScene("RoomCloset", "LoadingScene");
                    }
                    isPersonIn = true;
                }
                else if (this.name.Equals("cabin 1"))
                {
                    Debug.Log("attaic in");

                    if (player.GetComponent<PlayerMoveController>().pv.IsMine)
                    {
                        player.GetComponent<PlayerMoveController>().loadingLocation = new Vector3(-20f, 4.95f, 10f);
                        player.GetComponent<PlayerMoveController>().isLoading = true;
                        GameObject.Find("DreamSound").GetComponent<DreamRoomSound>().ChangeAtticSound(true);
                        LoadingSceneManager.LoadScene("RoomAttic", "LoadingScene");
                    }
                }
                notice.text = "";
                isOnce = false;
                isPossible = false;
            }

        if (playerName != "" && this.name.Equals("Door_1_5m"))
        {
            isPersonIn = GameObject.Find(playerName).GetComponent<PlayerMoveController>().isClosetStart;
            if (GameObject.Find(playerName).GetComponent<PlayerMoveController>().isClosetStart)
            {
                SetAvailable(false);
            }
            else if (GameObject.Find(playerName).GetComponent<PlayerMoveController>().isClosetEnd)
            {
                SetAvailable(true);
                isOnce = true;
            }
        }

    }
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag.Equals("Player"))
        {
            if (!collision.gameObject.GetComponent<PlayerMoveController>().pv.IsMine)
                return;

            isPossible = true;
            player = collision.gameObject;

            if (this.gameObject.name.Equals("Door_1_5m"))
            {
                if (!isPersonIn)
                {
                    playerName = player.name;
                    player.GetComponent<PlayerMoveController>().isClosetStart = false;
                    player.GetComponent<PlayerMoveController>().isClosetEnd = false;
                    SetSignOwner();
                    this.GetComponent<PhotonView>().TransferOwnership(player.GetComponent<PhotonView>().Owner);
                    doorAnimator.SetTrigger("dooropen");
                    notice.text = "spacebar를 눌러 의상실로 이동하세요.";
                }
                else
                {
                    notice.text = "다른 사용자가 이용 중입니다.";
                }
            }
            else if (this.name.Equals("cabin 1"))
            {
                notice.text = "spacebar를 눌러 오두막으로 이동하세요.";
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            notice.text = "";
            isPossible = false;

            if (this.gameObject.name.Equals("Door_1_5m"))
            {
                doorAnimator.SetTrigger("doorclose");
            }
        }
    }

    private void SetSignOwner()
    {
        GameObject.Find("ClosetSign").transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(player.GetComponent<PhotonView>().Owner);
        GameObject.Find("ClosetSign").transform.GetChild(1).GetComponent<PhotonView>().TransferOwnership(player.GetComponent<PhotonView>().Owner);
    }

    public void SetAvailable(bool available)
    {
        //Debug.Log("set available : " + available);
        if (available)
        {
            GameObject.Find("ClosetSign").transform.GetChild(0).gameObject.transform.localScale = new Vector3(2, 2, 2);
            GameObject.Find("ClosetSign").transform.GetChild(1).gameObject.transform.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            GameObject.Find("ClosetSign").transform.GetChild(0).gameObject.transform.localScale = new Vector3(0, 0, 0);
            GameObject.Find("ClosetSign").transform.GetChild(1).gameObject.transform.localScale = new Vector3(2, 2, 2);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(isPossible);
            stream.SendNext(isOnce);
            stream.SendNext(isPersonIn);
            stream.SendNext(playerName);
        }
        else
        {
            // Network player, receive data
            this.isPossible = (bool)stream.ReceiveNext();
            this.isOnce = (bool)stream.ReceiveNext();
            isPersonIn = (bool)stream.ReceiveNext();
            playerName = (string)stream.ReceiveNext();
        }
    }

}