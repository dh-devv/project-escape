using Carten;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LastSpark.Editor
{
    // Validate the build's scene copy without opening or rewriting the user's working scenes.
    internal sealed class LastSparkBuildContentCheck : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null || (!scene.name.StartsWith("Stage1_Map") && scene.name != "Main UI")) return;
            bool hasMenu = false;
            bool hasPlayerUi = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MainMenuUI menu in root.GetComponentsInChildren<MainMenuUI>(true))
                {
                    Image background = new SerializedObject(menu).FindProperty("background").objectReferenceValue as Image;
                    Require(menu.isActiveAndEnabled && background != null && background.sprite != null,
                        scene, "시작 화면의 배경 또는 시작 컨트롤러가 연결되지 않았습니다.");
                    hasMenu = true;
                }
                foreach (GameplayUI hud in root.GetComponentsInChildren<GameplayUI>(true))
                {
                    Require(hud.isActiveAndEnabled && new SerializedObject(hud).FindProperty("font").objectReferenceValue != null,
                        scene, "HUD 또는 한글 폰트가 연결되지 않았습니다.");
                    hasPlayerUi = true;
                }
                foreach (EncounterController encounter in root.GetComponentsInChildren<EncounterController>(true))
                {
                    Require(encounter.isActiveAndEnabled, scene, encounter.name + " 전투 구역이 꺼져 있습니다.");
                    var data = new SerializedObject(encounter);
                    Object spawner = data.FindProperty("enemySpawner").objectReferenceValue;
                    Require(spawner != null, scene, encounter.name + " 적 생성기가 없습니다.");
                    Require(new SerializedObject(spawner).FindProperty("enemy1Prefab").objectReferenceValue != null,
                        scene, encounter.name + " 적 프리팹이 없습니다.");
                    SerializedProperty points = data.FindProperty("spawnPoints");
                    Require(points.arraySize > 0, scene, encounter.name + " 적 생성 위치가 없습니다.");
                    for (int i = 0; i < points.arraySize; i++)
                        Require(points.GetArrayElementAtIndex(i).objectReferenceValue != null,
                            scene, encounter.name + " 적 생성 위치가 비어 있습니다.");
                }
                foreach (BossController boss in root.GetComponentsInChildren<BossController>(true))
                    Require(boss.isActiveAndEnabled, scene, boss.name + " 보스가 꺼져 있습니다.");
            }
            if (scene.name == "Main UI") Require(hasMenu, scene, "시작 UI 컨트롤러가 없습니다.");
            if (scene.name == "Stage1_Map01") Require(hasPlayerUi, scene, "플레이어 HUD가 없습니다.");
        }

        private static void Require(bool condition, Scene scene, string message)
        {
            if (!condition) throw new BuildFailedException("[LAST SPARK] " + scene.name + ": " + message);
        }
    }
}
