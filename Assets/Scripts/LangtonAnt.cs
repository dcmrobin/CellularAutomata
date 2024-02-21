using UnityEngine;
using UnityEngine.UI;

public class LangtonAnt : MonoBehaviour
{
    int[,] cells;
    int antX;
    int antY;
    int currentDirection; // 0: up, 1: right, 2: down, 3: left

    [Header("UI")]
    public Slider delaySlider;
    public Toggle fourtyFiveDegreeToggle;

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
        delaySlider.value = delay = updateDelay;
        cells = new int[width, height];
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(-90, 0, 0);
        plane.GetComponent<MeshRenderer>().material.mainTexture = texture;

        GenerateAnt();
    }

    public void GenerateAnt()
    {
        antX = width / 2;
        antY = height / 2;
        currentDirection = Random.Range(0, 4);
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
        GenerateAnt();
        Render();
    }

    public void Render()
    {
        Color[] colors = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                colors[x + y * width] = (cells[x, y] == 1) ? Color.white : (cells[x, y] == 0 ? Color.black : Color.red);
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
                UpdateAnt();
                delay = delaySlider.value;
            }
        }

        HandleControls();
    }

    public void UpdateAnt()
    {
        int currentCellValue = cells[antX, antY];
        if (currentCellValue == 0) // If the cell is black
        {
            cells[antX, antY] = 1; // Change the color to white
            if (fourtyFiveDegreeToggle.isOn)
            {
                currentDirection = (currentDirection + 1) % 8; // Turn 45 degrees clockwise
            }
            else
            {
                currentDirection = (currentDirection + 1) % 4; // Turn 90 degrees clockwise
            }
        }
        else // If the cell is white
        {
            cells[antX, antY] = 0; // Change the color to black
            if (fourtyFiveDegreeToggle.isOn)
            {
                currentDirection = (currentDirection - 1 + 8) % 8; // Turn 45 degrees counter-clockwise
            }
            else
            {
                currentDirection = (currentDirection - 1 + 4) % 4; // Turn 90 degrees counter-clockwise
            }
        }

        // Move the ant forward in the direction it's facing
        if (fourtyFiveDegreeToggle.isOn)
        {
            switch (currentDirection)
            {
                case 0: // up
                    antY++;
                    break;
                case 1: // upper right
                    antX++;
                    antY++;
                    break;
                case 2: // right
                    antX++;
                    break;
                case 3: // lower right
                    antX++;
                    antY--;
                    break;
                case 4: // down
                    antY--;
                    break;
                case 5: // lower left
                    antX--;
                    antY--;
                    break;
                case 6: // left
                    antX--;
                    break;
                case 7: // upper left
                    antX--;
                    antY++;
                    break;
            }
        }
        else
        {
            switch (currentDirection)
            {
                case 0: // up
                    antY++;
                    break;
                case 1: // right
                    antX++;
                    break;
                case 2: // down
                    antY--;
                    break;
                case 3: // left
                    antX--;
                    break;
            }
        }

        // Ensure the ant wraps around the grid if it reaches the edge
        antX = (antX + width) % width;
        antY = (antY + height) % height;

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
