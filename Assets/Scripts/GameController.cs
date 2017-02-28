using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    static GameController Instance;
    public float FadeInDuration;
    public float FadeOutDuration;
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            SceneManager.LoadScene("Tutorial");
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            coroutine = loadScene("Level1");
            StartCoroutine(coroutine);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("Level2");
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            SceneManager.LoadScene("Level3");
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            SceneManager.LoadScene("ClosingScene");
        }
        if (Input.GetKeyUp(KeyCode.Alpha9))
        {
            SceneManager.LoadScene("OpeningScene");
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
        panel = (GameObject)Instantiate(Resources.Load("Prefabs/BlackPanel"));
        panel.transform.SetParent(canvas.transform);
        Debug.Log("After wait");
        fadeInCamera();
        // TODO: destroy panel on sceneLoad
        // while (panel.GetComponent<Image>().color.a > 0) ;
        //Destroy(panel);
    }
}
