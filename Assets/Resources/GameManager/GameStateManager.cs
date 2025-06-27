using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStateManager : MonoBehaviour
{
    public enum GameState { Cutscene, Start, Playing, Paused, GameOver, Won }
    public GameState currentState;

    [Header("Level Config")]
    public LevelConfig levelConfig;

    [Header("Managers")]
    public GameFlowManager gameFlowManager;
    public AlmanacManager almanacManager; // Add reference to AlmanacManager
    public PlayerConfig playerConfig; // Add reference to PlayerConfig

    [Header("Cutscene")]
    public GameObject cutscenePanel;
    public Image storyboardImage;
    public TMP_Text narrativeText;
    public Button skipButton, nextButton, previousButton;

    [Header("Start")]
    public GameObject startPanel;
    public Button startButton;

    [Header("Pause")]
    public GameObject pausePanel;
    public Button resumeButton, restartButton, mainMenuButton, pauseButton;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Button gameOverRestartButton, gameOverMainMenuButton;

    [Header("Win")]
    public GameObject winPanel;
    public Button playAgainButton, nextLevelButton, winMainMenuButton;

    [Header("Audio Sources")]
    public AudioSource cutscene_BGMSource;
    public AudioSource cutscene_SFXSource;

    private int currentSlideIndex = 0;
    private static bool isRestarting = false;

    void Start()
    {
        // Validate required references
        if (levelConfig == null) Debug.LogError("LevelConfig is not assigned!");
        if (cutscene_BGMSource == null || cutscene_SFXSource == null) Debug.LogError("AudioSources for cutscene BGM or SFX are missing!");
        if (almanacManager == null) Debug.LogError("AlmanacManager is not assigned!"); // Ensure AlmanacManager is assigned

        // Handle persistent game state on level load
        if (PersistentGameManager.Instance != null)
        {
            Debug.Log("Loaded Persistent Game State:");
            Debug.Log($"Has Key: {PersistentGameManager.Instance.hasKey}");
            Debug.Log($"Current Level Index: {PersistentGameManager.Instance.currentLevelIndex}");
            Debug.Log($"Unlocked Learning Entries (Loaded): {string.Join(", ", PersistentGameManager.Instance.unlockedLearningNames)}");
            Debug.Log($"Unlocked Buff Entries (Loaded): {string.Join(", ", PersistentGameManager.Instance.unlockedBuffEntryNames)}");

            // Load the unlocked Almanac entries into the local AlmanacManager
            almanacManager.LoadUnlockedEntries(
                PersistentGameManager.Instance.unlockedLearningNames, // ALREADY A HashSet<string>
                PersistentGameManager.Instance.unlockedBuffEntryNames    // ALREADY A HashSet<string>
            );

            // Load any level-specific progress if you implemented it
            PersistentGameManager.Instance.LoadLevelProgress(levelConfig.levelName);
        }
        else
        {
            Debug.LogError("PersistentGameManager Instance is null!");
        }

        // Handle restarting logic
        if (isRestarting)
        {
            SetGameState(GameState.Start);
            isRestarting = false;
        }
        else
        {
            SetGameState(GameState.Cutscene);
            PlayBackgroundMusic(levelConfig.cutsceneBGM);
        }

        AssignButtonListeners();
        UpdateCutscene();
    }

    private void AssignButtonListeners()
    {
        startButton.onClick.AddListener(OnStartGame);
        resumeButton.onClick.AddListener(OnResumeGame);
        restartButton.onClick.AddListener(OnRestartGame);
        mainMenuButton.onClick.AddListener(OnMainMenu);
        pauseButton.onClick.AddListener(OnPauseGame);
        playAgainButton.onClick.AddListener(OnRestartGame);
        nextLevelButton.onClick.AddListener(OnNextLevel);
        winMainMenuButton.onClick.AddListener(OnMainMenu);
        gameOverRestartButton.onClick.AddListener(OnRestartGame);
        gameOverMainMenuButton.onClick.AddListener(OnMainMenu);
        skipButton.onClick.AddListener(OnSkipCutscene);
        nextButton.onClick.AddListener(NextSlide);
        previousButton.onClick.AddListener(PreviousSlide);
    }

    public void SetGameState(GameState newState)
    {
        Time.timeScale = 1; // Normalize time
        currentState = newState;

        // Deactivate all panels first
        cutscenePanel.SetActive(false);
        startPanel.SetActive(false);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        switch (currentState)
        {
            case GameState.Cutscene:
                cutscenePanel.SetActive(true);
                Time.timeScale = 0;
                break;

            case GameState.Start:
                startPanel.SetActive(true);
                Time.timeScale = 0;
                StopAudio();
                break;

            case GameState.Playing:
                // Removed gameFlowManager.StartGamePlay() from here
                break;

            case GameState.Paused:
                pausePanel.SetActive(true);
                Time.timeScale = 0;
                break;

            case GameState.GameOver:
                gameOverPanel.SetActive(true);
                Time.timeScale = 0;
                break;

            case GameState.Won:
            winPanel.SetActive(true);
            Time.timeScale = 0;
            // CALL THE LEVEL WON LOGIC HERE
            LevelWon();
            break;
        }
    }

    private void StopAudio()
    {
        if (cutscene_BGMSource != null) cutscene_BGMSource.Stop();
    }

    private void PlayBackgroundMusic(AudioClip clip)
    {
        if (cutscene_BGMSource != null && clip != null)
        {
            cutscene_BGMSource.clip = clip;
            cutscene_BGMSource.loop = true;
            cutscene_BGMSource.Play();
        }
    }

    private void PlaySlideSFX(AudioClip clip)
    {
        if (cutscene_SFXSource != null && clip != null)
        {
            cutscene_SFXSource.PlayOneShot(clip);
        }
    }

    private void UpdateCutscene()
    {
        if (levelConfig.cutsceneImages.Length > 0 && currentSlideIndex < levelConfig.cutsceneImages.Length)
        {
            storyboardImage.sprite = levelConfig.cutsceneImages[currentSlideIndex];
            narrativeText.text = levelConfig.cutsceneNarratives[currentSlideIndex];
            previousButton.interactable = currentSlideIndex > 0;

            if (currentSlideIndex < levelConfig.cutsceneImages.Length - 1)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(NextSlide);
                nextButton.GetComponentInChildren<TMP_Text>().text = "Next";
            }
            else
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(OnCutsceneStart);
                nextButton.GetComponentInChildren<TMP_Text>().text = "Start";
            }

            if (levelConfig.cutsceneSFX.Length > currentSlideIndex && levelConfig.cutsceneSFX[currentSlideIndex] != null)
            {
                PlaySlideSFX(levelConfig.cutsceneSFX[currentSlideIndex]);
            }
        }
    }

    private void NextSlide()
    {
        currentSlideIndex++;
        UpdateCutscene();
    }

    private void PreviousSlide()
    {
        currentSlideIndex--;
        UpdateCutscene();
    }

    void OnStartGame()
    {
        SetGameState(GameState.Playing);
        if (gameFlowManager != null)
        {
            gameFlowManager.StartGamePlay(); // Start the game manually
        }
    }

    private void OnRestartGame()
    {
        isRestarting = true;
        StopAudio();

        // Reset objective data (if you want to reset per level restart)
        if (levelConfig != null && levelConfig.objectives != null)
        {
            for (int i = 0; i < levelConfig.objectives.Length; i++)
            {
                LevelConfig.ObjectiveData objective = levelConfig.objectives[i];
                objective.isCompleted = false;
                objective.currentCount = 0;
                levelConfig.objectives[i] = objective; // Assign back the modified struct
            }
            Debug.Log("Objective data reset on restart.");
        }
        else
        {
            Debug.LogWarning("LevelConfig or objectives array is null, cannot reset objective data.");
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void OnMainMenu()
    {
        isRestarting = false;
        StopAudio();
        Time.timeScale = 1;

        // Reset selected effects when going to the main menu
        if (playerConfig != null)
        {
            playerConfig.selectedEffects.Clear();
            Debug.Log("Selected effects cleared on PlayerConfig.");
        }

        // Save Almanac data before going to the main menu
        if (PersistentGameManager.Instance != null && almanacManager != null)
        {
            PersistentGameManager.Instance.unlockedLearningNames = almanacManager.unlockedLearningNames; // Directly assign HashSet
            PersistentGameManager.Instance.unlockedBuffEntryNames = almanacManager.unlockedBuffEntryNames;     // Directly assign HashSet
            PersistentGameManager.Instance.SaveAlmanacData();
        }
        SceneManager.LoadScene("MainMenu");
    }

    private void OnNextLevel()
    {
        // Save Almanac data before loading the next level
        if (PersistentGameManager.Instance != null && almanacManager != null)
        {
            PersistentGameManager.Instance.unlockedLearningNames = almanacManager.unlockedLearningNames; // Directly assign HashSet
            PersistentGameManager.Instance.unlockedBuffEntryNames = almanacManager.unlockedBuffEntryNames;     // Directly assign HashSet
            PersistentGameManager.Instance.SaveAlmanacData();

            // Save any level-specific progress before loading the next scene
            PersistentGameManager.Instance.SaveLevelProgress(levelConfig.levelName);

            // Unlock the next level in the current act
            int selectedActIndex = PlayerPrefs.GetInt("SelectedActIndex", 1) - 1;
            PersistentGameManager.Instance.UnlockNextLevel(selectedActIndex, PersistentGameManager.Instance.currentLevelIndex);
            PersistentGameManager.Instance.currentLevelIndex++; // Increment for the next load
        }

        // Clear selected effects before loading the next level
        if (playerConfig != null)
        {
            playerConfig.ClearSelectedEffects();
        }

        SceneManager.LoadScene("LevelMapScene");
    }

    public void LevelWon()
    {
        Debug.Log("GameStateManager.LevelWon() called.");

        int selectedActIndex = PlayerPrefs.GetInt("SelectedActIndex", 1) - 1; // 0-based index for the current Act
        int currentLevelIndex = PersistentGameManager.Instance.currentLevelIndex; // Get the current level index

        // Get the current ActData from the ActSelectionController's allActs array
        // Make sure ActSelectionController.Instance is not null
        if (ActSelectionController.Instance == null || ActSelectionController.Instance.allActs == null || ActSelectionController.Instance.allActs.Length == 0)
        {
            Debug.LogError("ActSelectionController.Instance or its allActs array is not properly initialized!");
            // Handle this error appropriately, perhaps return or load main menu
            return;
        }

        ActData currentAct = null;
        if (selectedActIndex >= 0 && selectedActIndex < ActSelectionController.Instance.allActs.Length)
        {
            currentAct = ActSelectionController.Instance.allActs[selectedActIndex];
        }
        else
        {
            Debug.LogError($"Invalid selected Act Index: {selectedActIndex}. Cannot determine if it's the last act.");
            return;
        }


        // 1. Unlock the next level (standard progression)
        PersistentGameManager.Instance.UnlockNextLevel(selectedActIndex, currentLevelIndex);

        // 2. Check if this is the LAST level of the CURRENT act
        bool isLastLevelInCurrentAct = (currentAct != null && currentLevelIndex == currentAct.levels.Length - 1);

        // 3. Check if this is the LAST act in the entire game
        bool isLastActInGame = (selectedActIndex == ActSelectionController.Instance.allActs.Length - 1);

        // 4. Determine the next scene to load
        if (isLastLevelInCurrentAct && isLastActInGame)
        {
            Debug.Log("🎉🎊 Congratulations! Last level of the last act completed. Loading End Credits. 🎉🎊");
            // Optionally, stop any background music that might be playing in the level
            // (You might have a LevelMusicManager or similar)
            // Example: AudioManager.Instance.StopBGM();

            SceneManager.LoadScene("EndCutscene"); // <--- Load your Credits scene here!
        }
        else if (isLastLevelInCurrentAct)
        {
            // Last level of the current act, but not the last act in the game
            Debug.Log($"Act {selectedActIndex + 1} completed! Returning to Act Selection to show next act.");
            PersistentGameManager.Instance.UnlockFirstLevelNextAct(selectedActIndex); // Unlock the first level of the next act
            SceneManager.LoadScene("ActSelectionScene"); // Go back to Act Selection
        }
        else
        {
            // Not the last level of the current act, just unlock the next level and go to the level map
            Debug.Log($"Level {currentLevelIndex + 1} in Act {selectedActIndex + 1} completed. Going to Level Map.");
            // PersistentGameManager.Instance.currentLevelIndex++; // This increment is typically handled in OnNextLevel()
            SceneManager.LoadScene("LevelMapScene"); // Go back to the Level Map
        }
    }


    private void OnCutsceneStart()
    {
        StopAudio();
        SetGameState(GameState.Start);
    }

    private void OnSkipCutscene()
    {
        SetGameState(GameState.Start);
    }

    private void OnResumeGame()
    {
        SetGameState(GameState.Playing);
    }

    private void OnPauseGame()
    {
        SetGameState(GameState.Paused);
    }

    // Add this method
    public void GameOver()
    {
        SetGameState(GameState.GameOver);
    }

    private void OnApplicationQuit()
    {
        // Save Almanac data when the application quits
        if (PersistentGameManager.Instance != null && almanacManager != null)
        {
            PersistentGameManager.Instance.unlockedLearningNames = almanacManager.unlockedLearningNames; // Directly assign HashSet
            PersistentGameManager.Instance.unlockedBuffEntryNames = almanacManager.unlockedBuffEntryNames;     // Directly assign HashSet
            PersistentGameManager.Instance.SaveAlmanacData();
        }
    }
}