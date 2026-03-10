using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [Header("Impostazioni Scena")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip pauseSound; 
    private AudioSource audioSource;

    private bool isPaused = false; 

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Quando Time.timeScale è a 0, Unity blocca anche i suoni di default.  L' AudioSource ignora la pausa globale, permettendo ai suoni dei bottoni di funzionare anche se il gioco è frizzato
        audioSource.ignoreListenerPause = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (audioSource != null && pauseSound != null)
        {
            audioSource.PlayOneShot(pauseSound);
        }

        pausePanel.SetActive(true); 
        Time.timeScale = 0f; 
        isPaused = true; 

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseMusic();
        }
    }

    public void Resume()
    {
        PlaySound(); 

        pausePanel.SetActive(false); 
        Time.timeScale = 1f;
        isPaused = false; // 

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeMusic();
        }
    }

    public void Retry()
    {
        StartCoroutine(RetryCoroutine());
    }

    public void LoadMainMenu()
    {
        StartCoroutine(MainMenuCoroutine());
    }

    private void PlaySound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    // --- COROUTINE  ---

    IEnumerator RetryCoroutine()
    {
        PlaySound();

        yield return new WaitForSecondsRealtime(0.2f);

        Time.timeScale = 1f;

        if (GameManager.Instance != null) Destroy(GameManager.Instance.gameObject);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator MainMenuCoroutine()
    {
        PlaySound();
        yield return new WaitForSecondsRealtime(0.2f);

        Time.timeScale = 1f;

        if (GameManager.Instance != null) Destroy(GameManager.Instance.gameObject);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}