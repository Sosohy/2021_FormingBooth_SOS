using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMove : MonoBehaviour
{
    GameManager gameManager;

    private Renderer backgroundRenderer;
    public Texture BG1;
    public Texture BG2;
    public Texture BG3;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").transform.GetComponent<GameManager>();
        backgroundRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (GameManager.isExplainEnd)
        {
            MovingBG();
            ChangeBG(gameManager.GetLevel() + 1);
        }
    }


    public void MovingBG()
    {
        Vector2 textureOffset = new Vector2(Time.time * gameManager.moveSpeed, 0);
        backgroundRenderer.material.mainTextureOffset = textureOffset;
    }

    public void ChangeBG(int index)
    {
        if(index == 1)
        {
            backgroundRenderer.material.SetTexture("_MainTex", BG1);
        }
        else if(index == 2)
        {
            backgroundRenderer.material.SetTexture("_MainTex", BG2);
        }
        else if (index == 3)
        {
            backgroundRenderer.material.SetTexture("_MainTex", BG3);
        }
    }
}
