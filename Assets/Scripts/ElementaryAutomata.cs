using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ElementaryAutomata : MonoBehaviour
{
    [HideInInspector] public int[,] cells;
    
    [Header("UI")]
    public Slider delaySlider;
    public TMP_InputField ruleInputField;

    [Header("Controls")]
    public int ruleNumber = 90;
    public int width = 50;
    public int height = 50;
    public bool paused;
    public float updateDelay = 3f;
    float delay;
    [HideInInspector] public Texture2D texture;
    GameObject plane;
    [HideInInspector] public RaycastHit hit;

    private int[] ruleBinary = new int[8];

    public void Start()
    {
        width = Int32.TryParse(GameObject.Find("Menu").GetComponent<Loader>().sizeInputfield.text, out int parsedWidth) ? parsedWidth : 100;
        height = width;

        delaySlider.value = delay = updateDelay;
        cells = new int[width, height];
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(-90, 0, 0);
        var meshRenderer = plane.GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = texture;
        meshRenderer.material.SetFloat("_Glossiness", 0f);

        cells[width / 2, 0] = 1;
        ComputeRuleBinary();
    }

    private void ComputeRuleBinary()
    {
        string binaryString = Convert.ToString(ruleNumber, 2).PadLeft(8, '0');
        for (int i = 0; i < 8; i++)
        {
            ruleBinary[i] = binaryString[i] - '0';
        }
    }

    public void Clear()
    {
        Array.Clear(cells, 0, cells.Length);
        Render();
    }

    public void Render()
    {
        Color[] colors = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                colors[x + y * width] = (cells[x, y] == 1) ? Color.white : Color.black;
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
    }

    public void Update()
    {
        if (!paused)
        {
            delay -= Time.deltaTime;
            if (delay <= 0)
            {
                UpdateCells();
                delay = delaySlider.value;
            }
        }

        HandleControls();
    }

    public void UpdateCells()
{
    int[,] newCells = new int[width, height];
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            int left = (x == 0) ? cells[width - 1, y] : cells[x - 1, y];
            int right = (x == width - 1) ? cells[0, y] : cells[x + 1, y];
            int newState = ApplyRule(left, cells[x, y], right);
            int newY = (y + 1) % height;
            newCells[x, newY] = newState;

            // Preserve existing cell state if it's not being updated
            if (newState == 0 && cells[x, y] == 1)
                newCells[x, y] = 1;
        }
    }
    cells = newCells; // Update entire grid with the new generation
    Render();
}

    private int ApplyRule(int left, int center, int right)
    {
        string pattern = $"{left}{center}{right}";
        int index = 7 - Convert.ToInt32(pattern, 2);
        return ruleBinary[index];
    }

    public void HandleControls()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            paused = !paused;

        if (Mouse.current.leftButton.isPressed)
        {
            SetCell(1);
        }
        else if (Mouse.current.rightButton.isPressed)
        {
            SetCell(0);
        }
    }

    public void UpdateRule()
    {
        ruleNumber = Convert.ToInt32(ruleInputField.text);
        ComputeRuleBinary();
    }

    public void SetCell(int cellValue)
    {
        if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= texture.width;
            pixelUV.y *= texture.height;
            cells[(int)pixelUV.x, (int)pixelUV.y] = cellValue;
            Render();
        }
    }
}
