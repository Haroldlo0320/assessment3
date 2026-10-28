using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public enum GhostState { Normal, Scared, Recovering, Dead }

    [SerializeField] public int ghostId = 1;
    [SerializeField] public float baseSpeed = 3.6f;
    [SerializeField] public Vector3 homeSpawnPos;
    [SerializeField] public Vector3 exitGatePos;

    private GhostState currentState = GhostState.Normal;
    private bool isLerping = false;
    private Vector3 currentGridPos;
    private Vector3 targetGridPos;
    private Vector3 lastMoveDir = Vector3.zero;

    private bool canMove = false;
    private bool isInSpawnZone = true;

    private Animator animator;
    private LevelGenerator levelGen;
    private Transform pacStudentTransform;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        levelGen = FindAnyObjectByType<LevelGenerator>();
        GameObject pac = GameObject.Find("PacStudent");
        if (pac != null) pacStudentTransform = pac.transform;

        homeSpawnPos = transform.position;
        currentGridPos = transform.position;
        targetGridPos = currentGridPos;

        if (ghostId == 1)
        {
            exitGatePos = new Vector3(13f, -11f, 0f);
        }
        else if (ghostId == 2)
        {
            exitGatePos = new Vector3(14f, -17f, 0f);
        }
        else if (ghostId == 3)
        {
            exitGatePos = new Vector3(14f, -11f, 0f);
        }
        else
        {
            exitGatePos = new Vector3(13f, -17f, 0f);
        }

        isInSpawnZone = true;
    }

    private void Update()
    {
        if (!canMove) return;

        if (!isLerping)
        {
            DecideNextMove();
        }
    }

    public void SetMovementEnabled(bool state)
    {
        canMove = state;
    }

    public GhostState GetState()
    {
        return currentState;
    }

    public void SetState(GhostState newState)
    {
        currentState = newState;
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (animator == null) return;

        switch (currentState)
        {
            case GhostState.Normal:
                animator.Play("Walk_Down");
                break;
            case GhostState.Scared:
                animator.Play("Ghost_Scared");
                break;
            case GhostState.Recovering:
                animator.Play("Ghost_Recovering");
                break;
            case GhostState.Dead:
                animator.Play("Ghost_Dead");
                break;
        }
    }

    private void DecideNextMove()
    {
        if (currentState == GhostState.Dead)
        {
            MoveTowardsHome();
            return;
        }

        if (isInSpawnZone)
        {
            MoveTowardsExitGate();
            return;
        }

        Vector3 nextDir = ChooseDirection();
        if (nextDir != Vector3.zero)
        {
            lastMoveDir = nextDir;
            targetGridPos = currentGridPos + nextDir;
            StartCoroutine(LerpGhostRoutine());
        }
    }

    private void MoveTowardsExitGate()
    {
        Vector3 dir = exitGatePos - currentGridPos;
        if (dir.magnitude <= 0.2f)
        {
            isInSpawnZone = false;
            currentGridPos = exitGatePos;
            transform.position = exitGatePos;
            lastMoveDir = (ghostId == 1 || ghostId == 3) ? Vector3.up : Vector3.down;

            Vector3 nextDir = ChooseDirection();
            if (nextDir != Vector3.zero)
            {
                lastMoveDir = nextDir;
                targetGridPos = currentGridPos + nextDir;
                StartCoroutine(LerpGhostRoutine());
            }
            return;
        }

        Vector3 step = Vector3.zero;
        if (Mathf.Abs(dir.x) > 0.05f)
        {
            float stepSize = Mathf.Min(1f, Mathf.Abs(dir.x));
            step = new Vector3(Mathf.Sign(dir.x) * stepSize, 0, 0);
        }
        else if (Mathf.Abs(dir.y) > 0.05f)
        {
            float stepSize = Mathf.Min(1f, Mathf.Abs(dir.y));
            step = new Vector3(0, Mathf.Sign(dir.y) * stepSize, 0);
        }

        lastMoveDir = step;
        targetGridPos = currentGridPos + step;
        StartCoroutine(LerpGhostRoutine());
    }

    private void MoveTowardsHome()
    {
        Vector3 dir = (homeSpawnPos - transform.position).normalized;
        float speed = baseSpeed * 0.5f;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, homeSpawnPos) < 0.2f)
        {
            transform.position = homeSpawnPos;
            currentGridPos = homeSpawnPos;
            targetGridPos = homeSpawnPos;
            isInSpawnZone = true;
            GameHUD hud = FindAnyObjectByType<GameHUD>();
            if (hud != null && hud.IsGhostScaredActive())
            {
                SetState(hud.IsGhostRecovering() ? GhostState.Recovering : GhostState.Scared);
            }
            else
            {
                SetState(GhostState.Normal);
            }
        }
    }

    private Vector3 ChooseDirection()
    {
        List<Vector3> validDirs = new List<Vector3>();
        Vector3[] allDirs = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };

        foreach (Vector3 d in allDirs)
        {
            if (lastMoveDir != Vector3.zero && d == -lastMoveDir) continue;
            Vector3 testPos = currentGridPos + d;
            if (IsValidTile(testPos))
            {
                validDirs.Add(d);
            }
        }

        if (validDirs.Count == 0)
        {
            if (lastMoveDir != Vector3.zero && IsValidTile(currentGridPos - lastMoveDir))
            {
                validDirs.Add(-lastMoveDir);
            }
        }

        if (validDirs.Count == 0) return Vector3.zero;

        if (currentState == GhostState.Scared || currentState == GhostState.Recovering || ghostId == 1)
        {
            return PickFarthestDir(validDirs);
        }

        if (ghostId == 2)
        {
            return PickClosestDir(validDirs);
        }

        if (ghostId == 3)
        {
            return validDirs[Random.Range(0, validDirs.Count)];
        }

        if (ghostId == 4)
        {
            return PickClockwiseDir(validDirs);
        }

        return validDirs[Random.Range(0, validDirs.Count)];
    }

    private Vector3 PickFarthestDir(List<Vector3> dirs)
    {
        if (pacStudentTransform == null) return dirs[Random.Range(0, dirs.Count)];

        Vector3 bestDir = dirs[0];
        float maxDist = -1f;

        foreach (Vector3 d in dirs)
        {
            float dist = Vector3.Distance(currentGridPos + d, pacStudentTransform.position);
            if (dist > maxDist)
            {
                maxDist = dist;
                bestDir = d;
            }
        }
        return bestDir;
    }

    private Vector3 PickClosestDir(List<Vector3> dirs)
    {
        if (pacStudentTransform == null) return dirs[Random.Range(0, dirs.Count)];

        Vector3 bestDir = dirs[0];
        float minDist = float.MaxValue;

        foreach (Vector3 d in dirs)
        {
            float dist = Vector3.Distance(currentGridPos + d, pacStudentTransform.position);
            if (dist < minDist)
            {
                minDist = dist;
                bestDir = d;
            }
        }
        return bestDir;
    }

    private Vector3 PickClockwiseDir(List<Vector3> dirs)
    {
        Vector3[] order = { Vector3.up, Vector3.right, Vector3.down, Vector3.left };
        foreach (Vector3 pref in order)
        {
            if (dirs.Contains(pref)) return pref;
        }
        return dirs[0];
    }

    private IEnumerator LerpGhostRoutine()
    {
        isLerping = true;
        Vector3 start = currentGridPos;
        float currentSpeed = baseSpeed;

        if (currentState == GhostState.Scared || currentState == GhostState.Recovering)
        {
            currentSpeed = baseSpeed * 0.5f;
        }

        float dist = Vector3.Distance(start, targetGridPos);
        float duration = dist / currentSpeed;
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
    }

    private bool IsValidTile(Vector3 pos)
    {
        int col = Mathf.RoundToInt(pos.x);
        int row = Mathf.RoundToInt(-pos.y);

        if (col < 0 || col > 27 || row < 0 || row > 28) return false;
        if (row == 14 && (col < 0 || col > 27)) return false;

        int tile = GetTileTypeAt(col, row);
        if (tile == 8) return false;
        return tile == 0 || tile == 5 || tile == 6;
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

    public void ResetGhost()
    {
        StopAllCoroutines();
        isLerping = false;
        transform.position = homeSpawnPos;
        currentGridPos = homeSpawnPos;
        targetGridPos = homeSpawnPos;
        lastMoveDir = Vector3.zero;
        isInSpawnZone = true;
        SetState(GhostState.Normal);
    }
}
