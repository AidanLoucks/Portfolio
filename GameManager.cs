using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public static GameManager instance;

    [HideInInspector] public List<GameObject> players = new List<GameObject>();
    [HideInInspector] public Room currentRoom; // Last room the player fought enemies in
    [HideInInspector] public int levelNum;

    private GameCanvas gameCanvas;
    private InputManage inputManager;
    private CameraManager gameCamera;
    private SkillTree skillTree;

    
    private Vector3 lvlStartPos;
    public Vector2 dojoPlayerSpawn;
    public Vector2[] npcSpawns;
    public Vector2[] npcBuildingSpawns;


    private bool needsReset = false;

    [Header("Testing")]
    public bool testScene;
    public int startLvl = 1;

    public GameCanvas GameCanvas
    {
        get { return gameCanvas; }
        set { gameCanvas = value; }
    }

    public InputManage InputManager
    {
        get { return inputManager; }
        set { inputManager = value; }
    }

    public CameraManager CameraManager
    {
        get { return gameCamera; }
        set { gameCamera = value; }
    }

    public Vector2 PlayerPos
    {
        get{ return players[0].transform.position; }
    }

    // Singleton
    private void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Debug.Log("Game Manager instance already exists");
            Destroy(transform.parent.gameObject); // Destory the Manager obj instead of indvidual managers
            return;
        }
        instance = this;
        #endregion
    }

    // Start is called before the first frame update
    void Start()
    {

        if (testScene)
        {
            SceneSwitchManager.LoadLevel(startLvl);
        }
        else
        {
            SceneSwitchManager.LoadMainMenu();
        }

        levelNum = startLvl;

        // Get refrence to all active players
        players = InputInit.instance.players;

        foreach (var player in players)
        {
            // Make sure player doesnt get destoryed moving scene to scene
            DontDestroyOnLoad(player);

            //If the current scene is the main menu, disable the player
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
                player.SetActive(false);
        }

        if(GameObject.Find("EventSystem") == null)
        {
            Instantiate(Resources.Load("EventSystem"));
        }
    }

    public void MainMenuStarted()
    {
        //gameCamera.LockCamera(Vector2.zero);
        foreach(var player in players)
        {
            // player.GetComponent<Player>().ResetPlayer();
            player.SetActive(false);
        }

        needsReset = true;
    }


    /// <summary>
    /// Method to run needed code to start a level
    /// </summary>
    public void LevelStarted()
    {
        players[0].SetActive(true);
        players[0].GetComponent<PlayerStateMachine>().SetToIdleState();

        //gameCamera.UnlockCamera();
        gameCanvas.TurnOnMiniMap();

        ProceduralGeneration procGen = ProceduralGeneration.instance;

        List<NPC> npcs = new List<NPC>();

        // Adds a discoverable NPC to the lvl
        npcs.Add(NPCManager.instance.GetDiscoverableNPC());

        // If there is an npc with a mission queued, add them to the level
        if(NPCManager.instance.NPCQueue.Count > 0)
            npcs.Add(NPCManager.instance.NPCQueue.Peek());

        // Create the lvl
        procGen.CreateLevel(levelNum, npcs);

        // Get starting pos 
        lvlStartPos = procGen.StartPosition;

        GameObject mainCamera = GameObject.Find("Game Camera");
        mainCamera.transform.position = new Vector3(lvlStartPos.x, lvlStartPos.y, -100);

        // set player location to the start room
        foreach (GameObject player in InputInit.instance.players)
        {
            player.transform.position = lvlStartPos;
        }

        // Spawn a cornflower blue background behind all rooms
        Camera.main.backgroundColor = new Color(0.2941177f, 0.3568628f, 0.6705883f, 1f);

    }

    /// <summary>
    /// Code to run going back to the hub
    /// </summary>
    public void HubStarted()
    {
        players[0].SetActive(true);
        players[0].transform.position = dojoPlayerSpawn;


        // If the game is being reset, this is when it happens
        if (needsReset)
            ResetGame();

        gameCanvas.TurnOffMiniMap();

        NPCManager.instance.SpawnHubNPCs(npcSpawns, npcBuildingSpawns);

        Camera.main.backgroundColor = new Color(0, 0, 0, 1);
    }


    public void WinnerSceneStarted()
    {
        players[0].transform.position = Vector3.zero;
    }

    public void PlayerDied()
    {
        needsReset = true;
    }

    /// <summary>
    /// Reset the game back to the start
    /// </summary>
    public void ResetGame()
    {
        players[0].GetComponent<Player>().ResetPlayer();
        NPCManager.instance.ResetNPCs();
        gameCanvas.DestroyBossHealthBar();
        needsReset = false;
        skillTree.LoadSkillTree();
    }
}