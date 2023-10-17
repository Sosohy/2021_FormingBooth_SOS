using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBar : MonoBehaviour
{
    Image scoreBar;
    float maxScore = 50f;
    public static float score;

    GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").transform.GetComponent<GameManager>();
        scoreBar = GetComponent<Image>();
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (score <= maxScore && gameManager.isClear)
            scoreBar.fillAmount = score / maxScore;
       
    }
}
