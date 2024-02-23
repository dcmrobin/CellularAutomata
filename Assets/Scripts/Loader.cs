using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Loader : MonoBehaviour
{
    public int menuSceneIndex = 0;
    public int lifeSceneIndex = 1;
    public int customAutomataSceneIndex = 2;
    public int multicolorAutomataSceneIndex = 3;
    public int langtonAntSceneIndex = 4;
    public int wireworldSceneIndex = 5;
    public TMP_InputField sizeInputfield;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMenu()
    {
        Destroy(GameObject.Find("Menu"));
        Destroy(gameObject);
        SceneManager.LoadScene(menuSceneIndex);
    }

    public void LoadLife()
    {
        transform.Find("Panel").gameObject.SetActive(false);
        SceneManager.LoadScene(lifeSceneIndex);
    }

    public void LoadCreate()
    {
        transform.Find("Panel").gameObject.SetActive(false);
        SceneManager.LoadScene(customAutomataSceneIndex);
    }

    public void LoadMulticolor()
    {
        transform.Find("Panel").gameObject.SetActive(false);
        SceneManager.LoadScene(multicolorAutomataSceneIndex);
    }

    public void LoadLangtonAnt()
    {
        transform.Find("Panel").gameObject.SetActive(false);
        SceneManager.LoadScene(langtonAntSceneIndex);
    }

    public void LoadWireworld()
    {
        transform.Find("Panel").gameObject.SetActive(false);
        SceneManager.LoadScene(wireworldSceneIndex);
    }
}
