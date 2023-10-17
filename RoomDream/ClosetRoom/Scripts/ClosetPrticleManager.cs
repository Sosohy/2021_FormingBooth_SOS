using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using TMPro;

public class ClosetPrticleManager : MonoBehaviour
{
    public GameObject[] particles;
    GameObject player;
    public void Start()
    {
        DefaultUIManager.ActiveInfoImage(2);
    }

    private void Update()
    {
    }

    string[] effectSubtitle = { "[¶î¸®¸µ]", "[ÃÒ¶ó¶û]", "[¶ì¸®¸µ]", "[Æã!]", "[ÇÇÀ¶-Æã!]"}; 
    public void onClickEffecttBtn()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        string numString = name.Substring(name.Length - 1, 1);

        InitPrticles();
        PhotonNetwork.Instantiate(particles[int.Parse(numString)].name, GameObject.Find("FxPos"+ numString).transform.position, GameObject.Find("FxPos" + numString).transform.rotation);

        if (player.GetComponent<PhotonView>().IsMine)
            GameObject.Find("PR_DefaultUICanvas").transform.Find("Subtitle").GetComponentInChildren<TextMeshProUGUI>().text = effectSubtitle[int.Parse(numString)];
    }

    private void InitPrticles()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        for(int i = 0; i< players.Length; i++)
        {
            if (!players[i].name.Equals("closet@"))
            {
                player = players[i];
                break;
            }
        }

        GameObject[] particles = GameObject.FindGameObjectsWithTag("particles");
        for(int i = 0; i< particles.Length; i++)
        {
            particles[i].GetComponent<PhotonView>().TransferOwnership(player.GetComponent<PhotonView>().Owner);
            PhotonNetwork.Destroy(particles[i]);
        }

       
    }
}
