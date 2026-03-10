using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] bigAsteroids;
    public GameObject[] mediumAsteroids;
    public GameObject[] smallAsteroids;

    public Dictionary<ASTEROID.Type, GameObject[]> asteroids;

    [Header("Progressione Livelli")]
    public int numInitialAsteroids = 4;
    public int currentNumAsteroids;
    public float spawnRadius = 3;

    public int currentLevel = 1;
    public int maxAsteroids = 10;
    public int levelForDoubleUFO = 4;

    public int lives = 3;
    public int score = 0;
    private int nextExtraLifeScore = 4000;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public Transform livesContainer;
    public GameObject lifeIconPrefab; 

    [Header("Nemici (UFO)")]
    public GameObject ufoPrefab;
    public int activeUFOs = 0;


    [Header("Tempi di Spawn")]
    public float levelStartDelay = 3.0f; 
    public float respawnDelay = 2.0f; 
    public float ufoSpawnDelay = 3.0f;

    [Header("Audio SFX")]
    public AudioClip extraLifeSound;
    private AudioSource audioSource; 

    [Header("Musica di Sottofondo")]
    public AudioClip bgMusic;
    private AudioSource bgmSource;

    [Header("Scene")]
    public string gameOverSceneName = "GameOver";

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Distrugge il GameManager appena creato se ne esiste già un altro
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        asteroids = new Dictionary<ASTEROID.Type, GameObject[]>
        {
            { ASTEROID.Type.Big, bigAsteroids },
            { ASTEROID.Type.Medium, mediumAsteroids },
            { ASTEROID.Type.Small, smallAsteroids }
        };
    }

    private void Start()
    {
        SpawnInitialAsteroid();
        UpdateScoreUI();


        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (bgMusic != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = bgMusic;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;
            bgmSource.Play();
        }

        // Fa sì che le icone delle vite non lampeggino a inizio partita
        UpdateLivesUI(false);
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();

        // Check per la soglia della vita extra
        if (score >= nextExtraLifeScore)
        {
            lives++;
            nextExtraLifeScore += 4000;

            // Aggiorna l'UI e lampeggio per la nuova icona
            UpdateLivesUI(true);

            Debug.Log("<color=green>VITA EXTRA! Vite: " + lives + "</color>");

            if (extraLifeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(extraLifeSound);
            }
        }
    }

    // GESTIONE INTERFACCIA VITE

    // Il parametro 'blinkNew' decide se la nuova icona deve lampeggiare (utile quando si ottiene 1Up)
    public void UpdateLivesUI(bool blinkNew = false)
    {
        if (livesContainer == null || lifeIconPrefab == null) return;

        int currentIcons = livesContainer.childCount;

        if (currentIcons < lives)
        {
            for (int i = currentIcons; i < lives; i++)
            {
                GameObject newIcon = Instantiate(lifeIconPrefab, livesContainer);

                // Avvia l'animazione di lampeggio sull'icona appena creata, se si ha più vite
                if (blinkNew)
                {
                    StartCoroutine(BlinkLifeIcon(newIcon));
                }
            }
        }
        // Distrugge l'ultima icona a destra se si ha meno vite
        else if (currentIcons > lives)
        {
            for (int i = currentIcons - 1; i >= lives; i--)
            {
                Destroy(livesContainer.GetChild(i).gameObject); // Distrugge l'ultima icona a destra
            }
        }
    }

    // Coroutine che fa accendere e spegnere rapidamente l'icona della vita per un secondo
    IEnumerator BlinkLifeIcon(GameObject icon)
    {
        if (icon == null) yield break;

        // (sistema che lavora sul component Image)
        Image iconImage = icon.GetComponent<Image>();
        if (iconImage == null) yield break;

        float blinkDuration = 1.0f;
        float blinkInterval = 0.15f;
        float elapsedTime = 0f;

        while (elapsedTime < blinkDuration)
        {
            if (iconImage == null) break; // Interrompe se l'icona viene distrutta nel frattempo

            iconImage.enabled = !iconImage.enabled; // Alterna visibile/invisibile
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        // Assicura che alla fine dell'animazione l'icona rimanga accesa
        if (iconImage != null)
        {
            iconImage.enabled = true;
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    // --- GESTIONE DEL FLUSSO DEL GIOCO (LIVELLI E NEMICI) ---

    public void AsteroidDestroyed()
    {
        currentNumAsteroids--;
        // Se lo schermo è pulito dagli asteroidi, inizia la procedura di "fine ondata" (UFO + Prossimo livello)
        if (currentNumAsteroids <= 0) StartCoroutine(SpawnUFOCoroutine());
    }

    // Spawna gli UFO
    public void SpawnUFO(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Instantiate(ufoPrefab, new Vector3(-20, 0, 0), Quaternion.identity);
            activeUFOs++;
        }
    }

    // Coroutine che segna la "transizione" tra un'ondata e l'altra
    IEnumerator SpawnUFOCoroutine()
    {
        yield return new WaitForSeconds(ufoSpawnDelay);

        // Nei livelli più alti iniziano a nascere due UFO contemporaneamente
        int ufosToSpawn = (currentLevel >= levelForDoubleUFO) ? 2 : 1;
        SpawnUFO(ufosToSpawn);

        yield return new WaitForSeconds(levelStartDelay);
        StartNextLevel(); // Fa iniziare il livello successivo (spawna più asteroidi)
    }

    public void OnUFODefeated()
    {
        activeUFOs--;
        if (activeUFOs < 0) activeUFOs = 0;
    }

    void StartNextLevel()
    {
        currentLevel++;
        // Aumenta la difficoltà, aumentando numero iniziale di asteroidi
        if (numInitialAsteroids < maxAsteroids) numInitialAsteroids++;
        StartCoroutine(SpawnAsteroidsGradually());
    }

    // Fa apparire i nuovi asteroidi, uno per volta
    IEnumerator SpawnAsteroidsGradually()
    {
        currentNumAsteroids = 0;
        for (int i = 0; i < numInitialAsteroids; i++)
        {
            // Calcola una posizione appena oltre il bordo dello schermo
            float safeRadius = (ScreenWrapper.worldWidth > 0 ? ScreenWrapper.worldWidth : 15f) + 4f;
            float randomAngle = Random.Range(0f, 360f);
            Vector3 spawnPos = new Vector3(Mathf.Cos(randomAngle) * safeRadius, Mathf.Sin(randomAngle) * safeRadius, 0);

            SpawnAsteroid(spawnPos, ASTEROID.Type.Big);

            yield return new WaitForSeconds(respawnDelay); // Aspetta prima di generare il successivo asteroide
        }
    }

    // Genera gli asteroidi iniziali alla partenza del gioco (Livello 1)
    private void SpawnInitialAsteroid()
    {
        for (int i = 0; i < numInitialAsteroids; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector3 spawnPos = new Vector3(Mathf.Cos(randomAngle) * spawnRadius, Mathf.Sin(randomAngle) * spawnRadius, 0);
            SpawnAsteroid(spawnPos, ASTEROID.Type.Big);
        }
    }

    // Funzione base per istanziare un asteroide di un tipo specifico in una posizione specifica
    public void SpawnAsteroid(Vector2 position, ASTEROID.Type type)
    {
        GameObject asteroidPrefab = asteroids[type][Random.Range(0, asteroids[type].Length)];
        Instantiate(asteroidPrefab, position, Quaternion.identity);
        currentNumAsteroids++;
    }

    public void TriggerGameOver()
    {
        if (bgmSource != null) bgmSource.Stop();

        MainMenuController.CheckAndSaveScore(score);

        SceneManager.LoadScene(gameOverSceneName);
    }

    public void PauseMusic()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    public void ResumeMusic()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}