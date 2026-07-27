using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor often keeps a stale PATH (from Hub launch) and misses Python/uv.
/// MCP for Unity reports "Python not found in PATH" even when a terminal finds them.
/// This bootstrap prepends the known install dirs to the Unity process PATH every load.
/// </summary>
[InitializeOnLoad]
internal static class McpPythonPathBootstrap
{
    private const string UvxPrefKey = "MCPForUnity.UvxPath";

    private static readonly string[] PathDirs =
    {
        @"C:\Users\Intel\.local\bin",
        @"C:\Users\Intel\AppData\Local\Programs\Python\Python312",
        @"C:\Users\Intel\AppData\Local\Programs\Python\Python312\Scripts",
    };

    private const string UvxPath = @"C:\Users\Intel\.local\bin\uvx.exe";
    private const string PythonPath = @"C:\Users\Intel\AppData\Local\Programs\Python\Python312\python.exe";

    static McpPythonPathBootstrap()
    {
        try
        {
            PrependProcessPath(PathDirs);

            if (File.Exists(UvxPath))
                EditorPrefs.SetString(UvxPrefKey, UvxPath);

            // Ensure python.exe is discoverable via directories MCP already augments (~/.local/bin)
            EnsurePythonShimInLocalBin();

            Debug.Log(
                $"[MCP bootstrap] PATH patched. Python exists={File.Exists(PythonPath)}, uvx exists={File.Exists(UvxPath)}. " +
                "Re-open Window → MCP for Unity (or press Refresh) if it still shows Python missing.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MCP bootstrap] Failed to patch PATH: {ex.Message}");
        }
    }

    private static void PrependProcessPath(string[] dirs)
    {
        string current = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var parts = current.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string dir in dirs)
        {
            if (!Directory.Exists(dir))
                continue;

            bool already = false;
            foreach (string part in parts)
            {
                if (string.Equals(part.TrimEnd('\\', '/'), dir.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
                {
                    already = true;
                    break;
                }
            }

            if (!already)
                current = dir + Path.PathSeparator + current;
        }

        Environment.SetEnvironmentVariable("PATH", current);
    }

    private static void EnsurePythonShimInLocalBin()
    {
        if (!File.Exists(PythonPath))
            return;

        string localBin = PathDirs[0];
        Directory.CreateDirectory(localBin);

        foreach (string name in new[] { "python.exe", "python3.exe", "pythonw.exe" })
        {
            string target = Path.Combine(localBin, name);
            string source = Path.Combine(Path.GetDirectoryName(PythonPath)!, name);
            if (!File.Exists(source))
                source = PythonPath;

            if (File.Exists(target))
                continue;

            try
            {
                // Hard link preferred (no admin); fallback to copy
                if (!CreateHardLink(target, source, IntPtr.Zero))
                    File.Copy(source, target, overwrite: false);
            }
            catch
            {
                try { File.Copy(source, target, overwrite: false); }
                catch { /* ignore */ }
            }
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

    [MenuItem("Tools/MCP/Fix Python PATH Now")]
    private static void FixNow()
    {
        PrependProcessPath(PathDirs);
        if (File.Exists(UvxPath))
            EditorPrefs.SetString(UvxPrefKey, UvxPath);
        EnsurePythonShimInLocalBin();
        EditorUtility.DisplayDialog(
            "MCP Python PATH",
            "PATH patched for this Unity session.\n\nOpen Window → MCP for Unity and refresh dependency check.",
            "OK");
    }
}
