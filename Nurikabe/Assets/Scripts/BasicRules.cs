/*
BasicRules.cs
fills nurikabe field with basic rules. Basic rules meaning fields you can solve without guessing.
*/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasicRules : MonoBehaviour
{
    //const values
    const int UNKNOWN = StaticVars.UNKNOWN;
    const int SEA = StaticVars.SEA;

    //local version of static vars 
    int[,] boxesValues;
    int numOfRows;
    int numOfCols;
    GameObject[,] refGO;

    //script references
    AddBlock AddBlockScript;
    ViolationsCheck ViolationsCheckScript;

    public void initVars()
    {
        //set local vars from static vars
        boxesValues = StaticVars.boxesValues;
        refGO = StaticVars.refGO;
        numOfCols = StaticVars.numOfCols;
        numOfRows = StaticVars.numOfRows;


        //add references
        AddBlockScript = GetComponent<AddBlock>();
        ViolationsCheckScript = GetComponent<ViolationsCheck>();

    }
    public bool fillWithBasicRules()
    {
        initVars();
        //first pass sets islands and fills any fields around the islands of size 1 with sea
        for (int i = 0; i < numOfRows; i++)
        {
            for (int j = 0; j < numOfCols; j++)
            {
                //ONES (fill sea around any islands that have size 1)
                if (boxesValues[i, j] == 1)
                {
                    AddBlockScript.addSeaBlock(i + 1, j);

                    AddBlockScript.addSeaBlock(i - 1, j);

                    AddBlockScript.addSeaBlock(i, j + 1);

                    AddBlockScript.addSeaBlock(i, j - 1);
                }
                //MAKE ISLANDS (
                if (boxesValues[i, j] > 1)
                {
                    Island island = new Island();
                    island.center = new Vector2(i, j);
                    island.size = boxesValues[i, j];
                    island.islandBlocks = new List<Vector2>();
                    island.islandBlocks.Add(island.center);
                    StaticVars.unsolvedIslands.Add(island);
                }
            }
        }
        //we order islands by size so we will start guessing with the smallest later on
        StaticVars.unsolvedIslands = StaticVars.unsolvedIslands.OrderByDescending(o => o.size).ToList();

        //second pass loops while there were changes, because altering a field (filling a box)
        //can mean some prev rules will now provide some more solvable boxes
        bool changeHappened = true;
        while (changeHappened)
        {
            changeHappened = false;
            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfCols; j++)
                {
                    //top,right,down,left neighbours (in that order)
                    int[] values = AddBlockScript.getAllNeighbourValuesStraightLines(i, j);

                    //if current field is unknown
                    if (boxesValues[i, j] == UNKNOWN)
                    { 
                        //--ISLANDS TOGETHER (fill see in between two islands that are close)
                        //if up and down are islands or right and left islands we fill current field with sea
                        if ((values[0] > 0 && values[2] > 0) || (values[1] > 0 && values[3] > 0))
                        {
                            AddBlockScript.addSeaBlock(i, j);
                            changeHappened = true;
                        }
                        //if up and right are islands or up and left are islands or down and right are islands
                        //or down and left are islands we fill current field with sea
                        if ((values[0] > 0 && values[1] > 0) || (values[0] > 0 && values[3] > 0) ||
                            (values[2] > 0 && values[1] > 0) || (values[2] > 0 && values[3] > 0))
                        {
                            AddBlockScript.addSeaBlock(i, j);
                            changeHappened = true;
                        }

                        //--SEA FROM ALL SIDES (fill boxes that have sea up, right,left and down from them)
                        if (values[0] == SEA && values[1] == SEA && values[2] == SEA && values[3] == SEA)
                        {
                            AddBlockScript.addSeaBlock(i, j);
                        }
                    }
                    //SEA CAN ONLY EXPAND ONE WAY -(fill box with sea if it is the only possible continuation of a specific sea group) 
                    //sea groups are generated and formed in AddBlockScript.addSeaBlock function
                    //if current field is sea
                    if (boxesValues[i, j] == SEA)
                    {
                        //for every sea group
                        for (int l = 0; l < StaticVars.seaGroups.Count; l++)
                        {
                            List<Vector2> group = StaticVars.seaGroups[l].locations;

                            // how many blocks in a group have the potential to expand
                            int potentialPathCounter = 0; 

                            //position (row and col) of last visted box in sea group
                            Vector2 lastVector = new Vector2();

                            //counter for storing number of options to expand for a single box in sea group
                            int count = 0;

                            //for every box in sea group
                            for (int k = 0; k < group.Count; k++)
                            {
                                Vector2 v = group[k];

                                //we use this function only to check count
                                //(number of options to expand for a specific location); we don't need it's returns
                                AddBlockScript.coordinatesIfOnlyOneOption((int)v.x, (int)v.y, out count);

                                //if there is any option to expand we can
                                //increase the counter for how many boxes in sea group can expand 
                                if (count > 0)
                                {
                                    potentialPathCounter++;
                                    lastVector = v;
                                }
                                //once we find more than one expandable box in sea group
                                //we can break as the rule won't apply for this sea group
                                if (potentialPathCounter > 1)
                                {
                                    break;
                                }
                            }
                            //if we find one and only one possible box in sea group that can expand
                            if (potentialPathCounter == 1)
                            {
                                //function will add a sea block next to the box if the box has only one option to expand
                                ////in such case it will also return true and start checking all groups from the start(l=0)
                                if (AddBlockScript.addSeaIfOnlyOneOption((byte)lastVector.x, (byte)lastVector.y))
                                    l = 0;
                            }
                        }
                    }
                }
            }
        }
        return true;
    }
}
