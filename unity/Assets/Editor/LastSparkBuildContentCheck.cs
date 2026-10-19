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
            if (report == null) return;
            ValidateScene(scene);
        }

        internal static void ValidateScene(Scene scene)
        {
            if (!scene.name.StartsWith("Stage1_Map") && scene.name != "Main UI") return;
            bool hasMenu = false;
            bool hasPlayerUi = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MainMenuController menu in root.GetComponentsInChildren<MainMenuController>(true))
                {
                    bool connected = false;
                    foreach (GameObject sceneRoot in scene.GetRootGameObjects())
                        foreach (Button button in sceneRoot.GetComponentsInChildren<Button>(true))
                            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                                if (button.onClick.GetPersistentTarget(i) == menu &&
                                    button.onClick.GetPersistentMethodName(i) == "OnClickStartButton") connected = true;
                    Require(menu.isActiveAndEnabled && connected, scene, "시작 버튼이 메뉴 컨트롤러에 연결되지 않았습니다.");
                    hasMenu = true;
                }
                foreach (MainMenuUI menu in root.GetComponentsInChildren<MainMenuUI>(true))
                {
                    using (var data = new SerializedObject(menu))
                    {
                        Image background = data.FindProperty("background").objectReferenceValue as Image;
                        Require(menu.isActiveAndEnabled && background != null && background.sprite != null,
                            scene, "시작 화면의 배경 또는 시작 컨트롤러가 연결되지 않았습니다.");
                    }
                    hasMenu = true;
                }
                foreach (GameHUD hud in root.GetComponentsInChildren<GameHUD>(true))
                {
                    // Numeric labels, phase text and boss/skill panels are optional in the authored HUD.
                    Require(hud.isActiveAndEnabled && hud.HasPlayerHealthBar,
                        scene, "HUD 체력바가 연결되지 않았습니다.");
                    hasPlayerUi = true;
                }
                foreach (GameplayUI hud in root.GetComponentsInChildren<GameplayUI>(true))
                {
                    using (var data = new SerializedObject(hud))
                        Require(hud.isActiveAndEnabled && data.FindProperty("font").objectReferenceValue != null,
                            scene, "HUD 또는 한글 폰트가 연결되지 않았습니다.");
                    hasPlayerUi = true;
                }
                foreach (EncounterController encounter in root.GetComponentsInChildren<EncounterController>(true))
                {
                    Require(encounter.isActiveAndEnabled, scene, encounter.name + " 전투 구역이 꺼져 있습니다.");
                    using (var data = new SerializedObject(encounter))
                    {
                        Object spawner = data.FindProperty("enemySpawner").objectReferenceValue;
                        Require(spawner != null, scene, encounter.name + " 적 생성기가 없습니다.");
                        using (var spawnerData = new SerializedObject(spawner))
                            for (int enemyType = 1; enemyType <= 3; enemyType++)
                                if (data.FindProperty("enemy" + enemyType + "Count").intValue > 0)
                                    Require(spawnerData.FindProperty("enemy" + enemyType + "Prefab").objectReferenceValue != null,
                                        scene, encounter.name + " Enemy" + enemyType + " 프리팹이 없습니다.");
                        SerializedProperty points = data.FindProperty("spawnPoints");
                        Require(points.arraySize > 0, scene, encounter.name + " 적 생성 위치가 없습니다.");
                        for (int i = 0; i < points.arraySize; i++)
                            Require(points.GetArrayElementAtIndex(i).objectReferenceValue != null,
                                scene, encounter.name + " 적 생성 위치가 비어 있습니다.");
                    }
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
