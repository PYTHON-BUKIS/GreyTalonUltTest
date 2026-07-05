using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BirdMissileController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseSpeed = 30f;
    public float maxSpeed = 50f;
    public float accelerationRate = 15f;
    public float turnSpeed = 0.1f;

    [Header("Control Delay Settings")]
    public float controlDelay = 1.5f;

    [Header("Explosion Settings")]
    public float explosionRadius = 12f;
    public float damage = 315f;
    public float stunDuration = 0.75f;
    public GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private float currentSpeed;
    private bool isControlled = false;
    private float delayTimer;

    private float yaw;
    private float pitch;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = baseSpeed;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 currentEuler = transform.rotation.eulerAngles;
        yaw = currentEuler.y;

        pitch = currentEuler.x;
        if (pitch > 180f) pitch -= 360f;

        delayTimer = controlDelay;
    }

    void Update()
    {
        if (!isControlled && delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                ActivatePlayerControl();
            }
            else
            {
                currentSpeed = baseSpeed;
                return;
            }
        }

        if (!isControlled) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ReleaseControl();
            return;
        }

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            if (Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.y))
            {
                yaw += mouseDelta.x * turnSpeed;
            }
            else if (Mathf.Abs(mouseDelta.y) > Mathf.Abs(mouseDelta.x))
            {
                pitch -= mouseDelta.y * turnSpeed;
            }

            pitch = Mathf.Clamp(pitch, -85f, 85f);
        }

        if (Keyboard.current != null && Keyboard.current.wKey.isPressed)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, accelerationRate * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, baseSpeed, accelerationRate * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);

        if (isControlled)
        {
            rb.MoveRotation(targetRotation);
        }

        Vector3 movementDirection = targetRotation * Vector3.forward;
        Vector3 nextPosition = transform.position + movementDirection * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }

    void ActivatePlayerControl()
    {
        isControlled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ReleaseControl()
    {
        isControlled = false;

        rb.constraints = RigidbodyConstraints.None;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void OnTriggerEnter(Collider other)
    {
        Explode();
    }

    void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, transform.rotation);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"Hit {hit.name}! Dealt {damage} damage and stunned for {stunDuration}s.");
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
