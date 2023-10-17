using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BubbleParticelManager : MonoBehaviour
{
    private void Update()
    {
        if (!GetComponent<PhotonView>().IsMine)
            return;

        if (!this.GetComponent<ParticleSystem>().isPlaying)
            PhotonNetwork.Destroy(this.gameObject);
    }
}
