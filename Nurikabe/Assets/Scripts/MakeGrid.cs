/*
MakeGrid.cs
creates a nurikabe grid and visualizes it.
*/
using UnityEngine;
using UnityEngine.UI;

public class MakeGrid : MonoBehaviour
{
    //const values
    const int UNKNOWN = StaticVars.UNKNOWN;

    //contents of gridFile
    string[] gridText;

    //square sprite representing a box in nurikabe field
    public GameObject square;

    //hardcoded size of square sprite
    float distance = 27.1f;

    public bool makeGrid()
    {
        //get gridText from gridFile
        if (StaticVars.gridFile != null)
        {
            gridText = StaticVars.gridFile.text.Split('\n');
        }
        //fill static boxValues with initial values-islands
        bool ret=fillWithNumbers();

        //if error was encountered stop program
        if (ret == false)
            return false;

        //visualize nurikabe field
        GameObject Canvas = GameObject.Find("Canvas");
        for (int i = 0; i < StaticVars.numOfRows; i++)
        {
            for (int j = 0; j < StaticVars.numOfCols; j++)
            {
                GameObject squareClone = Instantiate(square, new Vector3(square.transform.position.x + distance * j, square.transform.position.y - distance * i, 0), transform.rotation, Canvas.transform);
                squareClone.name = i + " " + j;

                //save refrences to squares
                StaticVars.refGO[i, j] = squareClone;

                if (StaticVars.boxesValues[i, j] >= 0)
                    squareClone.transform.Find("Text").GetComponent<Text>().text = StaticVars.boxesValues[i, j].ToString();
            }
        }
        //deleting original square
        square.SetActive(false);
        return true;
    }
    public bool fillWithNumbers()
    {
        //init boxValues
        for (int i = 0; i < StaticVars.numOfRows; i++)
        {
            for (int j = 0; j < StaticVars.numOfCols; j++)
            {
                StaticVars.boxesValues[i, j] = UNKNOWN;
            }
        }
        //if gridFile is not set, use default nurikabe
        if (StaticVars.gridFile != null)
        {
            // grid file contains only the locations of the islands (numbers)
            //every line means new island
            for (int i = 1; i < gridText.Length; i++)
            {
                string[] pars = gridText[i].Split(' ');
                int k;
                bool success = int.TryParse(pars[0], out k);
                if (!success)
                {
                    print("Index not an integer at line:"+i);
                    return false;
                }
                int l;
                success = int.TryParse(pars[1], out l);
                if (!success)
                {
                    print("Index not an integer at line:"+i);
                    return false;
                }
                int val;
                success = int.TryParse(pars[2], out val);
                if (!success)
                {
                    print("Index not an integer at line: "+i);
                    return false;
                }
                if (k < 0 || l < 0 || k > StaticVars.numOfRows - 1 || l > StaticVars.numOfCols - 1)
                {
                    print("index out of bounds at line: "+i);
                    return false;
                }
                StaticVars.boxesValues[k, l] = val;
            }
        }
        else
        {
            StaticVars.boxesValues[0, 0] = 2;
            StaticVars.boxesValues[0, 9] = 2;
            StaticVars.boxesValues[1, 7] = 2;
            StaticVars.boxesValues[2, 1] = 2;
            StaticVars.boxesValues[2, 4] = 7;
            StaticVars.boxesValues[4, 6] = 3;
            StaticVars.boxesValues[4, 8] = 3;
            StaticVars.boxesValues[5, 2] = 2;
            StaticVars.boxesValues[5, 7] = 3;
            StaticVars.boxesValues[6, 0] = 2;
            StaticVars.boxesValues[6, 3] = 4;
            StaticVars.boxesValues[8, 1] = 1;
            StaticVars.boxesValues[8, 6] = 2;
            StaticVars.boxesValues[8, 8] = 4;
        }
        return true;
    }
}


