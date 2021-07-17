using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSoundManager : MonoBehaviour
{
    static AudioSource audioSrc;

    public static AudioClip hope;
    public static AudioClip passion;
    public static AudioClip regret;
    public static AudioClip despair;

    private void Awake()
    {
        hope = Resources.Load<AudioClip>("Hope");
        passion = Resources.Load<AudioClip>("Passion");
        regret = Resources.Load<AudioClip>("Regret");
        despair = Resources.Load<AudioClip>("Despair");
        audioSrc = GetComponent<AudioSource>();
    }

    public static void playBackgroundSong(string clip)
    {
        switch (clip)
        {
            case "Hope":
                audioSrc.PlayOneShot(hope);
                break;
            case "Passion":
                audioSrc.PlayOneShot(passion);
                break;
            case "Regret":
                audioSrc.PlayOneShot(regret);
                break;
            case "Despair":
                audioSrc.PlayOneShot(despair);
                break;

        }
        
    }
}
