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
    public TMP_Dropdown predefinedShapeDropdown;
    public TMP_InputField saveloadInputField;
    public Button saveButton;
    public Button loadButton;

    [Header("Controls")]
    public int width = 50;
    public int height = 50;
    public bool paused;
    public float updateDelay = 3;
    float delay;
    Texture2D texture;
    GameObject plane;
    RaycastHit hit;

    private Vector2Int initialMousePosition;
    private bool drawingLine = false;

    public void Start() {
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
                        // Conductor cell with exactly one or two electron head neighbors becomes a electron head
                        newCells[x, y] = 2;
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

        if (Input.GetMouseButtonDown(0))
        {
            StartDrawingLine();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrawingLine();
        }

        if (Input.GetMouseButton(1))
        {
            SetCell(0);
        }

        if (Input.GetMouseButtonDown(2)) // Middle mouse button
        {
            int[,] selectedShape = GetSelectedPredefinedShape();
            PlacePredefinedShape(selectedShape);
        }

        if (drawingLine && Input.GetMouseButton(0))
        {
            DrawLineToCurrentMousePosition();
        }
    }

    public void PlacePredefinedShape(int[,] shape)
    {
        if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            Vector2Int cellPosition = GetCellPositionFromMousePosition();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);

            // Calculate the position to place the shape based on the mouse click
            int startX = Mathf.Clamp(cellPosition.x - shapeWidth / 2, 0, width - shapeWidth);
            int startY = Mathf.Clamp(cellPosition.y - shapeHeight / 2, 0, height - shapeHeight);

            // Place the shape onto the grid
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    int gridX = startX + x;
                    int gridY = startY + y;
                    if (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height)
                    {
                        cells[gridX, gridY] = shape[x, y];
                    }
                }
            }

            // Render the updated grid
            Render();
        }
    }

    private int[,] GetSelectedPredefinedShape()
    {
        int selectedShapeIndex = predefinedShapeDropdown.value;
        switch (selectedShapeIndex)
        {
            case 0: // OR gate
                return new int[,]
                {
                    {1, 1, 0, 0, 0},
                    {0, 0, 1, 0, 0},
                    {0, 1, 1, 1, 1},
                    {0, 0, 1, 0, 0},
                    {1, 1, 0, 0, 0}
                };
            case 1: // XOR gate
                return new int[,]
                {
                    {1, 1, 0, 0, 0, 0, 0},
                    {0, 0, 1, 0, 0, 0, 0},
                    {0, 1, 1, 1, 1, 0, 0},
                    {0, 1, 0, 0, 1, 1, 1},
                    {0, 1, 1, 1, 1, 0, 0},
                    {0, 0, 1, 0, 0, 0, 0},
                    {1, 1, 0, 0, 0, 0, 0}
                };
            case 2: // NOT gate
                return new int[,]
                {
                    {0, 0, 1, 1, 0, 0, 0},
                    {1, 1, 0, 0, 1, 0, 1},
                    {0, 0, 0, 2, 2, 2, 0},
                    {0, 0, 3, 0, 3, 3, 0},
                    {0, 0, 1, 2, 1, 0, 0}
                };
            case 3: // AND gate
                return new int[,]
                {
                    {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    {0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    {0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0},
                    {1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0},
                    {0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 1},
                    {1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0},
                    {0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0}
                };
            case 4: // Flip-flop
                return new int[,]
                {
                    {1, 1, 1, 1, 0, 0, 0, 0},
                    {0, 0, 0, 0, 1, 0, 0, 0},
                    {0, 0, 0, 1, 1, 1, 0, 0},
                    {0, 0, 0, 0, 1, 0, 0, 0},
                    {0, 0, 0, 1, 0, 1, 1, 1},
                    {0, 0, 0, 1, 0, 1, 0, 0},
                    {0, 0, 1, 1, 1, 0, 0, 0},
                    {0, 0, 0, 1, 0, 0, 0, 0},
                    {1, 1, 1, 0, 0, 0, 0, 0}
                };
            case 5: // Timer
                return new int[,]
                {
                    {0, 1, 1, 1, 1, 0, 0},
                    {1, 0, 0, 0, 0, 1, 1},
                    {0, 1, 1, 1, 1, 0, 0}
                };
            case 6: // Diode
                return new int[,]
                {
                    {0, 0, 1, 1, 0, 0},
                    {1, 1, 1, 0, 1, 1},
                    {0, 0, 1, 1, 0, 0}
                };
            // Add more cases for other predefined shapes if needed...
            default:
                return new int[0, 0]; // Default to an empty shape
        }
    }

    private void StartDrawingLine()
    {
        if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            initialMousePosition = GetCellPositionFromMousePosition();
            drawingLine = true;
        }
    }

    private void EndDrawingLine()
    {
        drawingLine = false;
    }

    private void DrawLineToCurrentMousePosition()
    {
        if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            Vector2Int currentMousePosition = GetCellPositionFromMousePosition();
            DrawLine(initialMousePosition, currentMousePosition);
            initialMousePosition = currentMousePosition;
        }
    }

    private Vector2Int GetCellPositionFromMousePosition()
    {
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= texture.width;
        pixelUV.y *= texture.height;
        return new Vector2Int((int)pixelUV.x, (int)pixelUV.y);
    }

    private void DrawLine(Vector2Int start, Vector2Int end)
    {
        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);
        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            cells[start.x, start.y] = cellDrawTypeDropdown.value + 1; // Set the cell value based on the dropdown selection
            Render();
            if (start.x == end.x && start.y == end.y)
                break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                start.x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                start.y += sy;
            }
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
