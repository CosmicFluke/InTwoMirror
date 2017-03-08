using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    static GameController _gameController;

    public int Level = 0; // In case we need special stuff on certain levels
    private float _levelCompletion = 0;

    private bool _musicPlaying = true;
    private const bool DEBUG = true;

    // Use this for initialization
    void Start()
    {
        if (_gameController == null)
        {
            _gameController = GameObject.Find("GameController").GetComponent<GameController>();
        }
        _levelCompletion = 0;
    }

    void Update()
    {
        // On level complete
        if (_levelCompletion >= 100)
        {
            if (_gameController != null)
                _gameController.GotoScene("ClosingScene");
        }

        if (DEBUG)
        {
            if (Input.GetButtonDown("Start"))
            {
                if (_gameController != null)
                    _gameController.GotoScene("ClosingScene");
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
            _gameController.GotoScene(SceneManager.GetActiveScene().name);
    }
}