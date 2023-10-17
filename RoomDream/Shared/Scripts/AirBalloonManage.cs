using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class AirBalloonManage : MonoBehaviour
{
    public GameObject upTargetPosition;
    public GameObject downTargetPosition;
    public GameObject cameraPos;

    public AudioClip[] airballoonSounds = new AudioClip[2];

    private Vector3 vel = Vector3.zero;
    public bool isEnd = false;
    public bool isGround = true;
    public static int isPlayerEnter = 0;
    public int enter = 0;

    public bool isOnce = false;
    private bool isOnceSound = false;

    Animator airBalloonAnim;
    Vector3 targetPos;
    public GameObject cameraPoistion;
    public GameObject cameraPivot;
    public GameObject player;

    TextMeshProUGUI subtitle;
    TextMeshProUGUI notice;
    public GameObject backBtn;

    bool isUp = false;
    bool downSound = false;

    Transform tr;

    private void Start()
    {
        targetPos = upTargetPosition.transform.position;
        airBalloonAnim = this.GetComponentInChildren<Animator>();

        subtitle = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
        notice = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        DefaultUIManager.ActiveInfoImage(0);
        backBtn.SetActive(false);
        
        subtitle.text = "[♬활기찬 분위기의 음악이 흘러나온다.]";
        StartCoroutine(removeSubtitle());
    }

    void Update()
    {
        enter = isPlayerEnter;
        tr = this.transform;

        if (isPlayerEnter == 1)
        {
            player.GetComponent<PlayerMoveController>().StopJumpAndMouse(true);
        }

        if (!isOnce)
        {
            if (isPlayerEnter == 1 && Input.GetKey(KeyCode.Space))
                isUp = true;

            if(isUp)
                MoveToUPTarget();

        }
        if (isPlayerEnter == 1 && targetPos.y <= transform.position.y + 0.5)
            SetBubbleCameraPos();

        if (isEnd)
            MoveToDOWNTarget();

        if (isPlayerEnter == 3 && transform.position == downTargetPosition.transform.position && !isGround)
        {
            downSetting();
        }
    }

    IEnumerator removeSubtitle()
    {
        yield return new WaitForSeconds(10.0f);
        GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "";
    }

    private void downSetting()
    {
        player.transform.position = this.transform.position;
        Debug.Log(player.transform.position);
        isPlayerEnter = 0;
        isEnd = false;
        isGround = true;
        isOnceSound = false;
        downSound = false;
        airBalloonAnim.SetTrigger("opening");
        player.GetComponent<PlayerMoveController>().StopJumpAndMouse(false);
        if (airBalloonAnim.GetCurrentAnimatorStateInfo(0).IsName("opened_door"))
        {
            player.transform.position = transform.position;
        }
    }

    private void SetBubbleCameraPos()
    {
        isUp = false;
        isPlayerEnter = 2;
        Debug.Log("cameraPos");
        backBtn.SetActive(true);
        GameObject.Find("PR_RoomDreamStreaming").GetComponent<RoomDreamStreamingManager>().setBubbleStreaming(true);

        DefaultUIManager.ActiveInfoImage(1);

        isOnce = true;
        //카메라 위치 각도 조정
        cameraPoistion.transform.position = cameraPos.transform.position;
        cameraPoistion.transform.rotation = cameraPos.transform.rotation;
    }

    

    private void MoveToUPTarget()
    {
        isGround = false;
        notice.text = "";

        subtitle.text = "[슈우우욱, 열기구 올라가는 소리]";
        if (!isOnceSound)
        {
            this.GetComponent<AudioSource>().clip = airballoonSounds[0];
            this.GetComponent<AudioSource>().Play();
            SetAvailable(false);
            airBalloonAnim.SetTrigger("closed");
            isOnceSound = true;
        }


        this.gameObject.transform.position = Vector3.SmoothDamp(gameObject.transform.position, upTargetPosition.transform.position, ref vel, 1f);
        player.transform.position = transform.position;
    }

    private void SetAvailable(bool available)
    {
        GameObject.Find("AirBallonSign").transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(player.GetComponent<PhotonView>().Owner);
        GameObject.Find("AirBallonSign").transform.GetChild(1).GetComponent<PhotonView>().TransferOwnership(player.GetComponent<PhotonView>().Owner);

        if (available)
        {
            GameObject.Find("AirBallonSign").transform.GetChild(0).gameObject.transform.localScale = new Vector3(2, 2, 2);
            GameObject.Find("AirBallonSign").transform.GetChild(1).gameObject.transform.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            GameObject.Find("AirBallonSign").transform.GetChild(0).gameObject.transform.localScale = new Vector3(0,0,0);
            GameObject.Find("AirBallonSign").transform.GetChild(1).gameObject.transform.localScale = new Vector3(2,2,2);
        }
    }

    private void MoveToDOWNTarget()
    {
        subtitle.text = "[쉬이이익, 열기구 내려가는 소리]";
        //isEnd = false;]

        if (!downSound) {
            this.GetComponent<AudioSource>().clip = airballoonSounds[1];
            this.GetComponent<AudioSource>().Play();
            downSound = true;
        }

        isPlayerEnter = 3;

        //카메라 위치 각도 원래대로
        cameraPoistion.transform.position = cameraPivot.transform.position;
        cameraPoistion.transform.rotation = cameraPivot.transform.rotation;

        backBtn.SetActive(false);
        transform.position = Vector3.MoveTowards(gameObject.transform.position, downTargetPosition.transform.position, 0.1f);
        player.transform.position = transform.position;

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!collision.gameObject.GetComponent<PlayerMoveController>().pv.IsMine)
                return;

            if (!isEnd && !isGround && !isOnce)
            {
                Debug.Log("triggerEnter");

                isPlayerEnter = 1;
                player = collision.gameObject;
                cameraPoistion = collision.gameObject.transform.GetChild(0).gameObject;
                cameraPivot = collision.gameObject.transform.GetChild(1).gameObject;
                airBalloonAnim.SetTrigger("closing");

                this.GetComponent<PhotonView>().TransferOwnership(player.GetComponent<PhotonView>().Owner);
                notice.text = "spacebar를 눌러 열기구를 출발시키세요.";
                backBtn.SetActive(true);

            }
            if (airBalloonAnim.GetCurrentAnimatorStateInfo(0).IsName("opened_door"))
            {
                isOnce = false;
                isGround = false;
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!collision.gameObject.GetComponent<PlayerMoveController>().pv.IsMine)
                return;

            Debug.Log("triggerExit");
            if (isGround)
            {
                airBalloonAnim.SetTrigger("opening");
                if (airBalloonAnim.GetCurrentAnimatorStateInfo(0).IsName("opened_door"))
                {
                    isOnce = false;
                    isGround = false;
                    isPlayerEnter = 0;
                }
                SetAvailable(true);
            }
            subtitle.text = "";
            notice.text = "";
        }
    }

    public void OnClickBackBtn()
    {
        if (!player.GetComponent<PlayerMoveController>().pv.IsMine)
            return;

           isEnd = true;
        GameObject.Find("PR_RoomDreamStreaming").GetComponent<RoomDreamStreamingManager>().setBubbleStreaming(false);
    }

}
