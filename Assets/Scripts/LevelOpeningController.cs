using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOpeningController : MonoBehaviour {

    static GameController Instance;

    // Use this for initialization
    void Start()
    {
        if (Instance == null)
        {
            Instance = GameObject.Find("GameController").GetComponent<GameController>();
        }
    }

    // Update is called once per frame
    void Update()
    {


        // On level complete
        if (Input.GetKeyUp(KeyCode.B))
        {
            Debug.Log("Fade out camera...");
            Instance.loadScene("ClosingScene");
        }

    }
}
