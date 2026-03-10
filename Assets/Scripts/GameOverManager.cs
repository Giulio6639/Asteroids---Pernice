using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Nomi delle Scene")]
    public string gameSceneName = "MainScene";
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio SFX")]
    public AudioClip selectSound;
    public AudioClip backSound; 
    private AudioSource audioSource; 

    [Header("Musica di Sottofondo")]
    public AudioClip bgMusic;
    private AudioSource bgmSource;

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
    }


    public void Retry()
    {
        StartCoroutine(PlaySoundAndLoad(gameSceneName, selectSound));
    }

    public void MainMenu()
    {
        StartCoroutine(PlaySoundAndLoad(mainMenuSceneName, backSound));
    }

    public void Quit()
    {
        Debug.Log("Gioco chiuso!");
        StartCoroutine(PlaySoundAndQuit(backSound));
    }

    // --- COROUTINE ---

    // Funzione fatta per far sentire il SFX per i tasti, per poi caricare la scena
    IEnumerator PlaySoundAndLoad(string scene, AudioClip sound)
    {
        if (audioSource != null && sound != null) audioSource.PlayOneShot(sound);

        // Aspetta "x" secondi
        yield return new WaitForSecondsRealtime(0.2f);

        // Distruzione GameManager
        if (GameManager.Instance != null) Destroy(GameManager.Instance.gameObject);

        SceneManager.LoadScene(scene);
    }

    // Funzione fatta per far sentire il SFX per i tasti, per poi uscire dal gioco
    IEnumerator PlaySoundAndQuit(AudioClip sound)
    {
        if (audioSource != null && sound != null) audioSource.PlayOneShot(sound);
        yield return new WaitForSecondsRealtime(0.2f); // Aspetta che finisca il click
        Application.Quit();
    }
}