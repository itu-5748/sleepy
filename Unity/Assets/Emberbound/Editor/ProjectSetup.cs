using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emberbound.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/Scenes/Emberbound.unity";
        static ProjectSetup() { EditorApplication.delayCall += Configure; }
        static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            // First-import defaults are applied once, leaving later user settings intact.
            const string key = "EmberboundConfigured_";
            string pref = key + Application.dataPath;
            if (EditorPrefs.GetBool(pref)) return;
            PlayerSettings.companyName = "Emberbound";
            PlayerSettings.productName = "Emberbound";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.runInBackground = false;
            var settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (settings.Length > 0) {
                var serialized = new SerializedObject(settings[0]); var input = serialized.FindProperty("activeInputHandler");
                if (input != null) { input.intValue = 0; serialized.ApplyModifiedPropertiesWithoutUndo(); }
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorPrefs.SetBool(pref, true);
        }
        [MenuItem("Emberbound/Open Game Scene")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("Emberbound/Build Desktop (current platform)")]
        public static void BuildDesktop()
        {
            string destination = EditorUtility.SaveFolderPanel("Choose build folder", "", "EmberboundBuild");
            if (string.IsNullOrEmpty(destination)) return;
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            if (target != BuildTarget.StandaloneWindows64 && target != BuildTarget.StandaloneLinux64 && target != BuildTarget.StandaloneOSX) { Debug.LogError("Select a desktop platform in File > Build Profiles first."); return; }
            string name = target == BuildTarget.StandaloneWindows64 ? "Emberbound.exe" : target == BuildTarget.StandaloneOSX ? "Emberbound.app" : "Emberbound";
            var report = BuildPipeline.BuildPlayer(new[] { ScenePath }, System.IO.Path.Combine(destination, name), target, BuildOptions.None);
            Debug.Log("Emberbound build: " + report.summary.result);
        }
        // For licensed CI: Unity -batchmode -projectPath Unity -executeMethod Emberbound.Editor.ProjectSetup.BuildWeb -quit
        public static void BuildWeb()
        {
            Configure(); string path = System.Environment.GetEnvironmentVariable("EMBERBOUND_BUILD_PATH");
            if (string.IsNullOrEmpty(path)) path = "Builds/WebGL";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            var report = BuildPipeline.BuildPlayer(new[] { ScenePath }, path, BuildTarget.WebGL, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Web build failed: " + report.summary.result);
        }
        [System.Serializable] sealed class BrowserStats { public int health, power, haste; }
        [System.Serializable] sealed class BrowserSave { public int version, runs, embers; public BrowserStats stats; public string weapon; }
        [MenuItem("Emberbound/Import Browser Legacy JSON")]
        public static void ImportLegacy()
        {
            string file = EditorUtility.OpenFilePanel("Select exported browser save", "", "json"); if (string.IsNullOrEmpty(file)) return;
            try {
                var old = JsonUtility.FromJson<BrowserSave>(System.IO.File.ReadAllText(file));
                if (old == null || old.version != 1 || old.stats == null) throw new System.Exception("Not an Emberbound browser save.");
                var imported = new Legacy { runs = old.runs, embers = old.embers, health = old.stats.health, power = old.stats.power, haste = old.stats.haste, weapon = old.weapon == "spear" ? 1 : old.weapon == "orb" ? 2 : 0 };
                imported.Sanitize(); if (!EditorUtility.DisplayDialog("Replace Unity legacy?", "This replaces local Unity progression with the selected browser save.", "Import", "Cancel")) return;
                Game.StoreLegacy(imported); Debug.Log("Legacy imported. Start a new Play session to load it.");
            } catch (System.Exception ex) { Debug.LogError("Import failed: " + ex.Message); }
        }
    }
}
