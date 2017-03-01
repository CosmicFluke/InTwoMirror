using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTutorialController : MonoBehaviour {

    static GameController GameController;

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
        if (Input.GetKeyUp(KeyCode.B))
        {
            if(GameController != null)
            	GameController.GotoScene("ClosingScene");
        }

    }
}
