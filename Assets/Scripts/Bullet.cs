using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 500f;

    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.up * speed);
    }

    void Update()
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.x < 0 || screenPosition.x > Screen.width || screenPosition.y < 0 || screenPosition.y > Screen.height)
        {
            Destroy(gameObject);
        }
    }
}
