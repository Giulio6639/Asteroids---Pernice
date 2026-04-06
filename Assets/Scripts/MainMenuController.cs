using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Pannelli UI")]
    public GameObject mainMenuPanel;
    public GameObject leaderboardPanel;

    [Header("Impostazioni Scena")]
    public string gameSceneName = "MainScene";

    [Header("Testi Leaderboard")]
    public TextMeshProUGUI[] scoreTexts;
    private const int maxScores = 3;

    [Header("Audio SFX")]
    public AudioClip selectSound;
    public AudioClip backSound;
    private AudioSource audioSource; 

    [Header("Musica di Sottofondo")]
    public AudioClip bgMusic; 
    private AudioSource bgmSource; 

    private bool isStarting = true;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (bgMusic != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = bgMusic;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f; 
            bgmSource.Play();
        }

        ShowMainMenu();

        isStarting = false;
    }

    public void PlayGame()
    {
        StartCoroutine(PlaySoundAndLoad(gameSceneName, selectSound));
    }

    public void QuitGame()
    {
        Debug.Log("Uscita dal gioco...");
        StartCoroutine(PlaySoundAndQuit(backSound));
    }

    public void OpenLeaderboard()
    {
        if (audioSource != null && selectSound != null) audioSource.PlayOneShot(selectSound);

        mainMenuPanel.SetActive(false);
        leaderboardPanel.SetActive(true);

        UpdateLeaderboardUI();
    }

    public void ShowMainMenu()
    {
        if (!isStarting && audioSource != null && backSound != null)
        {
            audioSource.PlayOneShot(backSound);
        }

        leaderboardPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // --- COROUTINE ---

    IEnumerator PlaySoundAndLoad(string scene, AudioClip sound)
    {
        if (audioSource != null && sound != null) audioSource.PlayOneShot(sound);

        yield return new WaitForSecondsRealtime(0.2f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    IEnumerator PlaySoundAndQuit(AudioClip sound)
    {
        if (audioSource != null && sound != null) audioSource.PlayOneShot(sound);
        yield return new WaitForSecondsRealtime(0.2f);
        Application.Quit();
    }

    // --- GESTIONE LOGICA DELLA CLASSIFICA ---

    // Legge i punteggi salvati e li scrive nei testi a schermo
    void UpdateLeaderboardUI()
    {
        for (int i = 0; i < maxScores; i++)
        {
            // Si cerca il salvataggio ("HighScore0", "1", "2"). Se non c'è, restituisce 0
            int score = PlayerPrefs.GetInt("HighScore" + i, 0);

            // Verifica che il testo esista nell'array prima di scriverci sopra
            if (scoreTexts.Length > i && scoreTexts[i] != null)
            {
                scoreTexts[i].text = (i + 1) + ". " + score;

                if (i == 0)
                {
                    scoreTexts[i].color = new Color(1f, 0.84f, 0f);
                }
                else
                {
                    scoreTexts[i].color = Color.white;
                }
            }
        }
    }

    // Check di fine partita se si ha fatto un nuovo record
    public static void CheckAndSaveScore(int newScore)
    {
        for (int i = 0; i < maxScores; i++)
        {
            int currentHighScore = PlayerPrefs.GetInt("HighScore" + i, 0);

            if (newScore > currentHighScore)
            {
                for (int j = maxScores - 1; j > i; j--)
                {
                    PlayerPrefs.SetInt("HighScore" + j, PlayerPrefs.GetInt("HighScore" + (j - 1), 0));
                }

                PlayerPrefs.SetInt("HighScore" + i, newScore);

                PlayerPrefs.Save();

                break;
            }
        }
    }
}