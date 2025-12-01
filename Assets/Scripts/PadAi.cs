using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PadAi : MonoBehaviour
{
    private static readonly AiProfile NormalProfile = new AiProfile
    {
        reactionDelay = 0.28f,
        reactionJitter = new Vector2(-0.06f, 0.06f),

        predictionError = 0.25f,
        deadZone = 0.4f
    };

    private static readonly AiProfile HardProfile = new AiProfile
    {
        reactionDelay = 0.12f,
        reactionJitter = new Vector2(-0.03f, 0.03f),

        predictionError = 0.01f,
        deadZone = 0.4f
    };

    [SerializeField]
    private AiDifficulty difficulty = AiDifficulty.Normal;

    private Transform ball;
    private CircleCollider2D ballCol;
    private Rigidbody2D ballRb;
    private float bottomBound;
    private int currentDirection;
    private GameManager gameManager;

    private float nextDecisionTime;

    private PadLogic padLogic;

    private AiProfile profile;
    private float targetY;
    private float topBound;

    private void Awake()
    {
        padLogic = GetComponent<PadLogic>();
        gameManager = FindAnyObjectByType<GameManager>();
        difficulty = GameSessionSettingsRuntime.Instance.aiDifficulty;
        profile = GetProfile(difficulty);
        targetY = transform.position.y;
    }

    private void Start()
    {
        UpdateGameBorders();
    }

    private void FixedUpdate()
    {
        if (!ball || !padLogic)
            return;

        float now = Time.time;

        if (now < nextDecisionTime)
        {
            return;
        }

        float jitter = Random.Range(profile.reactionJitter.x, profile.reactionJitter.y);
        float delay = Mathf.Max(0.02f, profile.reactionDelay + jitter);
        nextDecisionTime = now + delay;

        if (ballRb.linearVelocityX > 0) // Assuming AI is on the right
        {
            targetY = ball.position.y;
            if (difficulty == AiDifficulty.Hard)
            {
                targetY = TryPredictImpactY();
            }

            targetY += Random.Range(0, profile.predictionError);
        }
        else
        {
            targetY = 0f; // Go to center when idle
            if (targetY - padLogic.transform.position.y < profile.deadZone * 2)
            {
                padLogic.Move(0);
                return;
            }
        }

        if (float.IsNaN(targetY))
        {
            padLogic.Move(0);
            return;
        }

        Vector3 padPos = padLogic.transform.position;
        float error = targetY - padPos.y;


        int direction = DetermineDirection(error);
        padLogic.Move(direction);
    }

    private void OnEnable() => gameManager.BallSpawn += HandleBallSpawn;
    private void OnDisable() => gameManager.BallSpawn -= HandleBallSpawn;

    private void UpdateGameBorders()
    {
        Camera mainCamera = Camera.main;
        Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        topBound = topRight.y;
        bottomBound = bottomLeft.y;
    }

    private void HandleBallSpawn(object sender, BallSpawnEventArgs args)
    {
        ball = args.Ball;
        ballRb = ball ? ball.GetComponent<Rigidbody2D>() : null;
        ballCol = ball ? ball.GetComponent<CircleCollider2D>() : null;
        nextDecisionTime = Time.time;
    }

    private float TryPredictImpactY()
    {
        float impactY = 0f;

        float targetX = transform.position.x;
        float xDistance = targetX - ballRb.position.x; // signed
        float timeToReach = xDistance / ballRb.linearVelocityX; // > 0 if moving toward us
        if (timeToReach <= 0f) return float.NaN;

        Vector2 pos = ballRb.position;
        Vector2 vel = ballRb.linearVelocity;

        float remainingTime = timeToReach;

        float ballRadius = ballCol.radius * ball.transform.localScale.y / 2;
        var ricochetPredictionsLeft = 6; // limit max ricochet predictions
        while (remainingTime > 0f && ricochetPredictionsLeft > 0)
        {
            // How long until we hit a horizontal boundary?
            float timeToWall;
            if (vel.y > 0f)
                timeToWall = (topBound - ballRadius - pos.y) / vel.y;
            else if (vel.y < 0f)
                timeToWall = (bottomBound + ballRadius - pos.y) / vel.y;
            else
                timeToWall = Mathf.Infinity; // travelling fluently horizontal

            if (timeToWall >= remainingTime)
            {
                // The ball reaches the paddle before hitting a wall.
                pos += vel * remainingTime;
                return pos.y;
            }

            // We’ll bounce first. Advance to the wall, reflect Y, continue.
            pos += vel * timeToWall;
            vel.y = -vel.y;
            remainingTime -= timeToWall;

            // Clamp so numerical error doesn’t push the ball past the wall.
            pos.y = Mathf.Clamp(pos.y, bottomBound + ballRadius, topBound - ballRadius);
            --ricochetPredictionsLeft;
        }

        return float.NaN;
    }

    private int DetermineDirection(float error)
    {
        if (Mathf.Abs(error) <= profile.deadZone)
        {
            currentDirection = 0;
            return currentDirection;
        }

        currentDirection = error > 0f ? 1 : -1;
        return currentDirection;
    }

    private AiProfile GetProfile(AiDifficulty diff) => diff switch
    {
        AiDifficulty.Normal => NormalProfile,
        AiDifficulty.Hard => HardProfile,
        _ => NormalProfile
    };

    [Serializable]
    private struct AiProfile
    {
        [Header("Timing")]
        public float reactionDelay; // seconds between decision updates

        public Vector2 reactionJitter; // +/- randomization for human feel

        [Header("Tracking")]
        public float predictionError; // seconds ahead to aim

        public float deadZone; // tolerance in world units
    }
}

public enum AiDifficulty
{
    Normal,
    Hard
}