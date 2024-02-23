using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using TMPro;
using Unity.VisualScripting;

public class Wireworld : MonoBehaviour
{
    int[,] cells;

    [Header("UI")]
    public Slider delaySlider;
    public TMP_Dropdown cellDrawTypeDropdown;

    [Header("Controls")]
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

        delaySlider.value = delay = updateDelay;
        cells = new int[width, height];
        texture = new(width, height);
        texture.filterMode = FilterMode.Point;

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(-90, 0, 0);
        plane.GetComponent<MeshRenderer>().material.mainTexture = texture;
        plane.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0);

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
                colors[x + y * width] = cells[x, y] == 1 ? Color.yellow : (cells[x, y] == 2 ? Color.cyan : (cells[x, y] == 3 ? Color.red : Color.black));
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
        int[,] newCells = new int[width, height];

        // Iterate over all cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get the number of electron head neighbors for the current cell
                int electronHeadNeighbors = GetSurroundingElectronHeadCount(x, y);

                // Apply Wireworld rules
                if (cells[x, y] == 1) // If the cell is conductor
                {
                    if (electronHeadNeighbors == 1 || electronHeadNeighbors == 2)
                    {
                        // Empty cell with exactly one or two electron head neighbors becomes a conductor
                        newCells[x, y] = 1;
                    }
                    else
                    {
                        // conductor cells remain conductor cells
                        newCells[x, y] = 1;
                    }
                }
                else if (cells[x, y] == 0) // If the cell is empty
                {
                    // empty remains empty
                    newCells[x, y] = 0;
                }
                else if (cells[x, y] == 2) // If the cell is an electron head
                {
                    // Electron heads become electron tails
                    newCells[x, y] = 3;
                }
                else if (cells[x, y] == 3) // If the cell is an electron tail
                {
                    // Electron tails become conductors
                    newCells[x, y] = 1;
                }
            }
        }

        // Update the cells array with the new state
        cells = newCells;

        // Render the updated grid
        Render();
    }

    int GetSurroundingElectronHeadCount(int gridX, int gridY)
    {
        int electronHeadCount = 0;
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                int neighbourX = (gridX + offsetX + width) % width;
                int neighbourY = (gridY + offsetY + height) % height;
                if (cells[neighbourX, neighbourY] == 2) // Electron head state
                {
                    electronHeadCount++;
                }
            }
        }
        // Exclude the central cell from the count
        return electronHeadCount - (cells[gridX, gridY] == 2 ? 1 : 0);
    }

    public void HandleControls()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            paused = !paused;

        if (Input.GetMouseButton(0))
        {
            SetCell(cellDrawTypeDropdown.value + 1);
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
