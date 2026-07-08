using System.Collections;
using UnityEngine;

public class CustomerMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public bool moveHorizontal = true;
    [Header("Hit Stop")]
    public float hitStopDuration = 0.4f;

    private bool isPaused = false;
    private Coroutine pauseCoroutine;
    [Header("Walk Animation")]
    public Sprite[] walkSprites;      // â°à⁄ìÆóp
    public Sprite[] walkUpSprites;    // è„à⁄ìÆóp
    public Sprite[] walkDownSprites;  // â∫à⁄ìÆóp
    public float animationInterval = 0.15f;

    private Vector3 startPosition;
    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 targetPosition;

    private int moveDirection = 1;

    private SpriteRenderer spriteRenderer;
    private Sprite[] currentWalkSprites;
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

        UpdateCurrentWalkSprites();
    }

    void Update()
    {
        if (isPaused)
        {
            return;
        }

        MoveCustomer();
        UpdateCurrentWalkSprites();
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

    void UpdateCurrentWalkSprites()
    {
        Sprite[] selectedSprites;

        if (moveHorizontal)
        {
            selectedSprites = walkSprites;
        }
        else
        {
            if (moveDirection == 1)
            {
                selectedSprites = walkUpSprites;
            }
            else
            {
                selectedSprites = walkDownSprites;
            }
        }

        if (selectedSprites == null || selectedSprites.Length == 0)
        {
            selectedSprites = walkSprites;
        }

        if (currentWalkSprites != selectedSprites)
        {
            currentWalkSprites = selectedSprites;
            walkFrameIndex = 0;
            animationTimer = 0f;

            if (spriteRenderer != null && currentWalkSprites != null && currentWalkSprites.Length > 0)
            {
                spriteRenderer.sprite = currentWalkSprites[0];
            }
        }
    }

    void UpdateWalkAnimation()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (currentWalkSprites == null || currentWalkSprites.Length == 0)
        {
            return;
        }

        animationTimer += Time.deltaTime;

        if (animationTimer >= animationInterval)
        {
            animationTimer = 0f;

            walkFrameIndex++;
            walkFrameIndex = walkFrameIndex % currentWalkSprites.Length;

            spriteRenderer.sprite = currentWalkSprites[walkFrameIndex];
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
    public void PauseAfterHit()
    {
        if (pauseCoroutine != null)
        {
            StopCoroutine(pauseCoroutine);
        }
        pauseCoroutine = StartCoroutine(PauseRoutine());
    }
    IEnumerator PauseRoutine()
    {
        isPaused = true;

        yield return new WaitForSeconds(hitStopDuration);

        isPaused = false;
        pauseCoroutine = null;
    }
}