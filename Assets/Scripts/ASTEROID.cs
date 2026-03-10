using UnityEngine;

public class ASTEROID : MonoBehaviour
{
    public float speed;
    public float maxAngularVelocity;

    public enum Type { Big, Medium, Small } 
    public Type type; 
    public int points; 

    public GameObject Explosion;

    [Header("Effetti Sonori Esplosione")]
    public AudioClip sfxBig;
    public AudioClip sfxMedium;
    public AudioClip sfxSmall;
    [Range(0f, 1f)] public float explosionVolume = 1f;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 direction;

        // --- CALCOLO DELLA DIREZIONE DI PARTENZA ---

        // Se l'asteroide nasce fuori dallo schermo (distanza dal centro maggiore della larghezza dello schermo)
        if (Vector2.Distance(transform.position, Vector2.zero) > ScreenWrapper.worldWidth)
        {
            // Direzione verso il centro dello schermo
            Vector2 directionToCenter = (Vector2.zero - (Vector2)transform.position).normalized;
            // Variazione casuale
            Vector2 randomVariation = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            direction = (directionToCenter + randomVariation).normalized;
        }
        else
        {
            // Se nasce già dentro lo schermo, variazione casuale
            direction = Random.insideUnitCircle.normalized;
        }

        rb.AddForce(direction * speed);
        rb.angularVelocity = Random.Range(-maxAngularVelocity, maxAngularVelocity);

        switch (type)
        {
            case Type.Big: points = 20; break;
            case Type.Medium: points = 50; break;
            case Type.Small: points = 100; break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {

            if (type == Type.Big)
            {
                if (sfxBig != null) GameManager.Instance.PlaySFX(sfxBig, explosionVolume);
                GameManager.Instance.SpawnAsteroid(transform.position, Type.Medium);
                GameManager.Instance.SpawnAsteroid(transform.position, Type.Medium);
            }
            else if (type == Type.Medium)
            {
                if (sfxMedium != null) GameManager.Instance.PlaySFX(sfxMedium, explosionVolume);
                GameManager.Instance.SpawnAsteroid(transform.position, Type.Small);
                GameManager.Instance.SpawnAsteroid(transform.position, Type.Small);
            }
            else if (type == Type.Small)
            {
                if (sfxSmall != null) GameManager.Instance.PlaySFX(sfxSmall, explosionVolume);
            }

            GameManager.Instance.AddScore(points);
            GameManager.Instance.AsteroidDestroyed();

            Instantiate(Explosion, transform.position, Quaternion.identity);

            Destroy(collision.gameObject);

            Destroy(gameObject);
        }
    }
}