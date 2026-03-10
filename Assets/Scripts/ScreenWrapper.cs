using UnityEngine;

public class ScreenWrapper : MonoBehaviour
{
    public static float worldWidth;
    public static float worldHeight;

    float leftConstrain;
    float rightConstrain;
    float bottomConstrain;
    float topConstrain;

    void Awake()
    {
        float distanceZ = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);

        Vector2 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, distanceZ));

        worldWidth = screenTopRight.x;
        worldHeight = screenTopRight.y;

        leftConstrain = -worldWidth;
        rightConstrain = worldWidth;
        bottomConstrain = -worldHeight;
        topConstrain = worldHeight;
    }

    void Update()
    {
        if (transform.position.x < leftConstrain)
        {
            transform.position = new Vector3(rightConstrain, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > rightConstrain)
        {
            transform.position = new Vector3(leftConstrain, transform.position.y, transform.position.z);
        }

        if (transform.position.y < bottomConstrain)
        {
            transform.position = new Vector3(transform.position.x, topConstrain, transform.position.z);
        }
        else if (transform.position.y > topConstrain)
        {
            transform.position = new Vector3(transform.position.x, bottomConstrain, transform.position.z);
        }
    }
}