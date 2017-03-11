using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    static GameController _gameController;

    private float _levelCompletion = 0;

    private bool _musicPlaying = true;
    private const bool levelDebug = true;

    // The amount of actions each player starts with, could be unique for each level.
    public int StartActionOne = 3;
    public int StartActionTwo = 3;

    public GameObject canvas;
    private static Text P1Health;
    private static Text P2Health;

    // Use this for initialization
    void Start()
    {
        //if (_gameController == null)
        //{
        //    _gameController = GameObject.Find("GameController").GetComponent<GameController>();
        //}

        _levelCompletion = 0;

        if (canvas == null && FindObjectOfType(typeof(Canvas)) == null)
        {
            canvas = (GameObject) Instantiate(Resources.Load("UI/BlankCanvas"));
            Debug.Log("Canvas instantiated");
        }
        else if (canvas == null)
        {
            canvas = (GameObject) FindObjectOfType(typeof(Canvas));
            Debug.Log("canvas linked");
        }

        // Programatically instantiate player health UI
        P1Health = ((GameObject) Instantiate(Resources.Load("UI/PlayerHealth"))).GetComponent<Text>();
        P1Health.transform.SetParent(canvas.transform);
        P2Health = ((GameObject)Instantiate(Resources.Load("UI/PlayerHealth"))).GetComponent<Text>();
        P2Health.transform.SetParent(canvas.transform);

        // TODO: finalize clean positioning
        Vector3 bottomLeft = new Vector3(100, 10, 0);
        Vector3 bottomRight = new Vector3(canvas.GetComponent<RectTransform>().rect.width - 50, 10, 0);

        P1Health.GetComponent<RectTransform>().position = bottomLeft;
        P2Health.GetComponent<RectTransform>().position = bottomRight;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            Debug.Log(player.name);
            UpdatePlayerHealth(player.GetComponent<PlayerMovementController>().player, player.GetComponent<PlayerMovementController>().HealthPoints);
        }
    }

    public static void UpdatePlayerHealth(PlayerID id, int hp)
    {
        if(id == PlayerID.P1)
            P1Health.text = "Player 1: " + hp;
        else if(id == PlayerID.P2)
            P2Health.text = "Player 2: " + hp;
    }
    void Update()
    {
        // On level complete
        if (_levelCompletion >= 100)
        {
            if (_gameController != null)
				_gameController.GotoScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        if (levelDebug)
        {
            if (Input.GetButtonDown("Start"))
            {
                if (_gameController != null)
					_gameController.GotoScene(SceneManager.GetActiveScene().buildIndex + 1);
            }

            if (Input.GetButtonDown("ToggleMusic"))
            {
                AudioSource music = GetComponent<AudioSource>();
                if (music != null)
                {
                    if (_musicPlaying) music.Stop();
                    else music.Play();
                    _musicPlaying = !_musicPlaying;
                }
            }
        }
    }

    /**
     *  Progresses through the level.
     */

    public void ProgressLevel(float percent)
    {
        _levelCompletion += percent;
    }

    public void ResetLevel()
    {
        _levelCompletion = 0;
        if (_gameController != null)
			_gameController.GotoScene(SceneManager.GetActiveScene().buildIndex);
    }
}