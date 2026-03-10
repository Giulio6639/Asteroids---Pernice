using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Collider2D playerCollider;
    AudioSource audioSource;

    [Header("Movement")]
    public float forwardSpeed = 10f;
    public float turnSpeed = 200f;

    [Header("Combat")]
    public GameObject bulletPrefab; 
    public GameObject explosionEffect; 
    public float respawnDelay = 3.0f; 

    [Header("Invulnerabilità")]
    public float invulnerabilityDuration = 2.0f; 
    public float blinkInterval = 0.15f;
    private bool isInvulnerable = false; 

    [Header("Audio")]
    public AudioClip shootSound; 
    public AudioClip deathSound;
    public AudioClip respawnSound; 
    public AudioClip teleportSound; 

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        if (GameManager.Instance != null) GameManager.Instance.UpdateLivesUI();

        StartCoroutine(InvulnerabilitySequence());
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(bulletPrefab, transform.position, transform.rotation);
            if (shootSound != null) audioSource.PlayOneShot(shootSound);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) TryTeleport();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        float forwardInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (forwardInput > 0) rb.AddForce(transform.up * forwardSpeed * forwardInput);

        if (turnInput != 0)
        {
            float rotationAmount = -turnInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotationAmount);
        }
    }
    void TryTeleport()
    {
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        TeleportRandomly();

        // Azzera la velocità e l'inerzia
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void TeleportRandomly()
    {
        // Calcola i bordi esatti della telecamera per la grandezza dello schermo
        float distanceZ = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector2 screenBottomleft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, distanceZ));
        Vector2 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - 1, Screen.height - 1, distanceZ));

        // Coordinate X e Y casuali
        float randomX = Random.Range(screenBottomleft.x, screenTopRight.x);
        float randomY = Random.Range(screenBottomleft.y, screenTopRight.y);

        transform.position = new Vector3(randomX, randomY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || isInvulnerable) return;

        if (collision.CompareTag("ASTEROID") || collision.CompareTag("EnemyBullet") || collision.CompareTag("Enemy"))
        {
            if (collision.CompareTag("EnemyBullet")) Destroy(collision.gameObject);

            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator DeathSequence()
    {
        isDead = true; // Blocca i comandi del giocatore

        if (deathSound != null && audioSource != null) audioSource.PlayOneShot(deathSound);
        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);

        GameManager.Instance.lives--;
        GameManager.Instance.UpdateLivesUI();

        spriteRenderer.enabled = false;
        playerCollider.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (GameManager.Instance.lives <= 0)
        {
            Debug.Log("<color=red>GAME OVER INIZIATO. Attesa di 2 secondi...</color>");
            yield return new WaitForSeconds(2f);
            GameManager.Instance.TriggerGameOver(); 
        }
        else
        {
            yield return new WaitForSeconds(respawnDelay);
            Respawn();
        }
    }

    void Respawn()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Riaccende la grafica e le collisioni
        spriteRenderer.enabled = true;
        playerCollider.enabled = true;

        if (respawnSound != null && audioSource != null) audioSource.PlayOneShot(respawnSound);

        // Sblocca i comandi
        isDead = false;

        StartCoroutine(InvulnerabilitySequence());
    }

    IEnumerator InvulnerabilitySequence()
    {
        isInvulnerable = true;
        float elapsedTime = 0f;

        // Loop che fa accendere e spegnere la grafica finché non scade il timer
        while (elapsedTime < invulnerabilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Inverte: se era acceso si spegne, viceversa
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }
}