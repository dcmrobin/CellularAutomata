using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class Manager : MonoBehaviour
{
    int[,] cells;

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
        delay = updateDelay;
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
                cells[x, y] = (UnityEngine.Random.value < density)?1:0;
            }
        }
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

    //OLD RENDERING CODE
    /*public void OnDrawGizmos() {
        if (cells != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (cells[x, y] == 1)?Color.white:Color.black;
                    Vector2 pos = new(-width/2 + x + .5f, -height/2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector2.one);
                }
            }
        }
    }*/

    public void Update() {
        if (!paused)
        {
            delay -= .1f;
            if (delay <= 0)
            {
                UpdateCells();
                delay = updateDelay;
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

    //CRAZY LIFE
    /*public void UpdateCells()
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
    }*/

    int GetSurroundingAliveCellCount(int gridX, int gridY)
    {
        int aliveCellCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        aliveCellCount += cells[neighbourX, neighbourY];
                    }
                }
            }
        }
        return aliveCellCount;
    }

    public void HandleControls()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!paused)
            {
                paused = true;
            }
            else
            {
                paused = false;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out hit, Mathf.Infinity))
            {
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= texture.width;
                pixelUV.y *= texture.height;
                cells[(int)pixelUV.x, (int)pixelUV.y] = 1;
                Render();
            }
        }
        else if (Input.GetMouseButton(1))
        {
            if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out hit, Mathf.Infinity))
            {
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= texture.width;
                pixelUV.y *= texture.height;
                cells[(int)pixelUV.x, (int)pixelUV.y] = 0;
                Render();
            }
        }
    }
}
