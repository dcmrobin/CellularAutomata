using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class LangtonAnt : MonoBehaviour
{
    struct Ant
    {
        public int x;
        public int y;
        public int direction;
    }

    int[,] cells;
    List<Ant> ants = new List<Ant>(); // List to hold multiple ants

    [Header("UI")]
    public Slider delaySlider;
    public Toggle loopsToggle;
    public Toggle lifeToggle;
    public GameObject antsListScrollview;
    public GameObject UIAntPrefab;
    public GameObject UIAntPanel;

    [Header("Controls")]
    public int width = 100;
    public int height = 100;
    public bool paused;
    public float updateDelay = 0.5f;
    float delay;
    Texture2D texture;
    GameObject plane;
    RaycastHit hit;

    public void Start()
    {
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
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(-90, 0, 0);
        plane.GetComponent<MeshRenderer>().material.mainTexture = texture;
        plane.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0);
        GenerateStartingAnt();
    }

    public void GenerateStartingAnt()
    {
        Ant ant;
        ant.x = width / 2;
        ant.y = height / 2;
        ant.direction = UnityEngine.Random.Range(0, 4);
        ants.Add(ant);
        Render();

        GameObject newAntGameObject = Instantiate(UIAntPrefab, antsListScrollview.transform.Find("Viewport").Find("Content"));
        newAntGameObject.name = ants.Count.ToString();
        newAntGameObject.transform.Find("Text").GetComponent<TMP_Text>().text = "Ant " + ants.Count;
        newAntGameObject.transform.Find("deleteButton").GetComponent<Button>().onClick.AddListener(() => DeleteAnt(newAntGameObject.transform.GetSiblingIndex()));
        newAntGameObject.GetComponent<Button>().onClick.AddListener(() => ViewAnt(Convert.ToInt32(newAntGameObject.name)-1));
    }

    public void GenerateRandomAnt()
    {
        Ant ant;
        ant.x = UnityEngine.Random.Range(0, width);
        ant.y = UnityEngine.Random.Range(0, height);
        ant.direction = UnityEngine.Random.Range(0, 4);
        ants.Add(ant);
        Render();

        GameObject newAntGameObject = Instantiate(UIAntPrefab, antsListScrollview.transform.Find("Viewport").Find("Content"));
        newAntGameObject.name = ants.Count.ToString();
        newAntGameObject.transform.Find("Text").GetComponent<TMP_Text>().text = "Ant " + ants.Count;
        newAntGameObject.transform.Find("deleteButton").GetComponent<Button>().onClick.AddListener(() => DeleteAnt(newAntGameObject.transform.GetSiblingIndex()));
        newAntGameObject.GetComponent<Button>().onClick.AddListener(() => ViewAnt(Convert.ToInt32(newAntGameObject.name)-1));
    }

    public void ViewAnt(int index)
    {
        UIAntPanel.SetActive(true);
        UIAntPanel.transform.Find("Position").Find("xy").GetComponent<TMP_Text>().text = "X: " + ants[index].x.ToString() + " Y: " + ants[index].y.ToString();
        UIAntPanel.transform.Find("Heading").Find("Direction").GetComponent<TMP_Text>().text = ants[index].direction.ToString();
    }

    public void DeleteAnt(int n)
    {
        ants.RemoveAt(n);
        for (int i = 0; i < antsListScrollview.transform.Find("Viewport").Find("Content").childCount; i++)
        {
            if (!antsListScrollview.transform.Find("Viewport").Find("Content").GetChild(i).gameObject.activeSelf)
            {
                Destroy(antsListScrollview.transform.Find("Viewport").Find("Content").GetChild(i).gameObject);
            }
        }
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

        for (int i = 0; i < antsListScrollview.transform.Find("Viewport").Find("Content").childCount; i++)
        {
            Destroy(antsListScrollview.transform.Find("Viewport").Find("Content").GetChild(i).gameObject);
        }

        ants.Clear();
        GenerateStartingAnt();
        Render();
    }

    public void Render()
    {
        Color[] colors = new Color[width * height];
        
        // Set all colors to black initially
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }

        // Render ants
        foreach (var ant in ants)
        {
            colors[ant.x + ant.y * width] = Color.red;
        }

        // Render cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cells[x, y] == 1)
                {
                    colors[x + y * width] = Color.white;
                }
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
                UpdateAnts(); // Update all ants
                if (lifeToggle.isOn)
                {
                    UpdateLife();
                }
                delay = delaySlider.value;
            }
        }

        HandleControls();
    }

    public void UpdateAnts()
    {
        for (int i = 0; i < ants.Count; i++)
        {
            Ant ant = ants[i];
            int currentCellValue = cells[ant.x, ant.y];
            if (currentCellValue == 0) // If the cell is black
            {
                cells[ant.x, ant.y] = 1; // Change the color to white
                if (loopsToggle.isOn)
                {
                    ant.direction = (ant.direction + 1) % 8; // Turn 45 degrees clockwise
                }
                else
                {
                    ant.direction = (ant.direction + 1) % 4; // Turn 90 degrees clockwise
                }
            }
            else // If the cell is white
            {
                cells[ant.x, ant.y] = 0; // Change the color to black
                if (loopsToggle.isOn)
                {
                    ant.direction = (ant.direction - 1 + 8) % 8; // Turn 45 degrees counter-clockwise
                }
                else
                {
                    ant.direction = (ant.direction - 1 + 4) % 4; // Turn 90 degrees counter-clockwise
                }
            }

            // Move the ant forward in the direction it's facing
            MoveAntForward(ref ant);

            // Ensure the ant wraps around the grid if it reaches the edge
            ant.x = (ant.x + width) % width;
            ant.y = (ant.y + height) % height;

            // Update the ant back into the list
            ants[i] = ant;
        }

        // Render the updated grid
        Render();
    }

    void MoveAntForward(ref Ant ant)
    {
        // Move the ant forward in the direction it's facing
        if (loopsToggle.isOn)
        {
            switch (ant.direction)
            {
                case 0: // up
                    ant.y++;
                    break;
                case 1: // upper right
                    ant.x++;
                    ant.y++;
                    break;
                case 2: // right
                    ant.x++;
                    break;
                case 3: // lower right
                    ant.x++;
                    ant.y--;
                    break;
                case 4: // down
                    ant.y--;
                    break;
                case 5: // lower left
                    ant.x--;
                    ant.y--;
                    break;
                case 6: // left
                    ant.x--;
                    break;
                case 7: // upper left
                    ant.x--;
                    ant.y++;
                    break;
            }
        }
        else
        {
            switch (ant.direction)
            {
                case 0: // up
                    ant.y++;
                    break;
                case 1: // right
                    ant.x++;
                    break;
                case 2: // down
                    ant.y--;
                    break;
                case 3: // left
                    ant.x--;
                    break;
            }
        }
    }

    public void UpdateLife()
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
