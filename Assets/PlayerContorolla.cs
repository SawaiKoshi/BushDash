using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerContorolla : MonoBehaviour
{
    public float speed = 5f;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI holdingText;
    public TextMeshProUGUI remainingText;
    public TextMeshProUGUI penaltyText;
    public GameObject sinkActionText;
    public GameObject[] heldDishIcons;
    public float heldDishSpacing = 0.22f;
    public GameObject startPanel;
    public int clearScore;
    public int hitPenaltyTime = 5;
    public float hitCooldown = 1.0f;
    public string stageName = "Stage 1";
    public string nextSceneName = "Stage2";
    public bool showStartScreen = true;
    public int maxCarry = 3;
    public float heldDishDistance = 0.7f;
    public float timeLimit = 60f;
    public bool isFinishStage = false;
    public AudioClip pickDishSound;
    public AudioClip putDishSound;
    public AudioClip hitCustomerSound;
    public AudioClip countdownSound;
    public AudioClip goSound;
    public AudioClip clearSound;
    public AudioClip gameOverSound;
    public CameraShake cameraShake;
    public float hitStunDuration = 0.3f;
    public TextMeshProUGUI hitCountText;
    public AudioSource bgmSource;
    public Sprite[] walkDownSprites;
    public Sprite[] walkUpSprites;
    public Sprite[] walkSideSprites;
    public Sprite idleDownSprite;
    public Sprite idleUpSprite;
    public Sprite idleSideSprite;
    public float walkFrameInterval = 0.15f;
    public float dishSearchRadius = 1.0f;


    private float walkAnimationTimer = 0f;
    private int walkFrameIndex = 0;
    private Vector2 lastMoveDirection = Vector2.down;
    private AudioSource audioSource;
    private TextMeshPro currentDishActionText;
    private GameObject nearbyDish;
    private int carriedDishCount = 0;
    private bool nearSink = false;
    private int score;
    private bool isGameFinished = false;
    private bool isGameStarted = false;
    private bool isGameClear = false;
    private Vector2 facingDirection = Vector2.up;
    private bool canTakeHit = true;
    private bool isStunned = false;
    private SpriteRenderer playerSprite;
    private Color originalPlayerColor;
    private bool isCountdownRunning = false;
    private int hitCount = 0;


void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();

        if (playerSprite != null)
        {
            originalPlayerColor = playerSprite.color;
        }
        
        audioSource = GetComponent<AudioSource>();

        clearScore = GameObject.FindGameObjectsWithTag("Dish").Length;

        UpdateScoreText();
        UpdateTimerText();
        UpdateHitCountText();
        UpdateHoldingText();
        UpdateHeldDishIcon();
        UpdateRemainingText();
        UpdateSinkActionText();
        UpdateHeldDishPosition();
        HidePenaltyText();

        if (resultText != null)
        {
            resultText.text = "";
        }

        if (showStartScreen == true)
        {
            isGameStarted = false;

            if (startPanel != null)
            {
                startPanel.SetActive(true);
            }

            SetCustomersMoving(false);
        }
        else
        {
            isGameStarted = false;

            if (startPanel != null)
            {
                startPanel.SetActive(false);
            }

            StartCoroutine(StartStageCountdown());
        }
    }

    void Update()
    {
        UpdateNearestDish();

        if (isGameFinished == true)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
            if (isGameClear == true && isFinishStage == false && Input.GetKeyDown(KeyCode.N))
            {
                GoToNextStage();
            }
            return;
        }
        if (isGameStarted == false)
        {
            if (showStartScreen == true && isCountdownRunning == false && Input.GetKeyDown(KeyCode.Return))
            {
                if (startPanel != null)
                {
                    startPanel.SetActive(false);
                }
                StartCoroutine(StartStageCountdown());
            }
            return;
        }

        UpdateSinkActionText();

        timeLimit -= Time.deltaTime;
        UpdateTimerText();

        if (timeLimit <= 0)
        {
            timeLimit = 0;
            isGameFinished = true;
            UpdateTimerText();
            GameOver();
            return;
        }

        if (isStunned == true)
        {
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(x, y, 0).normalized;
        UpdatePlayerWalkAnimation(new Vector2(direction.x, direction.y));
        if (direction != Vector3.zero)
        {
            facingDirection = new Vector2(direction.x, direction.y);
          
            UpdateHeldDishPosition();
        }
        transform.position += direction * speed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (nearbyDish != null && carriedDishCount < maxCarry)
            {
                GameObject pickedDish = nearbyDish;

                HideDishActionText();

                nearbyDish = null;
                currentDishActionText = null;

                pickedDish.SetActive(false);
                Destroy(pickedDish);

                carriedDishCount += 1;

                PlaySound(pickDishSound);

                UpdateHoldingText();
                UpdateHeldDishIcon();
                UpdateSinkActionText();
                UpdateHeldDishPosition();

                Debug.Log("皿を拾った！");
            }
            else if (nearSink == true && carriedDishCount > 0)
            {
                score += carriedDishCount;
                carriedDishCount = 0;

                PlaySound(putDishSound);

                UpdateScoreText();
                UpdateHoldingText();
                UpdateHeldDishIcon();
                UpdateRemainingText();

                Debug.Log("皿をまとめて置いた！スコア：" + score);
                
                if (score >= clearScore)
                {
                    GameClear();
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.name == "sink")
        {
            nearSink = true;
            Debug.Log("洗い場の近くにいる。");

        }
        if (other.CompareTag("Customer"))
        {
            TakeHitPenalty(other.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "sink")
        { 
            nearSink = false;
        Debug.Log("洗い場から離れた。");
      }
  }
    void UpdateScoreText()
    {
    scoreText.text = "Score: " + score;
    }
    void UpdateTimerText()
    {
        timerText.text = "Time: " + Mathf.CeilToInt(timeLimit);
    }
    string GetRank(int finalTime)
    {
        if (finalTime >= 40)
        {
            return "S";
        }
        else if (finalTime >= 30)
        {
            return "A";
        }
        else if (finalTime >= 20)
        {
            return "B";
        }
        else
        {
            return "C";
        }
    }
    void GameClear()
    {
        isGameFinished = true;
        isGameClear = true;

        if (bgmSource != null)
        {
            bgmSource.Stop();
        }

        StartCoroutine(PlaySoundDelayed(clearSound, 0.5f));
        HideAllActionText();
        StopCustomers();

        int finalTime = Mathf.CeilToInt(timeLimit);
        string rank = GetRank(finalTime);
    
        if (resultText != null)
        {
            if (isFinishStage == true)
            {
                resultText.text = "All CLEAR\n" + "Time Left: " + finalTime + "\n" + "Rank: " + rank + "\n" + "\nPress R to Retry";
            }
            else
            {
                resultText.text = "STAGE CLEAR!\n" + "Time Left: " + finalTime + "\n" + "Rank: " + rank + "\n" + "Press N to Next Stage\n" + "Press R to Retry";
            }
        }
        Debug.Log("クリア! 残り時間: " + finalTime + "ランク:" + rank);
    }

    void GameOver()
    {
        isGameFinished = true;
        isGameClear = false;

        if (bgmSource != null)
        {
            bgmSource.Stop();
        }

        PlaySound(gameOverSound);
        HideAllActionText();
        StopCustomers();

        if (resultText != null)
        {
            resultText.text = "GAME OVER\nScore: " + score + "\nPress R to Retry";
        }
        Debug.Log("時間切れ! ゲームオーバー");
    }
    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void UpdateHoldingText()
    {
       holdingText.text = "Tray: " + carriedDishCount + "/" + maxCarry;
    }
    void UpdateHeldDishIcon()
    {
        if (heldDishIcons == null)
        {
            return;
        }
        
        for (int i= 0; i < heldDishIcons.Length; i++)
        {
            if (heldDishIcons[i] != null)
            {
                heldDishIcons[i].SetActive(i < carriedDishCount);
            }
        }
    }
    void UpdateRemainingText()
    {
        int remaining = clearScore - score;

        if (remaining < 0)
        {
            remaining = 0;
        }
        remainingText.text = "Remaining: " + remaining;

    }
    void UpdateSinkActionText()
    {
        if (sinkActionText != null)
        {
            if (nearSink == true && carriedDishCount > 0)
            {
                sinkActionText.SetActive(true);
            }
            else
            {
                sinkActionText.SetActive(false);
            }
        }
    }
    void HideDishActionText()
    {
        if (currentDishActionText != null)
        {
            currentDishActionText.gameObject.SetActive(false);
            currentDishActionText = null;
        }
    }
    void HideAllActionText()
    {
        HideDishActionText();

        if (sinkActionText != null)
        {
            sinkActionText.SetActive(false);
        }
    }
    void GoToNextStage()
    {
        SceneManager.LoadScene(nextSceneName);
    }
    void UpdateHeldDishPosition()
    {
      if (heldDishIcons == null)
        {
            return;
        }

        Vector3 basePosition = new Vector3(facingDirection.x, facingDirection.y, 0)
                               * heldDishDistance;

        Vector3 sideDirection = new Vector3(-facingDirection.y, facingDirection.x, 0);

        int visibleCount = Mathf.Min(carriedDishCount, heldDishIcons.Length);

        for (int i = 0; i < visibleCount; i++)
        {
            float offset = (i - (visibleCount - 1) / 2f) * heldDishSpacing;

            heldDishIcons[i].transform.localPosition = basePosition + sideDirection * offset;
        }                             
    }
    void TakeHitPenalty(GameObject customer)
    { 
        if (isGameStarted == false || isGameFinished == true)
        {
            return;
        }

        if (canTakeHit == false)
        { 
            return;
        }

        StartCoroutine(HitReaction());
        timeLimit -= hitPenaltyTime;

        hitCount += 1;
        UpdateHitCountText();

        PlaySound(hitCustomerSound);

        if (cameraShake != null)
        {
            cameraShake.Shake();
        }

        if (timeLimit < 0)
        {
            timeLimit = 0;
        }

       
        UpdateTimerText();
        ShowCustomerPenaltyText(customer);

        canTakeHit = false;
        Invoke("ResetHitCooldown", hitCooldown);

        Debug.Log("お客さんにぶつかった! - " + hitPenaltyTime + "秒");
    }
    void ResetHitCooldown()
    {
        canTakeHit = true;
    }
    void ShowPenaltyText()
    {
        if (penaltyText != null) 
        {
            penaltyText.text = "-" + hitPenaltyTime + "sec!";
            penaltyText.gameObject.SetActive(true);

            CancelInvoke(nameof(HidePenaltyText));
            Invoke(nameof(HidePenaltyText), 0.8f);
        }
    }
    void HidePenaltyText()
    {
        if (penaltyText != null)
        {
            penaltyText.gameObject.SetActive(false);
        }
    }
    void ShowCustomerPenaltyText(GameObject customer)
    {
        TextMeshPro penaltyText = customer.GetComponentInChildren<TextMeshPro>(true);

        if (penaltyText != null)
        {
            penaltyText.text = "-" + hitPenaltyTime + "sec!";
            penaltyText.gameObject.SetActive(true);

            StartCoroutine(HideCustomerPenaltyText(penaltyText));
        }
    }
    IEnumerator HideCustomerPenaltyText(TextMeshPro penaltyText)
    {
        yield return new WaitForSeconds(0.85f);

        if (penaltyText != null)
        {
            penaltyText.gameObject.SetActive(false);
        }
    }
    void SetCustomersMoving(bool shouldMove)
    {
        CustomerMover[] customers = FindObjectsByType<CustomerMover>(FindObjectsSortMode.None);

        foreach (CustomerMover customer in customers)
        {
            customer.enabled = shouldMove;
        }
    }
    void StopCustomers()
    {
        SetCustomersMoving(false);
    }
    IEnumerator StartStageCountdown()
    {
        if (isCountdownRunning == true)
        {
            yield break;
        }

        isCountdownRunning = true;
        isGameStarted = false;

        SetCustomersMoving(false);


        resultText.text = stageName + "\n3";
        PlaySound(countdownSound);
        yield return new WaitForSeconds(1f);
       
        resultText.text = "\n2";
        PlaySound(countdownSound);
        yield return new WaitForSeconds(1f);
      
        resultText.text = "\n1";
        PlaySound(countdownSound);
        yield return new WaitForSeconds(1f);

        resultText.text = "GO!";    
        PlaySound(goSound);
        yield return new WaitForSeconds(0.7f);
             
        resultText.text = "";
       
        isGameStarted = true;
        isCountdownRunning = false;

        SetCustomersMoving(true);
    }
    void PlaySound(AudioClip sound)
    {
        if (audioSource != null && sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }
    IEnumerator PlaySoundDelayed(AudioClip sound, float delay)
    {
        yield return new WaitForSeconds(delay);

        PlaySound(sound);
    }
    IEnumerator HitReaction()
    {
        isStunned = true;

        if (playerSprite != null)
        {
            playerSprite.color = Color.red;
        }

        yield return new WaitForSeconds(hitStunDuration);

        if (playerSprite != null)
        {
            playerSprite.color = originalPlayerColor;
        }

        isStunned = false;
    }
    void UpdateHitCountText()
    {
        if (hitCount != null)
        {
            hitCountText.text = "Hits: " + hitCount;
        }
    }
   void UpdatePlayerWalkAnimation(Vector2 direction)
    {
        if (playerSprite == null)
        {
            return;
        }

        bool isMoving = direction != Vector2.zero;

        if (isMoving == true)
        {
            lastMoveDirection = direction;

            walkAnimationTimer += Time.deltaTime;

            if (walkAnimationTimer >= walkFrameInterval)
            {
                walkAnimationTimer = 0f;
                walkFrameIndex += 1;
            }

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                if (walkSideSprites != null && walkSideSprites.Length > 0)
                {
                    playerSprite.sprite = walkSideSprites[walkFrameIndex % walkSideSprites.Length];
                }

                if (direction.x < 0)
                {
                    playerSprite.flipX = true;
                }
                else
                {
                    playerSprite.flipX = false;
                }
            }
            else
            {
                playerSprite.flipX = false;

                if (direction.y > 0)
                {
                    if (walkUpSprites != null && walkUpSprites.Length > 0)
                    {
                        playerSprite.sprite = walkUpSprites[walkFrameIndex % walkUpSprites.Length];
                    }
                }
                else if (direction.y < 0)
                {
                    if (walkDownSprites != null && walkDownSprites.Length > 0)
                    {
                        playerSprite.sprite = walkDownSprites[walkFrameIndex % walkDownSprites.Length];
                    }
                }
            }           
        }
        else
        {
            walkAnimationTimer = 0f;
            walkFrameIndex = 0;

            SetIdleSprite();
        }
    }
    void SetIdleSprite()
    {
        if (playerSprite == null)
        {
            return;
        }

        if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y))
        {
            playerSprite.sprite = idleSideSprite;

            if (lastMoveDirection.x < 0)
            {
                playerSprite.flipX = true;
            }
            else
            {
                playerSprite.flipX = false;
            }
        }
        else
        {
            playerSprite.flipX = false;

            if (lastMoveDirection.y > 0)
            {
                playerSprite.sprite = idleUpSprite;
            }
            else
            {
                playerSprite.sprite = idleDownSprite;
            }
        }
    }
    void UpdateNearestDish()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dishSearchRadius);

        GameObject nearestDish = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("DishPickup") == false)
            {
                continue;
            }

            GameObject dish = hit.transform.parent.gameObject;

            if (dish == null || dish.activeSelf == false)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, dish.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestDish = dish;
            }
        }

        if (nearestDish != nearbyDish)
        {
            HideDishActionText();

            nearbyDish = nearestDish;
            currentDishActionText = null;

            if (nearbyDish != null)
            {
                currentDishActionText = nearbyDish.GetComponentInChildren<TextMeshPro>(true);
            }
        }

        if (nearbyDish == null)
        {
            HideDishActionText();
            return;
        }

        if (currentDishActionText != null)
        {
            if (carriedDishCount < maxCarry)
            {
                currentDishActionText.text = "Space: Pick Dish";
            }
            else
            {
                currentDishActionText.text = "Tray Full";
            }

            currentDishActionText.gameObject.SetActive(true);
        }
    }
}




