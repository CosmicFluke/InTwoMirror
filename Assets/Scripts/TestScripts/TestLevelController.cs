using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLevelController : MonoBehaviour {

    public GameObject gameBoardPrefab;
    private GameBoard gameBoard;

	// Use this for initialization
	void Awake () {
        gameBoard = Instantiate(gameBoardPrefab, transform).GetComponent<GameBoard>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    [ExecuteInEditMode]
    private void Editor()
    {
        gameBoard = Instantiate(gameBoardPrefab, transform).GetComponent<GameBoard>();
        if (gameBoard.IsEmpty) gameBoard.GenerateHexes();
    }
}
