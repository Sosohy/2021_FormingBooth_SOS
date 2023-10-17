namespace IL3DN
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Photon.Realtime;
    using Photon.Pun;
    using UnityStandardAssets.Utility;

    using Random = UnityEngine.Random;
    using UnityEngine.SceneManagement;
    using System;
    using Newtonsoft.Json;
    using FrostweepGames.VoicePro;
    using TMPro;
    using UnityEngine.UI;
    using System.Linq;
    using UnityEngine.Video;

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]

    ///A simplified version of the FPSController from standard assets 
    public class NoServerPlayer : MonoBehaviour, IPunObservable
    {
        [SerializeField] private bool m_IsWalking = false;
        [SerializeField] private float m_WalkSpeed = 2;
        [SerializeField] private float m_RunSpeed = 5;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten = 0.7f;
        [SerializeField] private float m_JumpSpeed = 5;
        [SerializeField] private float m_StickToGroundForce = 10;
        [SerializeField] private float m_GravityMultiplier = 2;
        [SerializeField] private IL3DN_SimpleMouseLook m_MouseLook = default;
        [SerializeField] private float m_StepInterval = 2;
        [SerializeField] private AudioClip[] m_FootstepSounds = default;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound = default;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound = default;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;
        private AudioClip[] footStepsOverride;
        private AudioClip jumpSoundOverride;
        private AudioClip landSoundOverride;
        private bool isInSpecialSurface;

        private Transform tr;

        // Animator playerAnim;
        public Animator playerAnim;
        public string playerEmail = "";
        public PlayerData playerData;

        GameObject notification;
        GameObject defaultUICanvas;
        int roomDreamIdx = 0;

        // Server to All Clients
        public string currentScene = "";

        private string[] kinectAccount = { "bubble@", "wave@" };
        private string kinectClosetAccount = "closet@";

        private bool stopJumping = false;

        GameObject video;
        public GameObject firefly;
        private bool isPossibleFire = false;
        private bool isOnce = true;

        PhotonView pv;

        /// <summary>
        /// Initialize the controller
        /// </summary>
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
            m_MouseLook.Init(transform, m_Camera.transform);

            playerAnim = GetComponent<Animator>();
            pv = PhotonView.Get(this);

            defaultUICanvas = GameObject.Find("PR_DefaultUICanvas").gameObject;
            notification = defaultUICanvas.transform.GetChild(3).gameObject;
            GameObject.Find("Server").transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { pv.RPC("CmdStartLimit", RpcTarget.AllBufferedViaServer); });
            GameObject.Find("Server").transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => { pv.RPC("CmdStopVideo", RpcTarget.AllBufferedViaServer); });

            tr = GetComponent<Transform>();

            if (PhotonNetwork.IsMasterClient)
                currentScene = "RoomRealScene";

            if (pv.IsMine)
            {

                Camera.main.GetComponent<SmoothFollow>().target = tr.Find("CamPivot").transform;
                /*
                                this.playerEmail = GameObject.Find("NetworkManager").GetComponent<PlayerNetworkManager>().playerEmail;*/
                /*                if (this.playerEmail.Contains("admin") || kinectAccount.Contains(this.playerEmail))
                                    playerAnim.SetTrigger("none");*/

                if (SetKinectScene())
                    return;

                /*** Voice Register **/
                int rand = Random.Range(0, 100);
                NetworkRouter.Instance.Register(rand, PhotonNetwork.NickName, Enumerators.NetworkType.PUN2);

                //Invoke("InitScene", 1.0f);
            }

            LoadingSceneManager.LoadScene("NewScene", "LoadingScene"); //RoomDreamScene
        }

        void InitScene()
        {
            Debug.Log("current " + currentScene);
            LoadingSceneManager.LoadScene("RoomDreamScene", "LoadingScene"); //NewScene //RoomRealScene

            /*if (currentScene == "RoomRealScene")
                    LoadingSceneManager.LoadScene(currentScene, "LoadingScene");
                else {
                    Debug.Log("다시 입장");
                    LoadingSceneManager.LoadScene(currentScene, "고정로딩씬");
                }*/
        }

        private void Update()
        {
            if (!pv.IsMine)
                return;

            if (this.playerEmail.Equals("admin1@") && (SceneManager.GetActiveScene().name == "RoomRealScene" || SceneManager.GetActiveScene().name == "RoomDreamScene"))
            {
                if (GameObject.Find("Server").transform.GetChild(0).gameObject.activeSelf == false)
                    GameObject.Find("Server").transform.GetChild(0).gameObject.SetActive(true);
                if (SceneManager.GetActiveScene().name.Equals("RoomDreamScene"))
                {
                    if (GameObject.Find("Server").transform.GetChild(1).gameObject.activeSelf == false)
                        GameObject.Find("Server").transform.GetChild(1).gameObject.SetActive(true);
                }
            }

            if (this.playerEmail.Equals("admin1@") && SceneManager.GetActiveScene().name.Equals("RoomRealScene"))
            {
                Camera.main.GetComponent<SmoothFollow>().target = GameObject.Find("ServerCameraPivot").transform;
                Camera.main.GetComponent<SmoothFollow>().target.rotation = GameObject.Find("ServerCameraPivot").transform.rotation;
            }

            /**의상신 씬도 OR로 추가*/
            if (SceneManager.GetActiveScene().name == "RoomDreamScene" || SceneManager.GetActiveScene().name.Equals("RoomCloset"))
            {
                if (SceneManager.GetActiveScene().name == "RoomDreamScene")
                {
                    video = GameObject.Find("DEV").transform.Find("Video").gameObject;
                    GameObject.Find("DEV").transform.Find("AirBaloon_Make").gameObject.SetActive(true);
                }
                SetKincetCamera();
            }

            if (isPossibleFire && video.GetComponent<VideoManager>().isVideoEnd && Input.GetKey(KeyCode.LeftShift))
            {
                if (isOnce)
                    Instantiate(firefly);
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                Debug.Log("Sit");
                Camera.main.GetComponent<SmoothFollow>().target = tr.GetChild(2).GetChild(1).GetChild(0).GetChild(0).transform;
                playerAnim.SetBool("run", false);
                playerAnim.SetTrigger("sit");
            }

            if (Input.GetMouseButton(1))
            {
                Debug.Log("right");
                if (SceneManager.GetActiveScene().name == "RoomDreamScene" && AirBalloonManage.isPlayerEnter == 2)
                {
                    m_MouseLook.SetCursorLock(false);
                }
                else
                {
                    m_MouseLook.SetCursorLock(true);
                    m_MouseLook.UpdateCursorLock();
                    RotateView();
                }
            }
            else
            {
                m_MouseLook.SetCursorLock(false);
            }


            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = Input.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        private bool SetKinectScene()
        {
            Debug.Log("in set");

            if (this.playerEmail.Equals("admin1@")) return false;

            if (kinectAccount.Contains(this.playerEmail) || this.playerEmail.Contains("admin"))
            {
                SceneManager.LoadScene("RoomDreamScene");
                return true;
            }

            /**의상신 씬으로 연결*/
            if (kinectClosetAccount.Equals(this.playerEmail))
            {
                SceneManager.LoadScene("RoomCloset");
                return true;
            }

            return false;
        }

        private void SetKincetCamera()
        {
            switch (this.playerEmail)
            {
                case "admin1@":
                    GameObject.Find("DEV").transform.Find("ServerCamera").gameObject.SetActive(true);
                    break;
                case "admin2@":
                case "admin3@":
                    GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(true);
                    GameObject.Find("Room").SetActive(false);
                    defaultUICanvas.SetActive(false);
                    break;
                case "bubble@":
                    m_Camera.enabled = false;
                    GameObject.Find("OfflinePivot").transform.Find("BubbleCamera").GetComponent<Camera>().enabled = true;
                    GameObject.Find("DEV").transform.Find("Canvas").gameObject.SetActive(false);
                    defaultUICanvas.SetActive(false);
                    /*                    Camera.main.GetComponent<SmoothFollow>().target = GameObject.Find("OfflinePivot").transform.Find("BubbleCameraPivot").transform;
                                        Camera.main.GetComponent<SmoothFollow>().target.rotation = GameObject.Find("OfflinePivot").transform.Find("BubbleCameraPivot").transform.rotation;*/
                    GameObject.Find("BalloonSystem").transform.Find("PR_Offline_Depth").gameObject.SetActive(true);
                    break;
                case "wave@":
                    Camera.main.GetComponent<SmoothFollow>().target = GameObject.Find("OfflinePivot").transform.Find("WaveCameraPivot").transform;
                    break;
                case "closet@":
                    m_Camera.enabled = false;
                    GameObject.Find("ClosetCamera").GetComponent<Camera>().enabled = true;
                    GameObject.Find("DEV").transform.Find("KincetController").gameObject.SetActive(true);
                    defaultUICanvas.SetActive(false);
                    break;
                default:
                    if (SceneManager.GetActiveScene().name == "RoomDreamScene")
                        GameObject.Find("OfflinePivot").transform.Find("BubbleCamera").GetComponent<Camera>().enabled = false;
                    else
                        GameObject.Find("ClosetCamera").GetComponent<Camera>().enabled = false;
                    break;
            }
        }

        [PunRPC]
        void RpcChangeScene(string sceneName, string loading)
        {
            if (sceneName == "RoomDreamScene")
                pv.RPC("RpcSetPlayerCube", RpcTarget.All, true);
            else
                pv.RPC("RpcSetPlayerCube", RpcTarget.All, false);

            LoadingSceneManager.LoadScene(sceneName, loading);
            currentScene = sceneName;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(currentScene);
            }
            else
            {
                // Network player, receive data
                this.currentScene = (string)stream.ReceiveNext();
            }
        }

        [PunRPC]
        void RpcSetLimitText()
        {
            if (this.playerEmail.Equals("wave@") || this.playerEmail.Equals("closet@")) return;
            string notifyText = "";
            if (SceneManager.GetActiveScene().name == "RoomRealScene")
            {
                GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[♬어두운 분위기의 음악이 서서히 줄어든다.]";
                notifyText = "현실의 방 체험 시간이 1분 남았습니다.\n스트리밍 카메라를 클릭해 키워주세요.";
            }
            else if (SceneManager.GetActiveScene().name == "RoomDreamScene")
            {
                switch (roomDreamIdx)
                {
                    case 0:
                        GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[우르르 쾅!, 천둥번개소리]\n[♬ 어두운 분위기의 음악이 시작된다.]";
                        notifyText = "오두막으로 모두 모여주세요.\n꿈의 세계에 이상이 있습니다.";
                        roomDreamIdx++;
                        break;
                    case 1: //3-2영상
                            //3-2영상 //사운드 플레이
                        pv.RPC("RpcSetVideoClip", RpcTarget.All, 0, video);
                        video.SetActive(true);
                        isPossibleFire = true;
                        roomDreamIdx++;
                        break;
                    case 2: //3-3 (반딧불이 후)
                            //3-3영상
                        pv.RPC("RpcSetVideoClip", RpcTarget.All, 4, video);
                        video.SetActive(true);
                        isPossibleFire = false;
                        roomDreamIdx++;
                        break;
                    case 3:
                        notifyText = "꿈의 방 체험이 모두 끝났습니다.\n1분 뒤에 현실로 돌아갑니다.";
                        roomDreamIdx++;
                        break;
                }
            }
            notification.GetComponent<TextMeshProUGUI>().text = notifyText;
        }

        [PunRPC]
        void RpcSetVideoClip(int index, GameObject video)
        {
            VideoManager vm = video.GetComponent<VideoManager>();
            switch (this.playerEmail)
            {
                case "admin2@": GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(false); vm.SetVideoClip(index + 0); break; // 메인
                case "admin3@": GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(false); vm.SetVideoClip(index + 1); break; // 오두막

                case "bubble@": vm.SetVideoClip(index + 2); break;
                default: vm.SetVideoClip(index + 3); break;
            }
        }

        [PunRPC]
        void RpcSetPlayerCube(bool cube)
        {
            tr.Find("PlayerCube").gameObject.SetActive(cube);
        }

        [PunRPC]
        void CmdStartLimit()
        {
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
            else if (SceneManager.GetActiveScene().name == "RoomDreamScene" && roomDreamIdx == 4)
            {
                yield return new WaitForSeconds(60.0f);
                notification.GetComponent<TextMeshProUGUI>().text = "";
                pv.RPC("RpcChangeScene", RpcTarget.All, "OpeningScene", "LoadingScene");
            }
        }

        [PunRPC]
        void CmdStopVideo()
        {
            StopVideo();
        }

        public void StopVideo()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (SceneManager.GetActiveScene().name == "RoomDreamScene")
                pv.RPC("RpcStopVideo", RpcTarget.All);
        }

        [PunRPC]
        void RpcStopVideo()
        {
            if (!this.playerEmail.Equals("admin1@") && this.playerEmail.Contains("admin"))
                GameObject.Find("VideoCanvas").transform.Find("Black4Admin").gameObject.SetActive(true);

            GameObject vp = GameObject.Find("DEV").transform.Find("Video").gameObject;
            vp.GetComponent<VideoManager>().StopVideo();
            vp.SetActive(false);
            roomDreamIdx--;
        }

        /// <summary>
        /// Plays a sound when Player touches the ground for the first time
        /// </summary>
        private void PlayLandingSound()
        {
            if (isInSpecialSurface)
            {
                m_AudioSource.clip = landSoundOverride;
            }
            else
            {
                m_AudioSource.clip = m_LandSound;
            }
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }

        /// <summary>
        /// Move the Player
        /// </summary>
        private void FixedUpdate()
        {
            if (!pv.IsMine)
                return;

            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump && !stopJumping)
                {
                    Camera.main.GetComponent<SmoothFollow>().target = tr.Find("CamPivot").transform;
                    playerAnim.SetTrigger("jump");
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;

                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);

            m_MouseLook.UpdateCursorLock();
        }

        /// <summary>
        /// Plays a jump sound
        /// </summary>
        private void PlayJumpSound()
        {
            if (isInSpecialSurface)
            {
                m_AudioSource.clip = jumpSoundOverride;
            }
            else
            {
                m_AudioSource.clip = m_JumpSound;
            }
            m_AudioSource.Play();
        }

        /// <summary>
        /// Play foot steps sound based on time and velocity
        /// </summary>
        /// <param name="speed"></param>
        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }

        /// <summary>
        /// Plays a random sound for a footstep 
        /// </summary>
        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            if (!isInSpecialSurface)
            {
                // pick & play a random footstep sound from the array,
                // excluding sound at index 0
                int n = Random.Range(1, m_FootstepSounds.Length);
                m_AudioSource.clip = m_FootstepSounds[n];
                m_AudioSource.PlayOneShot(m_AudioSource.clip);
                // move picked sound to index 0 so it's not picked next time
                m_FootstepSounds[n] = m_FootstepSounds[0];
                m_FootstepSounds[0] = m_AudioSource.clip;

            }
            else
            {
                int n = Random.Range(1, footStepsOverride.Length);
                if (n >= footStepsOverride.Length)
                {
                    n = 0;
                }
                m_AudioSource.clip = footStepsOverride[n];
                m_AudioSource.PlayOneShot(m_AudioSource.clip);
                footStepsOverride[n] = footStepsOverride[0];
                footStepsOverride[0] = m_AudioSource.clip;
            }
        }

        /// <summary>
        /// Reads user input
        /// </summary>
        /// <param name="speed"></param>
        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool waswalking = m_IsWalking;
#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;

            if (horizontal != 0 || vertical != 0)
            {
                Camera.main.GetComponent<SmoothFollow>().target = tr.Find("CamPivot").transform;
                // playerAnim.animator.SetBool("sit", false);
                playerAnim.SetBool("run", true);
            }
            else
            {
                playerAnim.SetBool("run", false);
            }

            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }
        }

        /// <summary>
        /// Moves camera based on player position
        /// </summary>
        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, Camera.main.GetComponent<SmoothFollow>().target);
        }

        /// <summary>
        /// Used to determine if a player is in a special area to override  the sounds
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            IL3DN_ChangeWalkingSound soundScript = other.GetComponent<IL3DN_ChangeWalkingSound>();
            if (soundScript != null)
            {
                footStepsOverride = soundScript.footStepsOverride;
                jumpSoundOverride = soundScript.jumpSound;
                landSoundOverride = soundScript.landSound;
                isInSpecialSurface = true;
            }
        }

        /// <summary>
        /// Player exits the special area
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            isInSpecialSurface = false;
        }

        public void SetPlayerExist(string email)
        {
            playerData.email = email;
            playerData.date = DateTime.Now.ToString("d");
            playerData.round = DataManager.GetCurrentRound();

            var req = JsonConvert.SerializeObject(playerData);
            Debug.Log(req);
            StartCoroutine(DataManager.sendDataToServer("player/setIsExist", req, (raw) =>
            {
                Debug.Log(raw);
            }));
        }

        public void StopJump(bool state)
        {
            stopJumping = state;
        }
    }
}

