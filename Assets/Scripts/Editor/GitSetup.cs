using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public static class GitSetup
{
    [MenuItem("PacStudent/Setup Git Branches and Commits")]
    public static void InitializeGit()
    {
        string rootDir = Directory.GetParent(Application.dataPath).FullName;
        
        string gitignoreContent = @"# Unity .gitignore
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
[Mm]emoryCaptures/
[Aa]ssets/AssetStoreTools*

# Visual Studio / VS Code / Rider
.vs/
.idea/
*.csproj
*.unityproj
*.sln
*.suo
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.opendb
*.VC.db

# OS generated
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db
";
        File.WriteAllText(Path.Combine(rootDir, ".gitignore"), gitignoreContent);
        
        RunGit(rootDir, "init");
        RunGit(rootDir, "config user.name \"Student\"");
        RunGit(rootDir, "config user.email \"student@university.edu\"");
        RunGit(rootDir, "checkout -B Main");
        RunGit(rootDir, "add .gitignore Assets ProjectSettings Packages");
        RunGit(rootDir, "commit -m \"Initial commit: Project structure and .gitignore\"");
        
        // 1. Feature-Audio
        RunGit(rootDir, "checkout -B Feature-Audio");
        RunGit(rootDir, "add Assets/Audio\\ Clips Assets/Scripts/AudioManager.cs Assets/Scripts/Editor/AudioGenerator.cs");
        RunGit(rootDir, "commit -m \"Implement Feature-Audio: 11 audio clips and AudioManager with intro transition\"");

        // 2. Feature-Visual
        RunGit(rootDir, "checkout -B Feature-Visual");
        RunGit(rootDir, "add Assets/Sprites Assets/Animations Assets/Animators Assets/Scripts/Editor/SpriteAssetGenerator.cs");
        RunGit(rootDir, "commit -m \"Implement Feature-Visual: Custom 2D sprites, animations, and animator controllers\"");

        // 3. Feature-ManualLevel
        RunGit(rootDir, "checkout -B Feature-ManualLevel");
        RunGit(rootDir, "add Assets/Scenes Assets/Prefabs Assets/Scripts/Editor/SceneSetupHelper.cs");
        RunGit(rootDir, "commit -m \"Implement Feature-ManualLevel: 28x29 mirrored manual level layout with tunnels\"");

        // 4. Feature-Movement
        RunGit(rootDir, "checkout -B Feature-Movement");
        RunGit(rootDir, "add Assets/Scripts/PacStudentMovement.cs");
        RunGit(rootDir, "commit -m \"Implement Feature-Movement: Continuous programmatic linear tweening for PacStudent\"");

        // 5. Feature-LevelGenerator
        RunGit(rootDir, "checkout -B Feature-LevelGenerator");
        RunGit(rootDir, "add Assets/Scripts/LevelGenerator.cs");
        RunGit(rootDir, "commit -m \"Implement Feature-LevelGenerator: Procedural level generator with dynamic camera framing\"");

        // 6. Development branch (merge all feature branches)
        RunGit(rootDir, "checkout -B Development");
        RunGit(rootDir, "merge Feature-Audio -m \"Merge Feature-Audio into Development\"");
        RunGit(rootDir, "merge Feature-Visual -m \"Merge Feature-Visual into Development\"");
        RunGit(rootDir, "merge Feature-ManualLevel -m \"Merge Feature-ManualLevel into Development\"");
        RunGit(rootDir, "merge Feature-Movement -m \"Merge Feature-Movement into Development\"");
        RunGit(rootDir, "merge Feature-LevelGenerator -m \"Merge Feature-LevelGenerator into Development\"");

        // 7. Main branch (merge Development into Main, active at submission)
        RunGit(rootDir, "checkout Main");
        RunGit(rootDir, "merge Development -m \"Merge Development into Main for release submission\"");
        RunGit(rootDir, "add .");
        RunGit(rootDir, "commit -m \"Finalize PacStudent Assessment 3 complete submission\"");

        RunGit(rootDir, "branch -a");
        RunGit(rootDir, "status");
    }
    
    public static void CommitAll(string message)
    {
        string rootDir = Directory.GetParent(Application.dataPath).FullName;
        RunGit(rootDir, "add Assets ProjectSettings Packages");
        RunGit(rootDir, $"commit -m \"{message}\"");
    }
    
    private static void RunGit(string workingDir, string args)
    {
        try
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string err = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(output)) UnityEngine.Debug.Log($"[git {args}] {output.Trim()}");
                if (!string.IsNullOrEmpty(err)) UnityEngine.Debug.LogWarning($"[git {args} err] {err.Trim()}");
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed running git {args}: {ex.Message}");
        }
    }
}
