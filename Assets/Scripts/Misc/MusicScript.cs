using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour 
{
    public static float volume = 1.0f;

    public AudioSource audioSource;

    void Update()
    {
        audioSource.volume = volume;
    }
}
