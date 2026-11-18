using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Writes Assets/Resources/BuildInfo.json before a player build and deletes it
/// afterwards, so every build carries its version and commit (dev-handbook
/// releases/versioning.md, rule 12). The file is gitignored; nothing here
/// changes a tracked file, and Player Settings > Version is only read.
///
/// CI passes the values on the Unity command line (see .github/workflows/ci.yml):
///   -buildInfoVersion 3.1.0-ci.42 -buildInfoCommit &lt;sha&gt; -buildInfoDirty false -buildInfoBuildId &lt;run id&gt;
/// A local build works them out from git instead, with the version
/// X.Y.Z-dev+&lt;short sha&gt; (or .dirty), so a laptop build can never pass for a release.
/// </summary>
public class BuildInfoWriter : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private const string AssetPath = "Assets/Resources/" + BuildInfo.ResourceName + ".json";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var commit = Argument("-buildInfoCommit") ?? Git("rev-parse HEAD") ?? "unknown";
        var dirtyArgument = Argument("-buildInfoDirty");
        var dirty = dirtyArgument != null
            ? string.Equals(dirtyArgument, "true", StringComparison.OrdinalIgnoreCase)
            : !string.IsNullOrEmpty(Git("status --porcelain --untracked-files=no"));

        var version = Argument("-buildInfoVersion");
        if (version == null)
        {
            version = PlayerSettings.bundleVersion + "-dev";
            if (commit.Length >= 7 && commit != "unknown")
            {
                version += "+" + commit.Substring(0, 7) + (dirty ? ".dirty" : "");
            }
        }

        var backend = PlayerSettings.GetScriptingBackend(
            NamedBuildTarget.FromBuildTargetGroup(report.summary.platformGroup));

        var info = new BuildInfo
        {
            version = version,
            commit = commit,
            dirty = dirty,
            buildId = Argument("-buildInfoBuildId") ?? "local",
            toolchain = "Unity " + Application.unityVersion + ", " + backend,
        };

        Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
        File.WriteAllText(AssetPath, JsonUtility.ToJson(info, true));
        AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceSynchronousImport);
        UnityEngine.Debug.Log("BuildInfo: " + info);
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        AssetDatabase.DeleteAsset(AssetPath);
    }

    // The value after `name` on the Unity command line, or null.
    private static string Argument(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(args[i + 1]))
            {
                return args[i + 1].Trim();
            }
        }
        return null;
    }

    // Trimmed stdout of a git command in the project folder, or null if git
    // isn't available or the command failed.
    private static string Git(string arguments)
    {
        try
        {
            var start = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = Directory.GetParent(Application.dataPath).FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using (var process = Process.Start(start))
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode == 0 ? output.Trim() : null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}
