using UnityEngine;

public class CustomerMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public bool moveHorizonal = true;

    private Vector3 startPosition;
    private int moveDirection = 1;

    void Start()
    {
        startPosition = transform.position;
    }
    void Update()
    {
        Vector3 direction;

        if (moveHorizonal == true)
        {
            direction = Vector3.right;
        }
        else
        {
            direction = Vector3.up;
        }

        transform.position += direction * moveDirection * moveSpeed * Time.deltaTime;

        float movedDistance;

        if (moveHorizonal == true)
        {
            movedDistance = transform.position.x - startPosition.x;
        }
        else
        {
            movedDistance = transform.position.y - startPosition.y;
        }

        if (Mathf.Abs(movedDistance) >= moveDistance)
        {
            Vector3 correctedPosition = transform.position;

            if (moveHorizonal == true)
            {
                correctedPosition.x = startPosition.x + moveDirection * moveDistance;
            }
            else
            {
                correctedPosition.y = startPosition.y + moveDirection * moveDistance;
            }
            transform.position = correctedPosition;

            moveDirection *= -1;
        }
    }
}
