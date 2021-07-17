using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip talk;

    static AudioSource audioSrc;


    private void Awake()
    {
        talk = Resources.Load<AudioClip>("dialogue");
        audioSrc = GetComponent<AudioSource>();
    }


    public static void playClip(string clip)
    {
        switch (clip)
        {
            case "talk":
                audioSrc.PlayOneShot(talk);
                break;
        }
    }

    public static void stopTalk()
    {
        audioSrc.Stop();
    }
}
