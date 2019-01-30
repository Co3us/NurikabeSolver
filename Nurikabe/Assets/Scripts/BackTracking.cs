/*
BackTracking.cs
Goes through all possible combinations of islands and backtracks whenever a violation occurs.
*/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BackTracking : MonoBehaviour
{
    //const values
    const int UNKNOWN = StaticVars.UNKNOWN;

    //helper class that stores island guesses and states before the guess
    public class IslandGuess
    {
        public Island island;
        public byte[] permutationArray;
        public State state;
    }
    //helper class to save state of nurikabe field
    public class State
    {
        public int[,] boxesSave;
        public List<SeaGroup> seaGroupsBeforeSave;
        public int[,] labelMatSave;
        public int maxLabelSave;
    }

    //setting this to true starts process
    bool isGuessing = false;
    //is problem solved
    bool solved = false;
    //enable step by step solving (user has to click next for each step)
    public bool stepByStep = false;

    //script refrences
    IslandCombinations ICScript;
    ViolationsCheck ViolationsCheckScript;
    AddBlock AddBlockScript;

    //size of unsolvedIslands in prev iteration
    int prevIterationSize;

    //current binary array for island combination
    byte[] currentPermutation;

    //stacks for islands and island guesses
    Stack<Island> unsolvedIslands;
    Stack pickedIslands = new Stack();

    public void init()
    {
        //get refrences
        ICScript = GetComponent<IslandCombinations>();
        AddBlockScript = GetComponent<AddBlock>();
        ViolationsCheckScript = GetComponent<ViolationsCheck>();

        //copy unsolved islands list to local stack
        unsolvedIslands = new Stack<Island>(StaticVars.unsolvedIslands);

        //init size of stack
        prevIterationSize = unsolvedIslands.Count();

        //set guessing to start
        isGuessing = true;

    }
    void Update()
    {
        if (isGuessing)
        {
            //get island from stack
            Island currentIsland = unsolvedIslands.Peek();

            //we store number of all nodes per level and unique nodes per level in tree of island combinations
            byte[] numOfNodesPerLevel;
            byte[] uniqueNodesPerLevel;

            //generate tree
            IslandCombinations.PathNode tree = ICScript.generateInitalTree(currentIsland, out numOfNodesPerLevel, out uniqueNodesPerLevel, false);


            //calculate the minimum nubers of level we need to go to if we want to get the full island
            byte sum = 1;
            byte sumUnique = 1;
            int minLevelsNeeded = 1;
            bool minLevelsSet = false;
            int Islandsize = currentIsland.size;
            for (int i = 1; i < numOfNodesPerLevel.Length; i++)
            {
                sum += numOfNodesPerLevel[i];
                sumUnique += uniqueNodesPerLevel[i];

                //if total of unique nodes is at least size needed  
                if (sumUnique >= Islandsize && minLevelsSet == false)
                {
                    minLevelsNeeded = i;
                    minLevelsSet = true;
                }
            }

            //binary array for island combinations
            byte[] permutation = new byte[sum];


            //checks if permutation increment fails from the start (before even calling pick island)
            bool initialIncrementFailed = false;

            //if we have more items in unsolvedIslands than in prev iteration, that means we came back
            //that means we need to continue from the next permutiation not start at the begining
            if (unsolvedIslands.Count > prevIterationSize)
            {
                //permutation is set through global var that stores the binary array of the last accepted island combination
                permutation = currentPermutation;

                //increment array
                permutation = ICScript.incrementPermutation(permutation);

                //if permutation is null no further permutations are possible
                if (permutation == null)
                {
                    initialIncrementFailed = true;
                }
            }
            //if we have less islands than in prev iteration
            else
            {
                //init permutation with '1' for size of island
                for (int i = 0; i < Islandsize; i++)
                {
                    permutation[i] = 1;
                }
            }

            //store the current size of unsolved islands in global var
            prevIterationSize = unsolvedIslands.Count();

            //checks if permutation increment fails when picking next island
            bool incrementFailed = false;

            //pick islands until no violation or increment of permutation fails
            while (initialIncrementFailed == false)
            {
                Stack locations = ICScript.pickIsland(tree, permutation, currentIsland, numOfNodesPerLevel, minLevelsNeeded);

                //if loactions returned null no further permutations are possible
                if (locations.Peek() == null)
                {
                    incrementFailed = true;
                    break;
                }
                //if loactions returned 0: permutation is not valid and we try next one
                //otherwise check island for violation
                if (locations.Peek().GetType() != typeof(int))
                {
                    //save previous state
                    State savedState = saveState();
                    Stack stateStack = new Stack();
                    stateStack.Push(savedState);

                    //local copy of stack
                    Stack locCopy = new Stack();

                    //we add island boxes first 
                    while (locations.Count > 0)
                    {
                        Vector2 loc = (Vector2)locations.Pop();
                        locCopy.Push(loc);
                        int i = (int)loc.x;
                        int j = (int)loc.y;
                        AddBlockScript.addLandBlock(i, j);
                    }
                    //add the root island box location
                    locCopy.Push(currentIsland.center);

                    //surround island boxes with sea boxes and save added locations
                    Stack seaLoc = AddBlockScript.surroundWithSea(locCopy);

                    //checks for violation of nurikabe rules 
                    bool isViolation = false;

                    //checks for sea blocks size 2x2
                    isViolation = ViolationsCheckScript.checkForSeaSquares(seaLoc);
                    if (isViolation == false)
                    {
                        //checks if any sea group has no option to expand
                        isViolation = ViolationsCheckScript.checkForSeaWithNoOption();
                    }
                    if (isViolation == false)
                    {
                        //checks if any of unsolved islands has no possible combinations after new boxes were placed
                        isViolation = ViolationsCheckScript.checkForIslandsWithNoOptions(currentIsland, new Stack(unsolvedIslands));
                    }

                    //if the path between sea group found
                    bool pathNotFound = false;

                    if (isViolation == false)
                    {
                        //save state
                        savedState = saveState();
                        stateStack.Push(savedState);

                        //try to quickly find a path between each sea group
                        isViolation = ViolationsCheckScript.checkIfSeaPathImpossibleQuickCheck();

                        //if violation path isn't found 
                        pathNotFound = isViolation;

                        //restore state
                        revertState(stateStack);
                        recolor();
                    }

                    //if the path is not found (violation = true) we check all the other options to see if there are any possible paths
                    if (pathNotFound == true)
                    {
                        isViolation = ViolationsCheckScript.checkIfSeaPathImpossible();
                    }

                    //if there was no violation we can accept this island as solved
                    if (isViolation == false)
                    {
                        //create new island guess and fill it
                        savedState = (State)stateStack.Pop();
                        IslandGuess islandGuess = new IslandGuess();
                        islandGuess.island = currentIsland;
                        islandGuess.permutationArray = permutation;
                        islandGuess.state = savedState;

                        //push guess on memory stack and pop working stack (so we move on to the next unsolved island)
                        pickedIslands.Push(islandGuess);
                        unsolvedIslands.Pop();
                        break;
                    }
                    //there was a violation
                    else
                    {
                        //restore state before island pick
                        revertState(stateStack);
                        recolor();
                    }
                }
                //if there was violation or permutation was not valid we increment permutation
                permutation = ICScript.incrementPermutation(permutation);

                //if permutation is null no further permutations are possible
                if (permutation == null)
                {
                    incrementFailed = true;
                    break;
                }
            }
            //if increment fails there is no possible correct option for 
            //island in current state so we go back to previous state and previous island
            if (incrementFailed || initialIncrementFailed)
            {
                revertStateFromGuess();
            }
            //if there are no more unsolved islands
            if (unsolvedIslands.Count == 0)
            {
                isGuessing = false;
                solved = true;
                //fill the rest of unknown fields with sea boxes
                for (int i = 0; i < StaticVars.numOfRows; i++)
                {
                    for (int j = 0; j < StaticVars.numOfCols; j++)
                    {
                        if (StaticVars.boxesValues[i, j] == UNKNOWN)
                        {
                            if (ViolationsCheckScript.checkForSeaSquares(i, j) == false)
                            {
                                AddBlockScript.addSeaBlock(i, j);
                            }
                            //if any of these violate the 2x2 sea block rule we have to revert the state again
                            else
                            {
                                revertStateFromGuess();
                                prevIterationSize = 0;
                                solved = false;
                                isGuessing = true;
                                break;
                            }
                        }
                    }
                }
            }
            //if step by step is on user has to call next() for each step
            if (stepByStep == true)
            {
                isGuessing = false;
            }
        }
    }
    //save state of field
    public State saveState()
    {
        State saveState = new State();
        saveState.boxesSave = (int[,])StaticVars.boxesValues.Clone();
        List<SeaGroup> seaGroupsBefore = new List<SeaGroup>();
        cloneList(seaGroupsBefore);
        saveState.seaGroupsBeforeSave = seaGroupsBefore;
        saveState.labelMatSave = (int[,])StaticVars.seaGroupLabelMatrix.Clone();
        saveState.maxLabelSave = StaticVars.maxSeaGroupLabel;
        return saveState;
    }
    //revert to previous state
    public void revertState(Stack stateStack)
    {
        State savedState = (State)stateStack.Pop();
        StaticVars.boxesValues = savedState.boxesSave;
        StaticVars.seaGroups = savedState.seaGroupsBeforeSave;
        StaticVars.seaGroupLabelMatrix = savedState.labelMatSave;
        StaticVars.maxSeaGroupLabel = savedState.maxLabelSave;
    }
    //clones sea group list
    public void cloneList(List<SeaGroup> seaGroups)
    {
        foreach (SeaGroup sg in StaticVars.seaGroups)
        {
            seaGroups.Add(new SeaGroup(sg.locations, sg.label));
        }
    }
    //reverts state to before island guess
    public void revertStateFromGuess()
    {
        //take the last valid island guess from stack 
        IslandGuess islandGuess = (IslandGuess)pickedIslands.Pop();

        //push the island back to unsolved islands stack
        unsolvedIslands.Push(islandGuess.island);

        //restore state from before the guess
        StaticVars.boxesValues = islandGuess.state.boxesSave;
        StaticVars.seaGroups = islandGuess.state.seaGroupsBeforeSave;
        StaticVars.seaGroupLabelMatrix = islandGuess.state.labelMatSave;
        StaticVars.maxSeaGroupLabel = islandGuess.state.maxLabelSave;
        currentPermutation = islandGuess.permutationArray;
        recolor();
    }

    //for user interaction (step by step mode)
    public void next()
    {
        if (solved == false)
            isGuessing = true;
    }

    //recolor the squares that are unknown to white
    public void recolor()
    {

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



}
