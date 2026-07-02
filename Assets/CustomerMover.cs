using UnityEngine;

public class CustomerMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public bool moveHorizontal = true;

    [Header("Walk Animation")]
    public Sprite[] walkSprites;
    public float animationInterval = 0.15f;

    private Vector3 startPosition;
    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 targetPosition;

    private int moveDirection = 1;

    private SpriteRenderer spriteRenderer;
    private int walkFrameIndex = 0;
    private float animationTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        Vector3 direction;

        if (moveHorizontal)
        {
            direction = Vector3.right;
        }
        else
        {
            direction = Vector3.up;
        }

        pointA = startPosition - direction * moveDistance;
        pointB = startPosition + direction * moveDistance;

        targetPosition = pointB;
        moveDirection = 1;

        if (walkSprites != null && walkSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = walkSprites[0];
        }
    }

    void Update()
    {
        MoveCustomer();
        UpdateWalkAnimation();
        UpdateFacingDirection();
    }

    void MoveCustomer()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            if (targetPosition == pointB)
            {
                targetPosition = pointA;
                moveDirection = -1;
            }
            else
            {
                targetPosition = pointB;
                moveDirection = 1;
            }
        }
    }

    void UpdateWalkAnimation()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (walkSprites == null || walkSprites.Length == 0)
        {
            return;
        }

        animationTimer += Time.deltaTime;

        if (animationTimer >= animationInterval)
        {
            animationTimer = 0f;

            walkFrameIndex++;
            walkFrameIndex = walkFrameIndex % walkSprites.Length;

            spriteRenderer.sprite = walkSprites[walkFrameIndex];
        }
    }

    void UpdateFacingDirection()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (moveHorizontal)
        {
            if (moveDirection == 1)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}