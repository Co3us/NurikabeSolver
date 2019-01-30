/*
ViolationsCheck.cs
functions that check if any newly added box has caused a violation in the nurikabe rules.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViolationsCheck : MonoBehaviour
{
    //const values
    const int SEA = StaticVars.SEA;
    const int UNKNOWN = StaticVars.UNKNOWN;

    //script references
    AddBlock AddBlockScript;
    IslandCombinations ICScript;
    SeaPathFind SeaPathFindScript;
    void Start()
    {
        //get refrences
        AddBlockScript = GetComponent<AddBlock>();
        ICScript = GetComponent<IslandCombinations>();
        SeaPathFindScript = GetComponent<SeaPathFind>();
    }
    //checks if any sea squares (blocks of sea boxes size 2x2) are formed by adding sea box at location (i,j)
    //this function uses a matrix of visited boxes to check against (i,j) loaction- these fields are not yet marked as seas but represent them
    //it also check if squares are formed by actual sea blocks
    public bool checkForSeaSquaresVisited(int i, int j, int[,] Mat, out bool squareWithVisitedEncountered)
    {
        //if a box from Mat is part of the 2x2 square of sea boxes we set this to true
        squareWithVisitedEncountered = false;

        //get neighbours from Mat as well as the actual neighbours in the nurikabe field
        int[] valuesStraightLab = AddBlockScript.getAllNeighbourValuesStraightLines(i, j, Mat);
        int[] valuesDiagonalLab = AddBlockScript.getAllNeighbourValuesDiagonalLines(i, j, Mat);
        int[] valuesStraight = AddBlockScript.getAllNeighbourValuesStraightLines(i, j);
        int[] valuesDiagonal = AddBlockScript.getAllNeighbourValuesDiagonalLines(i, j);

        //check if sea box added at (i,j) is top-left, top-right bottom-right or bottom-left corner of a sea block
        //if found also check if sea square contains values from Mat and but only if there is no sea block at that same location in nurikabe field

        //top-left corner
        if ((valuesDiagonal[0] == SEA || valuesDiagonalLab[0] > 0) &&
            (valuesStraight[0] == SEA || valuesStraightLab[0] > 0) &&
            (valuesStraight[3] == SEA || valuesStraightLab[3] > 0))
        {
            if (valuesDiagonalLab[0] > 0 && valuesDiagonal[0] != SEA ||
                valuesStraightLab[0] > 0 && valuesStraight[0] != SEA ||
                valuesStraightLab[3] > 0 && valuesStraight[3] != SEA)
                squareWithVisitedEncountered = true;
            return true;
        }

        //top-right corner
        if ((valuesDiagonal[1] == SEA || valuesDiagonalLab[1] > 0) &&
            (valuesStraight[0] == SEA || valuesStraightLab[0] > 0) &&
            (valuesStraight[1] == SEA || valuesStraightLab[1] > 0))
        {
            if (valuesDiagonalLab[1] > 0 && valuesDiagonal[1] != SEA ||
                valuesStraightLab[0] > 0 && valuesStraight[0] != SEA ||
                valuesStraightLab[1] > 0 && valuesStraight[1] != SEA)
                squareWithVisitedEncountered = true;
            return true;
        }
        //bot-right corner
        if ((valuesDiagonal[2] == SEA || valuesDiagonalLab[2] > 0) &&
           (valuesStraight[1] == SEA || valuesStraightLab[1] > 0) &&
           (valuesStraight[2] == SEA || valuesStraightLab[2] > 0))
        {

            if (valuesDiagonalLab[2] > 0 && valuesDiagonal[2] != SEA ||
                valuesStraightLab[1] > 0 && valuesStraight[1] != SEA ||
                valuesStraightLab[2] > 0 && valuesStraight[2] != SEA)
                squareWithVisitedEncountered = true;
            return true;
        }
        //bot-left corner
        if ((valuesDiagonal[3] == SEA || valuesDiagonalLab[3] > 0) &&
           (valuesStraight[2] == SEA || valuesStraightLab[2] > 0) &&
           (valuesStraight[3] == SEA || valuesStraightLab[3] > 0))
        {
            if (valuesDiagonalLab[3] > 0 && valuesDiagonal[3] != SEA ||
                valuesStraightLab[2] > 0 && valuesStraight[2] != SEA ||
                valuesStraightLab[3] > 0 && valuesStraight[3] != SEA)
                squareWithVisitedEncountered = true;
            return true;
        }
        return false;
    }
    //checks if any sea squares (blocks of sea boxes size 2x2) are formed by adding sea box at location (i,j)
    public bool checkForSeaSquares(int i, int j)
    {
        int[] valuesStraight = AddBlockScript.getAllNeighbourValuesStraightLines(i, j);
        int[] valuesDiagonal = AddBlockScript.getAllNeighbourValuesDiagonalLines(i, j);

        //check if sea box added at (i,j) is top-left, top-right bottom-right or bottom-left corner of a sea block

        //top-left corner
        if (valuesDiagonal[0] == SEA && valuesStraight[0] == SEA && valuesStraight[3] == SEA)
        {
            return true;
        }
        //top-right corner
        if (valuesDiagonal[1] == SEA && valuesStraight[0] == SEA && valuesStraight[1] == SEA)
        {
            return true;
        }
        //bot-right corner
        if (valuesDiagonal[2] == SEA && valuesStraight[1] == SEA && valuesStraight[2] == SEA)
        {
            return true;
        }
        //bot-left corner
        if (valuesDiagonal[3] == SEA && valuesStraight[2] == SEA && valuesStraight[3] == SEA)
        {
            return true;
        }
        return false;
    }
    //checks if any sea squares (blocks of sea boxes size 2x2) are formed by adding sea box at locations on stack
    public bool checkForSeaSquares(Stack locations)
    {
        while (locations.Count > 0)
        {
            Vector2 loc = (Vector2)locations.Pop();
            int i = (int)loc.x;
            int j = (int)loc.y;
            if (checkForSeaSquares(i, j))
            {
                return true;
            }
        }
        return false;
    }
    //checks if any of the unsolved island have no potential arangements of island boxes 
    //also specifies an island to ignore
    public bool checkForIslandsWithNoOptions(Island island, Stack unsolvedIslands)
    {
        while (unsolvedIslands.Count > 0)
        {
            //we don't need these but function demands it
            byte[] filler1;
            byte[] filler2;
            
            Island currentIsland = (Island)unsolvedIslands.Pop();

            //check all islands except the island to ignore 
            if (currentIsland != island)
            {
                //if generateInitalTree return null there was no possible combination of island boxes for current island
                if (ICScript.generateInitalTree(currentIsland, out filler1, out filler2, true) == null)
                {
                    return true;
                }
            }
        }
        return false;
    }
    //checks if any sea group has no option to expand
    public bool checkForSeaWithNoOption()
    {
        //if there is only one sea group left this check is not valid
        if (StaticVars.seaGroups.Count > 1)
        {
            foreach (SeaGroup group in StaticVars.seaGroups)
            {
                //if any expansion option is found this is set to true
                bool canExpand = false;

                foreach (Vector2 pos in group.locations)
                {
                    int i = (int)pos.x;
                    int j = (int)pos.y;
                    int[] values = AddBlockScript.getAllNeighbourValuesStraightLines((int)pos.x, (int)pos.y);

                    //we check all 4 straight line neighbours
                    //if any of them is still a blank field and adding a sea box to it's location doesn't result in a sea square 
                    //we found an option to expand

                    //up
                    if (values[0] == UNKNOWN && checkForSeaSquares(i - 1, j) == false)
                    {
                        canExpand = true;
                        break;
                    }
                    //right
                    if (values[1] == UNKNOWN && checkForSeaSquares(i, j + 1) == false)
                    {
                        canExpand = true;
                        break;
                    }
                    //down
                    if (values[2] == UNKNOWN && checkForSeaSquares(i + 1, j) == false)
                    {
                        canExpand = true;
                        break;
                    }
                    //left
                    if (values[3] == UNKNOWN && checkForSeaSquares(i, j - 1) == false)
                    {
                        canExpand = true;
                        break;
                    }
                }

                //if we found a sea group that has nowhere to expand
                if (canExpand == false)
                {
                    return true;
                }
            }
        }
        return false;
    }
    //check if all sea groups have a valid connection to each other
    public bool checkIfSeaPathImpossible()
    {
        if (SeaPathFindScript.findOneGroup() == false)
        {
            return true;
        }
        return false;
    }
    //quick check for connection between all sea groups that can find a valid path quickly
    //but not finding a valid path doesn't yet mean that no valid path exists
    public bool checkIfSeaPathImpossibleQuickCheck()
    {
        if (SeaPathFindScript.findOuneGroupQuickCheck() == false)
        {
            return true;
        }
        return false;
    }
}
