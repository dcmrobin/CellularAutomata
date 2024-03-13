using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Loader : MonoBehaviour
{
    public TMP_InputField sizeInputfield;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex != 0)
        {
            transform.Find("Panel").gameObject.SetActive(false);
        }
        else
        {
            Destroy(GameObject.Find("Menu"));
            Destroy(gameObject);
        }
        SceneManager.LoadScene(sceneIndex);
    }
}
