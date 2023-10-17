using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public RawImage mScreen = null;
    public VideoPlayer vp;
    public Texture videoTexture;
    public bool isVideoEnd = false;
    public bool isVideoStart = false;

    public VideoClip[] videoClip;

    private void Awake()
    {
        vp = gameObject.GetComponent<VideoPlayer>();
    }
    private void Start()
    {
        //GameObject.Find("UICanvas").transform.Find("GameManager").gameObject.SetActive(false);
        PlayVideo();
    }

    public void PlayVideo()
    {
        StartCoroutine(PlayingVideo());
    }

    IEnumerator PlayingVideo()
    {
        vp.Prepare();
        WaitForSeconds waitTime = new WaitForSeconds(1.0f);
        while (!vp.isPrepared)
        {
            Debug.Log("동영상 준비중...");
            yield return null;
            //yield return waitTime;
        }

        isVideoStart = true;
        // VideoPlayer의 출력 texture를 RawImage의 texture로 설정한다
        mScreen.texture = vp.texture;

        Debug.Log("동영상이 준비가 끝났습니다.");
        Debug.Log("동영상이 재생됩니다.");

        while (vp.isPlaying)
        {
            yield return null;
        }

        Debug.Log("영상이 끝났습니다.");
        isVideoEnd = true;

        /*GameObject.Find("UICanvas").transform.Find("GameManager").gameObject.SetActive(true);
        this.gameObject.SetActive(false);*/
    }

    public void StopVideo()
    {
        if (vp != null && vp.isPrepared)
        {
            // 비디오 멈춤
            vp.Stop();
        }
    }

    public void SetVideoClip(int index)
    {
        vp.clip = videoClip[index];
    }
}
