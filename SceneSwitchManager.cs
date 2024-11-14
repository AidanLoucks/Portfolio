using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSwitchManager
{
    public static void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu").completed += (AsyncOperation o) =>
        {
            GameManager.instance.MainMenuStarted();
            UIManager.instance.CreateMenuMenuCanvas();
            AudioManager.instance.PlayMenuTrack();
        };
    }
    public static void LoadHub()
    {
        SceneManager.LoadSceneAsync("HUB").completed += (AsyncOperation o) =>
        {
            UIManager.instance.CreateGameCanvas();
            GameManager.instance.HubStarted();
            AudioManager.instance.PlayHubTrack();
        };
    }

    public static void LoadLevel(int levelNum)
    {
        SceneManager.LoadSceneAsync("Froguelike").completed += (AsyncOperation o) =>
        {
            UIManager.instance.CreateGameCanvas();
            GameManager.instance.LevelStarted();
            AudioManager.instance.PlayLevelTrack();
            SkillTree.instance.LoadSkillTree();
        };
    }

    public static void LoadWinnerScene()
    {
        SceneManager.LoadSceneAsync("WinnerScene").completed += (AsyncOperation o) =>
        {
            GameManager.instance.WinnerSceneStarted();
        };
    }
}
