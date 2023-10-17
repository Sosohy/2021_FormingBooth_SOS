using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hand : MonoBehaviour
{
    public Transform mHandMash;

    private void Update()
    {
        mHandMash.position = Vector3.Lerp(mHandMash.position, transform.position, Time.deltaTime * 15.0f); //smooth move.
    }


    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.gameObject.CompareTag("Ballon"))
            return;

        if (SceneManager.GetActiveScene().name == "DreamRoomScene")
            collision.gameObject.SetActive(false); // ballon 
    }
}
