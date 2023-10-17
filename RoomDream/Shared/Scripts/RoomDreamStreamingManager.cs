using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomDreamStreamingManager : MonoBehaviour
{
    Vector3 originPos;
    float btnOriginY = 0;
    public int current = 0;
    
    public GameObject[] streamingObjs = new GameObject[4];
    GameObject[] btns = new GameObject[2];

    bool changeSize = false;
    bool isBubbleZone = false;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < streamingObjs.Length; i++)
        {
            streamingObjs[i] = GameObject.Find("StreamingObjs").transform.GetChild(i).gameObject;
        }

        btns[0] = GameObject.Find("PR_RoomDreamStreaming").transform.Find("StreamingButtons").gameObject;
        btns[1] = GameObject.Find("PR_RoomDreamStreaming").transform.Find("StreamingButtons_Size").gameObject;

        originPos = streamingObjs[0].transform.GetChild(0).localPosition;
        btnOriginY = btns[0].transform.GetChild(0).localPosition.y;  
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BtnPosChange()
    {
        if (changeSize)
        {
            btns[0].SetActive(false);
            btns[1].SetActive(true);
        }
        else
        {
            btns[0].SetActive(true);
            btns[1].SetActive(false);
        }
    }

    public void onClickStreaming()
    {
        if (isBubbleZone) return;

        if (!changeSize)
        {
            changeSize = true;
            for (int i = 0; i < streamingObjs.Length; i++)
            {
                streamingObjs[i].transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
                streamingObjs[i].transform.GetChild(0).localScale = new Vector3(2.5f, 2.5f, 1);
            }
        }
        else
        {
            changeSize = false;
            setOriginPos();
        }
        BtnPosChange();

    }

    public void onClickRoomDreamStreamingBtn()
    {

        GameObject click = EventSystem.current.currentSelectedGameObject;

        streamingObjs[current].SetActive(false);

        current = click.transform.GetSiblingIndex();
        streamingObjs[current].SetActive(true);
    }

    public void setBubbleStreaming(bool isBubble)
    {
        isBubbleZone = isBubble;
        if (isBubble)
        {
            GameObject.Find("Canvas").transform.Find("BubblePanel").gameObject.SetActive(true);
            GameObject.Find("Canvas").transform.Find("MiniMapImg").gameObject.SetActive(false);
            for (int i = 0; i < streamingObjs.Length; i++)
            {
                streamingObjs[i].transform.GetChild(0).localPosition = new Vector3(originPos.x + 33, 0, 0);
                btns[0].transform.GetChild(i).localPosition = new Vector3(btns[0].transform.GetChild(i).localPosition.x + 33, 220, 0);
            }
            streamingObjs[current].SetActive(false);
            current = 1;
            streamingObjs[current].SetActive(true); 
        }
        else
        {
            GameObject.Find("Canvas").transform.Find("MiniMapImg").gameObject.SetActive(true);
            GameObject.Find("Canvas").transform.Find("BubblePanel").gameObject.SetActive(false);
            setOriginPos();
        }
    }


    void setOriginPos()
    {
        for (int i = 0; i < streamingObjs.Length; i++)
        {
            streamingObjs[i].transform.GetChild(0).localPosition = originPos;
            streamingObjs[i].transform.GetChild(0).localScale = new Vector3(1, 1, 1);

            btns[0].transform.GetChild(i).localPosition = new Vector3(btns[0].transform.GetChild(i).localPosition.x - 33, btnOriginY, 0);
        }
    }
}
