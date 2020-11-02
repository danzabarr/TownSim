using System.Collections;
using System.Collections.Generic;
using TownSim.IO;
using TownSim.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Loading,
    Play,
    Pause,
    Cutscene,
    GameOver
}

public enum PlayMode
{
    Play,
    Build,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static GameState State { get; private set; }
    public static SaveData LoadedData { get; private set; }
    private static Coroutine loadRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool Load(SaveData data)
    {
        if (data == null)
            return false;

        if (loadRoutine != null)
            return false;

        LoadedData = data;
        SceneManager.LoadScene("Game");

        return true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            Time.timeScale = 0;
            State = GameState.Loading;
            TownSim.UI.LoadingScreen ls = FindObjectOfType<TownSim.UI.LoadingScreen>();
            ls.gameObject.SetActive(true);
            ls.LoadingMessage = "";
            ls.LoadingProgress = 0;
            loadRoutine = Instance.StartCoroutine(LoadRoutine());
        }
    }

    private static IEnumerator LoadRoutine()
    {
        TownSim.UI.LoadingScreen ls = FindObjectOfType<TownSim.UI.LoadingScreen>();
        Map map = Map.Instance;

        {
            map.ClearAll();

            if (GenerationSettings.Load(LoadedData.generationSettings, out GenerationSettings settings))
            {
                map.settings = settings;
            }
            else
            {
                Debug.Log("Couldn't find the generation settings for this saved game.");
                LoadingFailed();
                yield break;
            }

            map.Reseed(LoadedData.seed);

            ls.LoadingProgress = .2f;
            yield return null;
        }


        {
            map.AddTile(0, 0, true);
            ls.LoadingProgress = .5f;
            yield return null;
        }


        {
            PathFinding.Request.StartThreads();
            ls.LoadingProgress = .8f;
            yield return null;
        }

        ls.gameObject.SetActive(false);
        Time.timeScale = 1;
        loadRoutine = null;
        State = GameState.Play;
    }

    private static void LoadingFailed()
    {
        Debug.LogError("Loading failed.");
        SceneManager.LoadScene("Main Menu");
    }
}
