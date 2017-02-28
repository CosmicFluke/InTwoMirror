using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelClosingController : MonoBehaviour {

    static GameController GameController;

    // Use this for initialization
    void Start()
    {
        if (GameController == null)
        {
            GameController = GameObject.Find("GameController").GetComponent<GameController>();
        }
        //GameController.fadeInCamera();
    }

    // Update is called once per frame
    void Update()
    {


        // On level complete
        if (Input.GetKeyUp(KeyCode.B))
        {
            Debug.Log("Fade out camera...");
            GameController.GotoScene("OpeningScene");
        }

    }
}
