using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGeneration : MonoBehaviour
{
    // the wall prefab
    public GameObject wallPrefab;

    // cells per inch to generate
    public float cellsPerInch = 2f;

    // percentage of the cell size
    public float wallSize = 0.1f;

    // used screen variables
    private (int width, int height, float dpi, float aspectRatio) screenVariables;

    // screen size divided by dpi
    private (float width, float height) screenAwareSize;

    // game size in cells
    private (uint width, uint height) gameSize;

    // screen offset (from float truncation error)
    private (float offsetX, float offsetY) screenOffset;

    // Start is called before the first frame update
    void Start()
    {
        screenVariables = (Screen.width, Screen.height, Screen.dpi, Screen.width / Screen.height);

        screenAwareSize = (screenVariables.width / screenVariables.dpi, screenVariables.height / screenVariables.dpi);

        gameSize = ((uint)(screenAwareSize.width * cellsPerInch), (uint)(screenAwareSize.height * cellsPerInch));

        screenOffset = (screenVariables.width - gameToScreenTransform(gameSize.width), screenVariables.height - gameToScreenTransform(gameSize.height));

        // frame generation
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

        Debug.Log(gameSize);
    }

    // generate maze in a recursive manner, using cuts
    void wallGeneration(uint xPos, uint yPos, uint xEnd, uint yEnd)
    {
        uint xSize = xEnd - xPos;
        uint ySize = yEnd - yPos;

        if (xSize > 1 && ySize > 1)
        {
            // could use random here instead, but this generates more uniform (nicer) samples
            uint xSplit = (xPos + xEnd + 1) / 2;
            uint ySplit = (yPos + yEnd + 1) / 2;

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

        // transform points to world, including our modifications
        Vector3 worldPoints = Camera.main.ScreenToWorldPoint(new Vector3(gameToScreenTransform(x) + screenOffset.offsetX / 2, gameToScreenTransform(y) + screenOffset.offsetY / 2, 0));
        
        // same for scale
        Vector3 worldScale = Camera.main.ScreenToWorldPoint(Vector3.one) - Camera.main.ScreenToWorldPoint(Vector3.zero);

        wall.transform.position = new Vector3(worldPoints.x, worldPoints.y, wall.transform.position.z);

        wall.transform.localScale = gameToScreenTransform(worldScale);

        // rotate wall if vertical (y wall)
        if (vertical)
        {
            wall.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    float gameToScreenTransform(float input)
    {
        return input * screenVariables.dpi / cellsPerInch;
    }

    Vector3 gameToScreenTransform(Vector3 input)
    {
        return input * screenVariables.dpi / cellsPerInch;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
