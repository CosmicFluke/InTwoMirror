using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    static GameController _gameController;

    private float _levelCompletion = 0;
    [Range(1, 2)] public int actionPropagationDistance = 1;

    private bool _musicPlaying = true;
    private const bool levelDebug = true;

    // The amount of actions each player starts with, could be unique for each level.
    public int StartActionOne = 3;
    public int StartActionTwo = 3;



    // Use this for initialization
    void Start()
    {
//        if (_gameController == null)
//        {
//            _gameController = GameObject.Find("GameController").GetComponent<GameController>();
//        }

        _levelCompletion = 0;
    }

    void Update()
    {
        // On level complete
        if (_levelCompletion >= 100)
        {

            if (_gameController != null)
            {
                _gameController.GotoScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }

        }

        if (levelDebug)
        {
            if (Input.GetButtonDown("Start"))
            {
                if (_gameController != null)
                {
                    _gameController.GotoScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
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

        Debug.Log("Level is " + _levelCompletion + "% complete.");
    }

    public void ResetLevel()
    {
        _levelCompletion = 0;
        if (_gameController != null)
            _gameController.GotoScene(SceneManager.GetActiveScene().buildIndex);
    }
}