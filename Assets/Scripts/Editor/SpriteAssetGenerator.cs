using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public static class SpriteAssetGenerator
{
    private const int Size = 64;

    [MenuItem("PacStudent/Generate All Visual Assets")]
    public static void GenerateAllVisuals()
    {
        EnsureDirectories();

        // 1. Generate Wall and Environment Sprites
        GenerateEnvironmentSprites();

        // 2. Generate Items & UI Sprites
        GenerateItemSprites();

        // 3. Generate PacStudent Sprites & Animations
        GeneratePacStudentSpritesAndAnimations();

        // 4. Generate Ghosts Sprites & Animations
        GenerateGhostSpritesAndAnimations();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Successfully generated all visual sprites, animations, and animator controllers!");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = {
            "Assets/Sprites",
            "Assets/Sprites/Environment",
            "Assets/Sprites/Items",
            "Assets/Sprites/UI",
            "Assets/Sprites/PacStudent",
            "Assets/Sprites/Ghosts",
            "Assets/Animations",
            "Assets/Animations/PacStudent",
            "Assets/Animations/Ghosts",
            "Assets/Animations/Pellet",
            "Assets/Animators",
            "Assets/Prefabs"
        };

        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }

    #region Texture Drawing Helpers
    private static Texture2D CreateBlankTexture(Color bg)
    {
        Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        Color[] cols = new Color[Size * Size];
        for (int i = 0; i < cols.Length; i++) cols[i] = bg;
        tex.SetPixels(cols);
        return tex;
    }

    private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color, bool fill = true, int thickness = 2)
    {
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (fill)
                {
                    if (dist <= radius)
                    {
                        float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                        Color c = Color.Lerp(tex.GetPixel(x, y), color, alpha * color.a);
                        tex.SetPixel(x, y, c);
                    }
                }
                else
                {
                    if (dist >= radius - thickness && dist <= radius)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }
    }

    private static void DrawRect(Texture2D tex, int x0, int y0, int w, int h, Color color, bool fill = true, int thickness = 2)
    {
        for (int y = y0; y < y0 + h && y < Size; y++)
        {
            for (int x = x0; x < x0 + w && x < Size; x++)
            {
                if (x < 0 || y < 0) continue;
                if (fill || x < x0 + thickness || x >= x0 + w - thickness || y < y0 + thickness || y >= y0 + h - thickness)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void SaveTexture(Texture2D tex, string path, int ppu = 64)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
    #endregion

    #region Environment Sprites (Legend 0 - 8)
    private static void GenerateEnvironmentSprites()
    {
        Color neonCyan = new Color(0f, 0.9f, 1f, 1f);
        Color deepBlue = new Color(0.05f, 0.15f, 0.4f, 1f);
        Color darkBg = new Color(0.02f, 0.04f, 0.1f, 1f);
        Color gatePink = new Color(1f, 0.3f, 0.7f, 1f);

        // 0. Empty (transparent)
        Texture2D t0 = CreateBlankTexture(Color.clear);
        SaveTexture(t0, "Assets/Sprites/Environment/Wall_0_Empty.png");

        // 1. Outside corner (top-left rounded double corner)
        Texture2D t1 = CreateBlankTexture(darkBg);
        // Outer arc
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                // Top-left outer curve
                float d = Mathf.Sqrt(x * x + (Size - 1 - y) * (Size - 1 - y));
                if (d >= 20 && d <= 32)
                {
                    t1.SetPixel(x, y, neonCyan);
                }
                else if (d < 20)
                {
                    t1.SetPixel(x, y, deepBlue);
                }
            }
        }
        DrawRect(t1, 0, 0, Size, 8, neonCyan, true);
        DrawRect(t1, Size - 8, 0, 8, Size, neonCyan, true);
        SaveTexture(t1, "Assets/Sprites/Environment/Wall_1_OutsideCorner.png");

        // 2. Outside wall (straight horizontal double-line wall)
        Texture2D t2 = CreateBlankTexture(darkBg);
        DrawRect(t2, 0, 20, Size, 12, deepBlue, true);
        DrawRect(t2, 0, 32, Size, 6, neonCyan, true);
        DrawRect(t2, 0, 14, Size, 6, neonCyan, true);
        SaveTexture(t2, "Assets/Sprites/Environment/Wall_2_OutsideWall.png");

        // 3. Inside corner (inner turn wall)
        Texture2D t3 = CreateBlankTexture(darkBg);
        DrawRect(t3, 22, 0, 20, 42, deepBlue, true);
        DrawRect(t3, 22, 22, 42, 20, deepBlue, true);
        DrawRect(t3, 18, 0, 6, 46, neonCyan, true);
        DrawRect(t3, 18, 42, 46, 6, neonCyan, true);
        DrawCircle(t3, 24, 40, 8, neonCyan, true);
        SaveTexture(t3, "Assets/Sprites/Environment/Wall_3_InsideCorner.png");

        // 4. Inside wall (straight inner wall block)
        Texture2D t4 = CreateBlankTexture(darkBg);
        DrawRect(t4, 20, 0, 24, Size, deepBlue, true);
        DrawRect(t4, 16, 0, 6, Size, neonCyan, true);
        DrawRect(t4, 42, 0, 6, Size, neonCyan, true);
        SaveTexture(t4, "Assets/Sprites/Environment/Wall_4_InsideWall.png");

        // 7. T-Junction (3-way junction)
        Texture2D t7 = CreateBlankTexture(darkBg);
        DrawRect(t7, 0, 20, Size, 24, deepBlue, true);
        DrawRect(t7, 20, 0, 24, Size, deepBlue, true);
        DrawRect(t7, 0, 44, Size, 6, neonCyan, true);
        DrawRect(t7, 0, 14, 20, 6, neonCyan, true);
        DrawRect(t7, 44, 14, 20, 6, neonCyan, true);
        DrawRect(t7, 14, 0, 6, 20, neonCyan, true);
        DrawRect(t7, 44, 0, 6, 20, neonCyan, true);
        SaveTexture(t7, "Assets/Sprites/Environment/Wall_7_TJunction.png");

        // 8. Ghost exit wall (Hatched / cyber gate)
        Texture2D t8 = CreateBlankTexture(darkBg);
        DrawRect(t8, 0, 24, Size, 16, new Color(0.3f, 0.05f, 0.2f, 1f), true);
        for (int i = 0; i < Size; i += 8)
        {
            DrawRect(t8, i, 24, 4, 16, gatePink, true);
        }
        DrawRect(t8, 0, 38, Size, 4, gatePink, true);
        DrawRect(t8, 0, 22, Size, 4, gatePink, true);
        SaveTexture(t8, "Assets/Sprites/Environment/Wall_8_GhostExit.png");
    }
    #endregion

    #region Items Sprites (Standard Pellet, Power Pellet, Cherry, Life)
    private static void GenerateItemSprites()
    {
        Color gold = new Color(1f, 0.85f, 0.2f, 1f);
        Color goldGlow = new Color(1f, 0.95f, 0.6f, 0.9f);
        Color neonPurple = new Color(0.9f, 0.2f, 1f, 1f);
        Color brightWhite = Color.white;
        Color cherryRed = new Color(1f, 0.15f, 0.3f, 1f);
        Color stemGreen = new Color(0.2f, 0.9f, 0.3f, 1f);

        // Standard Pellet (5)
        Texture2D tPellet = CreateBlankTexture(Color.clear);
        DrawCircle(tPellet, 32, 32, 10, gold, true);
        DrawCircle(tPellet, 32, 32, 5, goldGlow, true);
        DrawCircle(tPellet, 32, 32, 2, brightWhite, true);
        SaveTexture(tPellet, "Assets/Sprites/Environment/Wall_5_StandardPellet.png");
        SaveTexture(tPellet, "Assets/Sprites/Items/Item_StandardPellet.png");

        // Power Pellet Frame 0 (6)
        Texture2D tPower0 = CreateBlankTexture(Color.clear);
        DrawCircle(tPower0, 32, 32, 22, neonPurple, true);
        DrawCircle(tPower0, 32, 32, 14, gold, true);
        DrawCircle(tPower0, 32, 32, 7, brightWhite, true);
        SaveTexture(tPower0, "Assets/Sprites/Environment/Wall_6_PowerPellet.png");
        SaveTexture(tPower0, "Assets/Sprites/Items/Item_PowerPellet_0.png");

        // Power Pellet Frame 1 (Flashing state)
        Texture2D tPower1 = CreateBlankTexture(Color.clear);
        DrawCircle(tPower1, 32, 32, 26, brightWhite, true);
        DrawCircle(tPower1, 32, 32, 18, neonPurple, true);
        DrawCircle(tPower1, 32, 32, 9, goldGlow, true);
        SaveTexture(tPower1, "Assets/Sprites/Items/Item_PowerPellet_1.png");

        // Bonus Cherry
        Texture2D tCherry = CreateBlankTexture(Color.clear);
        DrawCircle(tCherry, 22, 24, 14, cherryRed, true);
        DrawCircle(tCherry, 42, 20, 14, cherryRed, true);
        DrawCircle(tCherry, 20, 28, 4, brightWhite, true);
        DrawCircle(tCherry, 40, 24, 4, brightWhite, true);
        // Stems
        DrawRect(tCherry, 22, 36, 4, 16, stemGreen, true);
        DrawRect(tCherry, 38, 32, 4, 20, stemGreen, true);
        DrawRect(tCherry, 24, 50, 16, 4, stemGreen, true);
        SaveTexture(tCherry, "Assets/Sprites/Items/Item_BonusCherry.png");

        // Life Indicator
        Texture2D tLife = CreateBlankTexture(Color.clear);
        DrawCircle(tLife, 32, 32, 22, new Color(0f, 0.85f, 0.95f, 1f), true);
        // Scholar cap on icon
        DrawRect(tLife, 16, 44, 32, 6, Color.yellow, true);
        DrawRect(tLife, 28, 50, 8, 8, Color.yellow, true);
        // Cute visor eyes
        DrawRect(tLife, 22, 28, 8, 10, Color.black, true);
        DrawRect(tLife, 34, 28, 8, 10, Color.black, true);
        DrawRect(tLife, 24, 32, 3, 4, Color.cyan, true);
        DrawRect(tLife, 36, 32, 3, 4, Color.cyan, true);
        SaveTexture(tLife, "Assets/Sprites/UI/Life_Indicator.png");

        // Create Power Pellet Animator
        CreatePowerPelletAnimator();
    }
    #endregion

    #region PacStudent Sprites and Animations
    private static void GeneratePacStudentSpritesAndAnimations()
    {
        Color bodyCyan = new Color(0f, 0.85f, 0.95f, 1f);
        Color bodyGlow = new Color(0.6f, 0.95f, 1f, 1f);
        Color goldCap = new Color(1f, 0.8f, 0.1f, 1f);
        Color eyeBlack = new Color(0.05f, 0.05f, 0.1f, 1f);
        Color eyeGlow = new Color(0.2f, 1f, 0.5f, 1f);

        // --- UP ---
        // Frame 0 (back with pulsing antenna/treads)
        Texture2D up0 = CreateBlankTexture(Color.clear);
        DrawCircle(up0, 32, 30, 22, bodyCyan, true);
        DrawCircle(up0, 32, 30, 16, bodyGlow, true);
        DrawRect(up0, 18, 44, 28, 6, goldCap, true); // Cap
        DrawRect(up0, 29, 48, 6, 8, goldCap, true);
        DrawRect(up0, 14, 12, 10, 8, goldCap, true); // Left tread
        DrawRect(up0, 40, 12, 10, 8, goldCap, true); // Right tread
        SaveTexture(up0, "Assets/Sprites/PacStudent/PacStudent_Up_0.png");

        // Frame 1
        Texture2D up1 = CreateBlankTexture(Color.clear);
        DrawCircle(up1, 32, 32, 23, bodyCyan, true);
        DrawCircle(up1, 32, 32, 17, bodyGlow, true);
        DrawRect(up1, 16, 46, 32, 6, goldCap, true);
        DrawRect(up1, 29, 50, 6, 10, goldCap, true);
        DrawRect(up1, 12, 10, 12, 10, goldCap, true);
        DrawRect(up1, 40, 10, 12, 10, goldCap, true);
        SaveTexture(up1, "Assets/Sprites/PacStudent/PacStudent_Up_1.png");

        // --- DOWN ---
        // Frame 0 (front facing scholar bot with big eyes)
        Texture2D dn0 = CreateBlankTexture(Color.clear);
        DrawCircle(dn0, 32, 28, 22, bodyCyan, true);
        DrawRect(dn0, 18, 44, 28, 6, goldCap, true);
        DrawRect(dn0, 29, 48, 6, 8, goldCap, true);
        // Eyes
        DrawRect(dn0, 20, 24, 8, 12, eyeBlack, true);
        DrawRect(dn0, 36, 24, 8, 12, eyeBlack, true);
        DrawRect(dn0, 22, 28, 4, 6, eyeGlow, true);
        DrawRect(dn0, 38, 28, 4, 6, eyeGlow, true);
        // Smile mouth
        DrawRect(dn0, 28, 14, 8, 4, eyeBlack, true);
        SaveTexture(dn0, "Assets/Sprites/PacStudent/PacStudent_Down_0.png");

        // Frame 1 (open animated mouth)
        Texture2D dn1 = CreateBlankTexture(Color.clear);
        DrawCircle(dn1, 32, 28, 22, bodyCyan, true);
        DrawRect(dn1, 18, 44, 28, 6, goldCap, true);
        DrawRect(dn1, 29, 48, 6, 8, goldCap, true);
        DrawRect(dn1, 20, 26, 8, 10, eyeBlack, true);
        DrawRect(dn1, 36, 26, 8, 10, eyeBlack, true);
        DrawRect(dn1, 22, 28, 4, 6, eyeGlow, true);
        DrawRect(dn1, 38, 28, 4, 6, eyeGlow, true);
        // Wide open mouth
        DrawRect(dn1, 26, 12, 12, 8, eyeBlack, true);
        DrawRect(dn1, 28, 14, 8, 4, new Color(1f, 0.3f, 0.4f, 1f), true);
        SaveTexture(dn1, "Assets/Sprites/PacStudent/PacStudent_Down_1.png");

        // --- LEFT ---
        // Frame 0
        Texture2D lt0 = CreateBlankTexture(Color.clear);
        DrawCircle(lt0, 30, 28, 22, bodyCyan, true);
        DrawRect(lt0, 14, 44, 28, 6, goldCap, true);
        DrawRect(lt0, 25, 48, 6, 8, goldCap, true);
        DrawRect(lt0, 44, 20, 10, 18, goldCap, true); // Backpack on right
        DrawRect(lt0, 16, 26, 8, 12, eyeBlack, true);
        DrawRect(lt0, 18, 28, 4, 6, eyeGlow, true);
        DrawRect(lt0, 12, 16, 10, 4, eyeBlack, true); // Mouth closed
        SaveTexture(lt0, "Assets/Sprites/PacStudent/PacStudent_Left_0.png");

        // Frame 1
        Texture2D lt1 = CreateBlankTexture(Color.clear);
        DrawCircle(lt1, 30, 28, 22, bodyCyan, true);
        DrawRect(lt1, 14, 44, 28, 6, goldCap, true);
        DrawRect(lt1, 25, 48, 6, 8, goldCap, true);
        DrawRect(lt1, 44, 20, 10, 18, goldCap, true);
        DrawRect(lt1, 16, 26, 8, 12, eyeBlack, true);
        DrawRect(lt1, 18, 28, 4, 6, eyeGlow, true);
        // Open mouth on left
        DrawRect(lt1, 10, 14, 12, 8, eyeBlack, true);
        SaveTexture(lt1, "Assets/Sprites/PacStudent/PacStudent_Left_1.png");

        // --- RIGHT ---
        // Frame 0
        Texture2D rt0 = CreateBlankTexture(Color.clear);
        DrawCircle(rt0, 34, 28, 22, bodyCyan, true);
        DrawRect(rt0, 22, 44, 28, 6, goldCap, true);
        DrawRect(rt0, 33, 48, 6, 8, goldCap, true);
        DrawRect(rt0, 10, 20, 10, 18, goldCap, true); // Backpack on left
        DrawRect(rt0, 40, 26, 8, 12, eyeBlack, true);
        DrawRect(rt0, 42, 28, 4, 6, eyeGlow, true);
        DrawRect(rt0, 42, 16, 10, 4, eyeBlack, true);
        SaveTexture(rt0, "Assets/Sprites/PacStudent/PacStudent_Right_0.png");

        // Frame 1
        Texture2D rt1 = CreateBlankTexture(Color.clear);
        DrawCircle(rt1, 34, 28, 22, bodyCyan, true);
        DrawRect(rt1, 22, 44, 28, 6, goldCap, true);
        DrawRect(rt1, 33, 48, 6, 8, goldCap, true);
        DrawRect(rt1, 10, 20, 10, 18, goldCap, true);
        DrawRect(rt1, 40, 26, 8, 12, eyeBlack, true);
        DrawRect(rt1, 42, 28, 4, 6, eyeGlow, true);
        // Open mouth on right
        DrawRect(rt1, 42, 14, 12, 8, eyeBlack, true);
        SaveTexture(rt1, "Assets/Sprites/PacStudent/PacStudent_Right_1.png");

        // --- DEAD STATES ---
        // Frame 0 (X_X eyes)
        Texture2D d0 = CreateBlankTexture(Color.clear);
        DrawCircle(d0, 32, 28, 22, new Color(0.8f, 0.2f, 0.3f, 1f), true);
        DrawRect(d0, 18, 24, 10, 10, Color.black, true);
        DrawRect(d0, 36, 24, 10, 10, Color.black, true);
        DrawRect(d0, 20, 26, 6, 6, Color.red, true);
        DrawRect(d0, 38, 26, 6, 6, Color.red, true);
        SaveTexture(d0, "Assets/Sprites/PacStudent/PacStudent_Dead_0.png");

        // Frame 1 (spinning glitch)
        Texture2D d1 = CreateBlankTexture(Color.clear);
        DrawCircle(d1, 32, 28, 18, new Color(1f, 0.5f, 0f, 1f), true);
        DrawRect(d1, 14, 26, 36, 4, Color.yellow, true);
        DrawRect(d1, 30, 10, 4, 36, Color.yellow, true);
        SaveTexture(d1, "Assets/Sprites/PacStudent/PacStudent_Dead_1.png");

        // Frame 2 (dissolving particles)
        Texture2D d2 = CreateBlankTexture(Color.clear);
        DrawCircle(d2, 32, 28, 12, Color.white, true);
        DrawCircle(d2, 16, 40, 4, Color.cyan, true);
        DrawCircle(d2, 48, 40, 4, Color.cyan, true);
        DrawCircle(d2, 20, 14, 4, Color.cyan, true);
        DrawCircle(d2, 44, 14, 4, Color.cyan, true);
        SaveTexture(d2, "Assets/Sprites/PacStudent/PacStudent_Dead_2.png");

        // Frame 3 (rebooting small dot)
        Texture2D d3 = CreateBlankTexture(Color.clear);
        DrawCircle(d3, 32, 28, 6, Color.white, true);
        SaveTexture(d3, "Assets/Sprites/PacStudent/PacStudent_Dead_3.png");

        AssetDatabase.Refresh();
        CreatePacStudentAnimator();
    }
    #endregion

    #region Ghost Sprites and Animations
    private static void GenerateGhostSpritesAndAnimations()
    {
        Color[] ghostColors = {
            new Color(1.0f, 0.2f, 0.25f, 1f), // Ghost 1: Cyber Red (Blinky)
            new Color(1.0f, 0.45f, 0.75f, 1f), // Ghost 2: Cyber Pink (Pinky)
            new Color(0.2f, 0.85f, 1.0f, 1f), // Ghost 3: Cyber Cyan (Inky)
            new Color(1.0f, 0.65f, 0.15f, 1f)  // Ghost 4: Cyber Orange (Clyde)
        };

        Color scaredColor = new Color(0.1f, 0.2f, 0.85f, 1f);
        Color recoveringColor = new Color(0.9f, 0.95f, 1.0f, 1f);
        Color deadColor = new Color(0.3f, 0.8f, 0.9f, 0.8f);

        for (int g = 1; g <= 4; g++)
        {
            Color baseCol = ghostColors[g - 1];

            // 4 Directions x 2 frames = 8 sprites
            // Up
            DrawGhostFrame($"Ghost{g}_Up_0", baseCol, 0, 0, 0);
            DrawGhostFrame($"Ghost{g}_Up_1", baseCol, 0, 0, 1);
            // Down
            DrawGhostFrame($"Ghost{g}_Down_0", baseCol, 0, -1, 0);
            DrawGhostFrame($"Ghost{g}_Down_1", baseCol, 0, -1, 1);
            // Left
            DrawGhostFrame($"Ghost{g}_Left_0", baseCol, -1, 0, 0);
            DrawGhostFrame($"Ghost{g}_Left_1", baseCol, -1, 0, 1);
            // Right
            DrawGhostFrame($"Ghost{g}_Right_0", baseCol, 1, 0, 0);
            DrawGhostFrame($"Ghost{g}_Right_1", baseCol, 1, 0, 1);
        }

        // Shared Scared States (2 frames)
        DrawScaredGhostFrame("Ghost_Scared_0", scaredColor, 0);
        DrawScaredGhostFrame("Ghost_Scared_1", scaredColor, 1);

        // Recovering States (2 frames - flashing)
        DrawScaredGhostFrame("Ghost_Recovering_0", recoveringColor, 0);
        DrawScaredGhostFrame("Ghost_Recovering_1", scaredColor, 1);

        // Dead States (2 frames - eyes only / reboot core)
        DrawDeadGhostFrame("Ghost_Dead_0", 0);
        DrawDeadGhostFrame("Ghost_Dead_1", 1);

        AssetDatabase.Refresh();

        // Create Ghost Animators (GhostAnimator_Ghost1 to GhostAnimator_Ghost4)
        for (int g = 1; g <= 4; g++)
        {
            CreateGhostAnimator(g);
        }
    }

    private static void DrawGhostFrame(string name, Color bodyCol, int lookX, int lookY, int frame)
    {
        Texture2D t = CreateBlankTexture(Color.clear);
        // Cyber Bot ghost body: Dome top + robotic skirts
        DrawCircle(t, 32, 34, 22, bodyCol, true);
        DrawRect(t, 10, 16, 44, 20, bodyCol, true);

        // Skirts animation
        if (frame == 0)
        {
            DrawCircle(t, 16, 14, 6, bodyCol, true);
            DrawCircle(t, 32, 14, 6, bodyCol, true);
            DrawCircle(t, 48, 14, 6, bodyCol, true);
        }
        else
        {
            DrawCircle(t, 22, 14, 6, bodyCol, true);
            DrawCircle(t, 42, 14, 6, bodyCol, true);
        }

        // Robot Antenna on top
        DrawRect(t, 30, 52, 4, 8, Color.yellow, true);
        DrawCircle(t, 32, 58, 4, Color.cyan, true);

        // Eyes
        int eyeOffX = lookX * 4;
        int eyeOffY = lookY * 4;

        DrawCircle(t, 24 + eyeOffX, 34 + eyeOffY, 7, Color.white, true);
        DrawCircle(t, 40 + eyeOffX, 34 + eyeOffY, 7, Color.white, true);
        DrawCircle(t, 25 + eyeOffX * 2, 34 + eyeOffY * 2, 4, Color.blue, true);
        DrawCircle(t, 41 + eyeOffX * 2, 34 + eyeOffY * 2, 4, Color.blue, true);

        SaveTexture(t, $"Assets/Sprites/Ghosts/{name}.png");
    }

    private static void DrawScaredGhostFrame(string name, Color col, int frame)
    {
        Texture2D t = CreateBlankTexture(Color.clear);
        DrawCircle(t, 32, 34, 22, col, true);
        DrawRect(t, 10, 16, 44, 20, col, true);

        if (frame == 0)
        {
            DrawCircle(t, 16, 14, 6, col, true);
            DrawCircle(t, 32, 14, 6, col, true);
            DrawCircle(t, 48, 14, 6, col, true);
        }
        else
        {
            DrawCircle(t, 22, 14, 6, col, true);
            DrawCircle(t, 42, 14, 6, col, true);
        }

        // Scared Eyes (yellow dots)
        DrawCircle(t, 24, 34, 4, Color.yellow, true);
        DrawCircle(t, 40, 34, 4, Color.yellow, true);

        // Wavy mouth
        for (int x = 20; x <= 44; x += 4)
        {
            int y = (x % 8 == 0) ? 22 : 26;
            DrawRect(t, x, y, 4, 3, Color.yellow, true);
        }

        SaveTexture(t, $"Assets/Sprites/Ghosts/{name}.png");
    }

    private static void DrawDeadGhostFrame(string name, int frame)
    {
        Texture2D t = CreateBlankTexture(Color.clear);
        // Floating holographic reboot eyes & core
        DrawCircle(t, 24, 32, 8, Color.white, true);
        DrawCircle(t, 40, 32, 8, Color.white, true);
        DrawCircle(t, 24, 32, 4, frame == 0 ? Color.cyan : Color.red, true);
        DrawCircle(t, 40, 32, 4, frame == 0 ? Color.cyan : Color.red, true);
        // Antenna reboot spark
        DrawRect(t, 30, 46, 4, 8, Color.yellow, true);

        SaveTexture(t, $"Assets/Sprites/Ghosts/{name}.png");
    }
    #endregion

    #region Animator Controllers Creation
    private static void CreatePowerPelletAnimator()
    {
        string clipPath = "Assets/Animations/Pellet/PowerPellet_Flash.anim";
        AnimationClip clip = new AnimationClip();
        clip.name = "PowerPellet_Flash";
        clip.frameRate = 4;

        Sprite s0 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/Item_PowerPellet_0.png");
        Sprite s1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/Item_PowerPellet_1.png");

        if (s0 != null && s1 != null)
        {
            EditorCurveBinding binding = new EditorCurveBinding();
            binding.type = typeof(SpriteRenderer);
            binding.path = "";
            binding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[2];
            keyframes[0] = new ObjectReferenceKeyframe { time = 0f, value = s0 };
            keyframes[1] = new ObjectReferenceKeyframe { time = 0.25f, value = s1 };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AssetDatabase.CreateAsset(clip, clipPath);

            string controllerPath = "Assets/Animators/PowerPelletAnimator.controller";
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var rootStateMachine = controller.layers[0].stateMachine;
            var state = rootStateMachine.AddState("Flashing");
            state.motion = clip;
            rootStateMachine.defaultState = state;
        }
    }

    private static void CreatePacStudentAnimator()
    {
        string controllerPath = "Assets/Animators/PacStudentAnimator.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Parameters
        controller.AddParameter("Direction", AnimatorControllerParameterType.Int);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

        var rootStateMachine = controller.layers[0].stateMachine;

        // Clips
        var clipUp = Create2FrameClip("PacStudent_Walk_Up", "Assets/Animations/PacStudent/PacStudent_Walk_Up.anim",
            "Assets/Sprites/PacStudent/PacStudent_Up_0.png", "Assets/Sprites/PacStudent/PacStudent_Up_1.png", 6);

        var clipDown = Create2FrameClip("PacStudent_Walk_Down", "Assets/Animations/PacStudent/PacStudent_Walk_Down.anim",
            "Assets/Sprites/PacStudent/PacStudent_Down_0.png", "Assets/Sprites/PacStudent/PacStudent_Down_1.png", 6);

        var clipLeft = Create2FrameClip("PacStudent_Walk_Left", "Assets/Animations/PacStudent/PacStudent_Walk_Left.anim",
            "Assets/Sprites/PacStudent/PacStudent_Left_0.png", "Assets/Sprites/PacStudent/PacStudent_Left_1.png", 6);

        var clipRight = Create2FrameClip("PacStudent_Walk_Right", "Assets/Animations/PacStudent/PacStudent_Walk_Right.anim",
            "Assets/Sprites/PacStudent/PacStudent_Right_0.png", "Assets/Sprites/PacStudent/PacStudent_Right_1.png", 6);

        var clipDead = CreateMultiFrameClip("PacStudent_Dead", "Assets/Animations/PacStudent/PacStudent_Dead.anim",
            new string[] {
                "Assets/Sprites/PacStudent/PacStudent_Dead_0.png",
                "Assets/Sprites/PacStudent/PacStudent_Dead_1.png",
                "Assets/Sprites/PacStudent/PacStudent_Dead_2.png",
                "Assets/Sprites/PacStudent/PacStudent_Dead_3.png"
            }, 4);

        var stateUp = rootStateMachine.AddState("Walk_Up");
        stateUp.motion = clipUp;

        var stateDown = rootStateMachine.AddState("Walk_Down");
        stateDown.motion = clipDown;

        var stateLeft = rootStateMachine.AddState("Walk_Left");
        stateLeft.motion = clipLeft;

        var stateRight = rootStateMachine.AddState("Walk_Right");
        stateRight.motion = clipRight;

        var stateDead = rootStateMachine.AddState("Dead");
        stateDead.motion = clipDead;

        rootStateMachine.defaultState = stateRight;

        // AnyState transitions to Walking directions
        AddAnyStateTransition(rootStateMachine, stateUp, "Direction", AnimatorConditionMode.Equals, 0);
        AddAnyStateTransition(rootStateMachine, stateDown, "Direction", AnimatorConditionMode.Equals, 1);
        AddAnyStateTransition(rootStateMachine, stateLeft, "Direction", AnimatorConditionMode.Equals, 2);
        AddAnyStateTransition(rootStateMachine, stateRight, "Direction", AnimatorConditionMode.Equals, 3);
        AddAnyStateTransition(rootStateMachine, stateDead, "IsDead", AnimatorConditionMode.If, 0);
    }

    private static void AddAnyStateTransition(AnimatorStateMachine sm, AnimatorState targetState, string paramName, AnimatorConditionMode mode, int threshold)
    {
        var trans = sm.AddAnyStateTransition(targetState);
        trans.AddCondition(mode, threshold, paramName);
        trans.duration = 0f;
        trans.canTransitionToSelf = false;
    }

    private static void CreateGhostAnimator(int ghostIndex)
    {
        string controllerPath = $"Assets/Animators/GhostAnimator_Ghost{ghostIndex}.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var rootStateMachine = controller.layers[0].stateMachine;

        // Create Clips for this ghost
        var clipRight = Create2FrameClip($"Ghost{ghostIndex}_Walk_Right", $"Assets/Animations/Ghosts/Ghost{ghostIndex}_Walk_Right.anim",
            $"Assets/Sprites/Ghosts/Ghost{ghostIndex}_Right_0.png", $"Assets/Sprites/Ghosts/Ghost{ghostIndex}_Right_1.png", 4);

        var clipDown = Create2FrameClip($"Ghost{ghostIndex}_Walk_Down", $"Assets/Animations/Ghosts/Ghost{ghostIndex}_Walk_Down.anim",
            $"Assets/Sprites/Ghosts/Ghost{ghostIndex}_Down_0.png", $"Assets/Sprites/Ghosts/Ghost{ghostIndex}_Down_1.png", 4);

        var clipLeft = Create2FrameClip($"Ghost{ghostIndex}_Walk_Left", $"Assets/Animations/Ghosts/Ghost{ghostIndex}_Walk_Left.anim",
            $"Assets/Sprites/Ghosts/Ghost{ghostIndex}_Left_0.png", $"Assets/Sprites/Ghosts/Ghost{ghostIndex}_Left_1.png", 4);

        var clipUp = Create2FrameClip($"Ghost{ghostIndex}_Walk_Up", $"Assets/Animations/Ghosts/Ghost{ghostIndex}_Walk_Up.anim",
            $"Assets/Sprites/Ghosts/Ghost{ghostIndex}_Up_0.png", $"Assets/Sprites/Ghosts/Ghost{ghostIndex}_Up_1.png", 4);

        var clipScared = Create2FrameClip($"Ghost_Scared", $"Assets/Animations/Ghosts/Ghost_Scared.anim",
            "Assets/Sprites/Ghosts/Ghost_Scared_0.png", "Assets/Sprites/Ghosts/Ghost_Scared_1.png", 4);

        var clipRecovering = Create2FrameClip($"Ghost_Recovering", $"Assets/Animations/Ghosts/Ghost_Recovering.anim",
            "Assets/Sprites/Ghosts/Ghost_Recovering_0.png", "Assets/Sprites/Ghosts/Ghost_Recovering_1.png", 4);

        var clipDead = Create2FrameClip($"Ghost_Dead", $"Assets/Animations/Ghosts/Ghost_Dead.anim",
            "Assets/Sprites/Ghosts/Ghost_Dead_0.png", "Assets/Sprites/Ghosts/Ghost_Dead_1.png", 4);

        var sRight = rootStateMachine.AddState("Walk_Right");
        sRight.motion = clipRight;

        var sDown = rootStateMachine.AddState("Walk_Down");
        sDown.motion = clipDown;

        var sLeft = rootStateMachine.AddState("Walk_Left");
        sLeft.motion = clipLeft;

        var sUp = rootStateMachine.AddState("Walk_Up");
        sUp.motion = clipUp;

        var sScared = rootStateMachine.AddState("Scared");
        sScared.motion = clipScared;

        var sRecovering = rootStateMachine.AddState("Recovering");
        sRecovering.motion = clipRecovering;

        var sDead = rootStateMachine.AddState("Dead");
        sDead.motion = clipDead;

        rootStateMachine.defaultState = sRight;

        // Transition cycle with Exit Time of 3.0s as required by Assessment specification:
        // "Ghost animators should cycle through all their states. Exit Time of 3.0 (or 2 seconds for the whole clip, whichever longer) on transitions."
        AddTimedTransition(sRight, sDown, 3.0f);
        AddTimedTransition(sDown, sLeft, 3.0f);
        AddTimedTransition(sLeft, sUp, 3.0f);
        AddTimedTransition(sUp, sScared, 3.0f);
        AddTimedTransition(sScared, sRecovering, 3.0f);
        AddTimedTransition(sRecovering, sDead, 3.0f);
        AddTimedTransition(sDead, sRight, 3.0f);
    }

    private static void AddTimedTransition(AnimatorState fromState, AnimatorState toState, float exitTimeDuration)
    {
        var trans = fromState.AddTransition(toState);
        trans.hasExitTime = true;
        // In Unity Animator, exitTime is normalized (1.0 = 1 full clip loop). 
        // A duration of 3.0 means 3 loops/seconds.
        trans.exitTime = exitTimeDuration;
        trans.duration = 0.1f;
        trans.hasFixedDuration = false;
    }

    private static AnimationClip Create2FrameClip(string clipName, string path, string sprite0Path, string sprite1Path, float fps)
    {
        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        AnimationClip clip = existing != null ? existing : new AnimationClip();
        clip.name = clipName;
        clip.frameRate = fps;

        Sprite s0 = AssetDatabase.LoadAssetAtPath<Sprite>(sprite0Path);
        Sprite s1 = AssetDatabase.LoadAssetAtPath<Sprite>(sprite1Path);

        if (s0 != null && s1 != null)
        {
            EditorCurveBinding binding = new EditorCurveBinding();
            binding.type = typeof(SpriteRenderer);
            binding.path = "";
            binding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[2];
            keyframes[0] = new ObjectReferenceKeyframe { time = 0f, value = s0 };
            keyframes[1] = new ObjectReferenceKeyframe { time = 1f / fps, value = s1 };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            if (existing == null) AssetDatabase.CreateAsset(clip, path);
            else EditorUtility.SetDirty(clip);
        }

        return clip;
    }

    private static AnimationClip CreateMultiFrameClip(string clipName, string path, string[] spritePaths, float fps)
    {
        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        AnimationClip clip = existing != null ? existing : new AnimationClip();
        clip.name = clipName;
        clip.frameRate = fps;

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "";
        binding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[spritePaths.Length];
        for (int i = 0; i < spritePaths.Length; i++)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
            keyframes[i] = new ObjectReferenceKeyframe { time = (float)i / fps, value = s };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (existing == null) AssetDatabase.CreateAsset(clip, path);
        else EditorUtility.SetDirty(clip);

        return clip;
    }
    #endregion
}
