using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using TMPro;
using Unity.VisualScripting;

public class Huegene : MonoBehaviour
{
    [HideInInspector] public int[,] cells;

    [Header("UI")]
    public Slider densitySlider;
    public Slider delaySlider;
    public Toggle mosaicToggle;

    [Header("Controls")]
    [Range(0, 1)]
    public float density;
    public int width = 50;
    public int height = 50;
    public bool paused;
    public float updateDelay = 3;
    float delay;
    [HideInInspector] public Texture2D texture;
    GameObject plane;
    [HideInInspector] public RaycastHit hit;

    public void Start() {
        if (GameObject.Find("Menu").GetComponent<Loader>().sizeInputfield.text != "")
        {
            width = Convert.ToInt32(GameObject.Find("Menu").GetComponent<Loader>().sizeInputfield.text);
            height = Convert.ToInt32(GameObject.Find("Menu").GetComponent<Loader>().sizeInputfield.text);
        }
        else
        {
            width = 100;
            height = 100;
        }

        densitySlider.value = density;
        delaySlider.value = delay = updateDelay;
        cells = new int[width, height];
        texture = new(width, height);
        texture.filterMode = FilterMode.Point;

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(-90, 0, 0);
        plane.GetComponent<MeshRenderer>().material.mainTexture = texture;
        plane.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0);

        GenerateRandomCells();
    }

    public void GenerateRandomCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = (UnityEngine.Random.value < densitySlider.value)?1:0;
                texture.SetPixel(x, y, UnityEngine.Random.ColorHSV());
            }
        }
        //Render();
    }

    public void Clear()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = 0;
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
                colors[x + y * width] = (cells[x, y] == 1) ? ((texture.GetPixel(x, y) == Color.black) ? Color.white : texture.GetPixel(x, y)) : Color.black;
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
                UpdateCells();
                delay = delaySlider.value;
            }
        }

        HandleControls();
    }

    public void UpdateCells()
    {
        // Create a copy of the current cells to preserve the past generation
        int[,] newCells = (int[,])cells.Clone();

        // Iterate through each cell in the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // If the cell is alive
                if (cells[x, y] == 1)
                {
                    // Find a random dead neighbor
                    List<Vector2Int> deadNeighbors = GetDeadNeighbors(x, y);
                    if (deadNeighbors.Count > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, deadNeighbors.Count);
                        Vector2Int randomNeighbor = deadNeighbors[randomIndex];
                        // Change the dead neighbor to alive in the new cells array
                        newCells[randomNeighbor.x, randomNeighbor.y] = 1;
                        
                        // Mutate the hue slightly of the new cell
                        if (!mosaicToggle.isOn)
                        {
                            Color currentColor = texture.GetPixel(x, y);
                            float hue = currentColor.r;
                            hue = (hue + UnityEngine.Random.Range(-0.1f, 0.1f)) % 1.0f;
                            Color newColor = Color.HSVToRGB(hue, currentColor.g, currentColor.b);
                            texture.SetPixel(randomNeighbor.x, randomNeighbor.y, newColor);
                        }
                        else
                        {
                            texture.SetPixel(randomNeighbor.x, randomNeighbor.y, texture.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        // Update the cells array with the new cells
        cells = newCells;

        // Render the updated grid
        Render();
    }

    // Helper method to get the dead neighbors of a cell
    public List<Vector2Int> GetDeadNeighbors(int x, int y)
    {
        List<Vector2Int> deadNeighbors = new List<Vector2Int>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int neighborX = x + i;
                int neighborY = y + j;

                // Ensure the neighbor is within the grid boundaries and not the center cell
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height &&
                    !(i == 0 && j == 0))
                {
                    if (cells[neighborX, neighborY] == 0)
                    {
                        deadNeighbors.Add(new Vector2Int(neighborX, neighborY));
                    }
                }
            }
        }

        return deadNeighbors;
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
            texture.SetPixel((int)pixelUV.x, (int)pixelUV.y, UnityEngine.Random.ColorHSV());
            Render();
        }
    }
}
