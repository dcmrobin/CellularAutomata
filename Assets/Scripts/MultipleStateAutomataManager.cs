using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.IO;

public enum CellState { Black_0, White_1, Red_2, Green_3, Blue_4 } // Add more states as needed

[System.Serializable]
public struct CustomRule {
    public int[] NeighborStatesToTriggerRule;
    [Range(0, 8)]
    public int[] NeighborCountsToTriggerRule; // Array of neighbor counts that trigger this rule
    public CellState OriginalState;
    public CellState TargetState; // State to change to
}

public class MultipleStateAutomataManager : MonoBehaviour {
    int[,] cells;
    public List<CustomRule> customRules = new List<CustomRule>();

    [Header("UI")]
    public Slider densitySlider;
    public Slider delaySlider;
    public TMP_Dropdown cellToDrawDropdown;
    public Button saveImageButton;
    public TMP_InputField saveImageInputfield;

    [Header("Controls")]
    [Range(0, 1)]
    public float density;
    public int width = 50;
    public int height = 50;
    public bool paused;
    public float updateDelay = 3;
    float delay;
    Texture2D texture;
    GameObject plane;
    RaycastHit hit;
    Camera mainCamera;

    void Start() {
        if (GameObject.Find("Menu") != null && GameObject.Find("Menu").GetComponent<Loader>().sizeInputfield.text != "")
        {
            width = Convert.ToInt32(GameObject.Find("Menu").GetComponent<Loader>().sizeInputfield.text);
            height = Convert.ToInt32(GameObject.Find("Menu").GetComponent<Loader>().sizeInputfield.text);
        }
        else
        {
            width = 100;
            height = 100;
        }

        mainCamera = Camera.main;
        densitySlider.value = density;
        delaySlider.value = delay = updateDelay;
        cells = new int[width, height];
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(-90, 0, 0);
        plane.GetComponent<MeshRenderer>().material.mainTexture = texture;
        plane.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0);

        cellToDrawDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(CellState))));
        cellToDrawDropdown.value = 1;

        GenerateRandomCells();
    }

    void GenerateRandomCells() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int randInt = UnityEngine.Random.Range(1, 4);
                cells[x, y] = (UnityEngine.Random.value < densitySlider.value) ? randInt : 0;
            }
        }
        Render();
    }

    void Clear() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                cells[x, y] = 0;
            }
        }
        Render();
    }

    void Render() {
        Color[] colors = new Color[width * height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                switch ((CellState)cells[x, y]) {
                    case CellState.White_1:
                        colors[x + y * width] = Color.white;
                        break;
                    case CellState.Red_2:
                        colors[x + y * width] = Color.red;
                        break;
                    case CellState.Green_3:
                        colors[x + y * width] = Color.green;
                        break;
                    case CellState.Blue_4:
                        colors[x + y * width] = Color.blue;
                        break;
                    default:
                        colors[x + y * width] = Color.black;
                        break;
                }
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
    }

    void Update() {
        if (!paused) {
            delay -= .1f;
            if (delay <= 0) {
                UpdateCustom();
                delay = delaySlider.value;
            }
        }
        HandleControls();

        if (saveImageInputfield.text != "")
        {
            saveImageButton.interactable = true;
        }
        else
        {
            saveImageButton.interactable = false;
        }
    }

    void UpdateCustom() {
        int[,] newCells = new int[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                foreach (CustomRule rule in customRules) {
                    for (int i = 0; i < rule.NeighborCountsToTriggerRule.Length; i++) {
                        for (int j = 0; j < rule.NeighborStatesToTriggerRule.Length; j++) {
                            if (cells[x, y] == (int)rule.OriginalState && rule.NeighborCountsToTriggerRule[i] == GetSurroundingCellOfStateCount(x, y, rule.NeighborStatesToTriggerRule[j])) {
                                newCells[x, y] = (int)rule.TargetState;
                                break;
                            }
                        }
                    }
                }
            }
        }
        cells = newCells;
        Render();
    }

    int GetSurroundingCellOfStateCount(int gridX, int gridY, int cellState) {
        int aliveCellCount = 0;
        for (int offsetX = -1; offsetX <= 1; offsetX++) {
            for (int offsetY = -1; offsetY <= 1; offsetY++) {
                int neighbourX = (gridX + offsetX + width) % width;
                int neighbourY = (gridY + offsetY + height) % height;
                if (cells[neighbourX, neighbourY] == cellState) {
                    aliveCellCount += cells[neighbourX, neighbourY];
                }
            }
        }
        aliveCellCount -= cells[gridX, gridY];
        return aliveCellCount;
    }

    void HandleControls() {
        if (Input.GetKeyDown(KeyCode.Space))
            paused = !paused;

        if (Input.GetMouseButton(0))
            SetCell(cellToDrawDropdown.value);
        else if (Input.GetMouseButton(1))
            SetCell(0);
    }

    void SetCell(int cellValue) {
        if (Physics.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), mainCamera.transform.forward, out hit, Mathf.Infinity)) {
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= texture.width;
            pixelUV.y *= texture.height;
            cells[(int)pixelUV.x, (int)pixelUV.y] = cellValue;
            Render();
        }
    }

    public void SaveTextureAsImage()
    {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SavedImages/";
        if(!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + saveImageInputfield.text + ".png", bytes);
    }
}
