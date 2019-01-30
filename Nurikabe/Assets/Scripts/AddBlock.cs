/*
AddBlock.cs
adds a sea or land block to nurikabe field. Also contains functions to get neighbouring values of given box
and to surround an island block with sea blocks.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddBlock : MonoBehaviour
{
    //const values
    const int SEA = StaticVars.SEA;
    const int LAND = StaticVars.LAND;
    const int UNKNOWN = StaticVars.UNKNOWN;
    const int OUT_OF_BOUNDS = StaticVars.OUT_OF_BOUNDS;

    //add land (island) box to nurikabe field
    public void addLandBlock(int i, int j)
    {
        //island boxes are represented by changing the box value to LAND and the sprite's color to yellow 
        StaticVars.boxesValues[i, j] = LAND;
        StaticVars.refGO[i, j].GetComponent<Image>().color = new Color(1, 1, 0, 1);
    }
    //add sea to any surrounding fields of each island box in an island-box group that are still unknown
    //also returns a list of all the locations of sea boxes that were added
    public Stack surroundWithSea(Stack locationsIn)
    {
        //stack to store locations of added sea boxes
        Stack locationsOut = new Stack();

        //for every location of island box on stack
        while (locationsIn.Count > 0)
        {
            Vector2 loc = (Vector2)locationsIn.Pop();
            int i = (int)loc.x;
            int j = (int)loc.y;

            int[] values = getAllNeighbourValuesStraightLines(i, j);
            if (values[0] == UNKNOWN)
            {
                addSeaBlock(i - 1, j);
                locationsOut.Push(new Vector2(i - 1, j));
            }
            if (values[1] == UNKNOWN)
            {
                addSeaBlock(i, j + 1);
                locationsOut.Push(new Vector2(i, j + 1));
            }
            if (values[2] == UNKNOWN)
            {
                addSeaBlock(i + 1, j);
                locationsOut.Push(new Vector2(i + 1, j));
            }
            if (values[3] == UNKNOWN)
            {
                addSeaBlock(i, j - 1);
                locationsOut.Push(new Vector2(i, j - 1));
            }
        }
        return locationsOut;
    }
   // add a sea box to nurikabe field
   // also handles changes in sea groups structure upon adding a new sea box
       public void addSeaBlock(int i, int j)
    {
        //if location out of bounds 
        if (i < 0 || i > StaticVars.numOfRows - 1 || j < 0 || j > StaticVars.numOfCols - 1)
        {
            return;
        }

        //if this location is already part of sea group
        if (StaticVars.seaGroupLabelMatrix[i, j] != 0)
        {
            return;
        }

        //sea boxes are represented by changing the box value to SEA and the sprite's color to black 
        StaticVars.boxesValues[i, j] = SEA;
        StaticVars.refGO[i, j].GetComponent<Image>().color = new Color(0, 0, 0, 1);

        //top,right,down,left neighbours (in that order)
        int[] values = getAllNeighbourValuesStraightLines(i, j, StaticVars.seaGroupLabelMatrix);

        //counter for how many sea groups the new sea box touches
        int counter = 0;

        //list of sea groups (lists of locations) that need to be merged 
        List<int> groupsToMergeLabels = new List<int>();

        //for each neighbour of location -val is label of sea group at neighbour location
        foreach (int val in values)
        {
            //if label is not zero it's part of a sea group
            if (val > 0)
            {
                bool add = true;
                //check if this group was already detected from some other neighbour
                foreach (int prevVal in groupsToMergeLabels)
                {
                    if (prevVal == val)
                    {
                        add = false;
                        break;
                    }
                }
                //if group wasn't detected yet
                if (add)
                {
                    //store label to list of groups that need to be merged 
                    groupsToMergeLabels.Add(val);
                    counter++;
                }
            }
        }

        //if the new sea box belongs to more sea groups (connects sea groups)
        if (counter != 1)
        {
            SeaGroup newGroup = new SeaGroup();

            //add location to new group
            newGroup.locations.Add(new Vector2(i, j));

            //location touches some sea groups
            if (groupsToMergeLabels.Count > 0)
            {
                newGroup.label = groupsToMergeLabels[0];
            }
            //location doesn't touch any sea group
            else
            {
                StaticVars.maxSeaGroupLabel++;
                newGroup.label = StaticVars.maxSeaGroupLabel;
            }

            //update seaGroupMatrix
            StaticVars.seaGroupLabelMatrix[i, j] = newGroup.label;

            //for every group to merge add all group locations to new group
            foreach (int group in groupsToMergeLabels)
            {
                foreach (SeaGroup seaGroup in StaticVars.seaGroups)
                {
                    if (seaGroup.label == group)
                    {
                        foreach (Vector2 loc in seaGroup.locations)
                        {
                            newGroup.locations.Add(loc);
                            StaticVars.seaGroupLabelMatrix[(int)loc.x, (int)loc.y] = newGroup.label;
                        }
                    }
                }
            }
            //for every group to merge delete the old group
            for (int k = groupsToMergeLabels.Count - 1; k >= 0; k--)
            {
                for (int l = 0; l < StaticVars.seaGroups.Count; l++)
                {
                    if (StaticVars.seaGroups[l].label == groupsToMergeLabels[k])
                    {
                        StaticVars.seaGroups.RemoveAt(l);
                        break;
                    }
                }
            }

            //add new group to sea groups
            StaticVars.seaGroups.Add(newGroup);

        }
        //if box at location (i,j) touches only one sea group (counter==1)
        else
        {
            //add location index to found sea group
            foreach (SeaGroup seaGroup in StaticVars.seaGroups)
            {
                if (seaGroup.label == groupsToMergeLabels[0])
                {
                    seaGroup.locations.Add(new Vector2(i, j));
                    StaticVars.seaGroupLabelMatrix[i, j] = seaGroup.label;
                }
            }
        }
    }

    //helper function; returns new position (i,j) according to the k value
    //k value here means 0-up 1-right 2-down 3-right
    public Vector2 getPosFromK(int k, byte i, byte j)
    {
        Vector2 pos = new Vector2(i, j);
        if (k == 0)
            return pos + new Vector2(-1, 0);
        if (k == 1)
            return pos + new Vector2(0, 1);
        if (k == 2)
            return pos + new Vector2(1, 0);
        else
            return pos + new Vector2(0, -1);

    }
    //checks if there are any islands top, right, down or left of the given row and column
    //parameter island is used so we can ignore a certain island in this check
    public bool checkIfAnyIslandsAroudCell(byte i, byte j, Island island)
    {
        if (island != null)
        {
            int[] values = getAllNeighbourValuesStraightLines(i, j);

            //for all neighborus
            for (int k = 0; k < values.Length; k++)
            {
                //if we found a neighbouring island
                if (values[k] >= 0)
                {
                    //if the neighbor is part of the island we ignore 
                    bool isInIsland = false;

                    Vector2 kPos = getPosFromK(k, i, j);

                    //checking if the position is in the island we ignore
                    foreach (Vector2 pos in island.islandBlocks)
                    {
                        if (pos == kPos)
                        {
                            isInIsland = true;
                            break;
                        }
                    }

                    //if we found neighbouring island that isn't part of the island to ignore
                    if (isInIsland == false)
                        return true;
                }
            }
        }
        return false;
    }
    //checks the number of options to expand for given location and returns location if there is only one option
    //returns number of options through the count parameter
    public int[] coordinatesIfOnlyOneOption(int i, int j, out int count)
    {
        //return value is an array representing a row and column locations of the grid
        int[] coordinates = new int[2];

        int[] values = getAllNeighbourValuesStraightLines(i, j);

        //flags to check if expansion in the given direction is possible
        bool left = false, right = false, up = false, down = false;
        count = 0;
        if (values[0] == UNKNOWN )
        {
            up = true;
            count++;
        }
        if (values[1] == UNKNOWN )
        {
            right = true;
            count++;
        }
        if (values[2]==UNKNOWN)
        {
            down = true;
            count++;
        }
        if (values[3]==UNKNOWN)
        {
            left = true;
            count++;
        }

        //if only one viable option to expand
        if (count == 1)
        {
            if (up)
            {
                coordinates[0] = i - 1;
                coordinates[1] = j;
            }
            else if (down)
            {
                coordinates[0] = i + 1;
                coordinates[1] = j;
            }
            else if (right)
            {
                coordinates[0] = i;
                coordinates[1] = j + 1;
            }
            else if (left)
            {
                coordinates[0] = i;
                coordinates[1] = j - 1;
            }
        }
        return coordinates;
    }
    //adds sea box if there is only one option where it can expand
    //this should only be used on a sea box that is known to be the only member
    //of a sea group with an option to expand
    public bool addSeaIfOnlyOneOption(int i, int j)
    {
        int count = 0;
        int[] coordinates = coordinatesIfOnlyOneOption(i, j, out count);
        if (count == 1)
        {
            addSeaBlock(coordinates[0], coordinates[1]);
            return true;
        }
        return false;
    }
    public int[] getAllNeighbourValuesStraightLines(int i, int j, int[,] Mat = null)
    {
        int[] values = new int[4]; //top right bot left 
        int[,] matrix;

        if (Mat == null)
            matrix = StaticVars.boxesValues;
        else
            matrix = Mat;

        if (i > 0)
        {
            values[0] = matrix[i - 1, j];
        }
        else
        {
            values[0] = OUT_OF_BOUNDS;
        }

        if (j < StaticVars.numOfCols - 1)
        {
            values[1] = matrix[i, j + 1];
        }
        else
        {
            values[1] = OUT_OF_BOUNDS;
        }
        if (i < StaticVars.numOfRows - 1)
        {
            values[2] = matrix[i + 1, j];
        }
        else
        {
            values[2] = OUT_OF_BOUNDS;
        }
        if (j > 0)
        {
            values[3] = matrix[i, j - 1];
        }
        else
        {
            values[3] = OUT_OF_BOUNDS;
        }
        return values;
    }
    public int[] getAllNeighbourValuesDiagonalLines(int i, int j, int[,] Mat = null)
    {
        int[] values = new int[4]; //top-left top-right bot-right bot-left 

        int[,] matrix;

        if (Mat == null)
        {
            matrix = StaticVars.boxesValues;
        }
        else
        {
            matrix = Mat;
        }

        if (i > 0 && j > 0)
        {
            values[0] = matrix[i - 1, j - 1];
        }
        else
        {
            values[0] = OUT_OF_BOUNDS;
        }

        if (i > 0 && j < StaticVars.numOfCols - 1)
        {
            values[1] = matrix[i - 1, j + 1];
        }
        else
        {
            values[1] = OUT_OF_BOUNDS;
        }
        if (i < StaticVars.numOfRows - 1 && j < StaticVars.numOfCols - 1)
        {
            values[2] = matrix[i + 1, j + 1];
        }
        else
        {
            values[2] = OUT_OF_BOUNDS;
        }
        if (i < StaticVars.numOfRows - 1 && j > 0)
        {
            values[3] = matrix[i + 1, j - 1];
        }
        else
        {
            values[3] = OUT_OF_BOUNDS;
        }
        return values;
    }
}
