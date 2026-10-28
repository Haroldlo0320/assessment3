using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PacStudentController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private ParticleSystem wallBumpParticles;
    [SerializeField] private ParticleSystem deathParticles;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private string lastInput = null;
    private string currentInput = null;

    private bool isLerping = false;
    private Vector3 currentGridPos;
    private Vector3 targetGridPos;
    private Vector3 startSpawnPos = new Vector3(1f, -1f, 0f);

    private bool canControl = false;
    private bool isDead = false;

    private LevelGenerator levelGen;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        levelGen = FindAnyObjectByType<LevelGenerator>();
        currentGridPos = transform.position;
        targetGridPos = currentGridPos;
        startSpawnPos = currentGridPos;
        lastInput = null;
        currentInput = null;
    }

    private void Update()
    {
        if (!canControl || isDead)
        {
            if (dustParticles != null && dustParticles.isPlaying)
            {
                dustParticles.Stop();
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMovementSFX(false);
            }
            return;
        }

        GatherInput();

        if (!isLerping)
        {
            TryNextMove();
        }
    }

    private void GatherInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
        {
            lastInput = "w";
        }
        else if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
        {
            lastInput = "a";
        }
        else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
        {
            lastInput = "s";
        }
        else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
        {
            lastInput = "d";
        }
    }

    private void TryNextMove()
    {
        if (!string.IsNullOrEmpty(lastInput) && CanWalkInDirection(lastInput))
        {
            currentInput = lastInput;
            StartLerpMove(currentInput);
        }
        else if (!string.IsNullOrEmpty(currentInput) && CanWalkInDirection(currentInput))
        {
            StartLerpMove(currentInput);
        }
        else
        {
            if (dustParticles != null && dustParticles.isPlaying)
            {
                dustParticles.Stop();
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMovementSFX(false);
            }
            if (animator != null)
            {
                animator.speed = 0f;
            }
        }
    }

    private bool CanWalkInDirection(string dir)
    {
        Vector3 step = GetDirectionVector(dir);
        Vector3 nextPos = currentGridPos + step;

        if (IsTeleportTunnel(nextPos))
        {
            return true;
        }

        return IsWalkable(nextPos);
    }

    private void StartLerpMove(string dir)
    {
        Vector3 step = GetDirectionVector(dir);
        targetGridPos = currentGridPos + step;
        UpdateAnimation(dir);

        bool nextTileHasPellet = CheckIfPellet(targetGridPos);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMovementSFX(true, nextTileHasPellet);
        }

        if (dustParticles != null && !dustParticles.isPlaying)
        {
            dustParticles.Play();
        }

        StartCoroutine(LerpToTargetRoutine());
    }

    private IEnumerator LerpToTargetRoutine()
    {
        isLerping = true;
        Vector3 start = currentGridPos;
        float dist = Vector3.Distance(start, targetGridPos);
        float duration = dist / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, targetGridPos, t);
            yield return null;
        }

        transform.position = targetGridPos;
        currentGridPos = targetGridPos;
        isLerping = false;

        HandleTeleport();
    }

    private void HandleTeleport()
    {
        if (currentGridPos.x <= -0.5f)
        {
            currentGridPos.x = 27f;
            targetGridPos = currentGridPos;
            transform.position = currentGridPos;
            currentInput = "a";
        }
        else if (currentGridPos.x >= 27.5f)
        {
            currentGridPos.x = 0f;
            targetGridPos = currentGridPos;
            transform.position = currentGridPos;
            currentInput = "d";
        }
    }

    private bool IsTeleportTunnel(Vector3 pos)
    {
        int r = Mathf.RoundToInt(-pos.y);
        return r == 14 && (pos.x < 0f || pos.x > 27f);
    }

    private bool IsWalkable(Vector3 pos)
    {
        int col = Mathf.RoundToInt(pos.x);
        int row = Mathf.RoundToInt(-pos.y);

        if (row == 14 && (col < 0 || col > 27)) return true;
        if (col < 0 || col > 27 || row < 0 || row > 28) return false;

        int tileType = GetTileTypeAt(col, row);
        if (tileType == 8) return false;
        return tileType == 0 || tileType == 5 || tileType == 6;
    }

    private bool CheckIfPellet(Vector3 pos)
    {
        int col = Mathf.RoundToInt(pos.x);
        int row = Mathf.RoundToInt(-pos.y);
        int tileType = GetTileTypeAt(col, row);
        return tileType == 5 || tileType == 6;
    }

    private int GetTileTypeAt(int col, int row)
    {
        if (levelGen == null || levelGen.levelMap == null) return 0;
        int mapRows = levelGen.levelMap.GetLength(0);
        int mapCols = levelGen.levelMap.GetLength(1);

        int sampleCol = col;
        if (sampleCol >= mapCols) sampleCol = (mapCols * 2 - 1) - sampleCol;

        int sampleRow = row;
        if (sampleRow >= mapRows) sampleRow = (mapRows * 2 - 1) - sampleRow;

        if (sampleCol < 0 || sampleCol >= mapCols || sampleRow < 0 || sampleRow >= mapRows) return 1;
        return levelGen.levelMap[sampleRow, sampleCol];
    }

    private Vector3 GetDirectionVector(string dir)
    {
        switch (dir)
        {
            case "w": return Vector3.up;
            case "s": return Vector3.down;
            case "a": return Vector3.left;
            case "d": return Vector3.right;
            default: return Vector3.zero;
        }
    }

    private void UpdateAnimation(string dir)
    {
        if (animator == null) return;
        animator.speed = 1f;

        switch (dir)
        {
            case "w":
                animator.Play("PacStudent_Walk_Up");
                break;
            case "s":
                animator.Play("PacStudent_Walk_Down");
                break;
            case "a":
                animator.Play("PacStudent_Walk_Left");
                break;
            case "d":
                animator.Play("PacStudent_Walk_Right");
                break;
        }
    }

    public void OnWallBump(Vector3 bumpPos)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWallCollideSFX();
        }
        if (wallBumpParticles != null)
        {
            wallBumpParticles.transform.position = bumpPos;
            wallBumpParticles.Play();
        }
    }

    public void Die(System.Action onDeathComplete)
    {
        if (isDead) return;
        StartCoroutine(DeathRoutine(onDeathComplete));
    }

    private IEnumerator DeathRoutine(System.Action onDeathComplete)
    {
        isDead = true;
        canControl = false;
        StopAllCoroutines();
        isLerping = false;

        if (dustParticles != null) dustParticles.Stop();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMovementSFX(false);
            AudioManager.Instance.PlayDeathSFX();
        }

        if (deathParticles != null)
        {
            deathParticles.transform.position = transform.position;
            deathParticles.Play();
        }

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play("PacStudent_Dead");
        }

        yield return new WaitForSeconds(1.5f);

        ResetToSpawn();
        isDead = false;
        if (onDeathComplete != null) onDeathComplete.Invoke();
    }

    public void ResetToSpawn()
    {
        transform.position = startSpawnPos;
        currentGridPos = startSpawnPos;
        targetGridPos = startSpawnPos;
        lastInput = null;
        currentInput = null;
        isLerping = false;
        if (animator != null)
        {
            animator.speed = 0f;
            animator.Play("PacStudent_Walk_Right");
        }
    }

    public void SetControlEnabled(bool state)
    {
        canControl = state;
        if (!state)
        {
            if (dustParticles != null) dustParticles.Stop();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMovementSFX(false);
            if (animator != null) animator.speed = 0f;
        }
    }
}
