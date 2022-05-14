/* Saves variables related to the grid system such as size and the object that are used to
 * construct the grid (borders and background), contains methods to initialize and reset the
 * grid. This file also contains the Grid class which has functions for managing the Grid type.
 * 
 * Dependent on classes:
 * PermanentMonoSingleton - GameManager
 * MonoSingleton - LifeformManager */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoSingleton<MapManager>
{
    #region ---=== Serialized Variables ===---
    [Header("Editor Tools")]
    [SerializeField] public bool respawnAll = false;
    [Header("Grid")]
    [SerializeField] public int cellWidth = 16, cellHeight = 9;
    [SerializeField] public float wallDistance = 5;
    [SerializeField] public float animalSpawnPercent = 10f;
    [SerializeField] public float plantSpawnPercent = 50f;
    #endregion

    #region ---=== Nonserialized Variables ===---
    //References
    [NonSerialized] private GameValues gameValues;
    //Important Structures
    [NonSerialized] private GameObject background;
    [NonSerialized] private GameObject northWall, eastWall, southWall, westWall;

    //Constants
    [NonSerialized] private float gameCellSize;
    #endregion

    #region ---=== Get/Set Variables ===---



    #endregion

    #region ---=== Inspector Variables ===---

    #endregion

    #region ---=== Data Variables ===---
    [NonSerialized] public Grid generalGrid;
    [NonSerialized] public float mapWidth;
    [NonSerialized] public float mapHeight;
    #endregion

    #region ---=== Control Variables ===---
    [NonSerialized] private int lastCellWidth = -1, lastCellHeight = -1;
    #endregion


    protected override void Awake()
    {
        base.Awake();

        generalGrid = new Grid(cellWidth, cellHeight);

        respawnAll = true; //On first update SetupSpawn will happen
    }

    private void Start()
    {
        gameValues = GameManager.Instance.GameValues;
        gameCellSize = gameValues.GameCellSize;
    }

    private void Update()
    {
        //Will run first frame
        if (cellWidth != lastCellWidth || cellHeight != lastCellHeight)
        {
            UpdateGeneralGridSize();
        }

        lastCellWidth = cellWidth;
        lastCellHeight = cellHeight;

        if (respawnAll)
        {
            //Destory all life needs to be done distinctly before SetupSpawn because
            //Plants and animal only properly clean themselves up after they run OnDestroy()
            //Which can be called here but will not be done withing the calling method
            DestoryAllLife();
            Invoke("SetupSpawn", 0.01f);
            respawnAll = false;
        }

    }

    public void UpdateGeneralGridSize()
    {
        mapWidth = cellWidth * gameCellSize;
        mapHeight = cellHeight * gameCellSize;

        if (CameraManager.Instance != null)
            CameraManager.Instance.CamUpdateSize(mapWidth, mapHeight);

        UIManager.Instance.UpdateGridUI();

        if (CheckNull.SingleObjectNotNull(ref background, "MapBackground", true))
            background.transform.localScale = new Vector2 (cellWidth * gameCellSize, cellHeight * gameCellSize);

        generalGrid = new Grid(cellWidth, cellHeight, generalGrid);

        if (CheckNull.SingleObjectNotNull(ref northWall, "NorthWall", true))
        {
            northWall.transform.position = new Vector3((cellWidth * gameCellSize) / 2,
                                            cellHeight * gameCellSize + wallDistance / 2,
                                            northWall.transform.position.z);
            northWall.transform.localScale = new Vector3(cellWidth * gameCellSize,
                                                        wallDistance,
                                                        northWall.transform.localScale.z);
        }

        if (CheckNull.SingleObjectNotNull(ref eastWall, "EastWall", true))
        {
            eastWall.transform.position = new Vector3(cellWidth * gameCellSize + wallDistance / 2,
                                                    (cellHeight * gameCellSize) / 2,
                                                    eastWall.transform.position.z);
            eastWall.transform.localScale = new Vector3(wallDistance,
                                                        cellHeight * gameCellSize,
                                                        eastWall.transform.localScale.z);
        }

        if (CheckNull.SingleObjectNotNull(ref southWall, "SouthWall", true))
        {
            southWall.transform.position = new Vector3((cellWidth * gameCellSize) / 2,
                                                        -wallDistance / 2,
                                                        southWall.transform.position.z);
            southWall.transform.localScale = new Vector3(cellWidth * gameCellSize,
                                                        wallDistance,
                                                        southWall.transform.localScale.z);
        }

        if (CheckNull.SingleObjectNotNull(ref westWall, "WestWall", true))
        {
            westWall.transform.position = new Vector3(-wallDistance / 2,
                                                        (cellHeight * gameCellSize) / 2,
                                                        westWall.transform.position.z);
            westWall.transform.localScale = new Vector3(wallDistance,
                                                        cellHeight * gameCellSize,
                                                        westWall.transform.localScale.z);
        }
    }

    private void SetupSpawn()
    {
        //Spawns plants first because the have a permanent precence on the grid while creatures do not
        float plantSpawns = Mathf.Ceil(plantSpawnPercent/100 * cellWidth * cellHeight);
        float animalSpawns = Mathf.Ceil(animalSpawnPercent/100 * cellWidth * cellHeight);

        //Plant spawning
        int spawnedPlants = 0;
        while (spawnedPlants < plantSpawns)
        {
            Vector2Int spawnedCell = SpawnLifeRandomCell(true);

            if (spawnedCell.Equals(new Vector2Int(-1, -1)))
            {
                break;
            }

            spawnedPlants++;
        }

        //Creatures do not occupy a grid spot but when spawning multiple animals don't occupy a grid space
        //The old grid is stored with only plant data in it so animals can temporarily be tracked
        //At the end of this method the real grid will be set to the temp grid
        Grid storedGrid = new Grid(generalGrid);
        int spawnedAnimals = 0;
        while (spawnedAnimals < animalSpawns)
        {
            Vector2Int spawnedCell = SpawnLifeRandomCell(false);

            if (spawnedCell.Equals(new Vector2Int(-1, -1)))
            {
                break;
            }

            generalGrid.SetValue(spawnedCell, 1);
            spawnedAnimals++;
        }

        Player player = GameManager.Instance.player;
        if (player == null)
            return;

        player.transform.position = new Vector3(mapWidth / 2, mapHeight / 2, player.transform.position.z); ;

        generalGrid = new Grid(storedGrid);
    }

    private void DestoryAllLife()
    {
        //Destroy all plants and animals that currently exist
        foreach (GameObject plant in GameObject.FindGameObjectsWithTag("Plant"))
            Destroy(plant);

        foreach (GameObject animal in GameObject.FindGameObjectsWithTag("Animal"))
            Destroy(animal);
    }

    public Vector2Int SpawnLifeRandomCell(bool _plant)
    {
        Vector2Int randomCell;

        if (_plant)
        {
            Plant newPlant;

            //Attempts 20 times to find a empty grid space then just finds first empty or fails
            for (int i = 0; i < 20; i++)
            {
                randomCell = generalGrid.GetRandomCell();

                newPlant = SpawnPlantOnCell(randomCell);
                if (newPlant != null)
                {
                    newPlant.Randomize();
                    return randomCell;
                }
            }

            randomCell = generalGrid.GetFirstEmptyCell();

            newPlant = SpawnPlantOnCell(randomCell);
            if (newPlant != null)
            {
                newPlant.Randomize();
                return randomCell;
            }
        }
        else
        {
            Animal newAnimal;

            //Attempts 20 times to find a empty grid space then just finds first empty or fails
            for (int i = 0; i < 20; i++)
            {
                randomCell = generalGrid.GetRandomCell();

                newAnimal = SpawnAnimalOnCell(randomCell);
                if (newAnimal != null)
                {
                    newAnimal.Randomize();
                    return randomCell;
                }
            }

            randomCell = generalGrid.GetFirstEmptyCell();

            newAnimal = SpawnAnimalOnCell(randomCell);
            if (newAnimal != null)
            {
                newAnimal.Randomize();
                return randomCell;
            }
        }

        return new Vector2Int(-1, -1); //Indicates failure
    }

    public Plant SpawnPlantOnCell(Vector2Int _cell)
    {
        if (!generalGrid.WithinGrid(_cell))
        {
            return null;
        }

        if (generalGrid.GetValue(_cell) == 0)
        {
            Plant newPlant = LifeformManager.Instance.SpawnNewPlant(new Vector2(_cell[0] + gameCellSize / 2,
                                                                              _cell[1] + gameCellSize / 2));
            
            newPlant.gridLocation = _cell;

            return newPlant;
        }

        return null;
    }

    public Animal SpawnAnimalOnCell(Vector2Int _cell)
    {
        if (!generalGrid.WithinGrid(_cell))
        {
            return null;
        }

        if (generalGrid.GetValue(_cell) == 0)
        {
            Animal newAnimal = LifeformManager.Instance.SpawnNewAnimal(new Vector2(_cell[0] + gameCellSize / 2,
                                                                    _cell[1]+ gameCellSize / 2));

            return newAnimal;
        }

        return null;
    }
}

public class Grid
{
    private int width;
    private int height;
    private int[,] gridArray;

    public Grid() //Default Constructor
    {
        width = 64;
        height = 64;
        gridArray = new int[width, height];
    }

    public Grid(int _width, int _height)
    {
        width = _width;
        height = _height;
        gridArray = new int[width, height];
    }

    public Grid(int _width, int _height, Grid _otherGrid)
    {
        width = _width;
        height = _height;
        gridArray = new int[width, height];

        //Used to copy another grid of a same or different size
        for (int i = 0; i < _width; i++)
        {
            if (i >= _otherGrid.width)
                break;
            for (int j = 0; j < _height; j++)
            {
                if (j >= _otherGrid.height)
                    break;

                gridArray[i, j] = _otherGrid.gridArray[i, j];
            }
        }
    }

    public Grid(Grid _otherGrid)
    {
        width = _otherGrid.width;
        height = _otherGrid.height;

        //Direct Copy
        gridArray = new int[_otherGrid.width, _otherGrid.height];

        for (int i = 0; i < _otherGrid.width; i++)
        {
            for (int j = 0; j < _otherGrid.height; j++)
            {
                gridArray[i, j] = _otherGrid.gridArray[i, j];
            }
        }
    }

    public bool SetValue(int _x, int _y, int _value)
    {
        if (_x >= 0 && _y >= 0 && _x < width && _y < height)
        {
            gridArray[_x, _y] = _value;
            return true;
        }
            return false;
    }

    public bool SetValue(Vector2Int _cell, int _value)
    {
        if (_cell[0] >= 0 && _cell[1] >= 0 && _cell[0] < width && _cell[1] < height)
        {
            gridArray[_cell[0], _cell[1]] = _value;
            return true;
        }
            return false;
    }

    public bool SetValue(Vector2 _pos, int _value)
    {
        int _x = Mathf.FloorToInt(_pos.x);
        int _y = Mathf.FloorToInt(_pos.y);
        if (_x >= 0 && _y >= 0 && _x < width && _y < height)
        {
            gridArray[_x, _y] = _value;
            return true;
        }
        return false;
    }

    public bool SetValue(Vector3 _pos, int _value)
    {
        int _x = Mathf.FloorToInt(_pos.x);
        int _y = Mathf.FloorToInt(_pos.y);
        if (_x >= 0 && _y >= 0 && _x < width && _y < height)
        {
            gridArray[_x, _y] = _value;
            return true;
        }
            return false;
    }

    public int GetValue(int _x, int _y)
    {
        return gridArray[_x, _y];
    }

    public int GetValue(Vector2Int _cell)
    {
        return gridArray[_cell[0], _cell[1]];
    }

    public int GetValue(Vector3 _pos)
    {
        return gridArray[Mathf.FloorToInt(_pos.x), Mathf.FloorToInt(_pos.y)];
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public Vector2Int GetFirstEmptyCell()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (gridArray[j, i] == 0)
                    return new Vector2Int(j, i);
            }
        }

        return new Vector2Int(-1, -1);
    }

    public Vector2Int GetRandomCell()
    {
        return new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
    }

    public Vector2Int GetAdjacentRandomCell(Vector2Int _cell)
    {
        //TODO Might be faster to add as a variable to Grid
        List<Vector2Int> adjacentCells = new List<Vector2Int>();

        for (int i = _cell[0] - 1; i <= _cell[0] + 1; i++)
        {
            for (int j = _cell[1] - 1; j <= _cell[1] + 1; j++)
            {
                Vector2Int cell = new Vector2Int(i, j);

                if (WithinGrid(cell))
                    adjacentCells.Add(cell);
            }
        }

        if (adjacentCells.Count <= 0)
            return new Vector2Int(-1, -1);

        return adjacentCells[UnityEngine.Random.Range(0, adjacentCells.Count)];
    }

    public bool WithinGrid(Vector2Int _cell)
    {
        if ((_cell[0] >= 0 && _cell[0] < width)
             && (_cell[1] >= 0 && _cell[1] < height))
            return true;
        else
            return false;
    }

    public void PrintGrid()
    {
        for (int i = 0; i < height; i++)
        {
            string rowString = "";

            for (int j = 0; j < width; j++)
            {
                rowString += gridArray[j, i];
            }

            MonoBehaviour.print(rowString);
        }
    }
}
