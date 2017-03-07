using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTutorialController : MonoBehaviour {

    static GameController GameController;

    private bool musicPlaying = true;

    // Use this for initialization
    void Start()
    {
        if (GameController == null)
        {
            GameController = GameObject.Find("GameController").GetComponent<GameController>();
        }
    //    GameController.fadeInCamera();
    }

    void Update()
    {
        // On level complete
        if (Input.GetKeyUp(KeyCode.B) || Input.GetButtonDown("Start"))
        {
            if(GameController != null)
            	GameController.GotoScene("ClosingScene");
        }
        if (Input.GetButtonDown("ToggleMusic")) {
            AudioSource music = GetComponent<AudioSource>();
            if (music != null)
            {
                if (musicPlaying) music.Stop();
                else music.Play();
                musicPlaying = !musicPlaying;
            }
        }

    }
}
