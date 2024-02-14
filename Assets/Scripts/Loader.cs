using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public int menuSceneIndex = 0;
    public int lifeSceneIndex = 1;
    public int customAutomataSceneIndex = 2;
    public int multicolorAutomataSceneIndex = 3;

    public void LoadMenu()
    {
        SceneManager.LoadScene(menuSceneIndex);
    }

    public void LoadLife()
    {
        SceneManager.LoadScene(lifeSceneIndex);
    }

    public void LoadCreate()
    {
        SceneManager.LoadScene(customAutomataSceneIndex);
    }

    public void LoadMulticolor()
    {
        SceneManager.LoadScene(multicolorAutomataSceneIndex);
    }
}
