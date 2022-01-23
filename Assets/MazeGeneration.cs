using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGeneration : MonoBehaviour
{
    // the wall prefab
    public GameObject wallPrefab;

    // wall thickness (pixels)
    private float wallSize = 1f;

    // game size in cells (X, Y)
    private (uint width, uint height) gameSize;

    // scalers for width and height
    private (float scaleX, float scaleY) scalers;

    // screen offset (from float truncation error)
    private (float offsetX, float offsetY) screenOffset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void removeMaze()
    {
        foreach (var wall in GameObject.FindGameObjectsWithTag("Wall"))
        {
            Destroy(wall);
        }
    }

    public void generateMaze(uint gameWidth, uint gameHeight, uint chosenWidth, uint chosenHeight)
    {
        // save sizes
        gameSize = (gameWidth, gameHeight);

        scalers.scaleX = chosenWidth / (float)gameWidth;
        scalers.scaleY = chosenHeight / (float)gameHeight;

        // set the game scaler to fit the maze on screen
        //scaler = Mathf.Min(Screen.width / (float)gameWidth, Screen.height / (float)gameHeight);

        // place maze in the center of the screen
        screenOffset = (Screen.width - scalers.scaleX * gameWidth, Screen.height - scalers.scaleY * gameHeight);

        // 0.5f means half of the cell is taken by the wall
        wallSize = Mathf.Min(scalers.scaleX, scalers.scaleY) * 0.5f;

        // delete old maze
        removeMaze();

        for (uint i = 0; i < gameSize.width; i++)
        {
            placeWall(i, 0, false);
            placeWall(i, gameSize.height, false);
        }
        for (uint i = 0; i < gameSize.height; i++)
        {
            placeWall(0, i, true);
            placeWall(gameSize.width, i, true);
        }

        wallGeneration(0, 0, gameSize.width, gameSize.height);
    }

    // generate maze in a recursive manner, using cuts
    void wallGeneration(uint xPos, uint yPos, uint xEnd, uint yEnd)
    {
        uint xSize = xEnd - xPos;
        uint ySize = yEnd - yPos;

        if (xSize > 1 && ySize > 1)
        {
            // could use random here instead, but this generates more uniform (nicer) samples
            uint xSplit = (xPos + xEnd + 1) / 2; // (uint)Random.Range(xPos + 1, xEnd);
            uint ySplit = (yPos + yEnd + 1) / 2; // (uint)Random.Range(yPos + 1, yEnd);

            uint totalSize = xSize + ySize;

            // generate random cuts, x cuts
            uint randomCut1 = (uint)Random.Range(xPos, xSplit) - xPos;
            uint randomCut2 = (uint)Random.Range(xSplit, xEnd) - xPos;
            
            // y cuts
            uint randomCut3 = (uint)Random.Range(yPos, ySplit) - yPos;
            uint randomCut4 = (uint)Random.Range(ySplit, yEnd) - yPos;

            // drop one cut, but keep everything in the first 3 variables, and put y values after x variables (numerical offset)
            randomCut3 += xSize;
            randomCut4 += xSize;

            //pick only 3 cuts, order not dependant
            switch (Random.Range(0, 4))
            {
                case (0):
                    randomCut1 = randomCut4;
                    break;
                case (1):
                    randomCut2 = randomCut4;
                    break;
                case (2):
                    randomCut3 = randomCut4;
                    break;
                default:
                    break;
            }

            // generate walls
            for (uint i = 0; i < totalSize; i++)
            {
                // don't generate on cut
                if (i == randomCut1 || i == randomCut2 || i == randomCut3) 
                    continue;

                // y has an offset
                if (i >= xSize)
                {
                    // place along y coordinate, at xSplit
                    placeWall(xSplit, yPos + i - xSize, true);
                }
                else
                {
                    // place along x coordinate, at ySplit
                    placeWall(xPos + i, ySplit, false);
                }
            }

            // proceed recursively on the other 4 quadrants
            wallGeneration(xPos, yPos, xSplit, ySplit);
            wallGeneration(xSplit, yPos, xEnd, ySplit);
            wallGeneration(xPos, ySplit, xSplit, yEnd);
            wallGeneration(xSplit, ySplit, xEnd, yEnd);
        }
    }

    void placeWall(uint x, uint y, bool vertical)
    {
        // instantiate object
        GameObject wall = Instantiate(wallPrefab);

        // adjust wall thickness to our variable and to the width and height
        wall.transform.GetChild(0).localScale = new Vector3((vertical ? scalers.scaleY : scalers.scaleX) + wallSize, wallSize, 1f);
        wall.transform.GetChild(0).localPosition = new Vector3((vertical ? scalers.scaleY : scalers.scaleX) / 2, 0f, 0f);

        // transform points to world, including our modifications
        Vector3 worldPoints = Camera.main.ScreenToWorldPoint(new Vector3(x * scalers.scaleX + screenOffset.offsetX / 2, y * scalers.scaleY + screenOffset.offsetY / 2, 0));
        
        // same for scale
        Vector3 worldScale = Camera.main.ScreenToWorldPoint(Vector3.one) - Camera.main.ScreenToWorldPoint(Vector3.zero);

        wall.transform.position = new Vector3(worldPoints.x, worldPoints.y, wall.transform.position.z);

        wall.transform.localScale = worldScale;

        // rotate wall if vertical (y wall)
        if (vertical)
        {
            wall.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
