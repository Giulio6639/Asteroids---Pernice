using UnityEngine;

public class EnemyUFO : MonoBehaviour
{
    public float speed = 5f;
    public float fireRate = 1.5f;
    public GameObject enemyBulletPrefab;
    public int points = 200;
    public GameObject explosionEffect;

    [Header("Audio")]
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 1f; 

    public AudioClip explosionSound;
    [Range(0f, 1f)] public float explosionVolume = 1f;

    // VARIABILI INTERNE (Non visibili nell'Inspector)
    private float nextFireTime; // Momento esatto in cui l'UFO potrà sparare di nuovo
    private int direction = 1; // Direzione (1 Destra, 1 Sinistra

    void Start()
    {
        // --- SPAWN CASUALE ---

        // 50% di probabilità di nascere a sinistra o a destra
        if (Random.value > 0.5f)
        {
            direction = 1;
            // Altezza casuale
            transform.position = new Vector3(-ScreenWrapper.worldWidth, Random.Range(-3f, 3f), 0);
        }
        else
        {
            direction = -1;
            // Altezza casuale
            transform.position = new Vector3(ScreenWrapper.worldWidth, Random.Range(-3f, 3f), 0);
        }

        // Timer per il primo sparo
        nextFireTime = Time.time + fireRate;
    }

    void Update()
    {
        // MOVIMENTO

        // L'UFO si muove sempre nell'asse X costantemente
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // USCITA DALLO SCHERMO

        // Ciò che accade se l'UFO fuoriesce dallo schermo:
        if (Mathf.Abs(transform.position.x) > ScreenWrapper.worldWidth + 2f)
        {
            GameManager.Instance.OnUFODefeated();
            GameManager.Instance.SpawnUFO(1); // Ne fa nascere 1 altro dopo 1 secondo
            Destroy(gameObject);
        }

        // GESTIONE DELLO SPARO     

        // Controlla se il tempo attuale ha superato il tempo prefissato per il prossimo sparo
        if (Time.time >= nextFireTime)
        {
            ShootRandomly();
            nextFireTime = Time.time + fireRate; // Resetta il timer per lo sparo successivo
        }
    }

    void ShootRandomly()
    {
        // LOGICA DELLO SPARO 

        // Crea una rotazione casuale sull'asse Z
        Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        // Crea il proiettile nella posizione dell'UFO con la rotazione casuale
        Instantiate(enemyBulletPrefab, transform.position, randomRotation);

        if (shootSound != null)
        {
            GameManager.Instance.PlaySFX(shootSound, shootVolume);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // GESTIONE COLLISIONI

        if (collision.CompareTag("Bullet"))
        {
            GameManager.Instance.AddScore(points);
            Destroy(collision.gameObject);
            Die();
        }
        else if (collision.CompareTag("Player"))
        {
            Die();
        }
    }

    void Die()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (explosionSound != null)
        {
            GameManager.Instance.PlaySFX(explosionSound, explosionVolume);
        }

        GameManager.Instance.OnUFODefeated();

        Destroy(gameObject);
    }
}