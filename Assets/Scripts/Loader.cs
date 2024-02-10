using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public int menuSceneIndex = 0;
    public int lifeSceneIndex = 1;

    public void LoadMenu()
    {
        SceneManager.LoadScene(menuSceneIndex);
    }

    public void LoadLife()
    {
        SceneManager.LoadScene(lifeSceneIndex);
    }
}
