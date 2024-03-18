using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Linq;

public class MNCA : MonoBehaviour
{
    [HideInInspector] public int[,] cells;

    [Header("UI")]
    public Slider densitySlider;
    public Slider delaySlider;

    [Header("Controls")]
    [Range(0, 0.3f)]
    public float density;
    public int width = 50;
    public int height = 50;
    public bool paused;
    public float updateDelay = 3;
    float delay;
    [HideInInspector] public Texture2D texture;
    GameObject plane;
    [HideInInspector] public RaycastHit hit;

    // Define neighborhoods
    public enum NeighborhoodType { Moore, VonNeumann, Knight, Hexagonal }
    public NeighborhoodType neighborhoodType = NeighborhoodType.Moore;

    // Born (birth) and Survive conditions
    [Header("Rules")]
    public int[] bornConditions = { 3 }; // Number of live neighbors required for a dead cell to become alive
    public int[] surviveConditions = { 2, 3 }; // Number of live neighbors required for a live cell to survive

    public void Start()
    {
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

        densitySlider.value = density;
        delaySlider.value = delay = updateDelay;
        cells = new int[width, height];
        texture = new Texture2D(width, height);
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
                cells[x, y] = (UnityEngine.Random.value < densitySlider.value) ? 1 : 0;
                texture.SetPixel(x, y, UnityEngine.Random.ColorHSV());
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

        // Iterate through each cell
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get the state of the current cell
                int currentState = cells[x, y];

                // Get the states of neighboring cells based on the selected neighborhood type
                int[] neighborStates = GetNeighborStates(x, y);

                // Apply MNCA rules based on the states of neighboring cells
                int newState = ApplyMNCARules(currentState, neighborStates);

                // Update the new state of the cell
                newCells[x, y] = newState;
            }
        }

        // Update the cells with the new states
        cells = newCells;

        // Render the updated grid
        Render();
    }

    // Function to get the states of neighboring cells based on the selected neighborhood type
    private int[] GetNeighborStates(int x, int y)
    {
        List<int> neighborStates = new List<int>();

        // Moore neighborhood
        if (neighborhoodType == NeighborhoodType.Moore)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    // Skip if out of bounds or the same cell
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height || (dx == 0 && dy == 0))
                        continue;

                    neighborStates.Add(cells[nx, ny]);
                }
            }
        }
        // Von Neumann neighborhood
        else if (neighborhoodType == NeighborhoodType.VonNeumann)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Skip corners for Von Neumann neighborhood
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) > 1)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    // Skip if out of bounds or the same cell
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height || (dx == 0 && dy == 0))
                        continue;

                    neighborStates.Add(cells[nx, ny]);
                }
            }
        }
        // Hexagonal neighborhood
        else if (neighborhoodType == NeighborhoodType.Knight)
        {
            int[,] knightOffsets = {
                { 1, 2 }, { 2, 1 }, { 2, -1 }, { 1, -2 },
                { -1, -2 }, { -2, -1 }, { -2, 1 }, { -1, 2 }
            };

            for (int i = 0; i < knightOffsets.GetLength(0); i++)
            {
                int nx = x + knightOffsets[i, 0];
                int ny = y + knightOffsets[i, 1];

                // Skip if out of bounds
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                neighborStates.Add(cells[nx, ny]);
            }
        }
        // Hexagonal neighborhood
        else if (neighborhoodType == NeighborhoodType.Hexagonal)
        {
            int[,] hexOffsets = {
                { 1, 0 }, { 1, -1 }, { 0, -1 },
                { -1, -1 }, { -1, 0 }, { 0, 1 }
            };

            for (int i = 0; i < hexOffsets.GetLength(0); i++)
            {
                int nx = x + hexOffsets[i, 0];
                int ny = y + hexOffsets[i, 1];

                // Skip if out of bounds
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                neighborStates.Add(cells[nx, ny]);
            }
        }

        return neighborStates.ToArray();
    }

    // Function to apply MNCA rules based on the states of neighboring cells
    private int ApplyMNCARules(int currentState, int[] neighborStates)
    {
        int aliveCount = 0;
        foreach (int state in neighborStates)
        {
            if (state == 1)
                aliveCount++;
        }

        // Apply rules based on the neighborhood type
        switch (neighborhoodType)
        {
            case NeighborhoodType.Moore:
                if (currentState == 1 && (aliveCount < 2 || aliveCount > 3))
                    return 0; // Any live cell with fewer than two live neighbors dies, as if by underpopulation.
                else if (currentState == 0 && aliveCount == 3)
                    return 1; // Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction.
                else
                    return currentState; // Otherwise, the cell stays in its current state.
            case NeighborhoodType.VonNeumann:
                if (currentState == 1 && (aliveCount < 2 || aliveCount > 3))
                    return 0; // Any live cell with fewer than two live neighbors dies, as if by underpopulation.
                else if (currentState == 0 && aliveCount == 3)
                    return 1; // Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction.
                else
                    return currentState; // Otherwise, the cell stays in its current state.
            case NeighborhoodType.Knight:
                if (currentState == 1 && (aliveCount < 2 || aliveCount > 3))
                    return 0; // Any live cell with fewer than two live neighbors dies, as if by underpopulation.
                else if (currentState == 0 && aliveCount == 3)
                    return 1; // Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction.
                else
                    return currentState; // Otherwise, the cell stays in its current state.
            case NeighborhoodType.Hexagonal:
                if (currentState == 1 && (aliveCount < 2 || aliveCount > 3))
                    return 0; // Any live cell with fewer than two live neighbors dies, as if by underpopulation.
                else if (currentState == 0 && aliveCount == 3)
                    return 1; // Any dead cell with exactly three live neighbors becomes a live cell, as if by reproduction.
                else
                    return currentState; // Otherwise, the cell stays in its current state.
            default:
                return currentState; // Default: Return current state if no rules are applied
        }
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