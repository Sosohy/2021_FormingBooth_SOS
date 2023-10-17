using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.Utility;
using UnityEngine.UI;
using FrostweepGames.VoicePro;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System;
using Newtonsoft.Json;

public class PlayerMoveController : MonoBehaviour, IPunObservable
{
    public float movementSpeed = 2f;
    public float runSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float upDownRange = 90;
    public float jumpSpeed = 5;
    public float downSpeed = 0;

    private Vector3 speed;
    private float forwardSpeed;
    private float sideSpeed;

    AudioSource playerAudio;
    public AudioClip[] footStepsSounds;
    public AudioClip jumpSound;

    private float rotLeftRight;
    private float rotUpDown;
    private float verticalRotation = 0f;

    private float verticalVelocity = 0f;

    Animator playerAnim;
    private CharacterController cc;
    public PhotonView pv;
    private Camera m_Camera;
    private Transform tr;

    public string playerEmail = "";
    public PlayerData playerData;

    GameObject notification;
    GameObject defaultUICanvas;
    public int roomDreamIdx = 0;

    // Server to All Clients
    public string currentScene = "RoomRealScene";

    private string[] kinectAccount = {"bubble@", "closet@" };
    private string[] offlineAccount = { "admin2@", "admin3@" };

    private bool stopJumping = false;
    private bool stopMouse = false;

    GameObject video;
    public GameObject firefly;
    private bool isOnceSetting = false;
    private bool isOnceRoomIndexSetting = false;
    private bool isOnceAudioStop = false;
    private bool isPossibleFire = false;
    public bool isOnceVideoEnd = false;
    public bool isLoading = false;
    public Vector3 loadingLocation;

    public bool isClosetStart = false;
    public bool isClosetEnd = false;

    private bool startAtticParticle = false;
    private bool skyChange = false;
    float skyTime = 11f;
    float limitSkyTime = 16.8f;
    float alphaSkyTime = 0.15f;
    private string[] cueText = { "하늘 바꾸기", "오두막 1분 전", "영상 3-2", "회복하기", "영상 3-3", "하늘 되돌리기", "현실로 1분 전" };

    private bool isReturn = false;

    // Use this for initialization
    void Start()
    {
        cc = GetComponent<CharacterController>();
        pv = PhotonView.Get(this);
        playerAnim = GetComponentInChildren<Animator>();
        m_Camera = Camera.main;

        defaultUICanvas = GameObject.Find("PR_DefaultUICanvas").gameObject;
        notification = defaultUICanvas.transform.GetChild(3).gameObject;

        GameObject.Find("Server").transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { pv.RPC("CmdStartLimit", RpcTarget.AllBufferedViaServer); });
        GameObject.Find("Server").transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => { pv.RPC("CmdStopVideo", RpcTarget.AllBufferedViaServer); });
        tr = GetComponent<Transform>();
        playerAudio = tr.GetComponent<AudioSource>();

        this.name = pv.Owner.NickName;

        if (pv.IsMine)
        {
            Camera.main.GetComponent<SmoothFollow>().target = tr.Find("CamPivot").transform;
            GetComponent<AudioListener>().enabled = true;

            if (SetKinectScene())
                return;

            /*** Voice Register **/
            int rand = UnityEngine.Random.Range(0, 100);
            if(this.playerEmail.Equals("admin1@") || (!offlineAccount.Contains(this.playerEmail) && !kinectAccount.Contains(this.playerEmail)))
                NetworkRouter.Instance.Register(rand, PhotonNetwork.NickName, Enumerators.NetworkType.PUN2);

            // LoadingSceneManager.LoadScene("NewScene", "LoadingScene"); //RoomDreamScene
           

            if (kinectAccount.Contains(this.playerEmail) || this.playerEmail.Contains("admin") || this.playerEmail.Contains("test"))
            {
                if (pv.IsMine)
                    LoadingSceneManager.LoadScene("RoomRealScene", "LoadingScene");
            }
            else
                GetPlayerScene(name);
        }

    }

    void InitScene(string playerScene)
    {
        if (GameObject.Find("admin1@") != null)
            this.currentScene = GameObject.Find("admin1@").GetComponent<PlayerMoveController>().currentScene;
        else
            this.currentScene = "RoomRealScene";
        Debug.Log("current : " + currentScene + "/ playerScene : " + playerScene);

        string loading = "LoadingScene";
        if (playerScene != "OpeningScene" && playerScene != null)
            loading = "BackLoadingScene";

        if (currentScene.Equals("MiniGameScene"))
            currentScene = "RoomDreamScene";

        if (pv.IsMine)
            LoadingSceneManager.LoadScene(currentScene, loading); 
    }

    

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine)
            return;

        if (this.playerEmail.Contains("admin") || kinectAccount.Contains(this.playerEmail))
            this.transform.localScale = new Vector3(0, 0, 0);

        if (isReturn && SceneManager.GetActiveScene().name.Equals("OpeningScene"))
            StartCoroutine(ExitExhibition());


        if (isLoading)
            setDesPos(loadingLocation);

        if (SceneManager.GetActiveScene().name.Equals("MiniGameScene"))
            return;

        // Player Moving
        FPMove();

        if (Input.GetMouseButton(1) && !stopMouse)
            FPRotate();
        else
            MouseLock(false);

        if (Input.GetKeyDown(KeyCode.LeftControl))
            SetSitAnim();

        if (SceneManager.GetActiveScene().name == "RoomDreamScene" && tr.Find("PlayerCube").gameObject.activeSelf == false)
            SetPlayerCube();

        if(this.playerEmail.Equals("admin1@") && SceneManager.GetActiveScene().name.Equals("RoomDreamScene") && this.currentScene != "RoomDreamScene")
            this.currentScene = "RoomDreamScene";

        if (this.playerEmail.Equals("admin1@") &&  GameObject.Find("Server").transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf == false && SceneManager.GetActiveScene().name.Equals("OpeningScene"))
            GameObject.Find("Server").transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(true);

        if(this.playerEmail.Equals("admin1@") && Input.GetKeyDown(KeyCode.F4))
            if(GameObject.Find("Server").transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf == false)
                GameObject.Find("Server").transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(true);
            else
                GameObject.Find("Server").transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(false);

        if(this.playerEmail.Equals("admin1@"))
            if (GameObject.Find("Server").transform.GetChild(4).gameObject.activeSelf == false)
                GameObject.Find("Server").transform.GetChild(4).gameObject.SetActive(true);

        // Admin Setting
        if (this.playerEmail.Equals("admin1@") && (SceneManager.GetActiveScene().name.Equals("RoomRealScene") ||  SceneManager.GetActiveScene().name.Equals("RoomDreamScene") ))
        {
            if (GameObject.Find("Server").transform.GetChild(0).gameObject.activeSelf == false)
                GameObject.Find("Server").transform.GetChild(0).gameObject.SetActive(true);

            if (SceneManager.GetActiveScene().name.Equals("RoomRealScene"))
            {
                Camera.main.GetComponent<SmoothFollow>().target = GameObject.Find("ServerCameraPivot").transform;
                Camera.main.GetComponent<SmoothFollow>().target.rotation = GameObject.Find("ServerCameraPivot").transform.rotation;
                GameObject.Find("Room").GetComponent<AudioSource>().volume = 0;
            }
            else if (SceneManager.GetActiveScene().name.Equals("RoomDreamScene"))
            {
                if (GameObject.Find("Server").transform.GetChild(1).gameObject.activeSelf == false)
                {
                    GameObject.Find("Server").transform.GetChild(1).gameObject.SetActive(true);
                    GameObject.Find("Server").transform.GetChild(3).gameObject.SetActive(true);
                }
                GameObject.Find("DreamSound").GetComponent<AudioSource>().volume = 0;
                if (!isOnceAudioStop)
                {
                    GameObject.Find("Room").transform.Find("GameWorld").transform.Find("IL3DN_Audio").gameObject.SetActive(false);
                    isOnceAudioStop = true;
                }
            }
        }

        // kinect, video setting
        if (SceneManager.GetActiveScene().name.Equals("RoomDreamScene") || SceneManager.GetActiveScene().name.Equals("RoomCloset") || SceneManager.GetActiveScene().name.Equals("RoomAttic"))
        {
            video = GameObject.Find("DEV").transform.Find("Video").gameObject;

            if (!isOnceRoomIndexSetting)
            {
                roomDreamIdx = GameObject.Find("admin1@").GetComponent<PlayerMoveController>().roomDreamIdx;
                isOnceRoomIndexSetting = true;
            }

            if (SceneManager.GetActiveScene().name.Equals("RoomDreamScene") || SceneManager.GetActiveScene().name.Equals("RoomCloset"))
                if (!isOnceSetting)
                {
                    SetKinectCamera();
                    isOnceSetting = true;
                }
        }

        if (SceneManager.GetActiveScene().name.Equals("RoomAttic"))
        {
            if (startAtticParticle)
            {
                for (int i = 0; i < 2; i++)
                    if (!GameObject.Find("HealStream3").transform.GetChild(i).GetComponent<ParticleSystem>().isPlaying)
                        GameObject.Find("HealStream3").transform.GetChild(i).GetComponent<ParticleSystem>().Play();
            }
            else
            {
                for (int i = 0; i < 2; i++)
                    if (GameObject.Find("HealStream3").transform.GetChild(i).GetComponent<ParticleSystem>().isPlaying)
                        GameObject.Find("HealStream3").transform.GetChild(i).GetComponent<ParticleSystem>().Stop();
            }
        }

        // sky change
        if (SceneManager.GetActiveScene().name.Equals("RoomDreamScene"))
        {
            if (skyChange)
                if (roomDreamIdx == 6)
                {
                    if (skyTime >= limitSkyTime)
                    {
                        skyTime -= (Time.deltaTime * alphaSkyTime);
                        GameObject.Find("Azure[Sky]_Controller").GetComponent<AzureSky_Controller>().SetTime(skyTime, 0);
                    }
                    else
                        skyChange = false;
                }
                else
                {
                    if (skyTime <= limitSkyTime)
                    {
                        skyTime += (Time.deltaTime * alphaSkyTime);
                        GameObject.Find("Azure[Sky]_Controller").GetComponent<AzureSky_Controller>().SetTime(skyTime, 0);
                    }
                    else
                        skyChange = false;
                }
            else
            {
                if(roomDreamIdx == 6)
                {
                    if (skyTime <= limitSkyTime)
                        GameObject.Find("Azure[Sky]_Controller").GetComponent<AzureSky_Controller>().SetTime(limitSkyTime, 0);
                }
                else
                {
                    if (skyTime >= limitSkyTime)
                        GameObject.Find("Azure[Sky]_Controller").GetComponent<AzureSky_Controller>().SetTime(limitSkyTime, 0);
                }
            }
        }

        // video 
        if ((SceneManager.GetActiveScene().name.Equals("RoomDreamScene") || SceneManager.GetActiveScene().name.Equals("RoomCloset") || SceneManager.GetActiveScene().name.Equals("RoomAttic")) && offlineAccount.Contains(this.playerEmail) && video.GetComponent<VideoManager>().isVideoStart)
        {
            GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(false);
        }

        if ((SceneManager.GetActiveScene().name.Equals("RoomDreamScene") || SceneManager.GetActiveScene().name.Equals("RoomCloset") || SceneManager.GetActiveScene().name.Equals("RoomAttic"))  && video.GetComponent<VideoManager>().isVideoEnd && isOnceVideoEnd)
        {
            video.SetActive(false);
            video.GetComponent<VideoManager>().isVideoEnd = false;
            video.GetComponent<VideoManager>().isVideoStart = false;
            GameObject.Find("VideoCanvas").transform.Find("RawImage").GetComponent<RawImage>().texture = video.GetComponent<VideoManager>().videoTexture;
            if(!offlineAccount.Contains(this.playerEmail) && !kinectAccount.Contains(this.playerEmail))
            {
                GameObject.Find("DreamSound").GetComponent<AudioSource>().volume = 1;
                GameObject.Find("DEV").transform.Find("Canvas").gameObject.SetActive(true);
                if(SceneManager.GetActiveScene().name.Equals("RoomDreamScene"))
                    GameObject.Find("Room").transform.Find("GameWorld").transform.Find("IL3DN_Audio").gameObject.SetActive(true);
            }else if (offlineAccount.Contains(this.playerEmail))
            {
                GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(true);
            }

            if (roomDreamIdx == 3 && !offlineAccount.Contains(this.playerEmail) && !kinectAccount.Contains(this.playerEmail) )
            {
                notification.GetComponent<TextMeshProUGUI>().text = "꿈의 세계에 회복이 필요합니다. 모두 강가의 다리로 이동합시다.";
                StartCoroutine(ShowTextWithDelay());
            }
            isPossibleFire = true;
            isOnceVideoEnd = false;
        }

        // firefly
        if (isPossibleFire && Input.GetKey(KeyCode.LeftShift))
        {
            PhotonNetwork.Instantiate(firefly.name, tr.position, Quaternion.identity);
        }

        
    }

    IEnumerator ExitExhibition()
    {
        isReturn = false;
        yield return new WaitForSeconds(60f);
        Application.Quit();
    }


    IEnumerator ShowTextWithDelay()
    {
        yield return new WaitForSeconds(15f);
        notification.GetComponent<TextMeshProUGUI>().text = "모두의 도움이 필요합니다. 각자의 작은 빛을 모아서 꿈의 세계를 밝혀 봅시다.\n왼쪽 shift 버튼을 눌러 반딧불이를 띄워주세요.";
    }


    // Player sit animation
    void SetSitAnim()
    {
        Camera.main.GetComponent<SmoothFollow>().target = tr.GetChild(2).GetChild(1).GetChild(0).GetChild(0).transform;
        playerAnim.SetBool("run", false);
        playerAnim.SetTrigger("sit");
    }

    // Mouse lock
    void MouseLock(bool m)
    {
        if (m)
            Cursor.lockState = CursorLockMode.Locked;
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    //Player의 x축, z축 움직임을 담당
    void FPMove()
    {
        bool isWalking = !Input.GetKey(KeyCode.LeftShift);
        float moveSpeed = isWalking ? movementSpeed : runSpeed;

        //move
        forwardSpeed = Input.GetAxis("Vertical") * moveSpeed;
        sideSpeed = Input.GetAxis("Horizontal") * moveSpeed;

        if (forwardSpeed != 0 || sideSpeed != 0)
        {
            Camera.main.GetComponent<SmoothFollow>().target = tr.Find("CamPivot").transform;
            playerAnim.SetBool("run", true);

            ProgressStepCycle(moveSpeed);
           // PlayFootStepAudio();
        }
        else
            playerAnim.SetBool("run", false);

        // jump
        if (cc.isGrounded && Input.GetButtonDown("Jump") && !stopJumping)
        {
            Camera.main.GetComponent<SmoothFollow>().target = tr.Find("CamPivot").transform;
            verticalVelocity = jumpSpeed;

            PlayJumpSound();
            playerAnim.SetTrigger("jump");
        }

        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        speed = new Vector3(sideSpeed, verticalVelocity, forwardSpeed);
        speed = transform.rotation * speed;

        cc.Move(speed * Time.deltaTime);
    }

    private float m_StepCycle = 0f;
    private float m_NextStep = 0f;

    private void ProgressStepCycle(float speed)
    {
        if (cc.velocity.sqrMagnitude > 0)
        {
            m_StepCycle += (cc.velocity.magnitude + (speed * 1f)) * Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + 2;

        PlayFootStepAudio();
    }

    private void PlayJumpSound()
    {
        playerAudio.clip = jumpSound;
        playerAudio.Play();
    }

    private void PlayFootStepAudio()
    {
        if (!cc.isGrounded)
            return;

        int n = UnityEngine.Random.Range(1, footStepsSounds.Length);
        playerAudio.clip = footStepsSounds[n];
        playerAudio.PlayOneShot(playerAudio.clip);
        // move picked sound to index 0 so it's not picked next time
        footStepsSounds[n] = footStepsSounds[0];
        footStepsSounds[0] = playerAudio.clip;
    }

    //Player의 회전을 담당
    void FPRotate()
    {
        MouseLock(true);
        //좌우 회전
        rotLeftRight = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0f, rotLeftRight, 0f);

        //상하 회전
        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        Camera.main.GetComponent<SmoothFollow>().target.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    public void setDesPos(Vector3 pos) {
        Vector3 desPos = pos;

        if (Vector3.Distance(tr.position, desPos) < 2f) {
            isLoading = false;
            return;
        }

        Vector3 destinationDir = tr.InverseTransformPoint(desPos);
        float angle = Mathf.Atan2(destinationDir.x, destinationDir.z) * Mathf.Rad2Deg;
        transform.Rotate(0f, angle, 0f);

        Vector3 direction = desPos - transform.position;
        direction = Vector3.Normalize(direction);

        cc.Move(direction * Time.deltaTime * 30.0f);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentScene);
            stream.SendNext(isClosetEnd);
            stream.SendNext(isClosetStart);
            stream.SendNext(roomDreamIdx);
        }
        else
        {
            this.currentScene = (string)stream.ReceiveNext();
            this.isClosetEnd = (bool)stream.ReceiveNext();
            this.isClosetStart = (bool)stream.ReceiveNext();
            this.roomDreamIdx = (int)stream.ReceiveNext();
        }
    }


    private bool SetKinectScene()
    {
        if (this.playerEmail.Equals("admin1@")) return false;

        if (this.playerEmail.Equals("bubble@") || this.playerEmail.Contains("admin"))
        {
            PhotonNetwork.IsMessageQueueRunning = false;
            SceneManager.LoadScene("RoomDreamScene");
            PhotonNetwork.IsMessageQueueRunning = true;
            return true;
        }

        if (this.playerEmail.Equals("closet@"))
        {
            PhotonNetwork.IsMessageQueueRunning = false;
            SceneManager.LoadScene("RoomCloset");
            PhotonNetwork.IsMessageQueueRunning = true;
            return true;
        }

        return false;
    }

    private void SetKinectCamera()
    {
        Debug.Log("SetKincetCamera " + this.playerEmail);
        switch (this.playerEmail)
        {
            case "admin1@":
                GameObject.Find("DEV").transform.Find("ServerCamera").gameObject.SetActive(true);
                break;
            case "admin2@":
            case "admin3@":
                this.GetComponent<AudioListener>().enabled = false;
                GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(true);
                GameObject.Find("Room").SetActive(false);
                GameObject.Find("DreamSound").GetComponent<AudioSource>().volume = 0;
                GameObject.Find("DEV").transform.Find("Canvas").gameObject.SetActive(false);
                SetDefaultUIInVisible();
                break;
            case "bubble@":
                Debug.Log("bubble");
                m_Camera.enabled = false;
                GameObject.Find("OfflinePivot").transform.Find("BubbleCamera").GetComponent<Camera>().enabled = true;
                GameObject.Find("DEV").transform.Find("Canvas").gameObject.SetActive(false);
                GameObject.Find("DreamSound").GetComponent<AudioSource>().volume = 0;
                GameObject.Find("Room").transform.Find("GameWorld").transform.Find("IL3DN_Audio").gameObject.SetActive(false);
                SetDefaultUIInVisible();
                GameObject.Find("BalloonSystem").transform.Find("PR_Offline_Depth").gameObject.SetActive(true);
                break;
            case "closet@":
                m_Camera.enabled = false;
                GameObject.Find("ClosetCamera").GetComponent<Camera>().enabled = true;
                GameObject.Find("DEV").transform.Find("KincetController").gameObject.SetActive(true);
                GameObject.Find("DEV").transform.Find("Canvas").gameObject.SetActive(false);
                GameObject.Find("DreamSound").GetComponent<AudioSource>().volume = 0;
                SetDefaultUIInVisible();
                break;
            default:
                if (SceneManager.GetActiveScene().name == "RoomDreamScene")
                    GameObject.Find("OfflinePivot").transform.Find("BubbleCamera").GetComponent<Camera>().enabled = false;
                else
                    GameObject.Find("ClosetCamera").GetComponent<Camera>().enabled = false;
                break;
        }
    }

    private void SetDefaultUIInVisible()
    {
        for(int i = 0; i< defaultUICanvas.transform.childCount-1; i++)
        {
            defaultUICanvas.transform.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < defaultUICanvas.transform.GetChild(3).childCount; i++)
        {
            defaultUICanvas.transform.GetChild(3).GetChild(i).gameObject.SetActive(false);
        }
    }

    // RPC 
    [PunRPC]
    void RpcChangeScene(string sceneName, string loading)
    {
        if(kinectAccount.Contains(this.playerEmail) || offlineAccount.Contains(this.playerEmail)) // kinect & offline account
        {
            if (sceneName.Equals("OpeningScene"))
            {
                GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(true);
            }
            return;
        }

        if (pv.IsMine)
        {
            if (sceneName.Equals("OpeningScene"))
                isReturn = true;

            LoadingSceneManager.LoadScene(sceneName, loading);
        }
        
        if(PhotonNetwork.IsMasterClient)
            currentScene = sceneName;
    }


    [PunRPC]
    void RpcSetLimitText()
    {
        if (!this.pv.IsMine) return;
        if (this.playerEmail.Equals("closet@")) return;

        string notifyText = "";
        if (SceneManager.GetActiveScene().name == "RoomRealScene")
        {
            FadeSound.fade = true;
            notifyText = "현실의 방 체험 시간이 1분 남았습니다.\n스트리밍 카메라를 클릭해 꿈의 체험자들과 함께 통로로 넘어갑니다.";
            if (this.playerEmail.Equals("admin1@"))
                GameObject.Find("Server").transform.GetChild(0).GetComponentInChildren<Text>().text = cueText[0];
        }
        else if(GameObject.Find("admin1@").GetComponent<PlayerMoveController>().currentScene.Equals("RoomDreamScene"))//if (SceneManager.GetActiveScene().name == "RoomDreamScene")
        {
            switch (GameObject.Find("admin1@").GetComponent<PlayerMoveController>().roomDreamIdx)
            {
                case 0: // 하늘
                    skyChange = true;
                    if(this.playerEmail.Equals("admin1@"))
                        GameObject.Find("Server").transform.GetChild(0).GetComponentInChildren<Text>().text = cueText[roomDreamIdx + 1];
                    roomDreamIdx++;
                    break;

                case 1:
                    if (this.playerEmail.Equals("admin1@"))
                        GameObject.Find("Server").transform.GetChild(0).GetComponentInChildren<Text>().text = cueText[roomDreamIdx + 1];
                    notifyText = "오두막으로 모두 모여주세요.\n꿈의 세계에 이상이 있습니다.";
                    if (!kinectAccount.Contains(this.playerEmail) && !offlineAccount.Contains(this.playerEmail))
                        GameObject.Find("DreamSound").GetComponent<DreamRoomSound>().ChangeDreamSound(1);
                    startAtticParticle = true;
                    roomDreamIdx++;
                    break;

                case 2: //3-2영상
                    if (this.playerEmail.Equals("admin1@"))
                        GameObject.Find("Server").transform.GetChild(0).GetComponentInChildren<Text>().text = cueText[roomDreamIdx + 1];
                    if (!kinectAccount.Contains(this.playerEmail) && !offlineAccount.Contains(this.playerEmail))
                    {
                        GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[♬ 무거운 분위기의 음악이 흘러나온다.]";
                        StartCoroutine(removeSubtitle());
                        GameObject.Find("DreamSound").GetComponent<AudioSource>().volume = 0;
                        GameObject.Find("DEV").transform.Find("Canvas").gameObject.SetActive(false);
                        if (SceneManager.GetActiveScene().name == "RoomDreamScene")
                            GameObject.Find("Room").transform.Find("GameWorld").transform.Find("IL3DN_Audio").gameObject.SetActive(false);
                    }
                    notifyText = "";
                    pv.RPC("RpcSetVideoClip", RpcTarget.All, 0);
                    startAtticParticle = false;
                    isOnceVideoEnd = true;
                    roomDreamIdx++;
                    break;

                case 3: // 회복 구름
                    skyTime = 16.8f;
                    limitSkyTime = 18f;
                    alphaSkyTime = 0.2f;
                    skyChange = true;
                    if (!kinectAccount.Contains(this.playerEmail) && !offlineAccount.Contains(this.playerEmail))
                        GameObject.Find("DreamSound").GetComponent<DreamRoomSound>().ChangeDreamSound(2);
                    if (this.playerEmail.Equals("admin1@"))
                        GameObject.Find("Server").transform.GetChild(0).GetComponentInChildren<Text>().text = cueText[roomDreamIdx + 1];
                    roomDreamIdx++;
                    break;

                case 4: //3-3 (반딧불이 후)
                        //3-3영상
                    if (this.playerEmail.Equals("admin1@"))
                        GameObject.Find("Server").transform.GetChild(0).GetComponentInChildren<Text>().text = cueText[roomDreamIdx + 1];
                    if (!kinectAccount.Contains(this.playerEmail) && !offlineAccount.Contains(this.playerEmail))
                    {
                        GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[♬ 잔잔하고 부드러운 선율의 음악이 흘러나온다.]";
                        StartCoroutine(removeSubtitle());
                        GameObject.Find("DreamSound").GetComponent<AudioSource>().volume = 0;
                        GameObject.Find("DEV").transform.Find("Canvas").gameObject.SetActive(false);
                        if (SceneManager.GetActiveScene().name == "RoomDreamScene")
                            GameObject.Find("Room").transform.Find("GameWorld").transform.Find("IL3DN_Audio").gameObject.SetActive(false);
                    }
                    notifyText = "";
                    pv.RPC("RpcSetVideoClip", RpcTarget.All, 4);
                    isOnceVideoEnd = true;
                    isPossibleFire = false;
                    roomDreamIdx++;
                    break;
                case 5: //구름 다시 돌아가기
                    skyTime = 18f;
                    limitSkyTime = 14f;
                    alphaSkyTime = 0.2f;
                    skyChange = true;
                    if (this.playerEmail.Equals("admin1@"))
                        GameObject.Find("Server").transform.GetChild(0).GetComponentInChildren<Text>().text = cueText[roomDreamIdx + 1];
                    roomDreamIdx++;
                    break;
                case 6:
                    notifyText = "꿈의 방 체험이 모두 끝났습니다. 1분 뒤에 현실로 돌아갑니다.\n꿈의 설계자와 체험자는 서로에게 박수를 보내주세요!";
                    roomDreamIdx++;
                    break;
            }
        }
        if (this.playerEmail.Equals("admin1@") || (!offlineAccount.Contains(this.playerEmail) && !kinectAccount.Contains(this.playerEmail)))
        {
            notification.GetComponent<TextMeshProUGUI>().text = notifyText;
        }
    }

    IEnumerator removeSubtitle() {
        yield return new WaitForSeconds(10.0f);
        GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "";
    }

    [PunRPC]
    void RpcSetVideoClip(int index)
    {
        if (!pv.IsMine)
            return;
        video = GameObject.Find("DEV").transform.Find("Video").gameObject;
        VideoManager vm = video.GetComponent<VideoManager>();
        switch (this.playerEmail)
        {
            case "admin2@": vm.SetVideoClip(index + 0); break; // 메인
            case "admin3@": vm.SetVideoClip(index + 1); break; // 오두막
            case "bubble@": vm.SetVideoClip(index + 2); break;
            default: vm.SetVideoClip(index + 3); break;
        }
        video.SetActive(true);
        video.GetComponent<VideoManager>().PlayVideo();
    }

    // Admin RPC
    [PunRPC]
    void CmdStartLimit()
    {
        if (!pv.IsMine)
            return;

        StartCoroutine(StartLimit());
    }

    public IEnumerator StartLimit()
    {
        if (!PhotonNetwork.IsMasterClient) yield return null;

        pv.RPC("RpcSetLimitText", RpcTarget.All);

        if (SceneManager.GetActiveScene().name == "RoomRealScene")
        {
            yield return new WaitForSeconds(60.0f);
            notification.GetComponent<TextMeshProUGUI>().text = "";
            pv.RPC("RpcChangeScene", RpcTarget.All, "MiniGameScene", "MiniGameLoadingScene");
        }
        else if (SceneManager.GetActiveScene().name == "RoomDreamScene" && roomDreamIdx == 7)
        {
            yield return new WaitForSeconds(60.0f);
            notification.GetComponent<TextMeshProUGUI>().text = "";
            pv.RPC("RpcChangeScene", RpcTarget.All, "OpeningScene", "LoadingScene");
        }
    }

    [PunRPC]
    void CmdStopVideo()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        pv.RPC("RpcStopVideo", RpcTarget.All);
    }

    [PunRPC]
    void RpcStopVideo()
    {
        if (!pv.IsMine)
            return;

        video = GameObject.Find("DEV").transform.Find("Video").gameObject;
        GameObject.Find("VideoCanvas").transform.Find("RawImage").GetComponent<RawImage>().texture = video.GetComponent<VideoManager>().videoTexture;
        video.GetComponent<VideoManager>().StopVideo();
        video.SetActive(false);
        video.GetComponent<VideoManager>().isVideoStart = false;
        notification.GetComponent<TextMeshProUGUI>().text = "";
        roomDreamIdx--;

        if (roomDreamIdx == 1)
        {
            startAtticParticle = false;

            if (!kinectAccount.Contains(this.playerEmail) && !offlineAccount.Contains(this.playerEmail))
                GameObject.Find("DreamSound").GetComponent<DreamRoomSound>().ChangeDreamSound(0);
        }

        if (roomDreamIdx == 2)
            startAtticParticle = true;

        if(roomDreamIdx == 3)
            if (!kinectAccount.Contains(this.playerEmail) && !offlineAccount.Contains(this.playerEmail))
                GameObject.Find("DreamSound").GetComponent<DreamRoomSound>().ChangeDreamSound(1);

        if (roomDreamIdx == 4)
            isPossibleFire = true;

        if(!kinectAccount.Contains(this.playerEmail) && !offlineAccount.Contains(this.playerEmail))
            GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[♬활기찬 분위기의 음악이 시작된다.]";

        if (offlineAccount.Contains(this.playerEmail)) // offline video account
        {
            GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(true);
        }
        else
        {
            GameObject.Find("Server").transform.GetChild(0).GetComponentInChildren<Text>().text = cueText[roomDreamIdx];
            GameObject.Find("DreamSound").GetComponent<AudioSource>().volume = 1;
            if(!kinectAccount.Contains(this.playerEmail))
                GameObject.Find("DEV").transform.Find("Canvas").gameObject.SetActive(true);
            if (SceneManager.GetActiveScene().name == "RoomDreamScene")
            {
                GameObject.Find("Room").transform.Find("GameWorld").transform.Find("IL3DN_Audio").gameObject.SetActive(true);
                switch (roomDreamIdx)
                {
                    case 0:
                        skyChange = false;
                        skyTime = 11f;
                        limitSkyTime = 16.8f;
                        alphaSkyTime = 0.15f;
                        GameObject.Find("Azure[Sky]_Controller").GetComponent<AzureSky_Controller>().SetTime(skyTime, 0);
                        break;
                    case 2:
                        skyChange = false;
                        limitSkyTime = 16.8f;
                        skyTime = limitSkyTime;
                        break;
                    case 5: 
                        skyChange = false;
                        limitSkyTime = 18f;
                        skyTime = 18f;
                        break;
                }
            }
        }
    }


    // Specific Case
    public void SetPlayerExist(string email)
    {
        playerData.email = email;
        playerData.date = DateTime.Now.ToString("d");
        playerData.round = DataManager.GetCurrentRound();
        playerData.currentScene = GameObject.Find("admin1@").GetComponent<PlayerMoveController>().currentScene;

        var req = JsonConvert.SerializeObject(playerData);
        Debug.Log(req);
        StartCoroutine(DataManager.sendDataToServer("player/setIsExist", req, (raw) =>
        {
            Debug.Log(raw);
        }));

        StartCoroutine(DataManager.sendDataToServer("player/setCurrentScene", req, (raw) =>
        {
            Debug.Log(raw);
        }));
    }

    public void GetPlayerScene(string email)
    {
        playerData = new PlayerData();
        playerData.email = email;
        playerData.date = DateTime.Now.ToString("d");
        playerData.round = DataManager.GetCurrentRound();

        var req = JsonConvert.SerializeObject(playerData);
        Debug.Log(req);
        StartCoroutine(DataManager.sendDataToServer("player/getCurrentScene", req, (raw) =>
        {
            Debug.Log(raw);
            InitScene(Convert.ToString(raw));
        }));
    }

    void SetPlayerCube()
    {
        if (!pv.IsMine)
            return;

        tr.Find("PlayerCube").gameObject.SetActive(true);
    }

    public void StopJumpAndMouse(bool state)
    {
        stopJumping = state;
        stopMouse = state;
    }
    
}