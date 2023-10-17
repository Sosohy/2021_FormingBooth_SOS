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
            Debug.Log("������ �غ���...");
            yield return null;
            //yield return waitTime;
        }

        isVideoStart = true;
        // VideoPlayer�� ��� texture�� RawImage�� texture�� �����Ѵ�
        mScreen.texture = vp.texture;

        Debug.Log("�������� �غ� �������ϴ�.");
        Debug.Log("�������� ����˴ϴ�.");

        while (vp.isPlaying)
        {
            yield return null;
        }

        Debug.Log("������ �������ϴ�.");
        isVideoEnd = true;

        /*GameObject.Find("UICanvas").transform.Find("GameManager").gameObject.SetActive(true);
        this.gameObject.SetActive(false);*/
    }

    public void StopVideo()
    {
        if (vp != null && vp.isPrepared)
        {
            // ���� ����
            vp.Stop();
        }
    }

    public void SetVideoClip(int index)
    {
        vp.clip = videoClip[index];
    }
}
