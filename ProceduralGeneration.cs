using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEditor.Rendering.Universal;

public class ProceduralGeneration : MonoBehaviour
{
    public static ProceduralGeneration instance;

    private Room[,] roomArray; // Turn roomList into a 2D array
    private List<GameObject> roomList; // ----- Kinda pointless (still currently used tho) -----
    private RoomData[,] roomDataArray;

    [Header("Room Prefabs")]
    public GameObject[] startRoomPrefabs;
    public GameObject[] endRoomPrefabs;
    public List<GameObject> lvl1RoomPrefabs;
    public List<GameObject> lvl2RoomPrefabs;
    public GameObject[] treasureRoomPrefabs;

    private List<GameObject>[] roomPrefabs;

    private int gridRows, gridColumns;

    [Header("Room Setup")]
    public float roomOffset;

    private int lvlNum;
    private Vector2 startPos;


    public Vector2 StartPosition => startPos;

    /// <summary>
    /// Helper method that gets refrence to specific room
    /// </summary>
    /// <param name="index">Room index requested</param>
    /// <returns>Room instance at specified index</returns>
    public Room GetRoom(Vector2 index)
    {
        return roomArray[(int)index.x, (int)index.y];
    }

    // Singleton
    private void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Debug.Log("Procedural Generation instance already exists");
            return;
        }
        instance = this;
        #endregion

        roomList = new List<GameObject>();
        roomPrefabs = new List<GameObject>[] { lvl1RoomPrefabs, lvl2RoomPrefabs };
    }


    /// <summary>
    /// THIS METHOD IS A WORK IN PROGRESS
    /// </summary>
    /// <param name="levelNum">Level # to produce a level for</param>
    public void CreateLevel(int levelNum, List<NPC> npcs)
    {
        lvlNum = levelNum;
        // ******** LOCKING LEVEL TO 1 FOR NOW ********
        levelNum = 1;


        const int multiplyer = 6;
        gridRows = levelNum * multiplyer;
        gridColumns = levelNum * multiplyer;

        roomDataArray = new RoomData[gridRows, gridColumns];
        roomArray = new Room[gridRows, gridColumns];

        roomList.Clear();

        // Set points for now
        Vector2 start = RandomVector2();

        Vector2 end; // Find a valid index, and make sure its not the same as start
        do { end = RandomVector2(); } while (end == start);
        
        // Gets a path of rooms from the Create.. method
        List<RoomData> mainPath = CreateMainPath(start, end, 5, 8);
        startPos = new Vector2(mainPath[0].index.x * roomOffset, mainPath[0].index.y * roomOffset);
        


        #region NPC

        foreach (NPC npc in npcs)
        {
            if(npc == null) continue;

            if(npc.RoomQueue.Count == 1)
            {
                RoomData connectedRoom = null;
                GameObject room = npc.RoomQueue.Dequeue(); // pull the room here incase connectRoom fails and runs again
                while (connectedRoom == null)
                {
                    connectedRoom = CreateConnectingRoom(mainPath[Random.Range(1, mainPath.Count - 3)].index, room);
                }
            }
            else
            {
                List<RoomData> connectedRoom = null;

                // need to run this while because there may not be a path between the two points
                while(connectedRoom == null)
                {
                    connectedRoom = CreateConnectingPath(mainPath[Random.Range(1, mainPath.Count - 1)].index, // connectPoint1
                                                         mainPath[Random.Range(1, mainPath.Count - 1)].index, // connectPoint2
                                                         4, 7, // Min and Max length of path
                                                         npc); // NPC to generate path for
                }
            }
        }
        #endregion


        #region Trerasure Room
        // Add a treasure room to the map
        RoomData connectingRoom = null;
        while (connectingRoom == null)
        {
            connectingRoom = CreateConnectingRoom(mainPath[Random.Range(1, mainPath.Count - 2)].index, treasureRoomPrefabs[lvlNum - 1]);
        }
        #endregion


        // Create base room prefabs
        CreateFrontendRooms();


        Vector2 RandomVector2()
        {
            return new Vector2(Random.Range(0, gridRows - 1), Random.Range(0, gridColumns - 1));
        }
    }

    /// <summary>
    /// Creates a random path between 2 points
    /// </summary>
    /// <param name="start">Starting point in path</param>
    /// <param name="end">Ending point in path</param>
    /// <param name="minLength">minimum length of the path</param>
    /// <param name="maxLength">maximum length of the path</param>
    /// <param name="main">If this is the main start to end branch of the generation</param>
    /// <returns>List of roomData info (index,type,etc..) linked together like a linked list</returns>
    public List<RoomData> CreateMainPath(Vector2 start, Vector2 end, int minLength, int maxLength)
    {
        List<RoomData> path = null;
        List<Vector2> blockers = null;

        // While loop helps to prevent a case where blockers cut off any possible path between start and end
        bool achievedPath = false;
        while (achievedPath == false)
        {
            path = new List<RoomData>();
            blockers = new List<Vector2>();
            achievedPath = true;

            // "Blocker" elements help to create randomness from DFS
            int blockerCount = 1;

            List<Vector2> invalidBlockSpots = new List<Vector2>();


            #region Blocker Creation

            // If this is the first, "main" branch created, add special rooms to spice up DFS search
            // Create 2 lists of vectors, and combine them
            invalidBlockSpots = GetValidNeighbors(start, true).Concat(GetValidNeighbors(end, true)).ToList();

            // Also dont place blockers ontop of start/end rooms
            invalidBlockSpots.Add(start);
            invalidBlockSpots.Add(end);

            // For however many blocker spots there will be
            for (int i = 0; i < blockerCount; i++)
            {
                Vector2 index = Vector2.zero;
                bool isValidIndex;
                do
                {
                    isValidIndex = true;

                    // Random index for a blocker
                    index = new Vector2(Random.Range(0, gridRows - 1), Random.Range(0, gridColumns - 1));
                    for (int j = 0; j < invalidBlockSpots.Count; j++)
                    {
                        if (index == invalidBlockSpots[j])
                        {
                            isValidIndex = false;
                            break; // Stop looping through all possible invalid spots, since it alrady found one
                        }
                    }

                    for (int k = 0; k < blockers.Count; k++)
                    {
                        if (index == blockers[k])
                        {
                            isValidIndex = false;
                            break;
                        }
                    }
                    // If both these for loops run entirely through, validIndex is never changed to false, and while loop ends
                }
                while (!isValidIndex);

                // Create temp backend room at vaild blocker index
                CreateBackendRoom(index);
                blockers.Add(index);
            }

            #endregion

            // Gets path from DFS alg. Converts Node data to RoomData aswell
            List<Node> nodeList = DFS(start, end, minLength, maxLength);
            Queue<GameObject> mainRoomQueue = new Queue<GameObject>();

            // Creates the GO queue
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (i == 0)
                {
                    mainRoomQueue.Enqueue(startRoomPrefabs[lvlNum - 1]);
                }
                else if (i == nodeList.Count - 1)
                {
                    mainRoomQueue.Enqueue(endRoomPrefabs[lvlNum - 1]);
                }
                else
                {
                    mainRoomQueue.Enqueue(roomPrefabs[lvlNum - 1][Random.Range(0, roomPrefabs[lvlNum - 1].Count)]);
                }
            }

            // Converts Node data to RoomData
            path = NodeToRoomData(nodeList, mainRoomQueue);

            if (path == null)
            {
                achievedPath = false;
            }
        }

        // Gets rid of all blocker room refrences
        int count = blockers.Count;
        for (int i = 0; i < count; i++)
        {
            DeleteBackendRoom(blockers[i]);
        }

        // Attaches the connectons to the "Linked List" of rooms
        ConnectRooms(path);
        return path;
    }

    public List<RoomData> CreateConnectingPath(Vector2 connectPoint1, Vector2 connectPoint2, int minLength, int maxLength, NPC npc)
    {
        RoomData startDataRef = null;
        RoomData endDataRef = null;

        if(connectPoint1 == connectPoint2)
        {
            Debug.Log("Can't connect path. Same Point");
            return null;
        }

        
        // Pull and save the existing connect points for later to reinsert
        startDataRef = roomDataArray[(int)connectPoint1.x, (int)connectPoint1.y];
        DeleteBackendRoom(connectPoint1);

        // Quick check to make sure that this string is getting connected to something already preexisting
        if (startDataRef == null)
        {
            Debug.Log("Can't connect path. Starting Point Non-Existent");
            return null;
        }
            

        // The end doesnt necessarily have to connect back, so if there isnt a room at the end, we dont connect it.
        if (roomDataArray[(int)connectPoint2.x, (int)connectPoint2.y] != null)
        {
            endDataRef = roomDataArray[(int)connectPoint2.x, (int)connectPoint2.y];
            DeleteBackendRoom(connectPoint2);
        }


        // Run DFS and and convert it to a RoomData path

        List<Node> dfsReturn = DFS(connectPoint1, connectPoint2, minLength, maxLength);

        if(dfsReturn == null)
        {
            // reinsert the start and end room since DFS failed
            roomDataArray[(int)connectPoint1.x, (int)connectPoint1.y] = startDataRef;
            if (endDataRef != null)
                roomDataArray[(int)connectPoint2.x, (int)connectPoint2.y] = endDataRef;
            Debug.Log("Can't connect path. No Path Found");
            return null;
        }

        // Same thing as the comment below but for the start. Start should be the room after connecting point 1
        dfsReturn.RemoveAt(0);

        // Remove the end node since we want the end to be the one before the existing room
        // Then end in this is the preexisting room at connecting point 2, but we still want a final room before it reconnects to the path
        // Only do it though if there is a point to re connect to

        if (endDataRef != null)
            dfsReturn.RemoveAt(dfsReturn.Count - 1);
       
            

        List<RoomData> path = NodeToRoomData(dfsReturn, npc.GeneratePath(dfsReturn.Count));
        


        // If DFS comes back and there is no path between points,
        // place back the start and end room, then return null
        if (path == null)
        {
            SwapBackendRoom(startDataRef);
            SwapBackendRoom(endDataRef);
            return null;
        }


        // Re insert the start room removed earlier
        SwapBackendRoom(startDataRef);
        path.Insert(0, startDataRef);


        // End could be null. If it is, this branch only connects to map at one point
        // aka, dont try reconnecting
        if (endDataRef != null)
        {
            path.Add(endDataRef);
            SwapBackendRoom(endDataRef);
        }
            

        // Connect togther rooms (Think Linked List)
        ConnectRooms(path);
        return path;
    }

    public RoomData CreateConnectingRoom(Vector2 connectingPoint, GameObject prefab)
    {
        List<Vector2> neighbors = GetValidNeighbors(connectingPoint, false);

        if (neighbors.Count == 0)
        {
            // Debug.Log("No valid neighbors");
            return null;
        }

        RoomData rD = CreateBackendRoom(neighbors[Random.Range(0, neighbors.Count)], prefab);

        // Connects the two rooms
        ConnectRooms(new List<RoomData> 
        { 
            roomDataArray[(int)connectingPoint.x, (int)connectingPoint.y],
            rD
        });

        return rD; // Returns the newly Created room
    }


    private List<Node> DFS(Vector2 start, Vector2 end, int minLength, int maxLength)
    {
        List<Node> path = new List<Node>();
        Stack<Node> stack = new Stack<Node>();

        Node startNode = new Node(start);
        stack.Push(startNode);

        // While a path is being found...
        while (stack.Count > 0)
        {
            Node current = stack.Peek();

            // If we've reached the end
            if (current.GetIndex == end)
            {
                // DFS found a path. It was too short. Go back and see if there are other paths
                if (path.Count + 1 < minLength)
                {
                    // Add the end as a bad neighbor to the last path node
                    path[path.Count - 1].AddBadNeighbor(current);
                    stack.Pop(); // Pull the bad end node out of the stack
                    continue;
                }

                // If not, then DFS is done, and a viable path was found
                path.Add(current); // Add the end
                return path; // Done
            }

            List<Vector2> neighbors = GetValidNeighbors(current.GetIndex, false);
            List<Vector2> goodNeighbors = new List<Vector2>();

            // If the path is already at the max length, dont check for neighbors
            if (path.Count < maxLength)
            {
                // Check if any "valid neigbors" are deemed "bad", and therfore are shunned away
                for (int i = 0; i < neighbors.Count; i++)
                {

                    if (current.IsBadNeighbor(neighbors[i]))
                        continue;

                    goodNeighbors.Add(neighbors[i]);

                }
            }

            // ---- Leftover Neighbor Outcome ----
            if (goodNeighbors.Count == 0) // No valid neighbors
            {
                // If the starting node has 0 valid neighbors, then no path can be made.
                if (path.Count == 0)
                {
                    return null;
                }


                Node lastPath = path.Last();
                current = stack.Pop();


                if (current == path.Last())
                {
                    if (path.Count == 1)
                        return null;

                    // Add the bad neighbor to the second to last path node since the current edge is an invalid tile and its about to get Destroyed
                    path[path.Count - 2].AddBadNeighbor(lastPath);
                    DeleteBackendRoom(current.GetIndex);
                    path.Remove(lastPath);

                    // Break out if there is no valid path

                }
                else
                {
                    // Add the bad neighbor to the current node of the path
                    lastPath.AddBadNeighbor(current);
                }
            }
            else // There are valid neighbors
            {
                CreateBackendRoom(current.GetIndex);
                path.Add(current);

                // Give'Um a nice shake
                Shuffle(goodNeighbors);
                Shuffle(goodNeighbors);
                Shuffle(goodNeighbors);

                // Create a new node on the stack for each valid neighbor
                for (int i = 0; i < goodNeighbors.Count; i++)
                {
                    Node temp = new Node(goodNeighbors[i]);
                    stack.Push(temp);
                }
            }

            // if (path.Count == 0)
            // {
            //     return null;
            // }
        }

        return null;
    }

    private List<RoomData> NodeToRoomData(List<Node> nodePath, Queue<GameObject> roomQueue)
    {
        if (nodePath == null)
            return null;

        List<RoomData> roomDataPath = new List<RoomData>();

        for (int i = 0; i < nodePath.Count; i++)
        {
            if (i == 0)
            {
                roomDataPath.Add(CreateBackendRoom(nodePath[i].GetIndex, roomQueue.Dequeue()));
            }
            else if (i == nodePath.Count - 1)
            {
                roomDataPath.Add(CreateBackendRoom(nodePath[i].GetIndex, roomQueue.Dequeue()));
            }
            else
            {
                roomDataPath.Add(CreateBackendRoom(nodePath[i].GetIndex, roomQueue.Dequeue()));
            }
        }

        return roomDataPath;
    }




    /// <summary>
    /// Creates the backend side of a single room
    /// </summary>
    /// <param name="rt">What type of room is it? (Start, End, Enemy, Boss..)</param>
    /// <param name="index">Position index within the 2D grid manager</param>
    /// <returns>Returns the backend room Obj information</returns>
    public RoomData CreateBackendRoom(Vector2 index, GameObject prefab = null)
    {
        // Creates a new RoomData obj
        RoomData temp = new RoomData(index, prefab);

        // Sets refrences to it in the data array and list
        roomDataArray[(int)index.x, (int)index.y] = temp;
        //roomDataList.Add(temp);

        return temp;
    }

    /// <summary>
    /// Swap(override) parameter room into the backend data containers 
    /// </summary>
    /// <param name="rD">Room that takes priority</param>
    public void SwapBackendRoom(RoomData rD)
    {
        //roomDataList.Remove(roomDataArray[(int)rD.index.x, (int)rD.index.y]);

        roomDataArray[(int)rD.index.x, (int)rD.index.y] = rD;

        //roomDataList.Add(rD);
    }

    /// <summary>
    /// Delete refrences to backend rooms
    /// </summary>
    /// <param name="roomList">List of rooms to delete</param>
    public void DeleteBackendRoom(Vector2 index)
    {
        // Removes it by making array index null, and removing reference from list
        roomDataArray[(int)index.x, (int)index.y] = null;
        //roomDataList.Remove(temp);
    }




    /// <summary>
    /// Gets called at end of room generation. Creates room objects in unity scene
    /// </summary>
    /// <param name="rooms">List of all of the rooms to be created</param>
    public void CreateFrontendRooms()
    {
        for (int i = 0; i < roomDataArray.GetLength(0); i++)
        {
            for (int j = 0; j < roomDataArray.GetLength(1); j++)
            {
                RoomData rD = roomDataArray[i, j];

                // Skip any null indicies
                if (rD == null)
                    continue;

                // Calculate the world position using each index and a set x and y offset
                Vector2 worldPosition = new Vector3(rD.index.x * roomOffset, rD.index.y * roomOffset, 0.0f);

                GameObject temp = Instantiate(rD.GetRoomPrefab, worldPosition, Quaternion.identity);

                Room room = temp.GetComponent<Room>();
                room.roomData = rD; // Give Room script refrence to corresponding roomData
                room.SetUpRoom(rD.NeighborCalculation()); 
                roomList.Add(temp); // Add room to overarching roomList
                roomArray[(int)rD.index.x, (int)rD.index.y] = room; // Add room to 2D array
            }
        }
    }

    /// <summary>
    /// Sets each roomData to have refrence to previous and next rooms
    /// </summary>
    /// <param name="rooms">List of rooms that need to get connected</param>
    public void ConnectRooms(List<RoomData> rooms)
    {
        // Loop through all rooms
        for (int i = 0; i < rooms.Count; i++)
        {

            if (i != rooms.Count - 1) // Adds next room to all but last since its the end
                rooms[i].AddNextRoom(rooms[i + 1]);

            if (i != 0) // Add previous room to all but first
                rooms[i].AddPrevRoom(rooms[i - 1]);
        }
    }



    /// <summary>
    /// Checks if an index is withing the boundry parameters aswell as if there is a room already existing at that spot
    /// </summary>
    /// <param name="index">The index of roomData[] to check</param>
    /// <returns></returns>
    private bool IsValidIndex(Vector2 index)
    {
        int x = Mathf.FloorToInt(index.x);
        int y = Mathf.FloorToInt(index.y);
        return x >= 0 && x < gridRows && y >= 0 && y < gridColumns && // If the index is within the array size
            roomDataArray[x, y] == null; // If there isnt a current room
    }

    /// <summary>
    /// Checks to see if the index position in all 4 cardinal directions are vaid
    /// </summary>
    /// <param name="index">Index to check neighbors of</param>
    /// <param name="dir8">IF TRUE: checks all cardial Directions
    ///                    IF FALASE: checks all 8 surrouding neighbors
    ///                    
    ///                    *Save processing power if 4 dirs only needed</param>
    /// <returns>Returns a list of vaild neighboring indices</returns>
    private List<Vector2> GetValidNeighbors(Vector2 index, bool dir8)
    {

        List<Vector2> neighbors = new List<Vector2>();

        Vector2[] dirVectors = // Array of vectors to check against param index 
            (!dir8) ? new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right } : // Neigbors for 4 dir
                     new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right, // 8 dir neighbors
                                     new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) };

        // Loop through and generate list of valid indices
        foreach (Vector2 direction in dirVectors)
        {
            Vector2 neighbor = index + direction;
            if (IsValidIndex(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Shuffle any lists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">List to shuffle</param>
    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        int r;
        for (int i = 0; i < n; i++)
        {
            r = Random.Range(0, n - 1);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }


    /// <summary>
    /// Node class used to track DFS
    /// </summary>
    public class Node
    {
        Vector2 index;
        List<Node> badNeighbors;

        public Node(Vector2 index)
        {
            this.index = index;
            badNeighbors = new List<Node>();
        }

        public Vector2 GetIndex
        {
            get { return index; }
        }
        public int IndexX
        {
            get { return (int)index.x; }
        }
        public int IndexY
        {
            get { return (int)index.y; }
        }

        public void AddBadNeighbor(Node neighbor)
        {
            badNeighbors.Add(neighbor);
        }
        public void RemoveBadNeighbor(Node neighbor)
        {
            badNeighbors.Remove(neighbor);
        }
        public bool IsBadNeighbor(Vector2 neighbor)
        {
            for (int i = 0; i < badNeighbors.Count; i++)
            {
                if (neighbor == badNeighbors[i].index)
                {
                    return true;
                }
            }

            return false;
        }
    }
}