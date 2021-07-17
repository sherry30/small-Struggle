using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;
    public int level = 1;
    public List<float> timeOfBackgroundSong;
    public List<string> naemOfSongs;

    private void Awake()
    {


        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    private void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void levleUp()
    {
        level++;
    }



}
