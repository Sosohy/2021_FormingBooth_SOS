using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Photon.Pun;
using UnityEngine.Video;
using TMPro;
using UnityEngine.UI;

public class CreateBubble : MonoBehaviour
{
    public GameObject[] itemPref = new GameObject[2];
    bool isClicked = false;
    public static PhotonView bubblePlayer;

    GameObject item;
    AudioSource bubbleAudio;
    public static Transform bubbleTr;
    float limitTime = 0f;
    bool isMakeDefault = false;

    private void Start()
    {
        bubbleAudio = this.GetComponent<AudioSource>();
        bubbleTr = this.transform;
    }

    void Update()
    {
        if ((Input.GetMouseButtonUp(0) || limitTime >= 3.5f) && AirBalloonManage.isPlayerEnter == 2 && item != null)
        {
            bubbleAudio.Stop();
            isClicked = false;
            limitTime = 0;
            item.GetComponent<Bubble>().BubbleUp();
            GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = "[똥!, 비눗방울이 커지는 소리]";
        }

        if(isClicked)
        {
            limitTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0) && AirBalloonManage.isPlayerEnter == 2)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int layerMask = 1 << LayerMask.NameToLayer("bubbleBG");  // Player 레이어만 충돌 체크함
            if (Physics.Raycast(ray, out hit, 100f, layerMask) && !isClicked) //(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) && !isClicked)
            {
                Vector3 hitPos = hit.point;
                hitPos.z = hitPos.z - 1;

                item = PhotonNetwork.Instantiate(itemPref[1].name, hitPos, Quaternion.identity); //Instantiate(itemPref[1]);
                bubbleAudio.Play();
                isClicked = true;
            }
        }
    }

    IEnumerator makeDefaultBubble() {
        float width = transform.localScale.x / 2;
        float height = transform.localScale.y / 2;

        while (isMakeDefault) {
            if (!GameObject.Find("DEV").transform.Find("Video").gameObject.GetComponent<VideoPlayer>().isPlaying) {
                GameObject bubbleDefault = PhotonNetwork.Instantiate(itemPref[0].name, new Vector3(Random.Range(transform.position.x - width, transform.position.x + width), transform.position.y - (height - 5), transform.position.z - 1), Quaternion.identity);
                bubbleDefault.GetComponent<PhotonView>().TransferOwnership(GameObject.Find("bubble@").GetComponent<PhotonView>().Owner);
                float r = Random.Range(0.5f, 1.5f);
                bubbleDefault.transform.localScale = new Vector3(r, r, r);
                bubbleDefault.GetComponent<Bubble>().BubbleUp();
                yield return new WaitForSeconds(2.0f);
            }
        }
    }

    public void BubbleDefaultStart() {
        if (!isMakeDefault) {
            isMakeDefault = true;
            StartCoroutine(makeDefaultBubble());
            GameObject.Find("StartDefaultBubble").GetComponentInChildren<Text>().text = "디폴트 버블 중단";
        }
        else {
            isMakeDefault = false;
            StopCoroutine(makeDefaultBubble());
            GameObject.Find("StartDefaultBubble").GetComponentInChildren<Text>().text = "디폴트 버블 시작";
        }
    }
}
