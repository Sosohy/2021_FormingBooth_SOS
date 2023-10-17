using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class item : MonoBehaviour
{
    public float speed;
    GameManager gameManager;

    TextMeshProUGUI subtitle;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").transform.GetComponent<GameManager>();
        subtitle = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
    }
    void Update()
    {
        transform.Translate(Vector2.left * (gameManager.moveSpeed * 18) * Time.deltaTime);

        // x 좌표가 -10보다 작으면 해당 오브젝트를 삭제.
        if (transform.position.x < -10)
        {
            Destroy(gameObject);
        }
    }

    //플레이어에게 부딪히면 사라짐
    void OnTriggerEnter2D(Collider2D collision)
    {
        /*Debug.Log(collision.transform.tag);*/
        if (collision.transform.tag == "Player")
        {
            switch (this.transform.tag)
            {
                case "good":
                case "bad":
                case "barrier":
                    this.gameObject.GetComponent<AudioSource>().Play();
                    break;
            }

            if (this.transform.tag == "good")
            {
                subtitle.text = "[또잉]";
                gameManager.stageScore++;
                ScoreBar.score++;
            }
            else
            {
                subtitle.text = "[탁!]";
                gameManager.stageScore--;
                ScoreBar.score--;
            }

            if (this.transform.tag == "good" || this.transform.tag == "bad" || this.transform.tag == "barrier")
                Destroy(gameObject, this.gameObject.GetComponent<AudioSource>().time + 0.1f);
        }
    }

    
}