using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;
using Unity.VisualScripting;


public class SkillTree : MonoBehaviour
{
    public static SkillTree instance;

    [Header("Scriptable Objects")]
    // public List<Skill> modifiers;
    //public List<Skill> spells;
    //public List<Skill> augments;
    public List<Skill> skills; // All Skills (Modifiers, Spells, Augments)

    public Dictionary<int, List<Skill>> tieredSkills;

    [Header("Alg Properties")]
    public float primaryChance;
    public float secondaryChance;
    public float counterWeight;

    public float offsetX = 50; // Shifts the tree on x axis
    private float radius;
    private Image[,] skillImagesArr;

    private SkillData nullRoot;
    private SkillData[,] nodeTree; // Random Walker Alg
    public Skill[,] skillTree; // Scriptable Objects

    private Player player;
    private GameCanvas gameCanvas;

    

    /*
    private void CreateSkillTree()
    {
        ResetTree();
        RandomizeTree();
        // ShiftTree(Random.Range(0, 15));
        CleanTree();
    }
    */

    private void SafeCreateSkillTree()
    {
        ResetTree();
        SafeRandomizeTree();
        // ShiftTree(Random.Range(0, 15));
    }

    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("Skill Tree instance already exists");
            return;
        }
        instance = this;
        Debug.Log("Skill Tree Initialized");
    }

    /// <summary>
    /// Should be called at the start of any run
    /// </summary>
    public void LoadSkillTree()
    {
        Debug.Log("Loading Skill Tree");
        gameCanvas = GameManager.instance.GameCanvas;
        skillImagesArr = new Image[3, 16];
        SetUpPossibleSkills();
        SafeCreateSkillTree();
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.R))
            // CreateSkillTree();

        /* Debugging Tests
        if (Input.GetKeyDown(KeyCode.T))
            SafeCreateSkillTree();

        if (Input.GetKeyDown(KeyCode.C))
            UnlockAllNodes();
        */
    }

    #region UI
    

    private void UnlockAllNodes()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                if (skillTree[i, j] != null)
                    skillTree[i, j].OverrideUnlock();
            }
        }
    }
    #endregion


    private void ResetTree()
    {
        if (nodeTree == null)
            return;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                if (nodeTree[i, j] != null)
                {
                    Destroy(skillImagesArr[i, j]);
                    nodeTree[i, j] = null;
                    skillTree[i, j] = null;
                }

                if (skillTree[i, j] != null)
                {
                    skillTree[i, j] = null;
                }
            }
        }

        nodeTree = new SkillData[3,16];
        skillTree = new Skill[3, 16];
        skillImagesArr = new Image[3, 16];
        nullRoot = null;

        SetUpPossibleSkills();
    }

    /// <summary>
    /// THIS DOESNT WORK CURRENTLY
    /// only shifts the nodeTree, not the skillTree
    /// </summary>
    /// <param name="shiftValue">Number of nodes to shiftdownward</param>
    private void ShiftTree(int shiftValue)
    {
        for (int i = 0; i < nodeTree.GetLength(0); i++)  // Loop through each ring
        {
            SkillData[] tempRow = new SkillData[nodeTree.GetLength(1)];  // Temporary storage for the shifted row

            for (int j = 0; j < nodeTree.GetLength(1); j++)  // Loop through each node in the ring
            {
                int newIndex = (j + shiftValue) % nodeTree.GetLength(1);  // Calculate new index with wrap-around
                if (newIndex < 0) newIndex += nodeTree.GetLength(1);  // Adjust for negative indices
                tempRow[newIndex] = nodeTree[i, j];  // Place node in new position
            }

            for (int j = 0; j < nodeTree.GetLength(1); j++)  // Copy the shifted nodes back into the main array
            {
                nodeTree[i, j] = tempRow[j];
            }
        }
    }

    /* Unsafe Randomize Tree
    private void RandomizeTree()
    {
        Color[] colorBranches = new Color[5] { Color.red, Color.green, Color.blue, Color.yellow, new Color(1.0f, 0.65f, 0.0f) };

        radius = 60;
        nodeTree = new SkillData[3, 16];

        DrawLines();


        // Pinpoint
        SkillData nullRoot = new SkillData(skill, null, Color.white, new Vector2(-1, -1));



        int count = 0;

        // *********************************************************************** * Branch count * **************
        int r = Random.Range(4, 6);
        for (int i = 0; i < r; i++)
        {
            SkillData root = nodeTree[0, CircleValue(count)];

            // Calculates the spacing for the next branch
            int rnd = Random.Range(1, 5);
            switch (rnd)
            {
                case 1:
                    count += 3;
                    break;
                case 2:
                    count += 5;
                    break;
                case 3:
                case 4:
                    count += 4;
                    break;
            }


            // Node root = nodeTree[0, i * 4];     // (1)
            root.depth = 0;
            root.prev.Add(nullRoot);
            root.Color = colorBranches[i];
            nullRoot.next.Add(root);

            // *********************************************************************** * Node Count * **************
            int ran = Random.Range(4, 7); // 4-6 Randomized Nodes
            CreateBranch(root, ran, colorBranches[i]);



            DrawNodes();
        }

    }
    */

    private void SafeRandomizeTree()
    {
        Color[] colorBranches = new Color[5] { Color.red, Color.green, Color.blue, Color.yellow, new Color(1.0f, 0.65f, 0.0f) };

        radius = 100;
        nodeTree = new SkillData[3, 16];

        DrawLines();


        // Pinpoint
        nullRoot = new SkillData(new Vector2(-1, -1), Color.white);

        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            SkillData root = nodeTree[0, CircleValue(count)] = new SkillData( new Vector2(0, CircleValue(count)), colorBranches[i]);

            // Calculates the spacing for the next branch
            int rnd = Random.Range(1, 5);
            switch (rnd)
            {
                case 1:
                    count += 3;
                    break;
                case 2:
                    count += 5;
                    break;
                case 3:
                case 4:
                    count += 4;
                    break;
            }


            // Node root = nodeTree[0, i * 4];     // (1)
            root.depth = 0;
            root.prev.Add(nullRoot);
            // root.Color = colorBranches[i];
            nullRoot.next.Add(root);


            CreateBranch(root, 5, colorBranches[i]);
        }

        DrawNodes();

    }


    private SkillData CreateBranch(SkillData parentNode, int nodeCount, Color color)
    {
        int depth = parentNode.depth + 1;
        int totalNodes = nodeCount;

        while (depth < totalNodes)
        {
            float weight = 0;
            var locations = (parentNode.prev[0].depth == -1) ? SmallWalk(parentNode) : LargeWalk(parentNode, weight);

            // If Walking came up with no possible locations
            if (locations.Item1 == null || locations.Item1.Count == 0)
            {
                // If we've reached the end of the tree, return null. Tree cant be formed
                if (parentNode.prev[0].depth == -1)
                {
                    break; // TODO: This will return null in the future, telling its method, a tree cannot be formed
                           //currentNode = stack.Peek();
                }
                else
                {
                    // Go back to the previous node
                    parentNode = parentNode.prev[0];
                    depth--;
                    continue;
                }
            }

            var result = GetNextNode(parentNode, locations.Item1, locations.Item2, color);
            parentNode = result.Item1;
            weight = result.Item2;
            parentNode.depth = depth;
            depth++;
        }

        return parentNode;
    }


    /// <summary>
    /// Calulates the next node from a ring 0 position
    /// Possible Walks: Left Middle Right
    /// </summary>
    /// <param name="currentNode">This trees parent node</param>
    /// <returns>List of Vector2 int pairs of possible indicie and chances</returns>
    private (List<(Vector2, float)>, float) SmallWalk(SkillData currentNode)
    {
        if (currentNode == null)
        {
            Debug.Log("Error: No nodes next.");
            return (null, 0);
        }

        List<(Vector2 idx, float value)> nextIdxValues = new List<(Vector2 idx, float value)>();
        float totalValue = 0;
        for (int i = -1; i < 2; i++)
        {
            if (nodeTree[1, CircleValue((int)currentNode.Index.y + i)] == null)
            {
                if (i == 0)
                {
                    nextIdxValues.Add((new Vector2(1, i), primaryChance));
                    totalValue += primaryChance;
                }
                else
                {
                    nextIdxValues.Add((new Vector2(1, i), secondaryChance));
                    totalValue += secondaryChance;
                }
                /* First Tree Output
                 * nextIdxValue = ([1, -1], 1)   ([1, 1], 1)  ([1, 0], 5)
                 *                            ex.  1,2,7
                 *  This is the output since no existing nodes
                 */
            }
        }

        return (nextIdxValues, totalValue);
    }


    /// <summary>
    /// Calculates Possible Next Node Locations.
    /// Large Walk Includes(rng/total): Cardinal Dirs(1), Same Ring Skipper(1), and Diagonal Dirs(5)
    /// </summary>
    /// <param name="currentNode">Node Alg is currently on</param>
    /// <returns>List of Vector2 Int Value Pairs</returns>
    private (List<(Vector2, float)>, float) LargeWalk(SkillData currentNode, float addWeight = 0)
    {
        if (currentNode == null)
        {
            Debug.Log("Error: No nodes next. THIS IS BAD. Node #");
            return (null, 0);
        }
        else if (currentNode.Index.x == 0)
        {
            Debug.Log("Error: Can't Large Jump from Ring 0");
            return (null, 0);
        }

        List<(Vector2 idx, float value)> nextIdxValues = new List<(Vector2 idx, float value)>();
        float totalValue = 0;

        // -2, -1, 0, 1, 2
        for (int i = -2; i <= 2; i++) // Same Level as itself
        {
            // If weve already checked this node for a potential path, skip it
            foreach (var badIndex in currentNode.badIndicies)
            {
                if (badIndex == new Vector2(currentNode.Index.x, CircleValue(i + (int)currentNode.Index.y)))
                    continue;
            }

            //    3 or 0      3      -1, -0.5, 0, 0.5, 1
            float weight = counterWeight * addWeight * Mathf.Clamp(i, -1, 1);

            // If there is a black node in the current spot were checking
            if (nodeTree[(int)currentNode.Index.x, CircleValue(i + (int)currentNode.Index.y)] == null)
            {
                if (Math.Abs(i) == 2) // (skipper)
                {
                    // If there is a black node in the spot we are skipping, dont skip it          (i.e. -1 or 1)
                    if (nodeTree[(int)currentNode.Index.x, CircleValue((int)currentNode.Index.y + InnerNeighbor(i))] != null)
                        continue;

                    // If there are already 2 nodes horizontally, skip this node
                    if (nodeTree[(int)currentNode.Index.x, CircleValue((int)currentNode.Index.y + -i)] != null
                        || nodeTree[(int)currentNode.Index.x, CircleValue((int)currentNode.Index.y + -InnerNeighbor(i))]!= null)
                        continue;

                    //                         Same Ring as the Current Node
                    nextIdxValues.Add((new Vector2(currentNode.Index.x, i), Mathf.Max(secondaryChance + weight, 0)));
                    totalValue += secondaryChance + weight;
                }
                else if (i == 0) // Itself
                {
                    continue;
                }
                else // Left or Right
                {
                    // If there are already 2 nodes horizontally, skip this node
                    if (nodeTree[(int)currentNode.Index.x, CircleValue((int)currentNode.Index.y + -i)] != null)
                        continue;


                    nextIdxValues.Add((new Vector2(currentNode.Index.x, i), Mathf.Max(primaryChance + weight, 0)));
                    totalValue += primaryChance + weight;
                }
            }

        }

        // -1 , 0, 1
        for (int i = -1; i < 2; i++) // Other Ring
        {
            // If weve already checked this node for a potential path, skip it
            foreach (var badIndex in currentNode.badIndicies)
            {
                if (badIndex == new Vector2(currentNode.Index.x, CircleValue(i + (int)currentNode.Index.y)))
                    continue;
            }

            float weight = counterWeight * addWeight * Mathf.Clamp(i, -1, 1);

            if (nodeTree[3 - (int)currentNode.Index.x, CircleValue(i + (int)currentNode.Index.y)] == null)
            {
                if (i == 0) // Up
                {
                    //                           * 3 - current.X Makes 1 = 2, and 2 = 1 *
                    nextIdxValues.Add((new Vector2(3 - currentNode.Index.x, i), Mathf.Max(primaryChance + weight, 0)));
                    totalValue += Mathf.Max(primaryChance + weight, 0);
                }
                else // Diagonal Left or Right
                {
                    nextIdxValues.Add((new Vector2(3 - currentNode.Index.x, i), Mathf.Max(secondaryChance + weight, 0)));
                    totalValue += Mathf.Max(secondaryChance + weight, 0);
                }
            }
        }

        // IF there are no open spaces to walk to
        if (nextIdxValues.Count == 0)
        {
            if (currentNode.prev.Count >= 2)
            {
                Debug.Log("This is a prevention for now. " +
                    "If your seeing this, its the future, and nodes can now have 2 previous nodes. " +
                    "Need to update this check if its the case");
                return (null, 0);
            }
            else
            {
                currentNode.prev[0].badIndicies.Add(currentNode.Index);
                return (null, 0);
            }
        }

        // MUST INTAKE -2 OR 2
        int InnerNeighbor(int i)
        {
            if (i == -2)
                return -1;
            if (i == 2)
                return 1;
            else
                return 0;
        }

        return (nextIdxValues, totalValue);
    }

    /// <summary>
    /// Picks a location for the next node
    /// </summary>
    /// <param name="currentNode">Current Node Alg is on</param>
    /// <param name="nextIV">List of possible next indicies + thier value chance</param>
    /// <param name="totalValue">Total value of next indicies</param>
    /// <returns>The node that it picked</returns>
    private (SkillData next, float weight) GetNextNode(
        SkillData currentNode, List<(Vector2 idx, float value)> nextIV, float totalValue, Color color)
    {
        float addWeight = 0;  // 0 = No Weight, 1 = Right Handed Weight. -1 = Left Handed Weight

        if (nextIV.Count == 0)
        {
            Debug.Log("Error: No nodes next.");
            return (null, 0.0f);
        }

        ProceduralGeneration.Shuffle(nextIV);

        float count = 0;
        float randomValue = Random.Range(1.0f, totalValue);

        for (int j = 0; j < nextIV.Count; j++)
        {
            if (randomValue >= count && randomValue <= count + nextIV[j].value)
            {
                // Get next node
                Vector2 nextIdx = new Vector2((int)nextIV[j].idx.x, CircleValue((int)(currentNode.Index.y + (int)nextIV[j].idx.y)));
                SkillData nextNode = nodeTree[(int)nextIdx.x, (int)nextIdx.y] = new SkillData(nextIdx, color);

                /* 
                 * IF You need to write code that runs based of the 
                 * previous direction, write the code here to determne what that is
                 */

                // What was the chosen node?
                if (nextNode.Index.x == currentNode.Index.x) // Same Ring/Level
                {
                    if (nextNode.Index.y + 2 == currentNode.Index.y) // Right Skipper
                    {
                        addWeight = -1.0f;
                    }
                    else if (nextNode.Index.y + 1 == currentNode.Index.y) // Right Neighbor
                    {
                        addWeight = -0.5f;
                    }
                    else if (nextNode.Index.y - 1 == currentNode.Index.y) // Left Neighbor
                    {
                        addWeight = 0.5f;
                    }
                    else if (nextNode.Index.y - 2 == currentNode.Index.y) // Left Skipper
                    {
                        addWeight = 1.0f;
                    }
                }


                // Change refrences
                currentNode.next.Add(nextNode);
                nextNode.prev.Add(currentNode);
                currentNode = nextNode;
                return (currentNode, addWeight); // Should always return something
            }
            else
            {
                count += nextIV[j].value;
            }
        }
        Debug.Log("Error: No node found. Skill Tree Line 545");
        return (null, addWeight);
    }

    private void SetUpPossibleSkills()
    {
        tieredSkills = new Dictionary<int, List<Skill>>();

        foreach (Skill modifier in skills)
        {
            if (tieredSkills.ContainsKey(modifier.tier))
            {
                tieredSkills[modifier.tier].Add(modifier);
            }
            else
            {
                tieredSkills.Add(modifier.tier, new List<Skill> { modifier });
            }
        }
    }

    /// <summary>
    /// Draws the tree on the UI.
    /// </summary>
    public void DrawLines()
    {
        Transform lineParent = new GameObject("Lines").transform;
        lineParent.SetParent(gameCanvas.treeParent, false);

  
        for (int j = 0; j < 16; j++)
        {
            CreateLine(j, radius, lineParent);
        }
    }

    Transform nodeParent = null;
    /// <summary>
    /// Draws each node on the UI after the algorithim has been ran. (No longer before)
    /// </summary>
    public void DrawNodes()
    {
        skillTree = new Skill[3, 16];

        if (player == null)
            player = GameManager.instance.players[0].GetComponent<Player>();

        if (nodeParent != null) // If nodes already exist, destroy them
            Destroy(nodeParent.gameObject);

        nodeParent = new GameObject("Nodes").transform;
        nodeParent.SetParent(gameCanvas.treeParent, false);

        for (int i = 0; i < 3; i++)
        {
            float currentRadius = radius * ((i + 1) / 3.0f); // Calculate radius for each row
            for (int j = 0; j < 16; j++)
            {
                if (nodeTree[i, j] == null)
                    continue;

                float angle = (Mathf.PI * 2 / 16) * j;
                float x = Mathf.Cos(angle) * currentRadius - offsetX;
                float y = Mathf.Sin(angle) * currentRadius;

                Skill rndSkill = Instantiate(PickSkill(i + 1)); // Random Skill

                // Creates the image on UI and sets pos
                Image skillImg = Instantiate(rndSkill.iconImg, nodeParent);

                ClickableImageUI mouseEventHandler = skillImg.GetComponent<ClickableImageUI>();
                mouseEventHandler.SubscribeToOnClick(rndSkill.HandleClick);
                mouseEventHandler.SubscribeToOnHover(rndSkill.OnPointerEnter);
                mouseEventHandler.SubscribeToOnHoverExit(rndSkill.OnPointerExit);

                skillImg.color = Color.black;
                rndSkill.SetUpSkill(nodeTree[i,j], skillImg, player, gameCanvas);

                skillImg.rectTransform.anchoredPosition = new Vector2(x, y);
                skillImg.rectTransform.localScale = Vector3.one * 2;

                skillImg.transform.SetSiblingIndex(1); // Index lines before nodes
                

                skillImagesArr[i,j] = skillImg; // Set Refrences
                skillTree[i, j] = rndSkill; // Set Refrences
                nodeTree[i, j].SetSkill(rndSkill); // Set Refrences
            }
        }
    }

    // Get a random skill based off it's tier
    public Skill PickSkill(int tier)
    {
        return tieredSkills[tier][Random.Range(0, tieredSkills[tier].Count)];
    }


    /// <summary>
    /// Draws the lines between the nodes
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="radius">Length of the lines</param>
    /// <param name="parent"></param>
    private void CreateLine(int j, float radius, Transform parent)
    {
        float angle = (Mathf.PI * 2 / 16) * j;
        GameObject line = new GameObject("Line_" + j);
        line.transform.SetParent(parent, false);
        Image lineImg = line.AddComponent<Image>();
        lineImg.color = Color.black;
        lineImg.rectTransform.sizeDelta = new Vector2(1.5f, radius * 2);
        line.transform.localPosition = new Vector3(-offsetX, 0, 0);
        line.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90);
    }

    /// <summary>
    /// Wraps the value around the circle (16)
    /// </summary>
    /// <param name="index">Current Index</param>
    /// <param name="length">Wrap around value</param>
    /// <returns>Index 0-15</returns>
    private int CircleValue(int index, int length = 16) { return (index + length) % length; }
}
