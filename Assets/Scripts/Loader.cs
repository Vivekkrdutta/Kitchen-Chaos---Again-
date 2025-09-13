using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        LoadingScene,
        GameScene,
        LobbyScene,
        CharacterSelectScene,
        GameScenePart1,
        GameScenePart2,
        GameScenePart3,
    }
    public static Scene targetScene;

    public static void Load(Scene scene)
    {
        targetScene = scene;
        
        // We need atleast one frame to render before calling the loadscene for the target scene.
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    // Inorder to load the players in the network , we need to use a different method
    public static void NetworkLoad(Scene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(),LoadSceneMode.Single);
    }

    public static void LoaderCallBack()
    {
        Debug.Log("Loading the scene "+targetScene.ToString());
        SceneManager.LoadScene(targetScene.ToString());
    }

    public static void NetworkLoadAdditively(Scene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(),LoadSceneMode.Additive);
    }
}
