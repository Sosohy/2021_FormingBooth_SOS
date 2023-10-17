using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using Photon.Pun;


public class Bubble : MonoBehaviour
{
    Vector3 targetPos;
    public bool isBubbleUp = false;
    float rotSpeed = 100f;

    public GameObject psObj;

    Transform tr;
    float limitTime = 0f;
    void Start()
    {

        tr = CreateBubble.bubbleTr;
        float width = tr.localScale.x / 2;

        // targetPos = new Vector3(-17.0f - Random.Range(0.0f, 10.0f), Random.Range(55.0f, 60.0f), transform.position.z - Random.Range(0f, 1.0f));
        targetPos = new Vector3(Random.Range(tr.position.x - width, tr.position.x + width), Random.Range(tr.position.y + 150.0f, tr.position.y + 200.0f), Random.Range((int)tr.position.z - 1, tr.position.z - 3));
    }
    void Update()
    {
        transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));

        if (this.GetComponent<PhotonView>().Owner.Equals(GameObject.Find("bubble@").GetComponent<PhotonView>().Owner) && !isBubbleUp)
            isBubbleUp = true;

        if (isBubbleUp)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.04f); //0.05f
            if (targetPos == transform.position)
                PhotonNetwork.Destroy(this.gameObject);
        }
        else
        {
            if(this.transform.localScale.x < 2.5f)
                this.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
        }
    }

    public void BubbleUp() {
        this.GetComponent<PhotonView>().TransferOwnership(GameObject.Find("bubble@").GetComponent<PhotonView>().Owner);
    }

    
    public void BubbleDestroyEffect()
    {
        psObj.transform.position = this.transform.position;
        PhotonNetwork.Instantiate(psObj.name, psObj.transform.position, Quaternion.identity);
        
    }

}
