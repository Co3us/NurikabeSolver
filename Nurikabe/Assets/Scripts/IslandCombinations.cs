/*
IslandCombinations.cs
Finds all possible combinations of islands and also contains functions to go through all of the combinations incrementally
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandCombinations : MonoBehaviour
{
    //const values
    const int UNKNOWN = StaticVars.UNKNOWN;

    //helper class for tree of island combinations
    public class PathNode
    {
        public PathNode parent;
        public PathNode[] children = new PathNode[4];
        public Vector2 position;
        public int level;
    }

    //script refrences
    AddBlock AddBlockScript;

    void Start()
    {
        ////get refrence
        AddBlockScript = GetComponent<AddBlock>();
    }

    //generates a tree structure containing all combinations of boxes that could make up a given island.
    //function also returns number of nodes per level in tree and number of unique nodes (unique box locations) per level
    //it also alows a flag to alter the behaviour (checkNodeNumFlag)- if this is set to true function will check if there is 
    //at least one possible comination for the given island
    public PathNode generateInitalTree(Island island, out byte[] numOfNodesPerLevel, out byte[] uniqueNodesPerLevel, bool checkNodeNumFlag)
    {
        int size = island.size;

        //we create the root
        PathNode root = new PathNode();
        root.parent = null;
        root.position = island.center;
        root.level = 1;


        //init of of nodes per level arrays
        //these contain number of nodes on level 1 in the first index
        //number of nodes on level 2 in second index ect. 
        numOfNodesPerLevel = new byte[island.size];
        uniqueNodesPerLevel = new byte[island.size];

        //the root level is always going to only have one node
        numOfNodesPerLevel[0] = 1;
        uniqueNodesPerLevel[0] = 1;

        //stack of nodes we need to process, starting with root
        Stack nodeList = new Stack();
        nodeList.Push(root);

        //counter for how many nodes we have added to tree
        int nodeCount = 1;

        // matrix that keeps track of box locations we have already visited
        //init value is 255-values represent level of tree 
        byte[,] visitedMat = new byte[StaticVars.numOfRows, StaticVars.numOfCols];
        for (int k = 0; k < StaticVars.numOfRows; k++)
        {
            for (int l = 0; l < StaticVars.numOfCols; l++)
            {
                visitedMat[k, l] = 255;
            }
        }

        //while we have nodes to process
        while (nodeList.Count > 0)
        {
            //if we found enough islands and we use checkNumFlag
            if (checkNodeNumFlag && nodeCount >= island.size)
            {
                return root;
            }
            PathNode currentNode = (PathNode)nodeList.Pop();

            //the max level we can reach is the island size
            if (currentNode.level < size)
            {
                //for up,right,down,left neighbour of vurrent node
                for (int k = 0; k < 4; k++)
                {
                    //get the position based on k index (0 means top, 1 means right ect.)
                    Vector2 newPos = currentNode.position + getVectorFromID(k);

                    //if field is valid for island and also isn't already part of the tree at the higher level
                    if (checkIfFieldValid(newPos, island) && visitedMat[(byte)newPos.x, (byte)newPos.y] > currentNode.level)
                    {
                        //creating a new node
                        PathNode newNode = new PathNode();
                        newNode.position = newPos;
                        newNode.parent = currentNode;
                        newNode.level = currentNode.level + 1;
                        currentNode.children[k] = newNode;

                        //pushing node on stack
                        nodeList.Push(newNode);

                        //if this boy hasn't been visted yet we count it as a unique node for current level
                        //otherwise another branch with this same node already exists
                        if (visitedMat[(byte)newPos.x, (byte)newPos.y] == 255)
                        {
                            uniqueNodesPerLevel[newNode.level - 1]++;
                        }
                        //set visitedMat to current level
                        visitedMat[(byte)newPos.x, (byte)newPos.y] = (byte)newNode.level;

                        //counters
                        numOfNodesPerLevel[newNode.level - 1]++;
                        nodeCount++;
                    }

                }
            }
        }
        //if we use the function to check if there are enough nodes and get to here
        //we indicate there isn't enough nodes by returnig null
        if (checkNodeNumFlag)
            return null;
        return root;
    }
    //takes a binary array and increments it to the next combination
    //example 1 (if we called this function 4 times starting with 1100): 1100->1010->1001->0101->0011
    //example 2 (if we called this function 3 times starting with 110010): 110010->110001->101100->101010
    public byte[] incrementPermutation(byte[] listIn)
    {
        //we make a copy for input list 
        byte[] list = new byte[listIn.Length];
        listIn.CopyTo(list, 0);

        //get lenght of array
        int len = list.Length - 1;

        //init k to length
        int k = len;

        //find index of first '1' from right
        while (list[k] == 0 && k >= 0)
        {
            k--;
        }
        // if last element is not '1'
        if (k < len)
        {
            //we move the '1' by one place right
            list[k] = 0;
            list[k + 1] = 1;
        }
        //if last elemnt is '1'
        else
        {
            //init l to k
            int l = k;

            //find index of first '0' to the left of last element
            while (list[l] == 1)
            {
                l--;
                //if we came to start of array
                if (l < 0)
                {
                    break;
                }
            }
            //if we are not on first element
            if (l > 0)
            {
                //number of '1' we need to move is the distance from start of '1's (k) to when we found '0' (l)
                int numOfOnes = k - l;

                //find first '1' on the left
                //0010011 --> 0010011
                // <--^         ^
                while (l >= 0 && list[l] == 0)
                {
                    l--;
                }
                //if we came to the first element and it was 0-increment is not possible
                //example: 00111
                if (l == -1)
                {
                    return null;
                }
                //if we found the '1' 
                else
                {
                    //moving '1' one place right
                    //example:110001->101001
                    list[l] = 0;
                    l++;
                    list[l] = 1;
                    l++;
                    //example:101001->101100.. we move the one from end to prev 1
                    for (int i = 0; i < numOfOnes; i++)
                    {
                        list[l] = 1;
                        l++;
                    }
                    while (l <= len)
                    {
                        list[l] = 0;
                        l++;
                    }
                }
            }
            //increment is not possible
            else
            {
                return null;
            }
        }
        byte[] newList = list;
        return newList;
    }

    //picks next possible combination of island blocks using a binary array and a tree or possible combinations
    //we use binary array to represent picked nodes of the tree 
    //first index is root then we go level by level
    //if the root has 2 sons 110 will pick root and first one and 101 will pick root and the second one
    public Stack pickIsland(PathNode tree, byte[] permutationList, Island island, byte[] NodesPerLevel, int minLevels)
    {
        //locations that we return
        Stack pickedLocationsOut = new Stack();

        //list of picked locations
        List<Vector2> pickedLocations = new List<Vector2>();

        //check if we have at least one "1" on second level
        //if not return null indicating that no further increments will be valid
        bool foundOne = false;
        int index = 0;
        for (int j = 0; j < NodesPerLevel[0]; j++)
        {
            if (permutationList[index] == 1)
            {
                foundOne = true;
            }
            index++;
        }
        if (foundOne == false)
        {
            pickedLocationsOut.Push(null);
            return pickedLocationsOut;
        }


        PathNode currentNode;

        //counter for picked boxes-the center of Island always picked
        byte boxesPicked = 1;

        //the first index is always 1
        int listIndex = 1;

        //push root to stack
        Stack nodeList = new Stack();
        nodeList.Push(tree);

        //while nodes left on stack or island size reached
        while (boxesPicked < island.size && nodeList.Count > 0)
        {

            currentNode = (PathNode)nodeList.Pop();

            //go through all the children of current node and add them to stack if binary array is '1' at their location
            foreach (PathNode node in currentNode.children)
            {
                if (node != null)
                {
                    if (permutationList[listIndex] == 1)
                    {
                        //check if box location was already picked
                        bool isSameLoc = false;
                        foreach (Vector2 loc in pickedLocations)
                        {
                            if (loc == node.position)
                            {
                                isSameLoc = true;
                                break;
                            }
                        }

                        //location wasn't picked before
                        if (isSameLoc == false)
                        {
                            pickedLocations.Add(node.position);
                            boxesPicked++;
                            nodeList.Push(node);
                        }
                    }
                    listIndex++;
                }
            }

        }
        //if we haven't found enough boxes we push 0 to indicate permutation is faulty
        if (boxesPicked < island.size)
            pickedLocationsOut.Push(0);
        else
            pickedLocationsOut = new Stack(pickedLocations);

        return pickedLocationsOut;
    }

    //helper function to get transformation vector based on code(0,1,2,3)
    public Vector2 getVectorFromID(int ID)
    {
        Vector2 vec = new Vector2();
        if (ID == 0)
        {
            vec = new Vector2(-1, 0);//up
        }
        else if (ID == 1)
        {
            vec = new Vector2(0, 1);//rigth
        }
        else if (ID == 2)
        {
            vec = new Vector2(1, 0);//down
        }
        else if (ID == 3)
        {
            vec = new Vector2(0, -1);//left
        }
        return vec;
    }

    //checking if island can be placed at location
    public bool checkIfFieldValid(Vector2 index, Island island)
    {
        byte i = (byte)index.x;
        byte j = (byte)index.y;

        //out of bounds
        if (i < 0 || j < 0 || j >= StaticVars.numOfCols || i >= StaticVars.numOfRows)
            return false;

        //box already sea or island or cell has any straight line neighbours that are island boxes
        if (StaticVars.boxesValues[i, j] != UNKNOWN || AddBlockScript.checkIfAnyIslandsAroudCell(i, j, island) == true)
            return false;

        return true;
    }
}
