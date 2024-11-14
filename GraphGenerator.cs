using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphGenerator
{
    private readonly int[,] nodeChart = new int[5, 3] {
        { 3, 3, 4 },
        { 4, 5, 6 },
        { 5, 6, 7 },
        { 6, 7, 8 },
        { 5, 6, 7 }
    };

    private readonly int[,] hubChart = new int[5, 3] {
        { 1, 1, 2 },
        { 2, 2, 3 },
        { 2, 3, 4 },
        { 2, 3, 4 },
        { 3, 3, 3 },
    };


    // public GraphGenerator()
    // {
    //     // Constructor
    // }

    /// <summary>
    /// Generates and compiles all the pieces that put together the entire dungeon graph
    /// </summary>
    /// <param name="lvlNum">Lvl to gen</param>
    /// <returns>The head node of the graph (start room)</returns>
    public GraphNode GenerateGraph(int lvlNum)
    {
        // Generate the graph

        List<GraphNode> hubs = new List<GraphNode>();
        float weight = 0;

        #region Determine Room Counts
        // Grab a random number of normal rooms based on lvl num
        int flex = 1;
        float ran = Random.Range(1f, 100f);
        if (ran < 12.5)
        {
            flex = 0;
            weight += 10f; // Increases the chance of extra rooms for the future
        }
        else if (ran < 25)
        { 
            flex = 2;
            weight -= 5f; // Decreases the chance of extra rooms for the future
        }

        int normalCount = nodeChart[lvlNum - 1, flex];

        // Grab a random number of hub rooms based on lvl num.
        flex = 1;
        ran = Random.Range(1f, 100f);
        if (ran < 12.5)
            flex = 0;
        else if (ran < 25)
            flex = 2;

        int hubCount = hubChart[lvlNum - 1, flex];
        #endregion

        #region Generate Graph Pieces
        // Generate the different room pieces to the graph
        GraphNode startConnect = new GraphNode(RoomType.Start);
        (GraphNode normalConnect, GraphNode hubConnect)[] criticalPath = GenerateCriticalPathPieces(hubCount, normalCount, ref weight);
        GraphNode endConnect = GenerateEndPiece(ref weight);
        List<GraphNode> rewardConnect = GenerateRewardPieces(lvlNum);
        #endregion

        #region Attach Graph Pieces Together
        // List to keep track of used incidies when randomly connecting pieces.
        // Prevents using the same piece twice
        List<int> usedIndexes = new List<int>();

        // Connect the critical path pieces together (start, crit path, end)
        GraphNode currentNode = startConnect;
        foreach((GraphNode normalConnect, GraphNode hubConnect) in criticalPath)
        {
            // Make sure we always get a new index
            int ranIndex;
            do {ranIndex = Random.Range(0, criticalPath.Length);} 
            while (usedIndexes.Contains(ranIndex));

            // Connect the new piece to the graph
            currentNode.Connect(criticalPath[ranIndex].normalConnect);

            currentNode = criticalPath[ranIndex].hubConnect;
            hubs.Add(currentNode);
            usedIndexes.Add(ranIndex);
        }
        currentNode.Connect(endConnect);


        // Attach the reward rooms to random hubs
        usedIndexes.Clear();
        foreach(GraphNode connect in rewardConnect)
        {
            int ranIndex;
            do { ranIndex = Random.Range(0, hubs.Count);} 
            while (usedIndexes.Contains(ranIndex));

            hubs[ranIndex].Connect(connect);
            usedIndexes.Add(ranIndex);
        }
        #endregion

        // Assigns depth to each node in the graph starting at the head
        startConnect.AssignDepth();
        Debug.Log("Graph Generated");
        return startConnect;
    }

    /// <summary>
    /// Generates two linked lists of nodes, 
    /// each representing the start and end pieces of the dungeon
    /// </summary>
    /// <returns>The two connection points for the start and end piece respectivly</returns>
    private GraphNode GenerateEndPiece(ref float weight) 
    {
        // There will always be atleast one normal before the boss
        GraphNode endConnect = new GraphNode(RoomType.Normal);
        endConnect.Connect(new GraphNode(RoomType.Boss));

        // Base 50% to add extra room to the boss string
        float ran = Random.Range(1f, 100f);
        if(ran < 50 + (weight * 2))
        {
            GraphNode temp = new GraphNode(RoomType.Normal);
            endConnect.ConnectPrev(temp);
            endConnect = temp;
            weight -= 10;
        }

        // Return the string of rooms
        return endConnect;
    }

    /// <summary>
    /// Generates the additional room strings that lead to the boss fight
    /// </summary>
    /// <returns>The connection points of each extra critical path piece</returns>
    private (GraphNode normalConnect, GraphNode hubConnect)[] GenerateCriticalPathPieces(int pieceCount, int nodeCount, ref float weight)
    {
        // Correction Checks
        if (nodeCount <= pieceCount) 
        { 
            Debug.LogError("Node count must be greater than piece count");
            return null; 
        }
        else if((nodeCount - 1) == pieceCount)
        {
            // If there is one more nodes than pieces, decrease the amount of pieces (makes longer strings)
            pieceCount--;
        }

        (GraphNode normalConnect, GraphNode hubConnect)[] connectPoints = new (GraphNode normalConnect, GraphNode hubConnect)[pieceCount];

        // Creates a hub room for each piece and then adds one normal room before each hub
        for (int i = 0; i < pieceCount; i++) { 
            connectPoints[i].hubConnect = new GraphNode(RoomType.Hub);
            connectPoints[i].normalConnect = new GraphNode(RoomType.Normal);
            connectPoints[i].hubConnect.ConnectPrev(connectPoints[i].normalConnect);
            nodeCount--;

            /*    N  (normalConnect)
             *    | 
             *    H  (hubConnect)
             */
        }

        // Randomly disperse the remaining normal rooms to the different pieces
        while(nodeCount > 0)
        {
            GraphNode temp = new GraphNode(RoomType.Normal); // Creatre a new normal room
            int ran = Random.Range(0, pieceCount); // Randomly select a piece to connect to
            connectPoints[ran].normalConnect.ConnectPrev(temp); // Connect it at the begining
            connectPoints[ran].normalConnect = temp; // Make the new room the new refrence point
            nodeCount--; // Decrease total node Count
        }

        // With a set chance, this will add a dead end room to the end of a HUB piece
        float rand = Random.Range(1f, 100f);
        if(rand < 16 + weight) // (15%)
        {
            int ran = Random.Range(0, pieceCount);
            connectPoints[ran].hubConnect.Connect(new GraphNode(RoomType.Normal));
            weight -= 10f;
        }
        else
            weight += 5f;
        
        return connectPoints;
    }

    /// <summary>
    /// Generates 1-3 linked lists of nodes that represent reward room strings
    /// </summary>
    /// <param name="lvlNum">Current level number determines number of normal nodes</param>
    /// <returns>The connection point of each piece</returns>
    private List<GraphNode> GenerateRewardPieces(int lvlNum)
    {
        List<GraphNode> connectPoints = new List<GraphNode>();
        int nodeCount = lvlNum; // The number of rooms to the reward rooms gets determined by the level number
        if (nodeCount == 1) { nodeCount++; } // If its the first round, still make 2 rooms

        // Form each of the reward room strings (could be done a better way ... this still works tho)
        while(nodeCount > 0)
        {
            GraphNode connectNode;
            if(nodeCount == 1)
            {
                connectNode = new GraphNode(RoomType.Normal);
                new GraphNode(RoomType.Reward).ConnectPrev(connectNode);
                nodeCount--;
            }
            else
            {
                connectNode = new GraphNode(RoomType.Normal);
                new GraphNode(RoomType.Reward).ConnectPrev(connectNode);
                GraphNode temp = new GraphNode(RoomType.Normal);
                connectNode.ConnectPrev(temp);
                connectNode = temp;
                nodeCount -= 2;
            }

            connectPoints.Add(connectNode);
        }
        
        // Shuffle the connection points to randomize the order before returning
        ProceduralGeneration.Shuffle(connectPoints);
        return connectPoints;
    }

    /// <summary>
    /// Generates a string of rooms that lead to an NPC room
    /// </summary>
    /// <param name="currNPC">NPC to generate a piece for</param>
    /// <returns>The head node of the string</returns>
    private GraphNode GenerateNPCPiece(NPC currNPC)
    {
        //          Generates a string of rooms(based on the difficulty of the NPC)
        var connecPoints = GenerateNormalString(GetRandomNPCLength(currNPC.difficulty));
        connecPoints.endConnect.Connect(new NPCNode(currNPC));
        return connecPoints.startConnect;
    }

    /// <summary>
    /// Creates a string of normal rooms
    /// </summary>
    /// <param name="length">Wanted length of the piece</param>
    /// <returns>Item1: Starting connection point of the piece
    ///             Item2: Ending connection point of the piece</returns>
    private (GraphNode startConnect, GraphNode endConnect) GenerateNormalString(int length)
    {
        // Creates the last node of the string. Saves the refrence to return later
        GraphNode current, endConnect;
        current = endConnect = new GraphNode(RoomType.Normal);

        for (int i = 0; i < length - 1; i++)
        {
            GraphNode temp = new GraphNode(RoomType.Normal);
            current.ConnectPrev(temp);
            current = temp;
        }

        return (current, endConnect);
    }

    /// <summary>
    /// Attaches a node into the graph at a specific depth
    /// </summary>
    /// <param name="room">Room to be injected</param>
    /// <param name="headNode">Node in graph to base room placement off of</param>
    private void InjectRoom(SpecialNode room, GraphNode headNode)
    {
        int dir = 1;
        int target, ogTarget;
        target = ogTarget = room.targetDepth;

        List<GraphNode> nodesAtDepth;

        do {
            nodesAtDepth = FindNodesAtDepth(headNode, target);

            // If there are no avaible nodes at any of the closer depths,
            // go back to the original target, and start checking upwards
            if (nodesAtDepth.Count == 0) {
                target = ogTarget - dir;
                dir *= -1;
            }

            target += dir; // if this loops back around, try one depth shorter
        } while(nodesAtDepth.Count != 0);

        GraphNode selectedNode = nodesAtDepth[Random.Range(0, nodesAtDepth.Count)];
        selectedNode.Connect(room);
    }

    /// <summary>
    /// Runs through the graph and finds all nodes at a specific depth
    /// </summary>
    /// <param name="headNode">Node to start depth search at</param>
    /// <param name="targetDepth">Target depth FROM THE HEAD NODEf</param>
    /// <param name="onlyEnemyRooms"></param>
    /// <returns>List of rooms that fit the requirments</returns>
    private List<GraphNode> FindNodesAtDepth(GraphNode headNode, int targetDepth, bool onlyEnemyRooms = false)
    {
        Queue<GraphNode> queue = new Queue<GraphNode>();
        List<GraphNode> nodesAtDepth = new List<GraphNode>();

        queue.Enqueue(headNode);

        while(queue.Count > 0)
        {
            GraphNode node = queue.Dequeue();

            // Found a node at target depth
            if (node.depth == targetDepth)
            {
                // If the param is true, only add the enemy rooms at the target depth
                if (onlyEnemyRooms)
                {
                    // Add the node if it's an enemy room
                    if (node.roomType == RoomType.Normal || node.roomType == RoomType.Hub || node.roomType == RoomType.Connector)
                        nodesAtDepth.Add(node);
                }
                else // Add any node at the target depth
                    nodesAtDepth.Add(node);
            }

            foreach (GraphNode next in node.nextNodes)
            {
                queue.Enqueue(next);
            }
        }

        return nodesAtDepth;
    }

    /// <summary>
    /// Gets a random length based on difficulty. Chances + Lengths are predetermined
    /// </summary>
    /// <param name="diff">NPC Difficuly level</param>
    /// <returns>how many rooms it takes to get to the NPC room (not including the NPC room)</returns>
    private int GetRandomNPCLength(NPCDfficulty diff)
    {
        int ran = Random.Range(1, 101);
        int length = -1;

        switch (diff) 
        {
            case NPCDfficulty.Easy: // 20% 2, 80% 3
                if (ran <= 20)
                    length = 2;
                else
                    length = 3;
                return length;
            case NPCDfficulty.Medium: // 15% 2, 15% 4, 70% 3
                if(ran <= 15)
                    length = 2;
                else if(ran <= 30)
                    length = 4;
                else
                    length = 3;
                return length;
            case NPCDfficulty.Hard: // 30% 4, 70% 3
                if (ran <= 30)
                    length = 4;
                else
                    length = 3;
                return length;
        }
        return -1;
    }
}

public enum RoomType
{
    Start,
    Boss,
    Normal,
    Hub,
    Connector,
    Reward,
    Special,
    NPC
}

// public class RoomTemplate
// {
//     public RoomType roomType;
//     public GameObject roomPrefab;
// }


