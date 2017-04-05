using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    static GameController _gameController; // TODO: use GameController singleton instead

    private float _levelCompletion = 0;

    private bool _musicPlaying = true;
    private const bool levelDebug = true;

    // The amount of actions each player starts with, could be unique for each level.
    public int StartActionOne = 3;
    public int StartActionTwo = 3;

    public Canvas PauseMenu;

	public AudioClip levelCompletionSound;
    public GameObject levelCompletionSplash;

    // Use this for initialization
    void Start()
    {
//        if (_gameController == null)
//        {
//            _gameController = GameObject.Find("GameController").GetComponent<GameController>();
//        }

        _levelCompletion = 0;
        PauseMenu = Instantiate(PauseMenu);
        PauseMenu.enabled = false;


        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player1"), LayerMask.NameToLayer("Player2"));
    }

    void Update()
    {
        if (Input.GetButtonDown("Start"))
        {
            if (PauseMenu.enabled)
            {
                PauseMenu.enabled = false;
                Time.timeScale = 1.0f;
            }
            else
            {
                PauseMenu.enabled = true;
                Time.timeScale = 0f;
            }
        }

        if (levelDebug)
        {
            if (Input.GetButtonDown("Cancel"))
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

		// On level complete
		if (_levelCompletion >= 100)
		{			
			StartCoroutine(CompleteLevel());
		}
    }

	IEnumerator CompleteLevel()
	{
		// Can stop music and play level end sound
		AudioSource audioSource = GetComponent<AudioSource> ();
		if (audioSource != null && levelCompletionSound != null) {
			audioSource.clip = levelCompletionSound;
            audioSource.loop = false;
			audioSource.Play ();
		}
        GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
        foreach (GameObject goal in goals)
        {
            Light light = goal.GetComponentInChildren<Light>();
            light.color = goal.GetComponentInChildren<MeshRenderer>().material.color;
        }
        for (int i = 0; i < 60; i++)
        {
            foreach (GameObject goal in goals)
            {
                Light light = goal.GetComponentInChildren<Light>();
                light.range += 5;
                light.intensity += 7f / 60;
            }
            yield return new WaitForSeconds(1f / 60);
        }
        yield return new WaitForSeconds(0.5f);

        if (GameController.Instance != null) {
            if (levelCompletionSplash != null)
                GameController.Instance.LoadingSplash(levelCompletionSplash, 2f);
            GameController.Instance.GotoScene (SceneManager.GetActiveScene ().buildIndex + 1);
		} else {
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
		}
	}



    public void ResetLevel()
    {
        Debug.Log("Reset");
        _levelCompletion = 0;
        Time.timeScale = 1.0f;

        if (_gameController != null)
            _gameController.GotoScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}