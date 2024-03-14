using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using TMPro;
using Unity.VisualScripting;

public class ElementryAutomata : MonoBehaviour
{
    [HideInInspector] public int[,] cells;

    [Header("UI")]
    public Slider delaySlider;

    [Header("Controls")]
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

        delaySlider.value = delay = updateDelay;
        cells = new int[width, height];
        texture = new(width, height);
        texture.filterMode = FilterMode.Point;

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(-90, 0, 0);
        plane.GetComponent<MeshRenderer>().material.mainTexture = texture;
        plane.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0);

        cells[width/2, 0] = 1;
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
                UpdateCells();
                delay = delaySlider.value;
            }
        }

        HandleControls();
    }

    public void UpdateCells()
    {
        // Temporary array to hold the updated cell values
        int[,] newCells = new int[width, height];

        // Apply Rule 90 to each cell
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int left = (x == 0) ? 0 : cells[x - 1, y]; // Value of left neighbor
                int right = (x == width - 1) ? 0 : cells[x + 1, y]; // Value of right neighbor

                // Apply Rule 90
                if ((left == 1 && right == 0) || (left == 0 && right == 1))
                {
                    newCells[x, (y + 1) % height] = 1; // Move the cell down one y-value
                }
                else
                {
                    newCells[x, (y + 1) % height] = 0; // Move the cell down one y-value
                }
            }
        }

        // Update the main cells array with the new cell values
        cells = newCells;

        // Render the updated grid
        Render();
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
