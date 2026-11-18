using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// The player build CI runs (.github/workflows/ci.yml, buildMethod: CiBuild.Build).
/// It does what game-ci/unity-builder's default build method does for this
/// project: builds the enabled scenes in Build Settings for -customBuildTarget
/// to -customBuildPath, with Player Settings > Version set to -buildVersion.
///
/// It also writes Unity's own build result to the file named by
/// -ciBuildResultFile, before the editor exits. The Linux editor can crash
/// while shutting down after a build that succeeded (exit code 139), which
/// game-ci reports as a failed build; CI accepts that crash only when this
/// file says the build succeeded.
/// </summary>
public static class CiBuild
{
    public static void Build()
    {
        var path = RequiredArgument("-customBuildPath");
        var target = (BuildTarget)Enum.Parse(typeof(BuildTarget), RequiredArgument("-customBuildTarget"));
        var resultFile = Argument("-ciBuildResultFile");
        if (resultFile != null)
        {
            resultFile = Path.Combine(Directory.GetParent(Application.dataPath).FullName, resultFile);
            if (File.Exists(resultFile))
            {
                File.Delete(resultFile);
            }
        }

        var version = Argument("-buildVersion");
        if (version != null && version != "none")
        {
            PlayerSettings.bundleVersion = version;
        }

        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = path,
            target = target,
            options = BuildOptions.None,
        };
        var summary = BuildPipeline.BuildPlayer(options).summary;

        Console.WriteLine(
            "\nCiBuild: " + summary.result + " in " + summary.totalTime
            + ", " + summary.totalWarnings + " warnings, " + summary.totalErrors + " errors, "
            + summary.totalSize + " bytes\n");

        if (resultFile != null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(resultFile));
            File.WriteAllText(resultFile, summary.result + "\n");
        }

        EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    private static string RequiredArgument(string name)
    {
        return Argument(name) ?? throw new ArgumentException("CiBuild needs " + name + " on the command line.");
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
}
