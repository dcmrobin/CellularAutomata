using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using TMPro;
using Unity.VisualScripting;

public class CustomAutomataManager : MonoBehaviour
{
    int[,] cells;

    [Header("UI")]
    public Slider densitySlider;
    public Slider delaySlider;
    public TMP_InputField bornInputfield;
    public TMP_InputField surviveInputfield;

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

    public void Start() {
        densitySlider.value = density;
        delaySlider.value = delay = updateDelay;
        cells = new int[width, height];
        texture = new(width, height);
        texture.filterMode = FilterMode.Point;

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(-90, 0, 0);
        plane.GetComponent<MeshRenderer>().material.mainTexture = texture;

        GenerateRandomCells();
    }

    public void GenerateRandomCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = (UnityEngine.Random.value < densitySlider.value)?1:0;
            }
        }
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

    public void Update() {
        if (!paused)
        {
            delay -= .1f;
            if (delay <= 0)
            {
                if (bornInputfield.text == "" && surviveInputfield.text == "")
                {
                    UpdateCells();
                    delay = delaySlider.value;
                }
                else
                {
                    UpdateCustom();
                    delay = delaySlider.value;
                }
            }
        }

        HandleControls();
    }

    public void UpdateCustom()
    {
        int[,] newCells = new int[width, height];

        char[] bornCharArray = bornInputfield.text.ToCharArray();
        char[] surviveCharArray = surviveInputfield.text.ToCharArray();

        int[] bornIntArray = new int[bornCharArray.Length];
        int[] surviveIntArray = new int[surviveCharArray.Length];

        for (int b = 0; b < bornCharArray.Length; b++)
        {
            for (int s = 0; s < surviveCharArray.Length; s++)
            {
                if (int.TryParse(bornCharArray[b].ToString(), out int B_result) && int.TryParse(surviveCharArray[s].ToString(), out int S_result))
                {
                    bornIntArray[b] = B_result;
                    surviveIntArray[s] = S_result;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (newCells[x, y] == 1)
                {
                    if (CheckArray(surviveIntArray, x, y))
                    {
                        newCells[x, y] = 1;
                    }
                    else
                    {
                        newCells[x, y] = 0;
                    }
                }
                else
                {
                    if (CheckArray(bornIntArray, x, y))
                    {
                        newCells[x, y] = 1;
                    }
                    else
                    {
                        newCells[x, y] = 0;
                    }
                }
            }
        }

        // Update the cells array with the new state
        cells = newCells;

        // Render the updated grid
        Render();
    }

    bool CheckArray(int[] charArray, int x, int y)
    {
        bool res = false;
        for (int i = 0; i < charArray.Length; i++)
        {
            if (GetSurroundingAliveCellCount(x, y) == charArray[i])
            {
                res = true;
            }
        }
        return res;
    }

    public void UpdateCells()
    {
        int[,] newCells = new int[width, height];

        // Iterate over all cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get the number of alive neighbors for the current cell
                int aliveNeighbours = GetSurroundingAliveCellCount(x, y);

                // Apply Conway's rules
                if (cells[x, y] == 1) // If the cell is alive
                {
                    if (aliveNeighbours < 2 || aliveNeighbours > 3)
                    {
                        // Any live cell with fewer than two live neighbors dies, or with more than three live neighbors dies
                        newCells[x, y] = 0;
                    }
                    else
                    {
                        // Any live cell with two or three live neighbors lives on to the next generation
                        newCells[x, y] = 1;
                    }
                }
                else // If the cell is dead
                {
                    if (aliveNeighbours == 3)
                    {
                        // Any dead cell with exactly three live neighbors becomes a live cell
                        newCells[x, y] = 1;
                    }
                    else
                    {
                        // Dead cells remain dead
                        newCells[x, y] = 0;
                    }
                }
            }
        }

        // Update the cells array with the new state
        cells = newCells;

        // Render the updated grid
        Render();
    }

    int GetSurroundingAliveCellCount(int gridX, int gridY)
    {
        int aliveCellCount = 0;
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                int neighbourX = (gridX + offsetX + width) % width;
                int neighbourY = (gridY + offsetY + height) % height;
                aliveCellCount += cells[neighbourX, neighbourY];
            }
        }
        // Subtract the central cell's value because it was added in the loop
        aliveCellCount -= cells[gridX, gridY];
        return aliveCellCount;
    }

    public void HandleControls()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            paused = !paused;

        if (Input.GetMouseButton(0))
        {
            SetCell(1);
        }
        else if (Input.GetMouseButton(1))
        {
            SetCell(0);
        }
    }

    public void SetCell(int cellValue)
    {
        if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= texture.width;
            pixelUV.y *= texture.height;
            cells[(int)pixelUV.x, (int)pixelUV.y] = cellValue;
            Render();
        }
    }
}
