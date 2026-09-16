using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public static class GitSetup
{
    [MenuItem("PacStudent/Setup Git Repository")]
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
        UnityEngine.Debug.Log($"Created .gitignore at {rootDir}");
        
        RunGit(rootDir, "init");
        RunGit(rootDir, "config user.name \"Student\"");
        RunGit(rootDir, "config user.email \"student@university.edu\"");
        RunGit(rootDir, "checkout -B Main");
        RunGit(rootDir, "add .gitignore Assets ProjectSettings Packages");
        RunGit(rootDir, "commit -m \"Initial commit: Project structure and .gitignore\"");
        
        string[] branches = new string[] {
            "Development",
            "Feature-Audio",
            "Feature-Visual",
            "Feature-ManualLevel",
            "Feature-Movement",
            "Feature-LevelGenerator"
        };
        
        foreach (var branch in branches)
        {
            RunGit(rootDir, $"branch {branch}");
            UnityEngine.Debug.Log($"Created branch: {branch}");
        }
        
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
