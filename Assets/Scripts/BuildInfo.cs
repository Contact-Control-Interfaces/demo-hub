using System;
using UnityEngine;

/// <summary>
/// What this build is and which commit it came from, logged at startup
/// (dev-handbook releases/versioning.md, rule 12).
///
/// Assets/Editor/BuildInfoWriter.cs writes Resources/BuildInfo.json into the
/// build and removes it afterwards; it is never committed. A run without it
/// (Play mode in the editor) reports the declared version with a "-dev" suffix
/// and an unknown commit.
/// </summary>
[Serializable]
public class BuildInfo
{
    public const string ResourceName = "BuildInfo";

    /// <summary>Full version: 3.1.0, 3.1.0-rc.1, 3.1.0-ci.42, 3.1.0-dev+3f2a9c1.dirty.</summary>
    public string version;

    /// <summary>Full 40-character SHA of the commit that was built.</summary>
    public string commit;

    /// <summary>True when tracked files had uncommitted changes at build time.</summary>
    public bool dirty;

    /// <summary>GitHub Actions run id for a CI build, "local" otherwise.</summary>
    public string buildId;

    /// <summary>The Unity editor and scripting backend that built the player.</summary>
    public string toolchain;

    private static BuildInfo current;

    public static BuildInfo Current
    {
        get
        {
            if (current == null)
            {
                current = Load();
            }
            return current;
        }
    }

    private static BuildInfo Load()
    {
        var asset = Resources.Load<TextAsset>(ResourceName);
        if (asset != null)
        {
            var info = JsonUtility.FromJson<BuildInfo>(asset.text);
            if (info != null && !string.IsNullOrEmpty(info.version))
            {
                return info;
            }
        }

        return new BuildInfo
        {
            version = Application.version + "-dev",
            commit = "unknown",
            dirty = false,
            buildId = Application.isEditor ? "editor" : "unknown",
            toolchain = "Unity " + Application.unityVersion,
        };
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LogAtStartup()
    {
        Debug.Log(Current.ToString());
    }

    public override string ToString()
    {
        return string.Format(
            "{0} {1} (commit {2}{3}, build {4}, {5})",
            Application.productName, version, commit, dirty ? ", dirty" : "", buildId, toolchain);
    }
}
