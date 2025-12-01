using System.Collections;
using UnityEngine;

public class PadLoseVfx : MonoBehaviour
{
    private static readonly int HitTrigger = Animator.StringToHash("hitTrigger");

    [SerializeField]
    private float launchForce = 20f;

    [SerializeField]
    private float spinImpulse = 10f;

    [SerializeField]
    private GameObject smokeVfx;

    private Animator animator;
    private Collider2D col;

    private Rigidbody2D rb;
    private GameObject effects;
    private Vector3 originalRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();
        // originalRotation = rb.rotation
    }

    public void LaunchFromExplosion(Vector2 explosionOrigin, bool destroyAfterDelay = true)
    {
        enabled = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.freezeRotation = false;


        animator.SetTrigger(HitTrigger);

        col.enabled = false;

        Debug.Log($"rb.position = {rb.position}");
        Debug.Log($"explosionOrigin = {explosionOrigin}");

        Vector2 direction = (rb.position - explosionOrigin).normalized;
        direction = new Vector2(direction.x, direction.y * 0.005f);
        Debug.Log($"direction = {direction}");
        rb.AddForce(direction * launchForce, ForceMode2D.Impulse);
        rb.AddTorque(spinImpulse, ForceMode2D.Impulse);

        if (!effects)
        {
            effects = Instantiate(smokeVfx, transform);
        }

        if (destroyAfterDelay)
        {
            StartCoroutine(DisableAfterSeconds(10f));
        }
    }

    public void UndoLaunch()
    {
        enabled = true;
        col.enabled = true;
        rb.linearVelocity = Vector2.zero;
        rb.totalTorque = 0;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.rotation = 0;
        if (effects)
        {
            Destroy(effects);
        }
    } 

    private IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}