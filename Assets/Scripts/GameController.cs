using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    static GameController Instance;


    // Use this for initialization
    void Start()
    {
        if (Instance == null)
        {
            GameObject.DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }

    void GotoScene(string newScene)
    {
        try
        {
            SceneManager.LoadScene(newScene);
        }
        catch (Exception e)
        {
            Debug.Log("Scene not found [" + newScene + "]");
        }
    }

    void Update()
    {
        if (0 != Input.GetAxis("GameControlTutorial"))
        {
            SceneManager.LoadScene("Tutorial");
        }
        if (0 != Input.GetAxis("GameControlLevel1"))
        {
            SceneManager.LoadScene("Level1");
        }
        if (0 != Input.GetAxis("GameControlLevel2"))
        {
            SceneManager.LoadScene("Level2");
        }
        if (0 != Input.GetAxis("GameControlLevel3"))
        {
            SceneManager.LoadScene("Level3");
        }
        if (0 != Input.GetAxis("GameControlClosing"))
        {
            SceneManager.LoadScene("ClosingScene");
        }
        if (0 != Input.GetAxis("GameControlOpening"))
        {
            SceneManager.LoadScene("OpeningScene");
        }

    }
}
