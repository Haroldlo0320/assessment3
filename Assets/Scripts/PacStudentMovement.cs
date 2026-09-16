using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4.0f; // Units per second
    [SerializeField] private bool autoStart = true;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Clockwise loop waypoints around top-left inner block in world coordinates
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentTargetIndex = 0;
    private bool isMoving = false;

    // Direction constants: 0 = Up, 1 = Down, 2 = Left, 3 = Right
    private const int DIR_UP = 0;
    private const int DIR_DOWN = 1;
    private const int DIR_LEFT = 2;
    private const int DIR_RIGHT = 3;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (autoStart)
        {
            StartMovementLoop();
        }
    }

    /// <summary>
    /// Configures the 4 clockwise corner waypoints around the top-left inner block.
    /// Default positions based on standard 1-unit grid with top-left origin.
    /// </summary>
    public void SetWaypoints(Vector3 pTopLeft, Vector3 pTopRight, Vector3 pBottomRight, Vector3 pBottomLeft)
    {
        waypoints.Clear();
        waypoints.Add(pTopLeft);      // e.g. (1, -1, 0)
        waypoints.Add(pTopRight);     // e.g. (6, -1, 0)
        waypoints.Add(pBottomRight);  // e.g. (6, -5, 0)
        waypoints.Add(pBottomLeft);   // e.g. (1, -5, 0)

        transform.position = waypoints[0];
        currentTargetIndex = 1;
    }

    public void StartMovementLoop()
    {
        if (waypoints.Count < 4)
        {
            // Default waypoints matching top-left block (Col 1, Row 1) -> (Col 6, Row 1) -> (Col 6, Row 5) -> (Col 1, Row 5)
            // Assuming world origin (0,0) is top-left cell center
            SetWaypoints(
                new Vector3(1f, -1f, 0f),
                new Vector3(6f, -1f, 0f),
                new Vector3(6f, -5f, 0f),
                new Vector3(1f, -5f, 0f)
            );
        }

        if (!isMoving)
        {
            StartCoroutine(TweenMovementRoutine());
        }
    }

    /// <summary>
    /// Frame-rate independent programmatic tweening (Lerp over time).
    /// Continuous linear motion, instant direction change at corners.
    /// No Rigidbody physics, no Vector3.MoveTowards.
    /// </summary>
    private IEnumerator TweenMovementRoutine()
    {
        isMoving = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMovementSFX(true);
        }

        while (true)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = waypoints[currentTargetIndex];

            // Determine direction and update animator instantly
            Vector3 dir = (targetPos - startPos).normalized;
            UpdateDirectionAnimation(dir);

            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / Mathf.Max(moveSpeed, 0.001f);
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                // Linear constant-speed programmatic tween
                transform.position = Vector3.Lerp(startPos, targetPos, t);

                yield return null;
            }

            transform.position = targetPos;

            // Move to next waypoint clockwise
            currentTargetIndex = (currentTargetIndex + 1) % waypoints.Count;
        }
    }

    private void UpdateDirectionAnimation(Vector3 dir)
    {
        if (animator == null) return;

        int animDir = DIR_RIGHT;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            animDir = (dir.x > 0f) ? DIR_RIGHT : DIR_LEFT;
        }
        else
        {
            animDir = (dir.y > 0f) ? DIR_UP : DIR_DOWN;
        }

        animator.SetInteger("Direction", animDir);
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMovementSFX(false);
        }
    }
}
