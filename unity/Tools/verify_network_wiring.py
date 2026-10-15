"""Verify that playable scenes can create a session and bosses can submit results."""

from pathlib import Path
import re
import sys


UNITY = Path(__file__).resolve().parents[1]
ASSETS = UNITY / "Assets"
HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)(?: stripped)?$", re.M)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def script_guid(relative_path):
    meta = (ASSETS / relative_path).with_suffix(".cs.meta")
    match = re.search(r"^guid: ([0-9a-f]{32})$", meta.read_text(encoding="utf-8-sig"), re.M)
    require(match is not None, f"{meta}: missing GUID")
    return match[1]


def documents(path):
    content = path.read_text(encoding="utf-8-sig")
    matches = list(HEADER.finditer(content))
    result = []
    for index, match in enumerate(matches):
        end = matches[index + 1].start() if index + 1 < len(matches) else len(content)
        result.append((match[1], match[2], content[match.start():end]))
    return content, result


def components_with_guid(path, guid):
    _, docs = documents(path)
    found = []
    for kind, file_id, block in docs:
        if kind != "114" or f"guid: {guid}" not in block:
            continue
        game_object = re.search(r"m_GameObject: \{fileID: (-?\d+)\}", block)
        require(game_object is not None, f"{path}: component {file_id} has no GameObject")
        found.append((file_id, game_object[1], block))
    return found


def block_by_id(path, expected_kind, file_id):
    _, docs = documents(path)
    matches = [block for kind, current_id, block in docs
               if kind == expected_kind and current_id == file_id]
    require(len(matches) == 1,
            f"{path.name}: expected one type {expected_kind} object {file_id}, found {len(matches)}")
    return matches[0]


def require_registered(path, game_object_id, component_id):
    game_object = block_by_id(path, "1", game_object_id)
    require(f"component: {{fileID: {component_id}}}" in game_object,
            f"{path.name}: component {component_id} is not registered on GameObject {game_object_id}")


def require_scene_root(path, game_object_id):
    _, docs = documents(path)
    transforms = [(file_id, block) for kind, file_id, block in docs
                  if kind == "4" and f"m_GameObject: {{fileID: {game_object_id}}}" in block]
    require(len(transforms) == 1,
            f"{path.name}: NetworkManager must have exactly one Transform")
    scene_roots = [block for kind, _, block in docs if kind == "1660057539"]
    require(len(scene_roots) == 1, f"{path.name}: expected one SceneRoots block")
    require(f"- {{fileID: {transforms[0][0]}}}" in scene_roots[0],
            f"{path.name}: NetworkManager Transform is not registered as a scene root")


def check_session(scene, api_guid, session_guid):
    api = components_with_guid(scene, api_guid)
    session = components_with_guid(scene, session_guid)
    require(len(api) == 1, f"{scene.name}: expected one LastSparkApiClient, found {len(api)}")
    require(len(session) == 1, f"{scene.name}: expected one LastSparkSession, found {len(session)}")
    require(api[0][1] == session[0][1], f"{scene.name}: API client and session must share one GameObject")
    require_registered(scene, api[0][1], api[0][0])
    require_registered(scene, session[0][1], session[0][0])
    require_scene_root(scene, api[0][1])
    require(re.search(r"baseUrl: https://[^\s]+/api/v1\s", api[0][2]) is not None,
            f"{scene.name}: Base URL must be the deployed HTTPS /api/v1 address")


def check_reporter(path, boss_guid, reporter_guid, boss_id, score):
    bosses = components_with_guid(path, boss_guid)
    reporters = components_with_guid(path, reporter_guid)
    require(len(bosses) == 1, f"{path.name}: expected one BossController, found {len(bosses)}")
    require(len(reporters) == 1, f"{path.name}: expected one BossResultReporter, found {len(reporters)}")
    require(bosses[0][1] == reporters[0][1], f"{path.name}: reporter must be on the boss GameObject")
    require_registered(path, reporters[0][1], reporters[0][0])
    require(re.search(rf"^  bossId: {boss_id}$", reporters[0][2], re.M) is not None,
            f"{path.name}: reporter bossId must be {boss_id}")
    require(re.search(rf"^  clearScore: {score}$", reporters[0][2], re.M) is not None,
            f"{path.name}: reporter clearScore must be {score}")


def check():
    api_guid = script_guid("Carten/Script/Network/LastSparkApiClient")
    session_guid = script_guid("Carten/Script/Network/LastSparkSession")
    reporter_guid = script_guid("Carten/Script/Network/BossResultReporter")
    boss_guid = script_guid("Carten/Script/Boss/BossController")

    for name in ("Stage1_Map01", "Stage1_Map05", "Stage1_Map08"):
        check_session(ASSETS / f"Scenes/{name}.unity", api_guid, session_guid)

    check_reporter(ASSETS / "Scenes/Stage1_Map05.unity", boss_guid, reporter_guid, 1, 1000)
    check_reporter(ASSETS / "Scenes/Stage1_Map08.unity", boss_guid, reporter_guid, 2, 3000)
    check_reporter(ASSETS / "Carten/Prefabs/MiniBoss.prefab", boss_guid, reporter_guid, 1, 1000)
    check_reporter(ASSETS / "Carten/Prefabs/FinalBoss.prefab", boss_guid, reporter_guid, 2, 3000)

    final_scene = (ASSETS / "Scenes/Stage1_Map08.unity").read_text(encoding="utf-8-sig")
    require("phase2HealthPercent: 70" in final_scene and "phase3HealthPercent: 30" in final_scene,
            "Stage1_Map08.unity: final boss phase thresholds must be 70% and 30%")

    build_settings = (UNITY / "ProjectSettings/EditorBuildSettings.asset").read_text(encoding="utf-8-sig")
    for number in range(1, 9):
        path = f"Assets/Scenes/Stage1_Map{number:02}.unity"
        require(f"path: {path}" in build_settings, f"Build Settings: missing {path}")

    print("PASS: network sessions, boss result reporters and Stage1 build scenes are wired")


if __name__ == "__main__":
    try:
        check()
    except (OSError, ValueError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        sys.exit(1)
