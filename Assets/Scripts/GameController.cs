using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    static GameController Instance;

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

    public void NextScene()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (scene.name.Equals("Tutorial"))
            GotoScene("Level1");
        if (scene.name.Equals("Level1"))
            GotoScene("Level2");
        if (scene.name.Equals("Level2"))
            GotoScene("Level3");
        if (scene.name.Equals("Level3"))
            GotoScene("ClosingScene");
        if (scene.name.Equals("ClosingScene"))
            GotoScene("OpeningScene");
        if (scene.name.Equals("OpeningScene"))
            GotoScene("Tutorial");
    }
    
    public void GotoScene(String newScene)
    {
        SceneManager.LoadScene(newScene);
    }
    
    void Update()
    {
        // TODO: Clean this up to use indices once level orders have been finalized
        if (0 != Input.GetAxis("GameControlTutorial")) //F9
        {
            GotoScene("Tutorial");
        }
        if (0 != Input.GetAxis("GameControlLevel1"))
        {
            GotoScene("Level1");
        }
        if (0 != Input.GetAxis("GameControlLevel2"))
        {
            GotoScene("Level2");
        }
        if (0 != Input.GetAxis("GameControlLevel3"))
        {
            GotoScene("Level3");
        }
        if (0 != Input.GetAxis("GameControlClosing"))
        {
            GotoScene("ClosingScene");
        }
        if (0 != Input.GetAxis("GameControlOpening"))
        {
            GotoScene("OpeningScene");
        }

    }
}
