using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LastSpark.Editor
{
    // Only saved game scenes belong in the build; URP templates live outside this folder.
    [InitializeOnLoad]
    internal static class LastSparkBuildScenes
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string MenuScene = SceneFolder + "/UI/Main UI.unity";
        private static bool queued;
        private static bool synchronizing;

        static LastSparkBuildScenes()
        {
            EditorApplication.projectChanged += Schedule;
            EditorBuildSettings.sceneListChanged += Schedule;
            Schedule();
        }

        private static void Schedule()
        {
            if (queued || synchronizing) return;
            queued = true;
            EditorApplication.delayCall += SynchronizeWhenReady;
        }

        private static void SynchronizeWhenReady()
        {
            queued = false;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Schedule();
                return;
            }
            Synchronize(false);
        }

        internal static EditorBuildSettingsScene[] CollectScenes()
        {
            return AssetDatabase.FindAssets("t:Scene", new[] { SceneFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path == MenuScene ? 0 : path.StartsWith(SceneFolder + "/Stage", StringComparison.Ordinal) ? 1 : 2)
                .ThenBy(path => path, StringComparer.Ordinal)
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();
        }

        internal static bool Matches(EditorBuildSettingsScene[] actual, EditorBuildSettingsScene[] expected)
        {
            return actual != null && actual.Length == expected.Length &&
                actual.Zip(expected, (a, b) => a.path == b.path && a.enabled == b.enabled).All(equal => equal);
        }

        [MenuItem("LAST SPARK/Sync Build Scenes")]
        private static void SynchronizeFromMenu()
        {
            Synchronize(true);
        }

        internal static void Synchronize(bool report)
        {
            if (synchronizing) return;
            var scenes = CollectScenes();
            if (scenes.Length == 0 || scenes[0].path != MenuScene)
            {
                Debug.LogWarning("[LAST SPARK] 시작 씬이 없습니다: " + MenuScene + ". 친구의 최신 씬을 받은 뒤 다시 동기화해 주세요.");
                return;
            }

            synchronizing = true;
            try
            {
                bool changed = !Matches(EditorBuildSettings.globalScenes, scenes);
                if (changed) EditorBuildSettings.globalScenes = scenes;

                // Profiles with overrides would otherwise continue to use an old list.
                foreach (string guid in AssetDatabase.FindAssets("t:BuildProfile", new[] { "Assets" }))
                {
                    var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(AssetDatabase.GUIDToAssetPath(guid));
                    if (profile == null || !profile.overrideGlobalScenes || Matches(profile.scenes, scenes)) continue;
                    profile.scenes = scenes;
                    EditorUtility.SetDirty(profile);
                    AssetDatabase.SaveAssetIfDirty(profile);
                    changed = true;
                }

                if (changed || report)
                    Debug.Log("[LAST SPARK] Scene List 동기화 완료: Main UI + 게임 씬 " + (scenes.Length - 1) + "개.");
            }
            finally
            {
                synchronizing = false;
            }
        }
    }

    internal sealed class LastSparkBuildSceneCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return -1000; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            var expected = LastSparkBuildScenes.CollectScenes();
            if (expected.Length == 0 || expected[0].path != "Assets/Scenes/UI/Main UI.unity")
                throw new BuildFailedException("LAST SPARK 시작 UI 씬을 찾지 못했습니다. 최신 씬을 받아 주세요.");

            if (!LastSparkBuildScenes.Matches(EditorBuildSettings.scenes, expected))
            {
                LastSparkBuildScenes.Synchronize(true);
                // Unity may already have captured the old list for this build.
                throw new BuildFailedException("Scene List를 갱신했습니다. Build를 다시 눌러 새 목록으로 빌드해 주세요.");
            }
        }
    }
}
