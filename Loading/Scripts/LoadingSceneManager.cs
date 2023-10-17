using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Photon.Pun;

public class LoadingSceneManager : MonoBehaviour
{
    /*public Slider slider;*/
    public Sprite[] bgImage;
    public AudioClip[] audioClips;
    public static string nextScene;
    private float time;
    private bool isStart = false;
    private bool isOnce = true;

    TextMeshProUGUI subtitle;
    VideoManager videoManager;
    AudioSource audioSource;

    void Start()
    {
        subtitle = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
        videoManager = GameObject.Find("Video Player").GetComponent< VideoManager>();
        audioSource = GetComponent<AudioSource>();
        switch (SceneManager.GetActiveScene().name)
        {
            case "LoadingScene":
                audioSource.clip = audioClips[0];
                if (nextScene.Equals("RoomRealScene"))
                {
                    GameObject.Find("BGImg").GetComponent<Image>().sprite = bgImage[0];
                    GameObject.Find("ExplainText").GetComponent<Text>().text = "온/오프라인 전시가 동시 진행 됩니다.\n실시간 카메라를 통해 오프라인 관객들의 모습을 관람하실 수 있습니다.";
                }else if (nextScene.Equals("OpeningScene"))
                {
                    subtitle.text = "[♬ 벅차오르는 느낌의 음악이 흘러나온다.]";
                    audioSource.clip = audioClips[1];
                    GameObject.Find("ExplainText").GetComponent<Text>().text = "감각의 감각 온라인 관객 공연이 종료되었습니다.\n꿈의 세계를 설게하고 지켜주신 설계자 여러분 감사합니다.";
                }
                videoManager.SetVideoClip(0);
                if (nextScene.Equals("RoomCloset") || nextScene.Equals("RoomAttic") || nextScene.Equals("RoomDreamScene"))
                {
                    videoManager.SetVideoClip(1);
                    GameObject.Find("ExplainText").GetComponent<Text>().text = "꿈의 세계 안에 숨겨진 또 다른 세계가 있을지도?";
                    GetStreamingLinks.dream = false;
                    /*GameObject player =  GameObject.FindGameObjectWithTag("Player");
                    player.GetComponent<PlayerMoveController>().setDesPos(new Vector3(-14.116f, 8.089001f, 35.885f));*/
                }
                subtitle.text = "[♬ 잔잔한 분위기의 음악이 흘러나온다.]";
                audioSource.Play();
                StartCoroutine(LoadAsynSceneWithLoadingBarCoroutine());
                break;
            case "MiniGameLoadingScene":
                if (nextScene.Equals("MiniGameScene"))
                {
                    videoManager.SetVideoClip(0);
                    subtitle.text = "[♬ 신비한 분위기의 음악이 흘러나온다.]";
                }else if (nextScene.Equals("RoomDreamScene"))
                {
                    videoManager.SetVideoClip(1);
                    subtitle.text = "[찌르르, 새가 지저귀는 소리]";
                }
                StartCoroutine(LoadAsynSceneCoroutine());
                break;
            case "BackLoadingScene":
                audioSource.clip = audioClips[0];
                audioSource.Play();
                StartCoroutine(LoadAsynSceneWithLoadingBarCoroutine());
                break;
        }
    }

    private void Update()
    {
        if (isStart)
        {
            if (isOnce)
            {
                UltimateClean.SliderAnimation slider = GameObject.Find("Loading Bar (Color)").GetComponent<UltimateClean.SliderAnimation>();
                slider.SetDuration(25);
                if (nextScene.Equals("RoomCloset") || nextScene.Equals("RoomAttic") || nextScene.Equals("RoomDreamScene"))
                    slider.SetDuration(5);
                slider.startSlidbar();
                isOnce = false;
            }
        }
    }

    public static void LoadScene(string nextSceneName, string loadingSceneName) {
        nextScene = nextSceneName;
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneManager.LoadScene(loadingSceneName); 
    }

    IEnumerator LoadAsynSceneWithLoadingBarCoroutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextScene);
        operation.allowSceneActivation = false;

        time = 0;
        while (!operation.isDone)
        {
            if (videoManager.isVideoStart)
            {
                isStart = true;
                time += Time.deltaTime;
            }
/*                time += Time.deltaTime; ;
            slider.value = time;*/
            if(SceneManager.GetActiveScene().name.Equals("LoadingScene"))
            {
                if(nextScene.Equals("RoomRealScene") )
                {
                    if (time > 4 && time < 8)
                        GameObject.Find("ExplainText").GetComponent<Text>().text = "온라인으로 송출되는 모든 화면은 화면 녹화 및 유포가 철저히 금지되어 있습니다.\n녹화 및 유출 시 민형사상 책임을 질 수 있습니다.";
                    else if (time >= 8 && time < 12)
                        GameObject.Find("ExplainText").GetComponent<Text>().text = "F2 키를 누르시면 베리어프리 자막이 제공됩니다. \n원활한 관람을 위해 이어폰 또는 헤드폰 착용을 권장합니다.";
                    else if (time >= 12 && time < 16)
                        GameObject.Find("ExplainText").GetComponent<Text>().text = "앞으로 들어갈 공간에서는 자유롭게 움직일 수 있습니다. \n발생하는 상황에 맞춰서 유연하게 대처가 가능한 상태를 유지해주십시오.";
                    else if (time >= 16 && time < 20)
                        GameObject.Find("ExplainText").GetComponent<Text>().text = "한 극장에는 현실에 지쳐 꿈을 경험하게 될 체험자들이 리셉션에서 대기하고 있습니다.\n서로에게 영향을 미치며 전시를 경험하고, 때로는 관찰자가 되어 서로를 바라보기도 할 것입니다. ";
                    else if (time > 20)
                        GameObject.Find("ExplainText").GetComponent<Text>().text = "꿈의 ‘설계자’여러분들을 환영합니다. \n꿈의 ‘체험자’들과 함께 현실의 방을 탐색해봅시다.";
                }

                if (nextScene.Equals("OpeningScene"))
                {
                    if (time > 8 && time < 17)
                        GameObject.Find("ExplainText").GetComponent<Text>().text = "수면과 꿈이 회복의 시간임을 믿습니다.\n밤이 잉여와 불모의 시간으로 여겨질지라도 밤에 힘을 빌려 나에게 귀 기울이는 마음이 있다면 그것이 회복의 과정이라 생각합니다.";
                    else if (time >= 17)
                        GameObject.Find("ExplainText").GetComponent<Text>().text = "꿈의 세계는 종료되었지만, 현실로 체크인하는 이미 키는 여러분에게 있습니다. \n조심히 돌아가세요. ";
                }
            }


            if (videoManager.isVideoEnd)
            {
                if (nextScene.Equals("OpeningScene"))
                    Destroy(GameObject.Find("PR_DefaultUICanvas").gameObject);
                operation.allowSceneActivation = true;
                PhotonNetwork.IsMessageQueueRunning = true;
            }
            yield return null;
        }
    }

    IEnumerator LoadAsynSceneCoroutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextScene);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            if (videoManager.isVideoEnd)
            {
                operation.allowSceneActivation = true;
                PhotonNetwork.IsMessageQueueRunning = true;
            }
            yield return null;
        }
    }
}
