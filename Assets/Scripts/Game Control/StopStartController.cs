using Assets.EventSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StopStartController : MonoBehaviour
{
    public static bool gameIsFinished;
    public static bool gameIsActive;
    [Header("Start And Stop")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject stopButton;
    [Header("Event References")]
    [SerializeField] private GameStoppedEvent gameStoppedEvent;
    [SerializeField] private AsteroidDestroyedEvent asteroidDestroyedEvent;
    [SerializeField] private LoadNextLevelEvent loadNextLevelEvent;
    [Header("Asteroid Count")]
    private int numAsteroidsTotal;
    private int currentNumAsteroids;

    private void Awake()
    {
        Launcher[] launchers = FindObjectsOfType<Launcher>();
        numAsteroidsTotal = launchers.Length;
        stopButton.SetActive(false);
        gameIsActive = false;
        gameIsFinished = false;
    }
    private void OnEnable()
    {
        asteroidDestroyedEvent.AddListener(OnAsteroidDestroyed);
    }
    private void OnDisable()
    {
        asteroidDestroyedEvent.RemoveListener(OnAsteroidDestroyed);
    }

    public void Activate()
    {
        if (gameIsFinished)
            return;
        if (currentNumAsteroids > 0)
        {
            if (this is TutorialStartStopController tutor)
                tutor.TutorialStopGame();
            else
                StopGame(true);
        }
        else
        {
            if (this is TutorialStartStopController tutor)
                tutor.TutorialStartGame();
            else
                StartGame();
        }
    }

    public void StartGame()
    {
        foreach (Launcher launcher in FindObjectsOfType<Launcher>())
        {
            launcher.Launch();
        }
        startButton.SetActive(false);
        stopButton.SetActive(true);
        gameIsActive = true;
        currentNumAsteroids = numAsteroidsTotal;
    }

    public void StopGame(bool resetGenerators)
    {
        if (resetGenerators)
        {
            foreach (Generator generator in FindObjectsOfType<Generator>())
            {
                generator.RemoveInstance();
                generator.Generate();
            }
        }
        gameStoppedEvent.Raise(this, null);
        startButton.SetActive(true);
        stopButton.SetActive(false);
        gameIsActive = false;
        currentNumAsteroids = 0;
    }

    public void StopGame()
    {
        StopGame(true);
    }

    public void OnAsteroidDestroyed(object sender, EventParameters args)
    {
        currentNumAsteroids--;
        if (currentNumAsteroids <= 0)
        {
            StopGame(true);
        }
    }

    public void NextStage()
    {
        RaiseHighestLevel();
        loadNextLevelEvent.Raise(this, null);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void RaiseHighestLevel()
    {
        int PrevHighestLevel = StageSelectPanel.GetHighestLevelIndex();
        int levelToBeLoaded = SceneManager.GetActiveScene().buildIndex + 1;
        PlayerPrefs.SetInt("Highest Level", Mathf.Max(PrevHighestLevel, levelToBeLoaded));
    }

}
