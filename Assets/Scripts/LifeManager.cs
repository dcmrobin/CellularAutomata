using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using TMPro;
using Unity.VisualScripting;

public class Manager : MonoBehaviour
{
    int[,] cells;

    [Header("UI")]
    public Slider densitySlider;
    public Slider delaySlider;
    public Toggle daynightToggle;
    public Toggle mazeToggle;
    public Toggle highlifeToggle;
    public Toggle chaosToggle;

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
                if (!chaosToggle.isOn && !highlifeToggle.isOn && !mazeToggle.isOn && !daynightToggle.isOn)
                {
                    UpdateCells();
                }
                else if (chaosToggle.isOn)
                {
                    ChaosCells();
                }
                else if (highlifeToggle.isOn)
                {
                    HighlifeCells();
                }
                else if (mazeToggle.isOn)
                {
                    MazeCells();
                }
                else if (daynightToggle.isOn)
                {
                    DaynightCells();
                }
                delay = delaySlider.value;
            }
        }

        HandleControls();
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

    public void ChaosCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int aliveNeighbours = GetSurroundingAliveCellCount(x, y);
 
                if (cells[x, y] == 1)
                {
                    if (aliveNeighbours < 2)
                    {
                        cells[x, y] = 0;
                    }
                    else if (aliveNeighbours == 2 || aliveNeighbours == 3)
                    {
                        cells[x, y] = 1;
                    }
                    else if (aliveNeighbours > 3)
                    {
                        cells[x, y] = 0;
                    }
                }
                else if (cells[x, y] == 0)
                {
                    if (aliveNeighbours == 3)
                    {
                        cells[x, y] = 1;
                    }
                }
            }
        }
 
        Render();
    }

    public void HighlifeCells()
    {
        int[,] newCells = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int aliveNeighbours = GetSurroundingAliveCellCount(x, y);

                if (cells[x, y] == 1) // If the cell is alive
                {
                    if (aliveNeighbours < 2 || aliveNeighbours > 3)
                    {
                        newCells[x, y] = 0;
                    }
                    else
                    {
                        newCells[x, y] = 1;
                    }
                }
                else
                {
                    if (aliveNeighbours == 3 || aliveNeighbours == 6)
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

    public void MazeCells()
    {
        int[,] newCells = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int aliveNeighbours = GetSurroundingAliveCellCount(x, y);

                if (cells[x, y] == 1) // If the cell is alive
                {
                    if (aliveNeighbours < 1 || aliveNeighbours > 5)
                    {
                        newCells[x, y] = 0;
                    }
                    else
                    {
                        newCells[x, y] = 1;
                    }
                }
                else
                {
                    if (aliveNeighbours == 3)
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

    public void DaynightCells()
    {
        int[,] newCells = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int aliveNeighbours = GetSurroundingAliveCellCount(x, y);

                if (cells[x, y] == 1) // If the cell is alive
                {
                    if (aliveNeighbours == 3 || aliveNeighbours == 4 || aliveNeighbours == 6 || aliveNeighbours == 7 || aliveNeighbours == 8)
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
                    if (aliveNeighbours == 3 || aliveNeighbours == 6 || aliveNeighbours == 7 || aliveNeighbours == 8)
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
