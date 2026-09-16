using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class SceneSetupHelper
{
    [MenuItem("PacStudent/Setup Complete Scene")]
    public static void SetupCompleteScene()
    {
        // 1. Ensure all assets exist
        AudioGenerator.GenerateAllAudio();
        SpriteAssetGenerator.GenerateAllVisuals();

        // 2. Load or create SampleScene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        // Clear existing scene objects
        var rootObjects = scene.GetRootGameObjects();
        foreach (var obj in rootObjects)
        {
            Object.DestroyImmediate(obj);
        }

        // --- CAMERA SETUP ---
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.05f, 1f); // Dark retro cyber background
        cam.orthographic = true;
        camObj.AddComponent<AudioListener>();

        // Center camera for 28 cols x 29 rows (tileSize = 1.0)
        // Center X = 13.5, Center Y = -14.0
        camObj.transform.position = new Vector3(13.5f, -14.0f, -10f);
        cam.orthographicSize = 16.5f;

        // --- MANAGERS & AUDIO SETUP ---
        GameObject audioObj = new GameObject("AudioManager");
        AudioManager audioManager = audioObj.AddComponent<AudioManager>();

        // Load Audio Clips
        AudioClip introMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/Background music – game intro.wav");
        AudioClip startSceneMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/Background music – StartScene.wav");
        AudioClip ghostsNormalMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/Background music – ghosts normal state.wav");
        AudioClip ghostsScaredMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/Background music – ghosts scared state.wav");
        AudioClip ghostDeadMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/Background music – at least one ghost dead.wav");

        AudioClip sfxMove = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/SFX – PacStudent moving.wav");
        AudioClip sfxPellet = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/SFX – PacStudent eats pellet.wav");
        AudioClip sfxGhost = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/SFX – PacStudent eats ghost.wav");
        AudioClip sfxCherry = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/SFX – PacStudent eats bonus cherry.wav");
        AudioClip sfxWall = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/SFX – PacStudent collides with wall.wav");
        AudioClip sfxDeath = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio Clips/SFX – PacStudent death animation.wav");

        SerializedObject audioSO = new SerializedObject(audioManager);
        audioSO.FindProperty("introMusic").objectReferenceValue = introMusic;
        audioSO.FindProperty("startSceneMusic").objectReferenceValue = startSceneMusic;
        audioSO.FindProperty("ghostsNormalMusic").objectReferenceValue = ghostsNormalMusic;
        audioSO.FindProperty("ghostsScaredMusic").objectReferenceValue = ghostsScaredMusic;
        audioSO.FindProperty("ghostDeadMusic").objectReferenceValue = ghostDeadMusic;
        audioSO.FindProperty("pacStudentMoveSFX").objectReferenceValue = sfxMove;
        audioSO.FindProperty("eatPelletSFX").objectReferenceValue = sfxPellet;
        audioSO.FindProperty("eatGhostSFX").objectReferenceValue = sfxGhost;
        audioSO.FindProperty("eatCherrySFX").objectReferenceValue = sfxCherry;
        audioSO.FindProperty("wallCollideSFX").objectReferenceValue = sfxWall;
        audioSO.FindProperty("deathSFX").objectReferenceValue = sfxDeath;
        audioSO.ApplyModifiedProperties();

        // --- LEVEL GENERATOR SETUP ---
        GameObject genObj = new GameObject("LevelGenerator");
        LevelGenerator levelGen = genObj.AddComponent<LevelGenerator>();

        levelGen.sprite0_Empty = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_0_Empty.png");
        levelGen.sprite1_OutsideCorner = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_1_OutsideCorner.png");
        levelGen.sprite2_OutsideWall = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_2_OutsideWall.png");
        levelGen.sprite3_InsideCorner = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_3_InsideCorner.png");
        levelGen.sprite4_InsideWall = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_4_InsideWall.png");
        levelGen.sprite5_StandardPellet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_5_StandardPellet.png");
        levelGen.sprite6_PowerPellet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_6_PowerPellet.png");
        levelGen.sprite7_TJunction = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_7_TJunction.png");
        levelGen.sprite8_GhostExit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Wall_8_GhostExit.png");
        levelGen.powerPelletAnimator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animators/PowerPelletAnimator.controller");

        // --- MANUAL LEVEL SETUP (Pre-built in scene for Grade 15) ---
        BuildManualLevel(levelGen);

        // --- CHARACTERS SETUP ---
        GameObject charactersParent = new GameObject("Characters");

        // 1. PacStudent
        GameObject pacObj = new GameObject("PacStudent");
        pacObj.transform.SetParent(charactersParent.transform);
        pacObj.transform.position = new Vector3(1f, -1f, 0f);

        SpriteRenderer pacSR = pacObj.AddComponent<SpriteRenderer>();
        pacSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PacStudent/PacStudent_Right_0.png");
        pacSR.sortingOrder = 10;

        Animator pacAnim = pacObj.AddComponent<Animator>();
        pacAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animators/PacStudentAnimator.controller");

        PacStudentMovement pacMovement = pacObj.AddComponent<PacStudentMovement>();
        pacMovement.SetWaypoints(
            new Vector3(1f, -1f, 0f),
            new Vector3(6f, -1f, 0f),
            new Vector3(6f, -5f, 0f),
            new Vector3(1f, -5f, 0f)
        );

        // 2. Four Ghosts (placed inside/near ghost house to cycle animators)
        // Ghost House is near row 13..15, cols 11..16
        Vector3[] ghostPositions = new Vector3[] {
            new Vector3(12.5f, -13.0f, 0f), // Ghost 1 (Blinky / Red)
            new Vector3(14.5f, -13.0f, 0f), // Ghost 2 (Pinky / Pink)
            new Vector3(11.5f, -14.5f, 0f), // Ghost 3 (Inky / Cyan)
            new Vector3(15.5f, -14.5f, 0f)  // Ghost 4 (Clyde / Orange)
        };

        for (int g = 1; g <= 4; g++)
        {
            GameObject ghostObj = new GameObject($"Ghost_{g}");
            ghostObj.transform.SetParent(charactersParent.transform);
            ghostObj.transform.position = ghostPositions[g - 1];

            SpriteRenderer ghostSR = ghostObj.AddComponent<SpriteRenderer>();
            ghostSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Ghosts/Ghost{g}_Right_0.png");
            ghostSR.sortingOrder = 9;

            Animator ghostAnim = ghostObj.AddComponent<Animator>();
            ghostAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"Assets/Animators/GhostAnimator_Ghost{g}.controller");
        }

        // --- BONUS CHERRY & LIFE DISPLAY (HUD / Visuals) ---
        GameObject hudParent = new GameObject("HUD");
        // Bonus score cherry preview
        GameObject cherryObj = new GameObject("BonusScoreCherry");
        cherryObj.transform.SetParent(hudParent.transform);
        cherryObj.transform.position = new Vector3(13.5f, -17f, 0f);
        SpriteRenderer cherrySR = cherryObj.AddComponent<SpriteRenderer>();
        cherrySR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/Item_BonusCherry.png");
        cherrySR.sortingOrder = 5;

        // Life Indicators (3 icons in bottom left)
        for (int i = 0; i < 3; i++)
        {
            GameObject lifeObj = new GameObject($"Life_{i + 1}");
            lifeObj.transform.SetParent(hudParent.transform);
            lifeObj.transform.position = new Vector3(1f + (i * 1.5f), -29.5f, 0f);
            SpriteRenderer lifeSR = lifeObj.AddComponent<SpriteRenderer>();
            lifeSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Life_Indicator.png");
            lifeSR.sortingOrder = 10;
        }

        // Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Create Prefabs
        CreatePrefabs(pacObj, charactersParent, levelGen);

        Debug.Log("Successfully setup complete PacStudent scene in Assets/Scenes/SampleScene.unity!");
    }

    private static void BuildManualLevel(LevelGenerator gen)
    {
        GameObject manualLevelParent = new GameObject("ManualLevel");

        int qRows = gen.levelMap.GetLength(0);
        int qCols = gen.levelMap.GetLength(1);
        int fullRows = (qRows * 2) - 1;
        int fullCols = qCols * 2;

        int[,] fullGrid = new int[fullRows, fullCols];

        // 1. Top-Left Quadrant
        for (int r = 0; r < qRows; r++)
        {
            for (int c = 0; c < qCols; c++)
            {
                fullGrid[r, c] = gen.levelMap[r, c];
            }
        }

        // 2. Top-Right Quadrant
        for (int r = 0; r < qRows; r++)
        {
            for (int c = 0; c < qCols; c++)
            {
                int mirroredCol = (fullCols - 1) - c;
                fullGrid[r, mirroredCol] = gen.levelMap[r, c];
            }
        }

        // 3. Bottom-Left Quadrant
        for (int r = 0; r < qRows - 1; r++)
        {
            int targetRow = (fullRows - 1) - r;
            for (int c = 0; c < qCols; c++)
            {
                fullGrid[targetRow, c] = gen.levelMap[r, c];
            }
        }

        // 4. Bottom-Right Quadrant
        for (int r = 0; r < qRows - 1; r++)
        {
            int targetRow = (fullRows - 1) - r;
            for (int c = 0; c < qCols; c++)
            {
                int mirroredCol = (fullCols - 1) - c;
                fullGrid[targetRow, mirroredCol] = gen.levelMap[r, c];
            }
        }

        // Spawn Manual Tile GameObjects
        for (int r = 0; r < fullRows; r++)
        {
            for (int c = 0; c < fullCols; c++)
            {
                int tileType = fullGrid[r, c];
                if (tileType == 0) continue;

                Vector3 position = new Vector3(c * 1.0f, -r * 1.0f, 0f);
                Quaternion rotation = gen.CalculateTileRotation(fullGrid, r, c, tileType, fullRows, fullCols);

                GameObject tileObj = new GameObject($"ManualTile_{r}_{c}_Type{tileType}");
                tileObj.transform.SetParent(manualLevelParent.transform);
                tileObj.transform.position = position;
                tileObj.transform.rotation = rotation;

                SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sprite = gen.GetSpriteForType(tileType);
                sr.sortingOrder = (tileType == 5 || tileType == 6) ? 1 : 0;

                if (tileType == 6 && gen.powerPelletAnimator != null)
                {
                    Animator anim = tileObj.AddComponent<Animator>();
                    anim.runtimeAnimatorController = gen.powerPelletAnimator;
                }
            }
        }
    }

    private static void CreatePrefabs(GameObject pacObj, GameObject charactersParent, LevelGenerator gen)
    {
        PrefabUtility.SaveAsPrefabAsset(pacObj, "Assets/Prefabs/PacStudent.prefab");
        for (int g = 1; g <= 4; g++)
        {
            Transform t = charactersParent.transform.Find($"Ghost_{g}");
            if (t != null)
            {
                PrefabUtility.SaveAsPrefabAsset(t.gameObject, $"Assets/Prefabs/Ghost_{g}.prefab");
            }
        }
    }
}
