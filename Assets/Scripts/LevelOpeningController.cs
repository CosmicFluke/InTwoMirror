using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelOpeningController : MonoBehaviour
{

    static LevelOpeningController Instance;
    public float FadeInDuration;
    public float FadeOutDuration;
    GameObject canvas;
    GameObject panel;
    bool fadeComplete;

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
            SceneManager.LoadScene("Level1");
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

    public IEnumerator fadeInCamera()
    {
        canvas = GameObject.Find("Canvas");
        panel = (GameObject)Instantiate(Resources.Load("Prefabs/BlackPanel"));
        panel.transform.SetParent(canvas.transform);


        float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        panel.transform.localScale = new Vector3(canvasHeight, canvasWidth, 1);

        panel.GetComponent<Graphic>().CrossFadeAlpha(0f, FadeInDuration, false);
        yield return new WaitForSeconds(FadeInDuration);
    }

    public IEnumerator fadeOutCamera()
    {
        Color color = panel.GetComponent<Image>().color;
        color.a = 1f;
        panel.GetComponent<Image>().color = color;
        panel.GetComponent<Image>().CrossFadeColor(Color.black, FadeOutDuration, false, true);

        yield return new WaitForSeconds(FadeOutDuration);
    }

    public void loadScene(string sceneName) {
        fadeOutCamera();
        SceneManager.LoadScene(sceneName);
        fadeInCamera();
        // TODO: destroy panel on sceneLoad
        // while (panel.GetComponent<Image>().color.a > 0) ;
        //Destroy(panel);
    }
}
