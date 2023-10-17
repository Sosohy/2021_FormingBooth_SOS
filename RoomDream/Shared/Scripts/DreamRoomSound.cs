using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DreamRoomSound : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip[] sounds = new AudioClip[4]; // 3 : ¿ÀµÎ¸·
    int idx = 0;

    bool isAttic = false;
    bool isDream = false;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = sounds[0];
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAttic && SceneManager.GetActiveScene().name == "RoomAttic")
        {
            isAttic = false;
            audioSource.clip = sounds[3];
            audioSource.Play();
        }

        if (isDream && SceneManager.GetActiveScene().name == "RoomDreamScene")
        {
            isDream = false;
            audioSource.clip = sounds[idx];
            audioSource.Play();
        }
    }

    public void ChangeDreamSound(int n) {
        idx = n;
        audioSource.clip = sounds[idx];
        audioSource.Play();
    }

    public void ChangeAtticSound(bool attic)
    {
        if (attic)
            isAttic = true;
        else
            isDream = true;
    }
}
