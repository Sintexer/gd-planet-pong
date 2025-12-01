using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    
    public event EventHandler<BallSpawnEventArgs> BallSpawn;
    
    [SerializeField]
    private TransitionController transitionController;

    [SerializeField]
    private int nextRoundDelay = 3;

    [SerializeField]
    private Transform[] ballPrefabs;

    [SerializeField]
    private TextMeshProUGUI leftScoreText;

    [SerializeField]
    private TextMeshProUGUI rightScoreText;

    [SerializeField]
    private GameObject countdownPanel;

    [SerializeField]
    private GameObject nextBallDirection;

    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private TextMeshProUGUI victoryText;

    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private GameObject goalParticlesPrefab;

    [SerializeField]
    private GameObject gameOverParticlesPrefab;

    [SerializeField]
    private GameObject leftPaddle;

    [SerializeField]
    private GameObject rightPaddle;

    private Transform ball;
    private float bottomBound;

    private TextMeshProUGUI countdownText;

    private bool lastScoredLeft = true;

    private float leftBound;

    private int leftScore;

    private bool paused;
    private float rightBound;
    private int rightScore;
    private float topBound;

    private Coroutine gameEndSounds;

    private void Awake()
    {
        countdownText = countdownPanel.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        countdownPanel.SetActive(false);
        pausePanel.SetActive(false);
        UpdateGameBorders();
        UpdateScoreUi();
        var paddleIntroSpawn = leftPaddle.GetComponent<PaddleIntroSpawn>();
        var spawnDuration = paddleIntroSpawn.Duration;
        paddleIntroSpawn.PlayIntro();
        rightPaddle.GetComponent<PaddleIntroSpawn>().PlayIntro();
        StartCoroutine(WhooshRoutine(spawnDuration));

        var spawnToLeft = Random.Range(0, 2) == 0;

        SFX.Instance.PlayLooping(false);
        // SpawnBall(lastScoredLeft);
        StartCoroutine(NextRoundCo(spawnToLeft));
    }

    private IEnumerator WhooshRoutine(float spawnDuration)
    {
        yield return new WaitForSeconds(spawnDuration / 2);
        
        SFX.Instance.Play("whoosh");
    }

    private void FixedUpdate()
    {
        if (!ball)
        {
            return;
        }

        var ballOutOfBounds = ball.position.x < leftBound || ball.position.x > rightBound;
        ballOutOfBounds |= ball.position.y < bottomBound || ball.position.y > topBound;
        if (ballOutOfBounds)
        {
            HandleBallScored();
        }
    }


    private void UpdateGameBorders()
    {
        Camera mainCamera = Camera.main;
        Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        leftBound = bottomLeft.x;
        rightBound = topRight.x;
        topBound = topRight.y;
        bottomBound = bottomLeft.y;
    }

    private void UpdateScoreUi()
    {
        leftScoreText.text = leftScore + "";
        rightScoreText.text = rightScore + "";
    }

    private void HandleBallScored()
    {
        var leftScored = ball.position.x > 0;
        if (leftScored)
        {
            leftScore++;
        }
        else
        {
            rightScore++;
        }

        Debug.Log("Goal");
        UpdateScoreUi();
        ball.GetComponentInChildren<BallLogic>().ShakeScreen();
        HandleGoalEffects(ball.position, leftScored);
        var lastPosition = ball.position;
        Destroy(ball.gameObject);
        var settings = GameSessionSettingsRuntime.Instance;
        if (leftScore >= settings.targetScore || rightScore >= settings.targetScore)
        {
            gameEndSounds = StartCoroutine(PlayGameEndRoutine());
            HandleGameOver(lastPosition);
        }
        else
        {
            StartCoroutine(NextRoundCo(leftScored));
        }
    }

    private IEnumerator PlayGameEndRoutine()
    {
        SFX.Instance.PlayLooping(false, true);
        SFX.Instance.Play("explosion");
        yield return new WaitForSeconds(0.3f);
        SFX.Instance.Play("game_over");
        yield return new WaitForSeconds(2f);
        SFX.Instance.Play("explosion");
        yield return new WaitForSeconds(3f);
        SFX.Instance.Play("explosion");
    }

    private void HandleGoalEffects(Vector2 scoredPosition, bool leftScored)
    {
        SFX.Instance.Play("hit_big");

        var y = scoredPosition.y;
        var xOffset = 3f;
        var x = scoredPosition.x + (leftScored ? xOffset : -xOffset);
        Quaternion particleRotation = Quaternion.FromToRotation(Vector3.up, leftScored ? Vector3.left : Vector3.right);
        Instantiate(goalParticlesPrefab, new Vector2(x, y), particleRotation);
    }

    private void HandleGameOver(Vector2 finalCollision)
    {
        var leftWon = leftScore > rightScore;
        TurnOffInput(leftPaddle);
        TurnOffInput(rightPaddle);

        LaunchLoser(leftWon ? rightPaddle : leftPaddle, finalCollision);
        gameOverPanel.SetActive(true);
        var x = (rightBound + 1f) * (leftWon ? 1f : -1f);
        var y = -2f;
        var rotation = Quaternion.identity;
        if (leftWon)
        {
            rotation = Quaternion.FromToRotation(Vector3.up, Vector2.left);
        }

        Instantiate(gameOverParticlesPrefab, new Vector2(x, y), rotation);
        victoryText.text = leftWon ? "Another Victory for Humanity" : "Failure - bot wins";
        Debug.Log("Game over");
    }

    private void TurnOffInput(GameObject paddle)
    {
        var inputHandler = paddle.GetComponent<PlayerInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.TurnOff();
        }
    }

    private void LaunchLoser(GameObject loser, Vector2 finalCollision)
    {
        loser.GetComponent<PadLoseVfx>().LaunchFromExplosion(finalCollision);
    }

    private IEnumerator NextRoundCo(bool leftScored)
    {
        // round end juice period
        yield return new WaitForSeconds(2);

        UpdateNextBallDirectionUi(leftScored);
        countdownPanel.SetActive(true);
        for (int i = nextRoundDelay; i > 0; i--)
        {
            nextBallDirection.SetActive(true);
            countdownText.text = i + "";
            SFX.Instance.Play("tick");
            yield return new WaitForSeconds(0.5f);
            nextBallDirection.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }

        countdownPanel.SetActive(false);
        SFX.Instance.Play("ball_spawn");
        SpawnBall(leftScored);
    }

    private void UpdateNextBallDirectionUi(bool leftScored)
    {
        if (lastScoredLeft != leftScored)
        {
            // nextBallDirection.transform.position = -nextBallDirection.transform.position;
            nextBallDirection.transform.Rotate(new Vector3(0, 0, 180));
        }

        lastScoredLeft = leftScored;
    }

    private void SpawnBall(bool spawnBallTowardsRight)
    {
        var ballPrefab = ballPrefabs[Random.Range(0, ballPrefabs.Length)];
        ball = Instantiate(ballPrefab, transform.position, Quaternion.identity);
        var ballDirection = spawnBallTowardsRight ? 1 : -1;
        var rb = ball.GetComponent<Rigidbody2D>();
        var initVelocity = GetBallInitialVelocity();
        rb.linearVelocity = new Vector2(initVelocity.x, 0) * ballDirection + GetRandomVerticalVelocity(initVelocity.x);
        float addRotation = Random.Range(-10f, 10f);
        rb.AddTorque(addRotation, ForceMode2D.Force);
        BallSpawn?.Invoke(this, new BallSpawnEventArgs { Ball = ball });
    }

    private Vector2 GetRandomVerticalVelocity(float yVelocity)
    {
        var verticalVelocity = Random.Range(-yVelocity, yVelocity);
        return new Vector3(0, verticalVelocity);
    }

    private Vector2 GetBallInitialVelocity()
    {
        var settings = GameSessionSettingsRuntime.Instance;
        if (settings == null)
        {
            return new Vector2(4, 1);
        }
        return settings.aiDifficulty switch
        {
            AiDifficulty.Normal => settings.normalInitSpeed,
            AiDifficulty.Hard => settings.hardInitSpeed,
            _ => settings.normalInitSpeed
        };
    }

    public void TogglePause()
    {
        if (paused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        SFX.Instance.Play("pause");
        Time.timeScale = 0;
        pausePanel.SetActive(true);
        paused = true;
    }

    public void ResumeGame()
    {
        SFX.Instance.Play("pause");
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        paused = false;
    }

    public void StartNextGame()
    {
        Time.timeScale = 1f;
        transitionController.ChangeScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SFX.Instance.Play("click_exit");
        transitionController.ChangeScene("MainMenu");
    }

    private void OnDisable()
    {
        if (gameEndSounds != null)
        {
            StopCoroutine(gameEndSounds);
        }
    }
}

public class BallSpawnEventArgs : EventArgs
{
    public Transform Ball { get; set; }
}