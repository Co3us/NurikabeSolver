/*
SeaPathFind.cs
Tries to find a valid sea path between all sea groups and determines if no such path exists
*/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SeaPathFind : MonoBehaviour
{
    //const values
    const int SEA = StaticVars.SEA;
    const int UNKNOWN = StaticVars.UNKNOWN;

    //helper class for generating a sea path
    public class Node
    {
        public Vector2 position;
        public Node parent;
        public Node[] children = new Node[4];
        public byte dir = 0;
    }

    //helper class for keeping track of the path of each sea group
    public class GroupPath
    {
        //public List<Node> trees = new List<Node>();
        public Stack nodes;
        public int groupIndex;
        public int startIndex;
    }

    //script refrences
    AddBlock AddBlockScript;
    ViolationsCheck ViolationsCheckScript;
    IslandCombinations IslandCombinationsScript;

    //helper matrices
    int[,] visitedMat;
    int[,] groupsMat;

    //index of biggest sea group
    int maxGroupIndex;

    void Start()
    {
        //get refrences
        AddBlockScript = GetComponent<AddBlock>();
        ViolationsCheckScript = GetComponent<ViolationsCheck>();
        IslandCombinationsScript = GetComponent<IslandCombinations>();
    }
    //refreshes colors
    public void recolor()
    {
        //we need to recolor the squares that are UNKNOWN to white
        for (int i = 0; i < StaticVars.numOfRows; i++)
        {
            for (int j = 0; j < StaticVars.numOfCols; j++)
            {
                if (StaticVars.boxesValues[i, j] == UNKNOWN)
                {
                    StaticVars.refGO[i, j].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                }
            }
        }
    }
    //quickly checks for possible sea path
    //doesn't check all possibilities
    public bool findOuneGroupQuickCheck()
    {
        int prevSize = -1;
        //loop until we only have one group
        while (StaticVars.seaGroups.Count > 1)
        {
            //number of sea groups hasn't changed from prev iteration
            if (prevSize == StaticVars.seaGroups.Count)
                break;

            prevSize = StaticVars.seaGroups.Count;

            //take the first group
            SeaGroup group = StaticVars.seaGroups[0];
            
            //for each sea block in group expand until you hit another sea group
            int len = group.locations.Count;
            for (int i = 0; i < len; i++)
            {
                if(prevSize> StaticVars.seaGroups.Count)
                {
                    break;
                }
                findPathQuickCheck(group.locations[i], group.locations);
            }
        }

        //we haven't found a path connecting all sea groups
        if (StaticVars.seaGroups.Count != 1)
            return false;
        else
            return true;
    }
    //go from starting sea box and expand with sea boxes until you hit another sea group or run out of options to exapand
    public void findPathQuickCheck(Vector2 block, List<Vector2> seaGroup)
    {
        //direction code (0-up,1-right,2-down,3-left)
        byte dir = 0;

        //init stack and push starting node to stack
        Stack nodes = new Stack();
        Node node = new Node();
        node.position = block;
        nodes.Push(node);

        //while nodes on stack
        while (nodes.Count > 0)
        {
            Node currentNode = (Node)nodes.Pop();

            //node stores in which direction from it we'll go next
            dir = currentNode.dir;

            Vector2 newPos;
            if (dir < 4)
                 newPos = currentNode.position+ IslandCombinationsScript.getVectorFromID(dir);
            else
                continue;

            //for next iteration we move on to next direction
            currentNode.dir++;

            nodes.Push(currentNode);

            //make new node 
            Node childNode = new Node();
            childNode.position = newPos;

            //add sea box if valid
            int i = (int)childNode.position.x;
            int j = (int)childNode.position.y;
            //if not out of bounds
            if (i >= 0 && i < StaticVars.numOfRows && j >= 0 && j < StaticVars.numOfCols)
            {
                if (StaticVars.boxesValues[i, j] == UNKNOWN)
                {
                    //if placing sea on this position would result in a 2x2 block of sea boxes
                    if (ViolationsCheckScript.checkForSeaSquares(i, j) == false)
                    {
                        //add sea box and push to stack
                        AddBlockScript.addSeaBlock(i, j);
                        nodes.Push(childNode);

                        //check if new sea box is touching any sea block belonging to different sea group
                        int[] values = AddBlockScript.getAllNeighbourValuesStraightLines(i, j);
                        bool isInSameGroupTop = false;
                        bool isInSameGroupRight = false;
                        bool isInSameGroupDown = false;
                        bool isInSameGroupLeft = false;

                        //if the reached group is part of the same sea group
                        foreach (Vector2 seaPos in seaGroup)
                        {
                            //up
                            if (seaPos == newPos + new Vector2(-1, 0))
                            {
                                isInSameGroupTop = true;
                                continue;
                            }
                            //right
                            if (seaPos == newPos + new Vector2(0, 1))
                            {
                                isInSameGroupRight = true;
                                continue;
                            }
                            //down
                            if (seaPos == newPos + new Vector2(1, 0))
                            {
                                isInSameGroupDown = true;
                                continue;
                            }
                            //left
                            if (seaPos == newPos + new Vector2(0, -1))
                            {
                                isInSameGroupLeft = true;
                                continue;
                            }
                        }
                        //if any neighbour is sea box and that box is not part of the same group 
                        if (values[0] == SEA && isInSameGroupTop == false)
                        {
                            break;
                        }
                        else if (values[1] == SEA && isInSameGroupRight == false)
                        {
                            break;
                        }
                        else if (values[2] == SEA && isInSameGroupDown == false)
                        {
                            break;
                        }
                        else if (values[3] == SEA && isInSameGroupLeft == false)
                        {
                            break;
                        }

                    }

                }
            }
           
        }
    }
    //robust version of finding if there is any possible path between every sea group
    //checks all options
    public bool findOneGroup()
    {
        //keeps track of boxes we chose as part of the path from one sea gruop to another
        visitedMat = new int[StaticVars.numOfRows, StaticVars.numOfCols];

        //keeps track of which box values belong to which sea group
        groupsMat = new int[StaticVars.numOfRows, StaticVars.numOfCols];
        
        //local ref
        List<SeaGroup> seaGroups = StaticVars.seaGroups;

        //sorting so the largest group is at end
        seaGroups=seaGroups.OrderBy(l => l.locations.Count()).ToList();

        maxGroupIndex = seaGroups.Count()-1;

        //init stack for sea groups and picked paths for each sea group
        Stack pickedPaths = new Stack();
        Stack groups = new Stack();

        //for every sea group
        for (int k = 0; k < seaGroups.Count; k++)
        {
            //fill group matrix
            for (int l = 0; l < seaGroups[k].locations.Count; l++)
            {
                Vector2 pos = seaGroups[k].locations[l];
                groupsMat[(int)pos.x, (int)pos.y] = k+1;
            }

            //add all sea groups except the last (biggest) to stack
            if(k< seaGroups.Count - 1)
            {
                GroupPath groupPath = new GroupPath();
                groupPath.groupIndex = k;
                groupPath.nodes = null;
                groups.Push(groupPath);
            }
        }

        //while groups on stack
        while(groups.Count>0)
        {
            //collecting group from stack
            GroupPath groupPath =(GroupPath) groups.Peek();
            List<Vector2> group = seaGroups[groupPath.groupIndex].locations;
            int j = groupPath.startIndex;
            Stack nodes = groupPath.nodes;

            //var to store the out value of findPath 
            bool squareVisited = false;
            int countSquareVisted = 0;
            //for every box in sea group
            while (j < group.Count())
            {
                //finds boxes belonging to sea path from current group to biggest group
                nodes = findPath(group[j], nodes, (byte)(groupPath.groupIndex + 1), out squareVisited);
                
                //keep track of squareVisited for each box in sea group
                if (squareVisited == true)
                {
                    countSquareVisted++;
                }
                //if path was found from current location we can break loop
                if (nodes != null)
                {
                    break;
                }
                j++;
            }
            
            //if no box in group is able to find a path to main group
            if (nodes==null)
            {
                //if we run out of combinations in the first group
                //field is not valid
                if (groupPath.groupIndex == maxGroupIndex-1)
                {
                    return false;
                }

                //if no 2x2 block was formed among the added blocks and block from any previous path
                //this means there will never be a valid path here no matter what other path combos are so we break
                if (countSquareVisted==0)
                {
                    return false;
                }

                //we take the prev group that found a path and try to find the next valid path 
                groups.Push(pickedPaths.Pop());
            }
            //if path is found
            else
            {
                //we pop the sea group from working stack and push it to memory stack (keeps track of prev picked paths)
                groups.Pop();
                groupPath.startIndex = j;
                groupPath.nodes = nodes;
                pickedPaths.Push(groupPath);
            }
        }
        return true;
    }
    //finds the path form a sea group to the biggest sea group 
    //checks if box from current path formed a 2x2 sea block with any of the boxes from previously selected paths (for other sea groups)
    public Stack findPath(Vector2 block, Stack nodes, byte label, out bool squareWithVisitedEncountered)
    {
        //var to check for 2x2 block among previous paths
        squareWithVisitedEncountered = false;

        //direction
        byte dir;

        //nodes is null at the initial stage and also if no paths were found for a sea group
        //if a sea group has gone through all the combinations it sets nodes to null 
        //so that we start from the first combination again when a different combination was selected on the previous group 
        if (nodes == null)
        {
            nodes = new Stack();
            Node root = new Node();
            root.position = block;
            visitedMat[(byte)root.position.x, (byte)root.position.y] = label;
            nodes.Push(root);
        }

        //while nodes on stack
        while (nodes.Count > 0)
        {
            Node currentNode = (Node)nodes.Pop();
            Vector2 newPos;

            //node stores in which direction from it we'll go next
            dir = currentNode.dir;
            if (dir < 4)
                newPos = currentNode.position + IslandCombinationsScript.getVectorFromID(dir);

            //if we can't expand sea box in path in any direction and it's not touching any other sea group
            //then this box will not be part of the path and we set visitedMat back to 0 and delete node form stack
            else
            {
                //if not root
                if (currentNode.parent != null)
                {
                    visitedMat[(byte)currentNode.position.x, (byte)currentNode.position.y] = 0;
                }
                //this also deletes node from path because we skip the re-adding of the node on stack and move on to next iteration
                continue;
            }

            //for next iteration we move on to next direction
            currentNode.dir++;

            //we push node back to stack
            nodes.Push(currentNode);

            //init for child node
            Node childNode = new Node();
            childNode.parent = currentNode;
            childNode.position = newPos;

            int i = (int)childNode.position.x;
            int j = (int)childNode.position.y;

            //if not out of bounds
            if (i >= 0 && i < StaticVars.numOfRows && j >= 0 && j < StaticVars.numOfCols)
            {
                //if not island and location wasn't already visited by the same path (current path)
                if ((StaticVars.boxesValues[i, j] == UNKNOWN || StaticVars.boxesValues[i, j] == SEA) && visitedMat[i, j] != label)
                {
                    bool squareVisited = false;
                    //if we don't form any 2x2 sea blocks
                    if (ViolationsCheckScript.checkForSeaSquaresVisited(i, j, visitedMat,out squareVisited) == false)
                    {
                        //add child to node and on stack
                        currentNode.children[dir] = childNode;
                        visitedMat[i, j] = label;
                        nodes.Push(childNode);

                        //check if new sea box is touching any sea block belonging to different sea group
                        int[] values = AddBlockScript.getAllNeighbourValuesStraightLines(i, j);
                        bool isInSameGroupTop = false;
                        bool isInSameGroupRight = false;
                        bool isInSameGroupDown = false;
                        bool isInSameGroupLeft = false;

                        //if the reached group is part of the same sea group
                        foreach (Node node in nodes)
                        {
                            //up
                            if (node.position == newPos + new Vector2(SEA, 0))
                            {
                                isInSameGroupTop = true;
                                continue;
                            }
                            //right
                            if (node.position == newPos + new Vector2(0, 1))
                            {
                                isInSameGroupRight = true;
                                continue;
                            }
                            //down
                            if (node.position == newPos + new Vector2(1, 0))
                            {
                                isInSameGroupDown = true;
                                continue;
                            }
                            //left
                            if (node.position == newPos + new Vector2(0, SEA))
                            {
                                isInSameGroupLeft = true;
                                continue;
                            }
                        }
                        //if any neighbour is sea box and that box is not part of the same group and group is max group
                        if (values[0] == SEA && isInSameGroupTop == false)
                        {
                            int groupTo = groupsMat[(int)(newPos.x - 1), (int)newPos.y];
                            if (groupTo == maxGroupIndex+1)
                                return nodes;

                        }
                        else if (values[1] == SEA && isInSameGroupRight == false)
                        {
                            int groupTo = groupsMat[(int)newPos.x, (int)(newPos.y + 1)];
                            if (groupTo == maxGroupIndex+1)
                                return nodes;
                        }
                        else if (values[2] == SEA && isInSameGroupDown == false)
                        {
                            int groupTo = groupsMat[(int)(newPos.x + 1), (int)newPos.y];
                            if (groupTo == maxGroupIndex+1)
                                return nodes;
                        }
                        else if (values[3] == SEA && isInSameGroupLeft == false)
                        {
                            int groupTo = groupsMat[(int)newPos.x, (int)(newPos.y - 1)];
                            if (groupTo == maxGroupIndex+1)
                                return nodes;
                        }

                    }
                    //if any added sea box violates 2x2 sea block limitation with another sea block
                    if (squareWithVisitedEncountered == false)
                    {
                        if (squareVisited)
                        {
                            squareWithVisitedEncountered = true;
                        }
                    }
                }
            }
        }
        return null;
    }
}
