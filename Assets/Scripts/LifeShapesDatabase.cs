using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LifeShapesDatabase : MonoBehaviour
{
    public struct PredefinedShape
    {
        public string name;
        public int[,] shape;
    }

    public List<PredefinedShape> predefinedShapes = new List<PredefinedShape>();
    public TMP_Dropdown predefinedShapesDropdown;


    private void Start() {
        AddShapes();
    }

    public void CreateShape(string newShapeName, int[,] Nshape)
    {
        PredefinedShape newShape = new PredefinedShape{
            name = newShapeName,
            shape = Nshape
        };

        predefinedShapes.Add(newShape);

        // Update the dropdown options
        List<string> dropdownOptions = new List<string>();
        foreach (var shape in predefinedShapes)
        {
            dropdownOptions.Add(shape.name);
        }
        predefinedShapesDropdown.ClearOptions();
        predefinedShapesDropdown.AddOptions(dropdownOptions);
    }

    public void PlacePredefinedShape(PredefinedShape shapeToPlace)
    {
        if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, out GetComponent<LifeManager>().hit, Mathf.Infinity))
        {
            Vector2 pixelUV = GetComponent<LifeManager>().hit.textureCoord;
            pixelUV.x *= GetComponent<LifeManager>().texture.width;
            pixelUV.y *= GetComponent<LifeManager>().texture.height;

            // Calculate the starting position to place the shape
            int startX = (int)pixelUV.x - shapeToPlace.shape.GetLength(0) / 2;
            int startY = (int)pixelUV.y - shapeToPlace.shape.GetLength(1) / 2;

            // Ensure the shape does not exceed the boundaries of the board
            startX = Mathf.Clamp(startX, 0, GetComponent<LifeManager>().width - shapeToPlace.shape.GetLength(0));
            startY = Mathf.Clamp(startY, 0, GetComponent<LifeManager>().height - shapeToPlace.shape.GetLength(1));

            // Place the predefined shape onto the board
            for (int x = 0; x < shapeToPlace.shape.GetLength(0); x++)
            {
                for (int y = 0; y < shapeToPlace.shape.GetLength(1); y++)
                {
                    int cellX = startX + x;
                    int cellY = startY + y;
                    if (shapeToPlace.shape[x, y] == 1 && cellX >= 0 && cellX < GetComponent<LifeManager>().width && cellY >= 0 && cellY < GetComponent<LifeManager>().height)
                    {
                        GetComponent<LifeManager>().cells[cellX, cellY] = 1;
                    }
                }
            }

            GetComponent<LifeManager>().Render();
        }
    }

    public void AddShapes()
    {
        CreateShape("Glider", new int[,] {
            {0, 1, 1},
            {1, 0, 1},
            {0, 0, 1}
        });

        CreateShape("Blinker", new int[,] {
            {1, 1, 1}
        });

        CreateShape("Toad", new int[,] {
            {0, 1, 1, 1},
            {1, 1, 1, 0}
        });

        CreateShape("Beacon", new int[,] {
            {1, 1, 0, 0},
            {1, 1, 0, 0},
            {0, 0, 1, 1},
            {0, 0, 1, 1}
        });

        CreateShape("Pulsar", new int[,] {
            {0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0},
            {1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0}
        });

        CreateShape("Spaceship", new int[,] {
            {0, 1, 1, 1, 1},
            {1, 0, 0, 0, 1},
            {0, 0, 0, 0, 1},
            {1, 0, 0, 1, 0}
        });

        CreateShape("Block", new int[,] {
            {1, 1},
            {1, 1}
        });

        CreateShape("Beehive", new int[,] {
            {0, 1, 1, 0},
            {1, 0, 0, 1},
            {0, 1, 1, 0}
        });

        CreateShape("Loaf", new int[,] {
            {0, 1, 1, 0},
            {1, 0, 0, 1},
            {0, 1, 0, 1},
            {0, 0, 1, 0}
        });

        CreateShape("Boat", new int[,] {
            {1, 1, 0},
            {1, 0, 1},
            {0, 1, 0}
        });

        CreateShape("Tub", new int[,] {
            {0, 1, 0},
            {1, 0, 1},
            {0, 1, 0}
        });

        CreateShape("Pentadecathlon", new int[,] {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1},
            {1, 1, 1},
            {1, 1, 1},
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        });

        CreateShape("Acorn", new int[,] {
            {0, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 1, 0, 0, 0},
            {1, 1, 0, 0, 1, 1, 1}
        });

        CreateShape("R-pentomino", new int[,] {
            {0, 1, 1},
            {1, 1, 0},
            {0, 1, 0}
        });

        CreateShape("Diehard", new int[,] {
            {0, 0, 0, 0, 0, 0, 1, 0},
            {1, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 0, 0, 0, 1, 1, 1}
        });

        CreateShape("Glider Gun", new int[,] {
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1},
            {0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1},
            {1,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {1,1,0,0,0,0,0,0,0,0,1,0,0,0,1,0,1,1,0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
        });

        CreateShape("Queen Bee Shuttle", new int[,] {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1},
            {1, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1},
            {1, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        });

        CreateShape("Block-Laying Switch Engine", new int[,] {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0},
            {1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
            {1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1},
        });
    }
}
