using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [System.NonSerialized]
    public int[,] levelMap = {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0}
    };

    [Header("Sprite References (Legend 0 - 8)")]
    public Sprite sprite0_Empty;
    public Sprite sprite1_OutsideCorner;
    public Sprite sprite2_OutsideWall;
    public Sprite sprite3_InsideCorner;
    public Sprite sprite4_InsideWall;
    public Sprite sprite5_StandardPellet;
    public Sprite sprite6_PowerPellet;
    public Sprite sprite7_TJunction;
    public Sprite sprite8_GhostExit;

    [Header("Power Pellet Animator Controller")]
    public RuntimeAnimatorController powerPelletAnimator;

    [Header("Settings")]
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private bool autoGenerateOnStart = true;
    [SerializeField] private string manualLevelObjectName = "ManualLevel";

    private GameObject generatedLevelParent;

    private void Start()
    {
        if (autoGenerateOnStart)
        {
            DeleteManualLevel();
            GenerateLevel();
            AdjustCamera();
        }
    }

    public void DeleteManualLevel()
    {
        GameObject manualLevel = GameObject.Find(manualLevelObjectName);
        if (manualLevel != null)
        {
            Destroy(manualLevel);
        }
    }

    public void GenerateLevel()
    {
        if (generatedLevelParent != null)
        {
            Destroy(generatedLevelParent);
        }

        generatedLevelParent = new GameObject("ProceduralLevel");
        generatedLevelParent.transform.SetParent(transform);

        int qRows = levelMap.GetLength(0);
        int qCols = levelMap.GetLength(1);

        int fullRows = (qRows * 2) - 1;
        int fullCols = qCols * 2;

        int[,] fullGrid = new int[fullRows, fullCols];

        for (int r = 0; r < qRows; r++)
        {
            for (int c = 0; c < qCols; c++)
            {
                fullGrid[r, c] = levelMap[r, c];
            }
        }

        for (int r = 0; r < qRows; r++)
        {
            for (int c = 0; c < qCols; c++)
            {
                int mirroredCol = (fullCols - 1) - c;
                fullGrid[r, mirroredCol] = levelMap[r, c];
            }
        }

        for (int r = 0; r < qRows - 1; r++)
        {
            int targetRow = (fullRows - 1) - r;
            for (int c = 0; c < qCols; c++)
            {
                fullGrid[targetRow, c] = levelMap[r, c];
            }
        }

        for (int r = 0; r < qRows - 1; r++)
        {
            int targetRow = (fullRows - 1) - r;
            for (int c = 0; c < qCols; c++)
            {
                int mirroredCol = (fullCols - 1) - c;
                fullGrid[targetRow, mirroredCol] = levelMap[r, c];
            }
        }

        for (int r = 0; r < fullRows; r++)
        {
            for (int c = 0; c < fullCols; c++)
            {
                int tileType = fullGrid[r, c];
                if (tileType == 0) continue;

                Vector3 position = new Vector3(c * tileSize, -r * tileSize, 0f);
                Quaternion rotation = CalculateTileRotation(fullGrid, r, c, tileType, fullRows, fullCols);

                CreateTileObject(tileType, position, rotation, r, c, generatedLevelParent.transform);
            }
        }
    }

    private GameObject CreateTileObject(int tileType, Vector3 position, Quaternion rotation, int row, int col, Transform parent)
    {
        Sprite sprite = GetSpriteForType(tileType);
        if (sprite == null && tileType != 0) return null;

        string name = $"Tile_{row}_{col}_Type{tileType}";
        GameObject tileObj = new GameObject(name);
        tileObj.transform.SetParent(parent);
        tileObj.transform.position = position;
        tileObj.transform.rotation = rotation;

        SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        if (tileType == 5)
        {
            tileObj.tag = "Pellet";
            CircleCollider2D col2d = tileObj.AddComponent<CircleCollider2D>();
            col2d.isTrigger = true;
            col2d.radius = 0.25f;
            sr.sortingOrder = 1;
        }
        else if (tileType == 6)
        {
            tileObj.tag = "PowerPellet";
            CircleCollider2D col2d = tileObj.AddComponent<CircleCollider2D>();
            col2d.isTrigger = true;
            col2d.radius = 0.4f;
            sr.sortingOrder = 1;

            if (powerPelletAnimator != null)
            {
                Animator anim = tileObj.AddComponent<Animator>();
                anim.runtimeAnimatorController = powerPelletAnimator;
            }
        }
        else
        {
            sr.sortingOrder = 0;
        }

        return tileObj;
    }

    public Sprite GetSpriteForType(int type)
    {
        switch (type)
        {
            case 0: return sprite0_Empty;
            case 1: return sprite1_OutsideCorner;
            case 2: return sprite2_OutsideWall;
            case 3: return sprite3_InsideCorner;
            case 4: return sprite4_InsideWall;
            case 5: return sprite5_StandardPellet;
            case 6: return sprite6_PowerPellet;
            case 7: return sprite7_TJunction;
            case 8: return sprite8_GhostExit;
            default: return null;
        }
    }

    public Quaternion CalculateTileRotation(int[,] grid, int r, int c, int type, int totalRows, int totalCols)
    {
        bool hasUp = IsWallOrSame(grid, r - 1, c, totalRows, totalCols);
        bool hasDown = IsWallOrSame(grid, r + 1, c, totalRows, totalCols);
        bool hasLeft = IsWallOrSame(grid, r, c - 1, totalRows, totalCols);
        bool hasRight = IsWallOrSame(grid, r, c + 1, totalRows, totalCols);

        if (type == 2 || type == 4)
        {
            if ((hasUp || hasDown) && !(hasLeft && hasRight))
            {
                return Quaternion.Euler(0, 0, 90f);
            }
            return Quaternion.Euler(0, 0, 0f);
        }

        if (type == 1 || type == 3)
        {
            if (hasRight && hasDown) return Quaternion.Euler(0, 0, 0f);
            if (hasLeft && hasDown) return Quaternion.Euler(0, 0, -90f);
            if (hasLeft && hasUp) return Quaternion.Euler(0, 0, 180f);
            if (hasRight && hasUp) return Quaternion.Euler(0, 0, 90f);

            int midR = totalRows / 2;
            int midC = totalCols / 2;
            if (r < midR && c < midC) return Quaternion.Euler(0, 0, 0f);
            if (r < midR && c >= midC) return Quaternion.Euler(0, 0, -90f);
            if (r >= midR && c >= midC) return Quaternion.Euler(0, 0, 180f);
            return Quaternion.Euler(0, 0, 90f);
        }

        if (type == 7)
        {
            if (hasLeft && hasRight && hasDown) return Quaternion.Euler(0, 0, 0f);
            if (hasUp && hasDown && hasLeft) return Quaternion.Euler(0, 0, -90f);
            if (hasLeft && hasRight && hasUp) return Quaternion.Euler(0, 0, 180f);
            if (hasUp && hasDown && hasRight) return Quaternion.Euler(0, 0, 90f);
        }

        return Quaternion.identity;
    }

    private bool IsWallOrSame(int[,] grid, int r, int c, int totalRows, int totalCols)
    {
        if (r < 0 || r >= totalRows || c < 0 || c >= totalCols) return false;
        int t = grid[r, c];
        return t == 1 || t == 2 || t == 3 || t == 4 || t == 7 || t == 8;
    }

    public void AdjustCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        int qRows = levelMap.GetLength(0);
        int qCols = levelMap.GetLength(1);
        int fullRows = (qRows * 2) - 1;
        int fullCols = qCols * 2;

        float centerX = (fullCols - 1) * tileSize * 0.5f;
        float centerY = -(fullRows - 1) * tileSize * 0.5f;

        cam.transform.position = new Vector3(centerX, centerY, -10f);
        cam.orthographic = true;

        float targetHeight = fullRows * tileSize * 0.5f + 2.0f;
        float targetWidth = (fullCols * tileSize * 0.5f + 2.0f) / Mathf.Max(cam.aspect, 0.1f);

        cam.orthographicSize = Mathf.Max(targetHeight, targetWidth);
    }
}
