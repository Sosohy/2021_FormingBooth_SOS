using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StreamingSize : MonoBehaviour
{
    Vector3 originPos;
    bool changeSize = false;

    // Start is called before the first frame update
    void Start()
    {
        originPos = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClickStreaming() {
        if (!changeSize)
        {
            changeSize = true;
            this.transform.localPosition = new Vector3(0, 0, 0);
            this.transform.localScale = new Vector3(2.5f, 2.5f, 1);     
        }
        else {
            changeSize = false;
            this.transform.localPosition = originPos;
            this.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
