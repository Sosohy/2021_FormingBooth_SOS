using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemCreateManager : MonoBehaviour
{
    private Vector3 spawnPos;
    private GameManager gameManager;

    public GameObject[] itemPrefab;
    public GameObject[] barrierPrefab;

    private float[] rangeY = { -0.3f, 2f, 4f};
    public float createTime = 0.1f;
    public int maxItem;
    private int itemCount = 0;
    
    public int maxBarrier;
    private int barrierCount = 0;
    /*public float barrierY = (float)-3.05;*/

    private bool isOnce = true;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").transform.GetComponent<GameManager>();
    }

    private void Update()
    {
        if (GameManager.isExplainEnd && isOnce)
        {
            StartCoroutine(CreateItem());
            StartCoroutine(CreateBarrier());
            isOnce = false;
        }
    }

    // create random items - good, bad
    IEnumerator CreateItem()
    {
        while (gameManager.isPlaying)
        {
            maxItem = Random.Range(1, 5);
            float time = 0f;
            if (itemCount <= maxItem)
            {
                time = Random.Range(1f, 2.5f);
                yield return new WaitForSeconds(time);

                /*yield return new WaitForSeconds(0.5f);      */ 

                int index = Random.Range(0, itemPrefab.Length);
                spawnPos = new Vector3(10, Random.Range(-1.5f, rangeY[gameManager.level]), 0f);

                if (gameManager.level == 0)
                    Instantiate(itemPrefab[1], spawnPos, itemPrefab[index].transform.rotation);
                else {
                    Instantiate(itemPrefab[index], spawnPos, itemPrefab[index].transform.rotation);

                    if (index == 0) {
                        yield return new WaitForSeconds(Random.Range(1f, 1.5f));
                        Instantiate(itemPrefab[1], spawnPos, itemPrefab[index].transform.rotation);
                        itemCount++;
                    }
                }
                itemCount++;
            }
            else
            {
                createTime = Random.Range(1f, 2.5f);
                yield return new WaitForSeconds(createTime);
                itemCount = 0;
            }
        }
    }

    // create random barriers
    IEnumerator CreateBarrier()
    {
        while (gameManager.isPlaying)
        {
            maxBarrier = Random.Range(1, 3);
            float time = 0f;
            if (barrierCount <= maxBarrier)
            {
                time = Random.Range(1f, 3.5f);
                yield return new WaitForSeconds(time);

                int index = Random.Range(0, barrierPrefab.Length);
                Vector3 spawnPosForBarrier = new Vector3(10, (float)-4.04, 0f);//3.05

                Instantiate(barrierPrefab[index], spawnPosForBarrier, Quaternion.identity);

                barrierCount++;
            }
            else
            {
                time = Random.Range(3f, 5f);
                yield return new WaitForSeconds(Random.Range(3f, 5f));
                barrierCount = 0;
            }
        }
    }
}
