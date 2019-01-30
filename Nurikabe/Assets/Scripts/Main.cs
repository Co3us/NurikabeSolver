/*
Main.cs
The starting point of the program.It builds a nurikabe from file and solves it.
If file not provided a default nurikabe is displayed.
*/

using System.Collections.Generic;
using UnityEngine;
public class Main : MonoBehaviour
{
    //grid file is a text file that builds a starting nurikabe field (set in unity editor)
    public TextAsset gridFile;

    //number of rows and columns of nurikabe field 
    int numOfRows = 10;
    int numOfCols = 10;

    //nurikabe field is represented as matrix of int
    //LEGEND 0=>land -1 => sea -2 =>unknown; for checking neighbours: -3=>out of bound value
    int[,] boxesValues;

    //matrix the same size as nurikabe field to mark which box belongs to which sea group
    //each group has a label starting from 1 (0 means box is not sea)
    int[,] seaGroupLabelMatrix;

    //matrix of gameObjects representing sprites of squares that make up the nurikabe grid
    GameObject[,] refGO;

    //helper groups
    List<Island> unsolvedIslands;
    List<SeaGroup> seaGroups;
    List<List<Vector2>> seaGroupsOld;
    
    //var that keeps track of the max label asigned to a sea group
    int maxSeaGroupLabel = 0;

    //script refrences
    BasicRules BasicRulesScript;
    BackTracking BackTrackingScript;
    MakeGrid MakeGridScript;

    void Start()
    {
        //get refrences
        BackTrackingScript = GetComponent<BackTracking>();
        MakeGridScript = GetComponent<MakeGrid>();
        BasicRulesScript = GetComponent<BasicRules>();

        //parse number of rows and columns from the provided gridFile
        if (gridFile != null)
        {
            string[] gridText = gridFile.text.Split('\n');

            string rowString = gridText[0].Split(' ')[0];
            bool success = int.TryParse(rowString, out numOfRows);
            if (!success)
            {
                print("Index" + rowString + " not an integer!");
                return;
            }
            string colString = gridText[0].Split(' ')[1];
            success = int.TryParse(colString, out numOfCols);
            if (!success)
            {
                print("Index" + colString + " not an integer!");
                return;
            }
        }
        //init all vars
        unsolvedIslands = new List<Island>();
        seaGroups = new List<SeaGroup>();
        seaGroupsOld = new List<List<Vector2>>();
        boxesValues = new int[numOfRows, numOfCols];
        seaGroupLabelMatrix = new int[numOfRows, numOfCols];
        refGO = new GameObject[numOfRows, numOfCols];

        //transfer vars to static
        StaticVars.boxesValues = boxesValues;
        StaticVars.seaGroupLabelMatrix = seaGroupLabelMatrix;
        StaticVars.refGO = refGO;
        StaticVars.numOfCols = numOfCols;
        StaticVars.numOfRows = numOfRows;
        StaticVars.unsolvedIslands = unsolvedIslands;
        StaticVars.seaGroups = seaGroups;
        //StaticVars.seaGroupsOld = seaGroupsOld;
        StaticVars.gridFile = gridFile;
        StaticVars.maxSeaGroupLabel = maxSeaGroupLabel;

        //create inital nurikabe structure and visualize it
        bool ret=MakeGridScript.makeGrid();

        //if there was no error while reading gridFile we can continue
        if (ret)
        {
            //solve any blocks you can with basic rules
            BasicRulesScript.fillWithBasicRules();

            //use backtracking to solve remaining blocks
            BackTrackingScript.init();
        }
    }


}


