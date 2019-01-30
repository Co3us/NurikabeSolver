using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVars : MonoBehaviour {
    //these are values for the box values matrix
    //positive values (0 and higher) represent land boxes
    //and(-1,-2,-3) negative values are used for different codes
    public const int LAND = 0;
    public const int SEA = -1;
    public const int UNKNOWN = -2;
    public const int OUT_OF_BOUNDS = -3;

    public static int[,] boxesValues;
    public static int[,] seaGroupLabelMatrix;
    public static GameObject[,] refGO;
    public static int numOfRows;
    public static int numOfCols;
    public static List<SeaGroup> seaGroups;
    public static int maxSeaGroupLabel;
    public static List<Island> unsolvedIslands;
    public static Island[,] islandsMatrix;
    public static TextAsset gridFile;
    public static List<Island> solvedIslands;
}
