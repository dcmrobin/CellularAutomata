using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    int[,] cells;

    [Range(0, 1)]
    public float frequency;
    public int width = 50;
    public int height = 50;
    public float updateDelay = 3;
    float delay;
    Texture2D texture;
    GameObject plane;

    public void Start() {
        delay = updateDelay;
        cells = new int[width, height];
        texture = new(width, height);

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
                cells[x, y] = (Random.value < frequency)?1:0;
            }
        }
    }

    public void Render()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, (cells[x, y] == 1)?Color.white:Color.black);
                texture.Apply();
            }
        }
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
        delay -= .1f;
        if (delay <= 0)
        {
            UpdateCells();
            delay = updateDelay;
        }
    }

    public void UpdateCells()
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
}
