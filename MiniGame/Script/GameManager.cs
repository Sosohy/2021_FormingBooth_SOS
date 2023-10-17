using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int level = 0;
    public int stageScore = 0;
    public bool isPlaying = true;
    public bool isClear = true;

    /**For explain Image */
    public static bool isExplainEnd = false;

    public float limitTime = 300;
    private float[] levelLimitTime = { 60, 120 };
    TextMeshProUGUI timerText;

    AudioSource gameAudio;

    int[] stageScores = { 10, 10, 20 };
    public AudioClip[] gameOverSounds = new AudioClip[2];
    public AudioClip[] gameBGM = new AudioClip[3];
    private string[] changeText = {"[♬ 부드러운 느낌의 피아노 선율이 흘러나온다.]", "[♬ 피아노 선율의 박자가 빨라진다.]", "[♬ 박자감이 느껴지는 명쾌한 음악이 흘러나온다.]" };

    public float moveSpeed = 0.4f;
    TextMeshProUGUI subtitle;

    // Start is called before the first frame update
    private void Start()
    {
        timerText = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        gameAudio = this.GetComponent<AudioSource>();
        DefaultUIManager.ActiveInfoImage(0);
        isExplainEnd = false;

        subtitle = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
        subtitle.text = changeText[level];
    }

    // Update is called once per frame
    private void Update()
    {
        if (isExplainEnd)
        {
            if (isPlaying)
            {
/*                if (!gameAudio.isPlaying)
                    gameAudio.Play();*/

                limitTime -= Time.deltaTime;

                if (Mathf.Round(limitTime % 60) == 60)
                    timerText.text = "TIME 0" + (int)limitTime / 60 + " : 00";
                else if (Mathf.Round(limitTime % 60) < 10)
                    timerText.text = "TIME 0" + (int)limitTime / 60 + " : 0" + Mathf.Round(limitTime % 60);
                else
                    timerText.text = "TIME 0" + (int)limitTime / 60 + " : " + Mathf.Round(limitTime % 60);

                //레벨 별 시간
                if (level < 2)
                    levelLimitTime[level] -= Time.deltaTime;
            }

            if (limitTime <= 0 && isPlaying)
            {
                isPlaying = false;
                GameOver();
            }

            if (level < 2 && ((stageScore >= stageScores[level] && levelLimitTime[level] > 0) || levelLimitTime[level] < 0) && isClear && isPlaying)
            {
                Debug.Log("레벨 변경");
                ChangeLevel();
            }
        }
    }

/*    bool CheckChangeLevel()
    {
        if ((stageScore >= stageScores[level]) && (levelLimitTime[level] <= 0))
            return true;
        else if ((stageScore >= stageScores[level]) || (levelLimitTime[level] > 0))
            return true;
        else if ((stageScore < stageScores[level]) || (levelLimitTime[level] <= 0))
            return true;
        else if ((stageScore < stageScores[level]) || (levelLimitTime[level] > 0))
            return false;
        return false;
    }*/

    void GameOver() {
        isPlaying = false;
        isExplainEnd = false;
        if (ScoreBar.score < 50)
            isClear = false;

        StartCoroutine(StartEndingAnim());
    }



    IEnumerator StartEndingAnim()
    {
        GameObject.Find("Child").GetComponent<Animator>().SetTrigger("ending");
        GameObject endingImg = transform.Find("EndingImg").gameObject;
        GameObject endingObj;
        if (isClear)
        {
            gameAudio.clip = gameOverSounds[1];
            subtitle.text = "[빰밤빰바라밤]";    
            endingImg.GetComponentInChildren<TextMeshPro>().text = "Game Clear!";
            endingObj = this.transform.Find("Goodending").gameObject;
        }
        else
        {
            subtitle.text = "[쿠궁]";
            gameAudio.clip = gameOverSounds[0];
            endingImg.GetComponentInChildren<TextMeshPro>().text = "Game Over!";
            endingObj = this.transform.Find("BadEnding").gameObject;
        }
        gameAudio.Play();
        endingImg.SetActive(true);
        endingObj.SetActive(true);
        yield return new WaitForSeconds(1.3f);
        endingObj.GetComponent<Animator>().SetTrigger("ending");
        yield return new WaitForSeconds(1f);


        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (!player.GetComponent<PlayerMoveController>().pv.IsMine) yield return null;

        player.GetComponent<PlayerMoveController>().loadingLocation = new Vector3(0, 0, 0); //(-35.3009f, 7.432f, -15.51192f);
        player.GetComponent<PlayerMoveController>().isLoading = true;

        LoadingSceneManager.LoadScene("RoomDreamScene", "MiniGameLoadingScene");
    }



    public void ChangeLevel()
    {
        subtitle.text = "[따란]";
        this.GetComponent<AudioSource>().Play();
        Debug.Log("점수 변경");
        stageScore = 0;
        level++;

        if (level < 3) //상태 어쩌구 두긴 둬야할듯
        {
            //캐릭터&커튼 애니메이션
            GameObject.Find("Player").transform.GetChild(level).gameObject.SetActive(true);
            GameObject.Find("Player").transform.GetChild(level - 1).gameObject.SetActive(false);
            GameObject.Find("Player").GetComponent<PlayerMoveManager>().ChangeAnimator(level);
            subtitle.text =  changeText[level];
            this.transform.GetChild(0).GetComponent<AudioSource>().clip = gameBGM[level];
            this.transform.GetChild(0).GetComponent<AudioSource>().Play();
        }
    }

    public void SetClear()
    {
        isClear = false;
    }

    public int GetLevel()
    {
        return level;
    }


/*    public void OnClickExplainImg()
    {
        GameObject.Find("ExplainImg").SetActive(false);
        isExplainEnd = true;
    }*/
}
