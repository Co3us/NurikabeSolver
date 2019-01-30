using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillGrid : MonoBehaviour {

    int[,] boxesValues;
    public int numOfRows;
    public int numOfCols;
    // Use this for initialization
    void Start () {

        boxesValues = new int[numOfRows, numOfCols];
        fillWithNumbers();
        fillWithBasicRules();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void fillWithNumbers()
    {
        boxesValues[0, 4] = 2;
        boxesValues[0, 8] = 4;
        boxesValues[1, 1] = 2;
        boxesValues[2, 6] = 6;
        boxesValues[3, 0] = 2;
        boxesValues[3, 3] = 5;
        boxesValues[3, 8] = 2;
        boxesValues[4, 9] = 2;
        boxesValues[5, 3] = 1;
        boxesValues[7, 1] = 1;
        boxesValues[8, 3] = 2;
        boxesValues[9, 1] = 4;
        boxesValues[9, 5] = 4;
        boxesValues[9, 7] = 4;
        boxesValues[9, 9] = 3;

    }
    public void fillWithBasicRules()
    {  
        for (int i = 0; i < numOfRows; i++)
        {
            for (int j = 0; j < numOfCols; j++)
            {
                //ONES
                if (boxesValues[i,j] == 1)
                {
                    boxesValues[i + 1, j] = 0;
                    boxesValues[i + 1, j] = 0;

                    boxesValues[i - 1, j] = 0;
                    boxesValues[i - 1, j] = 0;

                    boxesValues[i, j+1] = 0;
                    boxesValues[i, j+1] = 0;

                    boxesValues[i , j-1] = 0;
                    boxesValues[i,j-1] = 0;

                }
            }
        }
    }
}
