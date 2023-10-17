using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMoveManager : MonoBehaviour
{
    GameManager gameManager;
    Animator playerAnimManager;

    BoxCollider2D charCollder;
    public float[] jump = { 4.5f, 5.5f, 6.2f };
    int jumpCount = 0;

    TextMeshProUGUI subtitle;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").transform.GetComponent<GameManager>();
        playerAnimManager = this.GetComponentInChildren<Animator>();

        charCollder = this.GetComponent<BoxCollider2D>();
        charCollder.offset = new Vector2(0, 10);
        charCollder.size = new Vector2(135, 441);

        subtitle = GameObject.Find("PR_DefaultUICanvas").transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (GameManager.isExplainEnd)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                PlayerJump();

            Move();
        }
    }

    void Move()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            gameManager.moveSpeed = 0.6f;
        else if (Input.GetKeyUp(KeyCode.RightArrow))
            gameManager.moveSpeed = 0.4f;
        /*
         * float x = Input.GetAxisRaw("Horizontal");
         * float y = Input.GetAxisRaw("Vertical");
         * Vector3 moveVelocity = new Vector3(x, 0, 0) * moveSpeed * Time.deltaTime;
         * this.transform.position += moveVelocity;
         */

    }


    public void PlayerJump()
    {
        this.GetComponent<AudioSource>().Play();
        playerAnimManager.SetTrigger("jump");
        float j = jump[gameManager.level];

        if (jumpCount < 2)
            subtitle.text = "[»ÇÀ×]";
        
        if (jumpCount == 0)
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector3(0, gameObject.GetComponent<Rigidbody2D>().velocity.y + j, -0.5f);
        else if (jumpCount == 1)
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector3(0, gameObject.GetComponent<Rigidbody2D>().velocity.y / 2 + j, -0.5f);
        jumpCount += 1;
    }

    public void ChangeAnimator(int level) {
        playerAnimManager = this.transform.GetChild(level).GetComponent<Animator>();

        if (level == 1)
        {
            charCollder.offset = new Vector2(0, -17);
            charCollder.size = new Vector2(135, 366);
        }
        else {
            charCollder.offset = new Vector2(0, -37);
            charCollder.size = new Vector2(135, 319);
        }
    }


    //¹Ù´Ú°ú Ãæµ¹½Ã(Á¡ÇÁ ÈÄ ÂøÁöÇÏ¸é) µ¿ÀÛ
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.CompareTo("Land") == 0)
            jumpCount = 0;

        if (collision.transform.tag == "curtain")
            gameManager.stageScore--;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "good")
        {
            playerAnimManager.ResetTrigger("jump");
            playerAnimManager.ResetTrigger("bad");
            playerAnimManager.SetTrigger("good");
        }

        else if (collision.transform.tag == "bad" || collision.transform.tag == "barrier")
        {
            playerAnimManager.ResetTrigger("jump");
            playerAnimManager.ResetTrigger("good");
            playerAnimManager.SetTrigger("bad");
        }
         
    }
}