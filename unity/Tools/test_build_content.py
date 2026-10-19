"""Regression checks for the saved scenes shipped in a player build."""
from pathlib import Path
import json
import re
import unittest

UNITY = Path(__file__).resolve().parents[1]
ASSETS = UNITY / "Assets"
SCENES = ASSETS / "Scenes"


def documents(path):
    text = path.read_text(encoding="utf-8-sig")
    return {match[1]: (int(match[0]), match[2]) for match in
            re.findall(r"^--- !u!(\d+) &(-?\d+)[^\n]*\n([\s\S]*?)(?=^--- !u!|\Z)", text, re.M)}


class BuildContentTests(unittest.TestCase):
    def test_runtime_ui_declares_its_input_system_dependency(self):
        ui = (ASSETS / "Carten/Script/UI/GameplayUI.cs").read_text(encoding="utf-8-sig")
        references = json.loads((ASSETS / "Carten/Carten.asmdef").read_text(encoding="utf-8-sig"))["references"]
        if "using UnityEngine.InputSystem" in ui:
            self.assertTrue("Unity.InputSystem" in references or
                            "GUID:75469ad4d38634e559750d17036d5f7c" in references,
                            "Carten imports InputSystem but its assembly reference is missing")

    def test_menu_is_first_in_global_and_overriding_profiles(self):
        settings = (UNITY / "ProjectSettings/EditorBuildSettings.asset").read_text(encoding="utf-8-sig")
        expected = ["Assets/Scenes/UI/Main UI.unity"] + [f"Assets/Scenes/Stage1_Map{i:02}.unity" for i in range(1, 9)]
        self.assertEqual(re.findall(r"^    path: (.+)$", settings, re.M), expected)
        for profile in (ASSETS / "Settings/Build Profiles").glob("*.asset"):
            content = profile.read_text(encoding="utf-8-sig")
            if "m_OverrideGlobalSceneList: 1" in content:
                self.assertEqual(re.findall(r"^    m_path: (.+)$", content, re.M), expected, str(profile))

    def test_saved_encounters_and_bosses_are_active(self):
        ids = []
        for scene in SCENES.glob("*.unity"):
            docs = documents(scene)
            for _, body in docs.values():
                if "Carten.EncounterTrigger" in body:
                    ids.append(re.search(r"^  triggerId: (.+)$", body, re.M)[1])
                if "Carten.EncounterController" not in body and "Carten.BossController" not in body:
                    continue
                go = re.search(r"m_GameObject: \{fileID: (\d+)\}", body)[1]
                self.assertIn("m_IsActive: 1", docs[go][1], f"{scene.name}: disabled combat object")
        self.assertEqual(len(ids), len(set(ids)), "copied encounters share a one-time trigger ID")

    def test_door_spawn_points_exist(self):
        scenes = {p.stem: p.read_text(encoding="utf-8-sig") for p in SCENES.glob("*.unity")}
        for scene, text in scenes.items():
            for destination, spawn in re.findall(r"targetSceneName: ([^\n]+)\n  targetSpawnPointName: ([^\n]+)", text):
                self.assertIn(destination, scenes, scene)
                self.assertTrue("m_Name: " + spawn + "\n" in scenes[destination], f"{scene}: missing arrival spawn {spawn}")

    def test_menu_has_a_controller_and_background(self):
        menu = (SCENES / "UI/Main UI.unity").read_text(encoding="utf-8-sig")
        self.assertTrue("Carten.MainMenuUI" in menu or "Carten.MainMenuController" in menu, "start UI has no start controller")
        if "Carten.MainMenuController" in menu:
            self.assertIn("m_MethodName: OnClickStartButton", menu)
        self.assertTrue(re.search(r"m_Sprite: \{fileID: 21300000, guid: [a-f0-9]{32}", menu), "start UI has no background")

    def test_player_has_gameplay_ui(self):
        scene = (SCENES / "Stage1_Map01.unity").read_text(encoding="utf-8-sig")
        hud_guid = re.search(r"^guid: (\w+)", (ASSETS / "Carten/Menu/GameHUD.cs.meta").read_text(), re.M)[1]
        self.assertTrue("Carten.GameplayUI" in scene or hud_guid in scene, "player has no gameplay UI")

    def test_enemy_prefabs_reference_their_own_art(self):
        for name, directory in [("Enemy1", "04_Enemy_Melee"), ("Enemy2", "05_Enemy_Ranged"), ("Enemy3", "06_Enemy_Charger")]:
            prefab = (ASSETS / "Carten/Prefabs" / f"{name}.prefab").read_text(encoding="utf-8-sig")
            sprite_guids = set(re.findall(r"m_Sprite: \{fileID: -?\d+, guid: (\w+)", prefab))
            own_art = set()
            for meta in ASSETS.rglob(f"{name}_*.png.meta"):
                if directory in meta.parts:
                    own_art.add(re.search(r"^guid: (\w+)", meta.read_text(encoding="utf-8-sig"), re.M)[1])
            self.assertTrue(sprite_guids & own_art, f"{name}: missing its own character sprite")


if __name__ == "__main__":
    unittest.main()
