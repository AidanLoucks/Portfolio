using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TilemapIO : MonoBehaviour
{
    public Tilemap groundTilemap, wallTilemap;
    public TileBase groundTile, wallTile;
    public string mapFilePath1, mapFilePath2;

    private StreamReader reader;
    // private List<RoomInfo> roomInfos;

    [SerializeField] private const int MinOffset = 10;
    [SerializeField] private const int MaxOffset = 20;
    [SerializeField] private const int roomBuffer = 2;

    [SerializeField] private string filePath;
    private const string fileExtension = ".txt";

    private Dictionary<RoomType, List<RoomInfo>> fileContents; 




    private TiledRoom existingRoom;

    private Vector2 minGizmo, maxGizmo;

    private RoomInfo currentRoomInfo = null;
    private TiledRoom currentTiledRoom = null;

    private void Start()
    {
        GraphGenerator graphGenerator = new GraphGenerator();
        GraphNode headRoom = graphGenerator.GenerateGraph(3);

        ReadAllTextFiles();

        bool isFinished;
        do
        {
            existingRoom = null;
            currentRoomInfo = null;
            currentTiledRoom = null;

            isFinished = GenRooms(headRoom);

            if(isFinished == false)
            {
                groundTilemap.ClearAllTiles();
                wallTilemap.ClearAllTiles();

                ResestGraphNodes(headRoom);
            }
        }
        while (isFinished == false);

        // GenRooms(headRoom);
    }

    

    public bool GenRooms(GraphNode headRoom)
    {
        bool placedRoom = false;
        Direction ovrDir = (Direction)Random.Range(0, 4);
        Stack<(TiledRoom prevNode, GraphNode currNode)> nodeStack = new Stack<(TiledRoom prevNode, GraphNode currNode)>();
        nodeStack.Push((null,headRoom));

        while(nodeStack.Count > 0)
        {
            placedRoom = false;

            (TiledRoom prevRoom, GraphNode currNode) = nodeStack.Pop();

            if (prevRoom != null) { existingRoom = prevRoom; }
            GraphNode currentNode = currNode;

            if (currentNode.beenPlaced) continue;

            if(existingRoom != null)
            {
                (Direction dir, Vector2Int index) currentDoor = (Direction.North, new Vector2Int(0, 0));
                List<Direction> currentDirs = new List<Direction> { ovrDir };

                List<(Direction dir, Vector2Int index)> usableDoors = existingRoom.doors;
                List<(Direction dir, Vector2Int index, int weight)> sortedDoors = new List<(Direction dir, Vector2Int, int weight)>();

                //  This chunck of code is used to sort the doors so we could prioritize the doors that are in the direction we want to go

                // Loop through the 3 possible directions (one pair) and assign a weight to the doors in that direction
                for (int i = 3; i >= 1; i--)
                {
                    // Check if the door is in (one of) the current direction
                    for (int j = 0; j < usableDoors.Count; j++)
                    {
                        foreach (Direction currentDir in currentDirs)
                        {
                            if (usableDoors[j].dir == currentDir)
                            {
                                sortedDoors.Add((usableDoors[j].dir, usableDoors[j].index, i));
                                break;
                            }
                        }
                    }

                    /*  
                     *  i.e what this does is:
                     *  3. North
                     *  2. East & West
                     *  1. South 
                     */
                    currentDirs = GetPerpendicularDirs(currentDirs[0]);
                    if (i == 2)
                        currentDirs.Remove(ovrDir);
                }

                /*
             *  The code above sorts the doors by weight
             * 
             *  The code below does the work to select the door
             */
                while (!placedRoom)
                {
                    if (sortedDoors.Count == 0)
                    {
                        // Code was reached. Count: Alot
                        // Debug.LogError("No doors left to use. Need to implement more backtracing logic");

                        // This gets hit often. Current solution is to break out and keep trying until it works.
                        // Potential other solution could be implement backtracking that erases room tiles/pathways and goes back to reassess
                        // Would have to implement each room holding refrences to its own tiles
                        return false;
                    }


                    int totalWeight = 0;
                    for (int i = 0; i < sortedDoors.Count; i++)
                        totalWeight += sortedDoors[i].weight;

                    int rnd = Random.Range(0, totalWeight);

                    for (int i = 0; i < sortedDoors.Count; i++)
                    {
                        rnd -= sortedDoors[i].weight;
                        if (rnd < 0)
                        {
                            currentDoor = (sortedDoors[i].dir, sortedDoors[i].index);
                            sortedDoors.RemoveAt(i);
                            break;
                        }
                    }


                    List<Vector2Int> possibleVectors = GetPossibleNextUnitVectors(currentDoor.dir);

                    // Loops a max of 3 times
                    while (!placedRoom)
                    {
                        Vector2Int nextUnitVec = new Vector2Int(0, 0);

                        if (possibleVectors.Count <= 0)
                        {
                            break; // break outta this loop, and go back to the door selection code to pick a new door
                        }
                        else
                        {
                            nextUnitVec = possibleVectors[0];
                            possibleVectors.RemoveAt(0);
                        }

                        int maxAttempts = 3;
                        int attempts = 0;
                        
                        while(attempts < maxAttempts)
                        {
                            PathType pathType;
                            Vector2Int nextPos;

                            // Straight path
                            if (nextUnitVec.x == 0 || nextUnitVec.y == 0)
                            {
                                pathType = PathType.Straight;
                                nextPos = currentDoor.index + nextUnitVec * Random.Range(MinOffset, MaxOffset);
                            }
                            else // Rooms at an angle
                            {
                                pathType = (PathType)Random.Range(1, 3);

                                int minWidth = GetMinWidth(currentDoor.index, nextUnitVec);
                                int minHeight = GetMinHeight(currentDoor.index, nextUnitVec);
                                int maxWidth = GetMaxWidth(currentDoor.index, nextUnitVec);
                                int maxHeight = GetMaxHeight(currentDoor.index, nextUnitVec);

                                // Used for testing (drawing gizmos)
                                minGizmo = new Vector2(minWidth, minHeight);
                                maxGizmo = new Vector2(maxWidth, maxHeight);

                                nextPos = new Vector2Int(Random.Range(minWidth, maxWidth), Random.Range(minHeight, maxHeight));
                            }

                            // Finds the next room with the needed door direction
                            Direction nextDoorDir = GetOtherDoorDirectionByPathType(currentDoor.dir, pathType, nextUnitVec);

                            int minDoors = currentNode.nextNodes.Count + currentNode.prevNodes.Count;

                            currentRoomInfo = GetRoomInfoByType(currentNode.roomType, minDoors, nextDoorDir);
                            currentTiledRoom = new TiledRoom(currentRoomInfo, currentNode);


                            Vector2Int fileVector = GetFileDoorPositioning(currentRoomInfo, nextUnitVec);
                            (Direction dir, Vector2Int index) nextDoorInfo = GetDoor(currentRoomInfo, nextDoorDir, fileVector);

                            Vector2Int flippedDoorVec = new Vector2Int(nextDoorInfo.index.x, -nextDoorInfo.index.y);
                            Vector2Int roomPlacementOffset = nextPos - flippedDoorVec; // Gets the area for the next room to spawn

                            if (CheckRoomValidity(currentRoomInfo, roomPlacementOffset) 
                                || CheckPathwayValidity(currentDoor, nextDoorInfo, pathType))
                            {
                                attempts++;
                                continue;
                            }
                            else
                            {
                                // Draws the next room with the next door at the specified position
                                DrawRoom(currentRoomInfo, currentTiledRoom, roomPlacementOffset);

                                placedRoom = true;

                                // Need to get the door info again because the world pos of the door is calculated during the drawing of the room
                                nextDoorInfo = GetDoorForUse(currentTiledRoom, nextDoorDir, currentDoor.index);

                                // Creates the pathway between the two rooms
                                CreatePathway(currentDoor, nextDoorInfo, pathType);

                                existingRoom.doors.Remove(currentDoor);

                                break; // breaks out of the attempts if we succeed
                            }
                        }
                    }
                }
            }
            else
            {
                currentRoomInfo = GetRoomInfoByType(currentNode.roomType);
                currentTiledRoom = new TiledRoom(currentRoomInfo, currentNode);

                DrawRoom(currentRoomInfo, currentTiledRoom, new Vector2Int(0,0));
            }

            currentNode.beenPlaced = true;

            foreach(GraphNode node in currentNode.nextNodes)
            {
                if (!node.beenPlaced)
                {
                    nodeStack.Push((currentTiledRoom, node));
                }
            }
        }

        return true;
    }
    

    private RoomInfo GetRoomInfoByType(RoomType rt, int minDoors = 1, Direction? requiredDoorDir = null)
    {
        /*    
         *    This Code Checks the loaded Files for the   
         *    specified Room Type (Normal, Hub, Connector)
         */
        List<RoomInfo> possibleRooms = new List<RoomInfo>();
        fileContents.TryGetValue(rt, out possibleRooms);

        if(possibleRooms == null || possibleRooms.Count == 0)
        {
            Debug.LogError("No rooms of type: " + rt + " Within File Contents");
            return null;
        }
        /**/

        // Make sure the rooms have enough doors
        foreach(RoomInfo roomInfo in possibleRooms)
        {
            if(roomInfo.doors.Count < minDoors)
                possibleRooms.Remove(roomInfo);
        }

        /*
         * If there is a specific direction needed, 
         * loop through the list of rooms (of type..)
         * and pick one with the specfied door facing direction
         */
        if(requiredDoorDir != null)
        {
            int startIdx = Random.Range(0, possibleRooms.Count);
            int currentIdx = startIdx;

            do
            {
                if (possibleRooms[currentIdx].doors.Exists(x => x.dir == requiredDoorDir))
                    return possibleRooms[currentIdx];

                currentIdx = (currentIdx + 1) % possibleRooms.Count;
            } while (currentIdx != startIdx);


            Debug.LogError("No rooms of type: " + rt + " and direction: " + requiredDoorDir + " Within File Contents");
            return null;
        }
        /**/

        return possibleRooms[Random.Range(0, possibleRooms.Count)];
    }


    private void ReadAllTextFiles()
    {
        if(Directory.Exists(filePath))
        {
            string[] files = Directory.GetFiles(filePath, "*" + fileExtension);
            foreach (string file in files)
            {
                ReadRoomFile(file);
            }
        }
        else
        {
            Debug.LogError("Directory does not exist: " + filePath);
        }
    }

    /// <summary>
    /// Parses the information to form a room info object
    /// </summary>
    /// <param name="filename">file to parse</param>
    public void ReadRoomFile(string filename)
    {
        try
        {
            RoomInfo roomInfo = new RoomInfo();
            reader = new StreamReader(filename);

            string line = reader.ReadLine();
            string[] initInfo = line.Split(',');

            roomInfo.fileName = filename;

            RoomType roomType = ParseRoomType(initInfo[0]);
            roomInfo.width = int.Parse(initInfo[1]);
            roomInfo.height = int.Parse(initInfo[2]);


            roomInfo.groundArray = new bool[roomInfo.width, roomInfo.height];
            roomInfo.wallArray = new bool[roomInfo.width, roomInfo.height];

            roomInfo.doors = new List<(Direction dir, Vector2Int Index)>();


            for (int y = 0; y < roomInfo.height; y++)
            {
                line = reader.ReadLine();
                char[] rowChars = line.ToCharArray();

                for (int x = 0; x < roomInfo.width; x++)
                {
                    switch (rowChars[x])
                    {
                        case '^':
                            DoorPoint(x, y, Direction.North);
                            break;
                        case '>':
                            DoorPoint(x, y, Direction.East);
                            break;
                        case '~':
                            DoorPoint(x, y, Direction.South);
                            break;
                        case '<':
                            DoorPoint(x, y, Direction.West);
                            break;
                        case '1':
                            roomInfo.groundArray[x, y] = true;
                            roomInfo.wallArray[x, y] = true;
                            break;
                        case 'o':
                            roomInfo.groundArray[x, y] = true;
                            break;
                        case 'c':
                            Debug.Log("Error, still a C in file: " + filename);
                            break;
                        default:
                            break;
                    }
                }
            }

            // One of four possible door point types
            void DoorPoint(int x, int y, Direction dir)
            {
                roomInfo.groundArray[x, y] = true;
                roomInfo.wallArray[x, y] = true;
                roomInfo.doors.Add((dir, new Vector2Int(x, y)));
            }


            // Add the room info to the dictionary and return the room info
            AddRoom(roomType, roomInfo);
        }
        catch(Exception e)
        {
            Debug.LogError("Error reading file: " + filename + "\n" + e.Message);
        }

        // Adds room to dictionary according to type
        void AddRoom(RoomType roomType, RoomInfo roomInfo)
        {
            if (fileContents == null)
                fileContents = new Dictionary<RoomType, List<RoomInfo>>();


            // If that list of rooms already exists, add to it
            if (fileContents.ContainsKey(roomType))
            {
                fileContents[roomType].Add(roomInfo);
            }
            else // If not, create a new list and add to it
            {
                fileContents.Add(roomType, new List<RoomInfo> { roomInfo });
            }
        }

        // Takes string from file and converts it to RoomType
        RoomType ParseRoomType(string roomType)
        {
            switch (roomType)
            {
                case "hub":
                    return RoomType.Hub;
                case "normal":
                    return RoomType.Normal;
                case "boss":
                    return RoomType.Boss;
                case "start":
                    return RoomType.Start;
                case "reward":
                    return RoomType.Reward;
                default:
                    Debug.LogError("Invalid Room Type: " + roomType);
                    return RoomType.Normal;
            }
        }
    }

    

    /// <summary>
    /// Draws a room on the tilemap
    /// </summary>
    /// <param name="roomInfo">Specific room information</param>
    public void DrawRoom(RoomInfo roomInfo, TiledRoom tileRoom, Vector2Int offset)
    {
        List<(Direction, Vector2Int)> updatedDoorInfo = new List<(Direction, Vector2Int)>();
        for (int i = 0; i < roomInfo.doors.Count; i++)
        {
            updatedDoorInfo.Add((roomInfo.doors[i].dir,  // Keep the Dir
                new Vector2Int(roomInfo.doors[i].index.x + offset.x, // Update the x Index with the x offset
                -roomInfo.doors[i].index.y + offset.y))); // Also update the y index with the y offset
        }
        tileRoom.doors = updatedDoorInfo; // Give the new door info to the room info

        // loop through the room array and draw the tiles
        for (int y = 0; y < roomInfo.height; y++)
        {
            for (int x = 0; x < roomInfo.width; x++)
            {
                // Flip the Y coordinate to draw the room upright
                // Uses the offset to position all room tiles correctly
                Vector3Int tilePosition = new Vector3Int(x + offset.x, -y + offset.y, 0);

                if (roomInfo.groundArray[x, y])
                    groundTilemap.SetTile(tilePosition, groundTile);

                if (roomInfo.wallArray[x, y])
                    wallTilemap.SetTile(tilePosition, wallTile);
            }
        }
    }

    #region Create Pathways
    /// <summary>
    /// Calls specific path drawing function depending on the path type
    /// </summary>
    /// <param name="existingDoorPoint">Door point of exisitng room</param>
    /// <param name="connectingDoorPoint">Door point of cnnection</param>
    /// <param name="pathType">Type of path to draw in</param>
    private void CreatePathway((Direction dir, Vector2Int index) existingDoorPoint, 
                    (Direction dir, Vector2Int index) connectingDoorPoint, PathType pathType)
    {
        switch (pathType)
        {
            case PathType.Straight:
                CreateStraightPath(existingDoorPoint, connectingDoorPoint.index);
                break;

            case PathType.LShape:
                CreateLPath(existingDoorPoint, connectingDoorPoint);
                break;

            case PathType.ZigZag:
                CreateZigZagPath(existingDoorPoint, connectingDoorPoint);
                break;
        }
    }

    /// <summary>
    /// Draws a straight path between two points
    /// </summary>
    /// <param name="existingPoint">Existing Door Point</param>
    /// <param name="connectPoint">Connecting Door Point</param>
    private void CreateStraightPath((Direction dir, Vector2Int index) existingPoint, Vector2Int connectPoint)
    {
        if (existingPoint.dir == Direction.North || existingPoint.dir == Direction.South)
        {
            for (int y = Mathf.Min(existingPoint.index.y, connectPoint.y); y <= Mathf.Max(existingPoint.index.y, connectPoint.y); y++)
            {
                for (int offsetX = -2; offsetX <= 2; offsetX++) // Make the path 5 blocks wide
                {
                    Vector3Int tilePosition = new Vector3Int(existingPoint.index.x + offsetX, y, 0);
                    groundTilemap.SetTile(tilePosition, groundTile);
        
                    if (Mathf.Abs(offsetX) == 2)
                        wallTilemap.SetTile(tilePosition, wallTile); // Place Walls
                    else
                        wallTilemap.SetTile(tilePosition, null); // Remove walls in the path
                }
            }
        }
        else
        {
            for (int x = Mathf.Min(existingPoint.index.x, connectPoint.x); x <= Mathf.Max(existingPoint.index.x, connectPoint.x); x++)
            {
                // Runs 6 wide to account for wood walls
                for (int offsetY = -2; offsetY <= 3; offsetY++) // Make the path 5 blocks wide
                {
                    Vector3Int tilePosition = new Vector3Int(x, existingPoint.index.y + offsetY, 0);
        
                    if (Mathf.Abs(offsetY) != 3)
                        groundTilemap.SetTile(tilePosition, groundTile);
        
                    // if its the first or fourth row, dont place walls ORRR if it is the first, only draw at the end and begining
                    if ((offsetY != 1 && offsetY != -2) || (offsetY == -2 && (x == Mathf.Min(existingPoint.index.x, connectPoint.x) || x == Mathf.Max(existingPoint.index.x, connectPoint.x)))) //&& offsetY != 0) ((x == Mathf.Min(start.x, end.x) || x == Mathf.Max(start.x, end.x)) && offsetY != 0))
                        wallTilemap.SetTile(tilePosition, wallTile); // Place Walls
                    else
                        wallTilemap.SetTile(tilePosition, null); // Remove walls in the path
                }
            }
        }
    }

    /// <summary>
    /// Draws two Straight paths that connect at a cross point
    /// </summary>
    /// <param name="existingDoorPoint">Existing room door point</param>
    /// <param name="connectingDoorPoint">Connecting door point</param>
    private void CreateLPath((Direction dir, Vector2Int index) existingDoorPoint, 
                          (Direction dir, Vector2Int index) connectingDoorPoint)
    {
        // get the cross point
        Vector2Int crossPoint = GetCrossPoint(existingDoorPoint.index, connectingDoorPoint.index, existingDoorPoint.dir);

        // draw the first segment in first direction
        CreateStraightPath(existingDoorPoint, crossPoint);

        // Draw the second segment in the second direction
        CreateStraightPath(connectingDoorPoint, crossPoint);

        CleanCrossPoint(crossPoint, existingDoorPoint.dir, connectingDoorPoint.dir);
    }

    /// <summary>
    /// Creastes a zig zag path between two points
    /// </summary>
    /// <param name="existingDoorPoint">Door point from existing room</param>
    /// <param name="connectingDoorPoint">Point that were connecting to</param>
    private void CreateZigZagPath((Direction dir, Vector2Int index) existingDoorPoint,
                               (Direction dir, Vector2Int index) connectingDoorPoint) 
    {
        // Calculate both cross points
        var crossPoints = GetCrossPoints(existingDoorPoint, connectingDoorPoint);
        Vector2Int crossPoint1 = crossPoints.Item1;
        Vector2Int crossPoint2 = crossPoints.Item2;
        Direction crossDir = GetDirection(crossPoint1, crossPoint2);


        // Draw first segment from existing door to cross point 1
        CreateStraightPath(existingDoorPoint, crossPoint1);

        // Draw second segment from cross point 1 to cross point 2
        CreateStraightPath((crossDir, crossPoint1), crossPoint2);

        // Draw third segment from connecting door to cross point 2
        CreateStraightPath(connectingDoorPoint, crossPoint2);

        // Clean the two cross points
        CleanCrossPoint(crossPoint1, existingDoorPoint.dir, GetOppositeDirection(crossDir));
        CleanCrossPoint(crossPoint2, connectingDoorPoint.dir, crossDir);
    }


    /// <summary>
    /// Correctly drawns Wall and Ground Tiles at the cross point of a path
    /// </summary>
    /// <param name="crossPoint">Point where two paths collide</param>
    /// <param name="dir1">Direction pointing INTO the cross point</param>
    /// <param name="dir2">Direction pointing INTO the cross point</param>
    private void CleanCrossPoint(Vector2Int crossPoint, Direction dir1, Direction dir2)
    {
        for (int x = crossPoint.x - 2; x <= crossPoint.x + 2; x++)
        {
            for (int y = crossPoint.y - 2; y <= crossPoint.y + 3; y++)
            {
                bool drawWall = true;
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                if (y != 3)
                    groundTilemap.SetTile(tilePosition, groundTile);


                // Determine if the current position is on the border
                bool isBorder = (x == crossPoint.x - 2 || x == crossPoint.x + 2 ||
                                 y == crossPoint.y - 2 || y == crossPoint.y + 2);


                // Depending on the direction of both segments, we need to not draw walls in certain places
                if (dir1 == Direction.North || dir2 == Direction.North)
                {
                    if (y == crossPoint.y - 2 && x >= crossPoint.x - 1 && x <= crossPoint.x + 1)
                    {
                        drawWall = false;
                    }
                    else if(y <= crossPoint.y + 1 && !isBorder)
                    {
                        drawWall = false;
                    }
                }
                else if (dir1 == Direction.South || dir2 == Direction.South)
                {
                    if (y == crossPoint.y - 2)
                    {
                        drawWall = false;
                    }
                    else if (y == crossPoint.y + 2 && x >= crossPoint.x - 1 && x <= crossPoint.x + 1)
                    {
                        drawWall = false;
                    }
                    else if (y >= crossPoint.y + 1 && !isBorder)
                    {
                        drawWall = false;
                    }
                }

                if(dir1 == Direction.East || dir2 == Direction.East)
                {
                    if (y == crossPoint.y + 1 && x == crossPoint.x - 2)
                    {
                        drawWall = false;
                    }
                }
                else if (dir1 == Direction.West || dir2 == Direction.West)
                {
                    if (y == crossPoint.y + 1 && x == crossPoint.x + 2)
                    {
                        drawWall = false;
                    }
                }



                if (drawWall)
                    wallTilemap.SetTile(tilePosition, wallTile);
                else
                    wallTilemap.SetTile(tilePosition, null);
            }
        }
    }

    #region Check Validity
    /// <summary>
    /// Checks the space that the room will occupy for any overlap
    /// </summary>
    /// <param name="placedRoom">The info for the room being placed</param>
    /// <param name="placementOffset">The offset to where the room is placed (0,0) -> (offset.x, offset.y) </param>
    /// <returns>Returns 3 pieces
    ///             1. If the room has overlap
    ///             2. The min values of where tiles are located in the square
    ///             3. The max values of where tiles are located in the square</returns>
    private bool CheckRoomValidity(RoomInfo placedRoom, Vector2Int placementOffset)
    {
        /*
         *  This code was meant to do more than just check for overlap.
         *  In the future, I dont want to give up on this idea
         *  The point is find out where there are tiles within the area,
         *  and then offset the room to fit nicely next to existing rooms/paths.
         *  It may be a bit overkil for now, but I may want to come back to this
         */

        // Starting at -1, and going to x + 1 checks for a buffer between rooms
        for (int y = -roomBuffer; y < placedRoom.height + roomBuffer; y++)
        {
            for (int x = -roomBuffer; x < placedRoom.width + roomBuffer; x++)
            {
                Vector2Int tilePos = new Vector2Int(x + placementOffset.x, -y + placementOffset.y);

                if (groundTilemap.HasTile(new Vector3Int(tilePos.x, tilePos.y, 0)))
                {
                    return true;
                }
            }
        }
        return false;
    }


    private bool CheckPathwayValidity((Direction doorDir, Vector2Int index) currentDoor, (Direction doorDir, Vector2Int index) nextDoor, PathType path)
    {
        switch (path)
        {
            case PathType.Straight:
                if (CheckStraightPathValidity(currentDoor, nextDoor))
                    return true;
                break;

            case PathType.LShape:
                if (CheckLShapePathValidity(currentDoor, nextDoor))
                    return true;
                break;

            case PathType.ZigZag:
                if (CheckZigZagPathValidity(currentDoor, nextDoor))
                    return true;
                break;
        }
        return false;
    }

    /// <summary>
    /// Checks if the straight path between two points is valid, i.e. there are no tiles in the way
    /// </summary>
    /// <param name="currentDoor">Door pathway starts from</param>
    /// <param name="nextDoor">Pathway end</param>
    /// <returns>True if pathway is invalid/False if valid</returns>
    private bool CheckStraightPathValidity((Direction doorDir, Vector2Int index) currentDoor, (Direction doorDir, Vector2Int index) nextDoor)
    {
        if (currentDoor.doorDir == Direction.North)
        {
            for (int y = currentDoor.index.y + 2; y <= nextDoor.index.y - 3; y++)
            {
                for (int x = currentDoor.index.x - 3; x <= nextDoor.index.x + 3; x++)
                {
                    if (groundTilemap.HasTile(new Vector3Int(x, y, 0))
                        || wallTilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        return true;
                    }
                }
            }
        }
        else if (currentDoor.doorDir == Direction.South)
        {
            for (int y = currentDoor.index.y - 3; y >= nextDoor.index.y + 2; y--)
            {
                for (int x = currentDoor.index.x - 3; x <= nextDoor.index.x + 3; x++)
                {
                    if (groundTilemap.HasTile(new Vector3Int(x, y, 0))
                        || wallTilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        return true;
                    }
                }
            }
        }
        else if (currentDoor.doorDir == Direction.West)
        {
            for (int x = currentDoor.index.x - 2; x >= nextDoor.index.x + 2; x--)
            {
                for (int y = currentDoor.index.y + 3; y >= nextDoor.index.y - 3; y--)
                {
                    if (groundTilemap.HasTile(new Vector3Int(x, y, 0))
                        || wallTilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        return true;
                    }

                }
            }
        }
        else if (currentDoor.doorDir == Direction.East)
        {
            for (int x = currentDoor.index.x + 2; x <= nextDoor.index.x - 2; x++)
            {
                for (int y = currentDoor.index.y + 3; y <= nextDoor.index.y - 3; y++)
                {
                    if (groundTilemap.HasTile(new Vector3Int(x, y, 0))
                        || wallTilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        return true;
                    }

                }
            }
        }
        return false;
    }

    private bool CheckLShapePathValidity((Direction dir, Vector2Int index) currentDoor, (Direction dir, Vector2Int index) nextDoor)
    {
        Vector2Int crossPoint = GetCrossPoint(currentDoor.index, nextDoor.index, currentDoor.dir);

        if(currentDoor.dir == Direction.South)
        {
            if(CheckStraightPathValidity(currentDoor, (Direction.North, new Vector2Int(crossPoint.x, crossPoint.y - 5))))
            {
                return true;
            }

            if (nextDoor.dir == Direction.East)
            {
                if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint.x + 5, crossPoint.y))))
                {
                    return true;
                }
            }
            else
            {
                if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint.x - 5, crossPoint.y))))
                {
                    return true;
                }
            }
        }
        else if(currentDoor.dir == Direction.North)
        {
            if (CheckStraightPathValidity(currentDoor, (Direction.North, new Vector2Int(crossPoint.x, crossPoint.y + 5))))
            {
                return true;
            }

            if (nextDoor.dir == Direction.East)
            {
                if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint.x + 5, crossPoint.y))))
                {
                    return true;
                }
            }
            else
            {
                if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint.x - 5, crossPoint.y))))
                {
                    return true;
                }
            }
        }
        else if(currentDoor.dir == Direction.East)
        {
            if (CheckStraightPathValidity(currentDoor, (Direction.North, new Vector2Int(crossPoint.x + 5, crossPoint.y))))
            {
                return true;
            }

            if (nextDoor.dir == Direction.North)
            {
                if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint.x, crossPoint.y + 5))))
                {
                    return true;
                }
            }
            else
            {
                if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint.x, crossPoint.y - 5))))
                {
                    return true;
                }
            }
        }
        else if(currentDoor.dir == Direction.West)
        {
            if (CheckStraightPathValidity(currentDoor, (Direction.North, new Vector2Int(crossPoint.x - 5, crossPoint.y))))
            {
                return true;
            }

            if (nextDoor.dir == Direction.North)
            {
                if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint.x, crossPoint.y + 5))))
                {
                    return true;
                }
            }
            else
            {
                if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint.x, crossPoint.y - 5))))
                {
                    return true;
                }
            }
        }
            

        return false;
    }

    private bool CheckZigZagPathValidity((Direction dir, Vector2Int index) currentDoor, (Direction dir, Vector2Int index) nextDoor)
    {
        var crossPoints = GetCrossPoints(currentDoor, nextDoor);
        Vector2Int crossPoint1 = crossPoints.Item1;
        Vector2Int crossPoint2 = crossPoints.Item2;
        Direction crossDir = GetDirection(crossPoint1, crossPoint2);


        if(currentDoor.dir == Direction.South)
        {
            if(CheckStraightPathValidity(currentDoor, (Direction.North, new Vector2Int(crossPoint1.x, crossPoint1.y - 6))))
            {
                return true;
            }

            if(crossDir == Direction.West)
            {
                if (CheckStraightPathValidity((Direction.East, new Vector2Int(crossPoint1.x + 6, crossPoint1.y)), (Direction.North, new Vector2Int(crossPoint2.x - 6, crossPoint2.y))))
                {
                    return true;
                }
            }
            else if(crossDir == Direction.East)
            {
                if (CheckStraightPathValidity((Direction.West, new Vector2Int(crossPoint1.x - 6, crossPoint1.y)), (Direction.North, new Vector2Int(crossPoint2.x + 6, crossPoint2.y))))
                {
                    return true;
                }
            }

            if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint2.x, crossPoint2.y + 6))))
            {
                return true;
            }
        }
        else if(currentDoor.dir == Direction.North)
        {
            if (CheckStraightPathValidity(currentDoor, (Direction.North, new Vector2Int(crossPoint1.x, crossPoint1.y + 6))))
            {
                return true;
            }

            if (crossDir == Direction.West)
            {
                if (CheckStraightPathValidity((Direction.East, new Vector2Int(crossPoint1.x + 6, crossPoint1.y)), (Direction.North, new Vector2Int(crossPoint2.x - 6, crossPoint2.y))))
                {
                    return true;
                }
            }
            else if (crossDir == Direction.East)
            {
                if (CheckStraightPathValidity((Direction.West, new Vector2Int(crossPoint1.x - 6, crossPoint1.y)), (Direction.North, new Vector2Int(crossPoint2.x + 6, crossPoint2.y))))
                {
                    return true;
                }
            }

            if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint2.x, crossPoint2.y - 6))))
            {
                return true;
            }
        }
        else if(currentDoor.dir == Direction.East)
        {
            if(CheckStraightPathValidity(currentDoor, (Direction.North, new Vector2Int(crossPoint1.x + 5, crossPoint1.y))))
            {
                return true;
            }

            if(crossDir == Direction.North)
            {
                if (CheckStraightPathValidity((Direction.North, new Vector2Int(crossPoint1.x, crossPoint1.y - 5)), (Direction.North, new Vector2Int(crossPoint2.x, crossPoint2.y + 5))))
                {
                    return true;
                }
            }
            else if (crossDir == Direction.South)
            {
                if (CheckStraightPathValidity((Direction.North, new Vector2Int(crossPoint1.x, crossPoint1.y + 5)), (Direction.North, new Vector2Int(crossPoint2.x, crossPoint2.y - 5))))
                {
                    return true;
                }
            }

            if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint2.x - 5, crossPoint2.y))))
            {
                return true;
            }
        }
        else if(currentDoor.dir == Direction.West)
        {
            if (CheckStraightPathValidity(currentDoor, (Direction.North, new Vector2Int(crossPoint1.x - 5, crossPoint1.y))))
            {
                return true;
            }

            if (crossDir == Direction.North)
            {
                if (CheckStraightPathValidity((Direction.North, new Vector2Int(crossPoint1.x, crossPoint1.y - 5)), (Direction.North, new Vector2Int(crossPoint2.x, crossPoint2.y + 5))))
                {
                    return true;
                }
            }
            else if (crossDir == Direction.South)
            {
                if (CheckStraightPathValidity((Direction.North, new Vector2Int(crossPoint1.x, crossPoint1.y + 5)), (Direction.North, new Vector2Int(crossPoint2.x, crossPoint2.y - 5))))
                {
                    return true;
                }
            }

            if (CheckStraightPathValidity(nextDoor, (Direction.North, new Vector2Int(crossPoint2.x + 5, crossPoint2.y))))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #endregion

    #region Room Placement Constraints
    /// <summary>
    /// Gets Tilemap min x pos for the next room
    /// </summary>
    /// <param name="existingDoor">Door apart of existing room that will be connected to</param>
    /// <returns>Min door x pos for new room</returns>
    private int GetMinWidth(Vector2Int doorIndex, Vector2Int nextUnitVec)
    {
        int offset = MinOffset;
        if (nextUnitVec.x < 0)
            offset = -offset;

        return doorIndex.x + offset;
    }

    /// <summary>
    /// Gets Tilemap min y pos for the next room
    /// </summary>
    /// <param name="existingDoor">Door apart of existing room that will be connected to</param>
    /// <returns>Min x pos for door in new room</returns>
    private int GetMinHeight(Vector2Int doorIndex, Vector2Int nextUnitVec)
    {
        int offset = MinOffset;
        if (nextUnitVec.y < 0)
            offset = -offset;

        return doorIndex.y + offset;
    }

    /// <summary>
    /// Gets Tilemap max x pos for the next room
    /// </summary>
    /// <param name="existingDoor">Door apart of existing room that will be connected to</param>
    /// <returns>Min y pos for door in new room</returns>
    private int GetMaxWidth(Vector2Int doorIndex, Vector2Int nextUnitVec)
    {
        int offset = MaxOffset;
        if (nextUnitVec.x < 0)
            offset = -offset;

        return doorIndex.x + offset;
    }

    /// <summary>
    /// Gets Tilemap max y pos for the next room
    /// </summary>
    /// <param name="existingDoor">Door apart of existing room that will be connected to</param>
    /// <returns>Max y pos for door in new room</returns>
    private int GetMaxHeight(Vector2Int doorIndex, Vector2Int nextUnitVec)
    {
        int offset = MaxOffset;
        if (nextUnitVec.y < 0)
            offset = -offset;

        return doorIndex.y + offset;
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Gets one of three possible points in direction of dir
    /// </summary>
    /// <param name="dir">The dir the map is drawing in</param>
    /// <returns>Unit Vector Position of next room</returns>
    private Vector2Int GetNextUnitVectorPos(Direction dir)
    {
        Vector2Int[] values;
        int rnd = Random.Range(0, 3);
        switch (dir)
        {
            case Direction.North:
                values = new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1) };
                return values[rnd];

            case Direction.East:
                values = new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1) };
                return values[rnd];

            case Direction.South:
                values = new Vector2Int[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1) };
                return values[rnd];

            case Direction.West:
                values = new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(-1, -1) };
                return values[rnd];
        }

        Debug.LogError("No direction passed");
        return Vector2Int.zero;
    }

    private List<Vector2Int> GetPossibleNextUnitVectors(Direction dir)
    {
        List<Vector2Int> values =  new List<Vector2Int>();
        int rnd = Random.Range(0, 3);
        switch (dir)
        {
            case Direction.North:
                values = new List<Vector2Int> { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1) };
                break;

            case Direction.East:
                values = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1) };
                break;

            case Direction.South:
                values = new List<Vector2Int> { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1) };
                break;

            case Direction.West:
                values = new List<Vector2Int> { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(-1, -1) };
                break;
        }

        ProceduralGeneration.Shuffle(values);
        ProceduralGeneration.Shuffle(values);
        return values;
    }

    private Direction GetCurrentRoomDoorDir(Vector2Int nextUnitVec)
    {
        int rnd = Random.Range(0, 2);
        if (nextUnitVec == new Vector2Int(1, 1))
            return rnd == 0 ? Direction.North : Direction.East;
        else if (nextUnitVec == new Vector2Int(1, -1))
            return rnd == 0 ? Direction.East : Direction.South;
        else if (nextUnitVec == new Vector2Int(-1, -1))
            return rnd == 0 ? Direction.South : Direction.West;
        else if (nextUnitVec == new Vector2Int(-1, 1))
            return rnd == 0 ? Direction.West : Direction.North;

        else if (nextUnitVec == new Vector2Int(0, 1))
            return Direction.North;
        else if (nextUnitVec == new Vector2Int(1, 0))
            return Direction.East;
        else if (nextUnitVec == new Vector2Int(0, -1))
            return Direction.South;
        else if (nextUnitVec == new Vector2Int(-1, 0))
            return Direction.West;

        Debug.LogError("Invalid Unit Vector");
        return Direction.North;
    }

    /// <summary>
    /// Goes through list of doors, and returns the closest door to the connecting door. **Removes the door from avalible doors**
    /// </summary>
    /// <param name="currentRoom">Raw RoomInfo object</param>
    /// <param name="dir">Direction we need the door in</param>
    /// <param name="connectingDoorIndex">Tilemap index of the existing door</param>
    /// <returns>Closest Door</returns>
    private (Direction, Vector2Int) GetDoor(RoomInfo currentRoom, Direction dir, Vector2 indexPoint)
    {
        List<(Direction doorDir, Vector2Int index)> possibleDoors = new List<(Direction doorDir, Vector2Int index)>();

        // Collects all the doors with the required direction
        foreach ((Direction doorDir, Vector2Int index) doorInfo in currentRoom.doors)
        {
            if (doorInfo.doorDir == dir)
            {
                possibleDoors.Add(doorInfo);
            }
        }

        // Find the closest door to the connecting door
        (Direction doorDir, Vector2Int index) closestDoor = (Direction.North, new Vector2Int(0, 0));
        float shortestDistance = float.MaxValue;
        foreach ((Direction doorDir, Vector2Int index) doorInfo in possibleDoors)
        {
            if (Vector2.Distance(doorInfo.index, indexPoint) < shortestDistance)
            {
                shortestDistance = Vector2.Distance(doorInfo.index, indexPoint);
                closestDoor = doorInfo;
            }
        }

        return closestDoor;
        // // If there are no rooms in the required direction, randomly get a door from the room
        // int rnd = Random.Range(0, currentRoom.doors.Count);
        // Debug.Log("No door in " + dir + " direction");
        // return currentRoom.doors[rnd];
    }

    /// <summary>
    /// Goes through list of doors, and returns the closest door to the connecting door. **Removes the door from avalible doors**
    /// </summary>
    /// <param name="currentRoom">Raw RoomInfo object</param>
    /// <param name="dir">Direction we need the door in</param>
    /// <param name="connectingDoorIndex">Tilemap index of the existing door</param>
    /// <returns>Closest Door</returns>
    private (Direction, Vector2Int) GetDoorForUse(TiledRoom currentRoom, Direction dir, Vector2 connectingDoorIndex)
    {
        List<(Direction doorDir, Vector2Int index)> possibleDoors = new List<(Direction doorDir, Vector2Int index)>();

        // Collects all the doors with the required direction
        foreach((Direction doorDir, Vector2Int index) doorInfo in currentRoom.doors)
        {
            if (doorInfo.doorDir == dir)
            {
                possibleDoors.Add(doorInfo);
            }
        }

        // Find the closest door to the connecting door
        (Direction doorDir, Vector2Int index) closestDoor = (Direction.North, new Vector2Int(0, 0));
        float shortestDistance = float.MaxValue;
        foreach((Direction doorDir, Vector2Int index) doorInfo in possibleDoors)
        {
            if(Vector2.Distance(doorInfo.index, connectingDoorIndex) < shortestDistance)
            {
                shortestDistance = Vector2.Distance(doorInfo.index, connectingDoorIndex);
                closestDoor = doorInfo;
            }
        }
        

        currentRoom.doors.Remove(closestDoor);
        return closestDoor;
        // // If there are no rooms in the required direction, randomly get a door from the room
        // int rnd = Random.Range(0, currentRoom.doors.Count);
        // Debug.Log("No door in " + dir + " direction");
        // return currentRoom.doors[rnd];
    }

    private Vector2Int GetFileDoorPositioning(RoomInfo roomInfo, Vector2Int nextVector)
    {
        Vector2Int resultVector = new Vector2Int(0, 0);
       
        if(nextVector.x == -1)
        {
            resultVector.x = roomInfo.width + roomInfo.width / 2;
        }
        else if(nextVector.x == 1)
        {
            resultVector.x = -roomInfo.width / 2;
        }
        else
        {
            resultVector.x = roomInfo.width / 2;
        }

        if(nextVector.y == -1)
        {
            resultVector.y = roomInfo.height + roomInfo.height / 2;
        }
        else if(nextVector.y == 1)
        {
            resultVector.y = -roomInfo.height / 2;
        }
        else
        {
            resultVector.y = roomInfo.height / 2;
        }


        return resultVector;
    }

    private List<Direction> GetPerpendicularDirs(Direction dir)
    {
        switch (dir)
        {
            case Direction.North:
            case Direction.South:
                return new List<Direction> { Direction.East, Direction.West };
            case Direction.East:
            case Direction.West:
                return new List<Direction> { Direction.North, Direction.South };
            default:
                throw new ArgumentException("Invalid direction");
        }
    }

    /// <summary>
    /// Given a direction, returns the opposite direction
    /// </summary>
    /// <param name="direction">Original Direction</param>
    /// <returns>Opposite Direction</returns>
    /// <exception cref="ArgumentException"></exception>
    private Direction GetOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return Direction.South;
            case Direction.East:
                return Direction.West;
            case Direction.South:
                return Direction.North;
            case Direction.West:
                return Direction.East;
            default:
                throw new ArgumentException("Invalid direction");
        }
    }

    private Direction GetOtherDoorDirectionByPathType(Direction direction, PathType pathType, Vector2Int nextPosUnitVec)
    {
        switch (pathType)
        {
            case PathType.Straight:
                return GetOppositeDirection(direction);
            case PathType.LShape:
                return GetLConnectingDoorDirection(direction, nextPosUnitVec);
            case PathType.ZigZag:
                return GetOppositeDirection(direction);
            default:
                throw new ArgumentException("Invalid path type");
        }
    }

    private Direction GetLConnectingDoorDirection(Direction direction, Vector2Int nextPosUnitVec)
    {
        if(direction == Direction.North || direction == Direction.South)
        {
            if (nextPosUnitVec.x == 1)
                return Direction.West;
            else
                return Direction.East;
        }
        else
        {
            if (nextPosUnitVec.y == 1)
                return Direction.South;
            else
                return Direction.North;
        }
    }

    /// <summary>
    /// Returns the horizontal or vectical direction of a segment given two points
    /// </summary>
    /// <param name="point1">First Point</param>
    /// <param name="point2">Point Direction faces in</param>
    /// <returns>Direction facing the second point</returns>
    private Direction GetDirection(Vector2Int point1, Vector2Int point2)
    {
        if (point1.x == point2.x)
        {
            if (point1.y < point2.y)
                return Direction.North;
            else
                return Direction.South;
        }
        else
        {
            if (point1.x < point2.x)
                return Direction.East;
            else
                return Direction.West;
        }
    }

    // Recursively resets all the nodes in the graph
    public void ResestGraphNodes(GraphNode headNode)
    {
        if (headNode == null)
            return;

        headNode.beenPlaced = false;

        foreach (GraphNode node in headNode.nextNodes)
        {
            ResestGraphNodes(node);
        }
    }


    #region ZigZag Path
    /// <summary>
    /// Calulates the cross point between two points
    /// </summary>
    /// <param name="start">Point of door in existing room</param>
    /// <param name="end">Point of door in connecting room</param>
    /// <param name="dir">Direction the first segment drawns in</param>
    /// <returns>The point that intersect the perpendicular to the second room</returns>
    private Vector2Int GetCrossPoint(Vector2Int start, Vector2Int end, Direction dir)
    {
        if(dir == Direction.North || dir == Direction.South)
            return new Vector2Int(start.x, end.y);
        else // (dir == Direction.East || dir == Direction.West
            return new Vector2Int(end.x, start.y);
    }

    /// <summary>
    /// Depeding on both existing and connecting door points, calculates the two cross points
    /// </summary>
    /// <param name="existingDoorPoint">Door point for existing room</param>
    /// <param name="connectingDoorPoint">Door point for connecting room</param>
    /// <returns>Two points that represent the zig zag cross points</returns>
    private (Vector2Int, Vector2Int) GetCrossPoints((Direction dir, Vector2Int index) existingDoorPoint,
                                                (Direction dir, Vector2Int index) connectingDoorPoint)
    {
        (Vector2Int crossPoint1, Vector2Int crossPoint2) crossPoints = (Vector2Int.zero, Vector2Int.zero);

        int roundedXMidpoint = RoundedMidPoint(existingDoorPoint.index.x, connectingDoorPoint.index.x);
        int roundedYMidpoint = RoundedMidPoint(existingDoorPoint.index.y, connectingDoorPoint.index.y);

        // Calulate the first cross point depending on the direction of the first segment
        if (existingDoorPoint.dir == Direction.North || existingDoorPoint.dir == Direction.South)
            crossPoints.crossPoint1 = new Vector2Int(existingDoorPoint.index.x,
                            roundedYMidpoint);
        else
            crossPoints.crossPoint1 = new Vector2Int(roundedXMidpoint, existingDoorPoint.index.y);


        // Calculate the second cross point depending on the direction of the second segment
        if(connectingDoorPoint.dir == Direction.North || connectingDoorPoint.dir == Direction.South)
            crossPoints.crossPoint2 = new Vector2Int(connectingDoorPoint.index.x, roundedYMidpoint);
        else
            crossPoints.crossPoint2 = new Vector2Int(roundedXMidpoint, connectingDoorPoint.index.y);

        return crossPoints;
    }

    /// <summary>
    /// Returns the midpoint between two values, rounding up or down randomly
    /// </summary>
    /// <param name="a">Point 1</param>
    /// <param name="b">Point 2</param>
    /// <returns>Rounded Midpoint</returns>
    private int RoundedMidPoint(int a, int b)
    {
        float sum = a + b;
        float midpoint = sum / 2;

        if (sum % 2 != 0)
        {
            // Randomly round up or down
            int randomValue = Random.Range(0, 2); // Returns 0 or 1
            if (randomValue == 1)
                midpoint += .5f;
            else
                midpoint -= .5f;
        }

        return (int)midpoint;
    }
    #endregion

    #endregion



    private void OnDrawGizmos()
    {
        if (minGizmo != Vector2.zero && maxGizmo != Vector2.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3((minGizmo.x + maxGizmo.x) / 2, (minGizmo.y + maxGizmo.y) / 2, 0), new Vector3(maxGizmo.x - minGizmo.x, maxGizmo.y - minGizmo.y, 0));
        }
    }
}

[Serializable]
public class RoomInfo
{
    public string fileName;
    public int width;
    public int height;
    public bool[,] groundArray;
    public bool[,] wallArray;
    public List<(Direction dir, Vector2Int index)> doors;
}

public class TiledRoom
{
    public Vector2Int offset;
    public List<(Direction dir, Vector2Int index)> doors;

    private RoomInfo roomInfo;
    private GraphNode node;

    public GraphNode Node { get { return node; } }

    public TiledRoom(RoomInfo roomInfo, GraphNode node)
    {
        this.roomInfo = roomInfo;
        this.node = node;

        doors = roomInfo.doors;
    }
}


public enum Direction
{
    North,
    East,
    South,
    West
}

public enum PathType
{
    Straight,
    LShape,
    ZigZag,
    // TShape,
    // Cross
}
