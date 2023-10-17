using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeSound : MonoBehaviour
{
    public static bool fade = false;
    public static bool telephoneFade = false;
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fade) {
            audioSource.volume -= Time.deltaTime * 0.03f;
            if (audioSource.volume <= 0.05)
                fade = false;
        }

        if (telephoneFade) {
            audioSource.volume -= Time.deltaTime * 0.1f;
            if (audioSource.volume == 0)
                telephoneFade = false;
        }
    }
}
