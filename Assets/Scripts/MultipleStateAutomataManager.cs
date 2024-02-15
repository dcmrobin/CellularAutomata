using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using TMPro;
using Unity.VisualScripting;

public enum CellState { Off, On, State2, State3, State4 } // Add more states as needed

[System.Serializable]
public struct CustomRule {
    [Range(0, 8)]
    public int[] NeighborCounts; // Array of neighbor counts that trigger this rule
    public CellState TargetState; // State to change to
}

public class MultipleStateAutomataManager : MonoBehaviour
{
    int[,] cells;
    public List<CustomRule> customRules = new List<CustomRule>();

    [Header("UI")]
    public Slider densitySlider;
    public Slider delaySlider;

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
                colors[x + y * width] = (cells[x, y] == 1) ? Color.white : (cells[x, y] == 2 ? Color.red : (cells[x, y] == 3 ? Color.green : (cells[x, y] == 4 ? Color.blue : Color.black)));
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
                UpdateCustom();
                delay = delaySlider.value;
            }
        }

        HandleControls();
    }

    public void UpdateCustom()
    {
        int[,] newCells = new int[width, height];

        // Iterate over all cells
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int aliveNeighbours = GetSurroundingAliveCellCount(x, y);
                bool ruleApplied = false;

                // Check custom rules
                foreach (CustomRule rule in customRules) {
                    for (int i = 0; i < rule.NeighborCounts.Length; i++)
                    {
                        if (rule.NeighborCounts[i] == aliveNeighbours) {
                            newCells[x, y] = (int)rule.TargetState;
                            ruleApplied = true;
                            break;
                        }
                    }
                }

                // Apply default rules if no custom rule was applied
                if (!ruleApplied) {
                    // Apply default rules
                    // (e.g., Conway's Game of Life rules)
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

        /*if (bornInputfield.text != "")
        {
            surviveInputfield.text = "0";
        }
        else if (surviveInputfield.text != "")
        {
            bornInputfield.text = "0";
        }*/
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
