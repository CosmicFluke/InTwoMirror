using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    static GameController Instance;
    public float FadeInDuration = 1000;
    public float FadeOutDuration = 1000;
    GameObject canvas;
    GameObject panel;
    private IEnumerator coroutine;

    // Use this for initialization
    void Start()
    {
        if (Instance == null)
        {
            GameObject.DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }

    }

    public void GotoScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
        //coroutine = LoadScene(newScene);
        //StartCoroutine(coroutine);
    }

    IEnumerator LoadScene(string newScene)
    {
        Debug.Log("GotoScene "+newScene);
        fadeOutCamera();
        yield return new WaitForSeconds(FadeOutDuration);
        SceneManager.LoadScene(newScene);
        fadeInCamera();
    }

    void Update()
    {
        if (0 != Input.GetAxis("GameControlTutorial"))
        {
	    GotoScene("Tutorial");
        }
        if (0 != Input.GetAxis("GameControlLevel1"))
        {
	    GotoScene("Level1");
        }
        if (0 != Input.GetAxis("GameControlLevel2"))
        {
	    GotoScene("Level2");
        }
        if (0 != Input.GetAxis("GameControlLevel3"))
        {
	    GotoScene("Level3");
        }
        if (0 != Input.GetAxis("GameControlClosing"))
        {
	    GotoScene("ClosingScene");
        }
        if (0 != Input.GetAxis("GameControlOpening"))
        {
	    GotoScene("OpeningScene");
        }
    }

    // Fades from black
    public void fadeInCamera()
    {
        Debug.Log("Fade from black");
        panelExist();

        panel.GetComponent<Graphic>().CrossFadeAlpha(0.0f, FadeInDuration, true);
    }

    // Fades to black
    public void fadeOutCamera()
    {
        Debug.Log("Fade to black");
        panelExist();

        panel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        panel.GetComponent<Image>().CrossFadeColor(Color.black, FadeOutDuration, true, true);
    }

    public void panelExist()
    {
        if(canvas == null)
        {
            canvas = GameObject.Find("Canvas"); // Canvas must exist in scene
        }

        if(panel == null)
        {
            Debug.Log("Panel instatiated");
            panel = (GameObject)Instantiate(Resources.Load("Prefabs/BlackPanel"));
            panel.transform.SetParent(canvas.transform);
        }

        float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        panel.transform.localScale = new Vector3(canvasHeight, canvasWidth, 1);
    }
/*

    IEnumerator WaitAndFadeCamera(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        print("WaitAndPrint " + Time.time);
    }

    public IEnumerator loadScene(string sceneName) {
        Debug.Log("Loading scene");
        fadeOutCamera();
        yield return new WaitForSeconds(FadeOutDuration);
        SceneManager.LoadScene(sceneName);
        Debug.Log("After wait");
        // TODO: destroy panel on sceneLoad
        // while (panel.GetComponent<Image>().color.a > 0) ;
        //Destroy(panel);
    }
*/
}
