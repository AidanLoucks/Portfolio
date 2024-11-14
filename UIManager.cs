using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Singleton
    [HideInInspector] public static UIManager instance;

    [SerializeField] private GameObject menuCanvasPrefab;
    [SerializeField] private GameObject gameCanvasPrefab;

    private GameObject currentCanvas;


    // private bool isMenuCanvasActive;

    private void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Debug.Log("UI Manager instance already exists");
            return;
        }
        instance = this;
        #endregion
    }


    /// <summary>
    /// If current canvas is not the menu canvas, create the menu canvas
    /// </summary>
    public void CreateMenuMenuCanvas()
    {
        //isMenuCanvasActive = true;

        // if the current canvas is the menu canvas, do nothing
        if (currentCanvas != null && currentCanvas.CompareTag("MenuCanvas"))
            return;

        ChangeCanvas(menuCanvasPrefab);
    }

    /// <summary>
    /// If current canvas isnt a game canvas, create the game canvas
    /// </summary>
    public void CreateGameCanvas()
    {
        //isMenuCanvasActive = false;

        // if the current canvas is the game canvas, do nothing
        if (currentCanvas != null && currentCanvas.CompareTag("GameCanvas"))
            return;

        ChangeCanvas(gameCanvasPrefab);
    }

    /// <summary>
    /// If there is a current canvas, destroy it and create a new one
    ///    (If there isnt one, itll still create the new one)
    /// </summary>
    /// <param name="newCanvasPrefab">Canvas Prefab to be instantiated</param>
    public void ChangeCanvas(GameObject newCanvasPrefab)
    {
        // If current canvas exists, destroy it
        if(currentCanvas != null)
        {
            Destroy(currentCanvas);
        }

        // create new canvas and hold reference
        currentCanvas = Instantiate(newCanvasPrefab);
        DontDestroyOnLoad(currentCanvas);

        // If the new canvas is the game canvas, set up the dialogue manager
        DialogueBoxComponents components = currentCanvas.GetComponent<DialogueBoxComponents>();

        // This is null when the menu canvas is created
        // If so, skip the next part
        if (components == null)
            return;

        // aka. only set components of Game Canvas

        // Give reference to the dialogue manager
        DialogueManager.instance.SetUpManager(components);
        RectTransform rectTransform = components.dialogueBox.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -405);
    }
}
