using System.Collections;
using UnityEngine;

public class PadLogic : MonoBehaviour
{
    private const float MaxBallVelocity = 40f;
    private const float MinBallVelocityX = 4f;
    private const float MinBallVelocityY = 0f;
    private static readonly int HitTrigger = Animator.StringToHash("hitTrigger");

    [SerializeField]
    private float dampingFactor;

    [SerializeField]
    private LayerMask wallMask;

    [SerializeField]
    private GameObject hitParticlesPrefab;

    private Animator anim;
    private Collider2D col;

    private Rigidbody2D rb;
    private PadRecoil recoil;
    private PadLoseVfx padLoseVfx;
    private PaddleIntroSpawn paddleIntroSpawn;

    private float speed;
    private float wallCheckDistance;

    public int MoveDirection { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        recoil = GetComponentInChildren<PadRecoil>();
        padLoseVfx = GetComponentInChildren<PadLoseVfx>();
        paddleIntroSpawn = GetComponentInChildren<PaddleIntroSpawn>();
        anim = GetComponentInChildren<Animator>();
        wallCheckDistance = col.bounds.max.y;
        speed = GameSessionSettingsRuntime.Instance?.padSpeed ?? 8;
    }

    private void Start()
    {
        wallCheckDistance = col.bounds.max.y;
    }

    private void FixedUpdate()
    {
        if (MoveDirection != 0)
        {
            rb.linearVelocityY = MoveDirection * speed;
            var result = Physics2D.Raycast(transform.position, Vector2.up * MoveDirection, wallCheckDistance, wallMask);
            if (result.collider != null)
            {
                Stop();
                rb.linearVelocityY = 0;
            }

            return;
        }

        rb.linearVelocityY -= rb.linearVelocityY * dampingFactor;

        if (Mathf.Abs(rb.linearVelocityY) < 0.1f)
        {
            rb.linearVelocityY = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball")) return;
        SFX.Instance.Play("hit");
        SpawnCollisionParticles(other);
        anim.SetTrigger(HitTrigger);
        if (other.rigidbody.linearVelocityX >= MaxBallVelocity)
        {
            SFX.Instance.Play("explosion");
            padLoseVfx.LaunchFromExplosion(other.transform.position, false);
            StartCoroutine(ReturnRoutine());
        }
        else
        {
            recoil.HandleBallCollision(other);
        }
    }

    private IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(1);
        padLoseVfx.UndoLaunch();
        yield return new WaitForSeconds(2);
        paddleIntroSpawn.PlayIntro();
        SFX.Instance.Play("whoosh");
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball")) return;
        int ySign = other.rigidbody.linearVelocityY > 0 ? 1 : -1;
        int xSign = other.rigidbody.linearVelocityX > 0 ? 1 : -1;
        var step = Mathf.Abs(other.rigidbody.linearVelocityX) > MaxBallVelocity * 0.75f
            ? 0.5f
            : 2f;
        float nextXVelocity = Mathf.Clamp(Mathf.Abs(other.rigidbody.linearVelocityX) + step, MinBallVelocityX, MaxBallVelocity);
        float nextYVelocity = Mathf.Clamp(Mathf.Abs(other.rigidbody.linearVelocityY) + step / 2, MinBallVelocityY, MaxBallVelocity);
        other.rigidbody.linearVelocityX = xSign * nextXVelocity;
        other.rigidbody.linearVelocityY = ySign * nextYVelocity;
    }

    private void OnDrawGizmos()
    {
        if (wallCheckDistance == 0)
        {
            return;
        }

        // Visualize the ray in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * wallCheckDistance);
    }

    public void Stop()
    {
        Move(0);
    }

    public void Move(int direction)
    {
        MoveDirection = direction;
    }

    private void SpawnCollisionParticles(Collision2D other)
    {
        Vector2 hitNormal = other.contacts[0].normal * -1;
        Quaternion particleRotation = Quaternion.FromToRotation(Vector3.right, hitNormal);
        var particles = Instantiate(hitParticlesPrefab, other.contacts[0].point, particleRotation);
        StartCoroutine(DestroyParticlesWithDelayRoutine(3f, particles));
    }

    private IEnumerator DestroyParticlesWithDelayRoutine(float delay, GameObject particles)
    {
        yield return new WaitForSeconds(delay);
        Destroy(particles);
    }
}