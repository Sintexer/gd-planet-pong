using System.Collections;
using UnityEngine;

public class PadLogic : MonoBehaviour
{
    private const float MaxBallVelocity = 30f;
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

    private float speed;
    private float wallCheckDistance;

    public int MoveDirection { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        recoil = GetComponentInChildren<PadRecoil>();
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
        recoil.HandleBallCollision(other);
        anim.SetTrigger(HitTrigger);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball")) return;
        int ySign = other.rigidbody.linearVelocityY > 0 ? 1 : -1;
        int xSign = other.rigidbody.linearVelocityX > 0 ? 1 : -1;
        float nextXVelocity = Mathf.Clamp(Mathf.Abs(other.rigidbody.linearVelocityX) + 2f, MinBallVelocityX,
            MaxBallVelocity);
        float nextYVelocity = Mathf.Clamp(Mathf.Abs(other.rigidbody.linearVelocityY) + 1f, MinBallVelocityY,
            MaxBallVelocity);
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