using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLevel0Controller : MonoBehaviour {

    public GameObject gameBoardPrefab;

    private GameObject gameBoard;

	// Use this for initialization
	void Start () {
        gameBoard = Instantiate(gameBoardPrefab, transform);
        gameBoard.GetComponent<GameBoard>().Generate();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
