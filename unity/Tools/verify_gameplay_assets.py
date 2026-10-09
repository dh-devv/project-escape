"""Static source/asset checks. This does not replace Unity compilation or PlayMode tests."""
from pathlib import Path
import json
import re
import sys
import argparse

import yaml
from tree_sitter import Language, Parser
import tree_sitter_c_sharp

UNITY = Path(__file__).resolve().parents[1]
ASSETS = UNITY / "Assets"
HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)(?: stripped)?$", re.M)
GUID = re.compile(r"guid: ([0-9a-f]{32})")

# These default shape sprites are distributed with the Editor's 2D Sprite package.
# Keep them visible in the report: the installed package cannot be checked without Unity.
EDITOR_SHAPES = {
    "311925a002f4447b3a28927169b83ea6": "2D Sprite default square",
    "a86470a33a6bf42c4b3595704624658b": "2D Sprite default circle",
}


def require(condition, message):
    if not condition:
        raise ValueError(message)


def load_unity_yaml(path):
    content = path.read_text(encoding="utf-8-sig")
    normalized = re.sub(r"^%.*\n", "", content, flags=re.M)
    normalized = HEADER.sub("---", normalized)
    return content, list(yaml.safe_load_all(normalized))


def check_assets(verbose=False):
    guid_index = {}
    for meta in ASSETS.rglob("*.meta"):
        content = meta.read_text(encoding="utf-8-sig")
        match = re.search(r"^guid: ([0-9a-f]{32})$", content, re.M)
        require(match is not None, f"{meta}: missing GUID")
        value = match[1]
        require(value not in guid_index, f"{meta}: duplicate GUID {value}")
        asset = Path(str(meta)[:-5])
        require(asset.exists(), f"{meta}: orphan metadata")
        guid_index[value] = asset

    for path in ASSETS.rglob("*"):
        if path.suffix != ".meta":
            require(Path(str(path) + ".meta").exists(), f"{path}: missing metadata")

    package_index = json.loads((UNITY / "Tools/package-references.json").read_text(encoding="utf-8"))["guids"]
    external = set()
    unresolved_settings = {}
    files = [p for folder in (ASSETS, UNITY / "ProjectSettings") for p in folder.rglob("*")
             if p.is_file() and p.suffix in {".unity", ".prefab", ".asset", ".mat", ".scenetemplate"}]
    for asset in sorted(files):
        content, documents = load_unity_yaml(asset)
        ids = [fid for _, fid in HEADER.findall(content)]
        require(len(ids) == len(set(ids)), f"{asset}: duplicate file IDs")
        known_ids = set(ids)
        for reference in re.findall(r"\{fileID: ([^}]+)\}", content):
            match = GUID.search(reference)
            if match:
                value = match[1]
                if value in guid_index or value.startswith("00000000"):
                    continue
                if value in package_index or value in EDITOR_SHAPES:
                    external.add(value)
                    continue
                # Gameplay scenes/components must resolve; SDK settings are reported separately.
                if asset.suffix in {".unity", ".prefab", ".mat"}:
                    raise ValueError(f"{asset}: unresolved external GUID {value}")
                unresolved_settings.setdefault(value, set()).add(str(asset.relative_to(UNITY)))
            else:
                require(reference == "0" or reference in known_ids,
                        f"{asset}: unresolved internal file ID {reference}")

        if asset.suffix == ".unity" and "SceneRoots:" in content:
            roots = next(d["SceneRoots"]["m_Roots"] for d in documents if "SceneRoots" in d)
            listed = [str(item["fileID"]) for item in roots]
            actual = {fid for (kind, fid), doc in zip(HEADER.findall(content), documents)
                      if kind in {"4", "224"} and
                      doc.get("Transform", doc.get("RectTransform", {})).get("m_Father", {}).get("fileID") == 0}
            require(set(listed) == actual and len(listed) == len(set(listed)), f"{asset}: roots mismatch")

    _, build = load_unity_yaml(UNITY / "ProjectSettings/EditorBuildSettings.asset")
    scenes = build[0]["EditorBuildSettings"]["m_Scenes"]
    for scene in scenes:
        require(guid_index.get(scene["guid"]) == UNITY / scene["path"],
                f"Build Settings: wrong path/GUID for {scene['path']}")
    for name in ("MiniBossDemo", "FinalBossDemo"):
        require(any(s["enabled"] and s["path"] == f"Assets/Scenes/{name}.unity" for s in scenes),
                f"Build Settings: {name} not enabled")
        content = (ASSETS / f"Scenes/{name}.unity").read_text(encoding="utf-8")
        require("attackKey: 259" in content, f"{name}: demo attack key must match Keypad 3 in HUD")

    _, player_settings = load_unity_yaml(UNITY / "ProjectSettings/ProjectSettings.asset")
    settings = player_settings[0]["PlayerSettings"]
    require(settings["activeInputHandler"] in {0, 2}, "Legacy Input must be enabled for current player controls")
    require(settings["insecureHttpOption"] == 1, "Local HTTP demo must allow development connections")

    parser = Parser(Language(tree_sitter_c_sharp.language()))
    scripts = list(ASSETS.rglob("*.cs"))
    for script in scripts:
        data = script.read_bytes()
        data.decode("utf-8-sig")
        require(not parser.parse(data).root_node.has_error, f"{script}: C# syntax error")
    for path in ASSETS.rglob("*"):
        if path.suffix in {".asmdef", ".inputactions"}:
            json.loads(path.read_text(encoding="utf-8-sig"))
    for path in (UNITY / "Packages").glob("*.json"):
        json.loads(path.read_text(encoding="utf-8-sig"))

    print(f"PASS: {len(files)} Unity YAML files, local references and metadata")
    print(f"PASS: {len(scripts)} UTF-8 C# files parsed; assembly/input/package JSON")
    print(f"PASS: {len(scenes)} Build Settings scene paths and GUIDs")
    print(f"NOTE: {len(external)} external package/Editor GUIDs; package restoration requires Unity")
    if unresolved_settings:
        print(f"NOTE: {len(unresolved_settings)} SDK settings GUIDs need the installed Editor's package check")
        if verbose:
            for value, paths in sorted(unresolved_settings.items()):
                print(f"  {value}: {', '.join(sorted(paths))}")
    print("NOT RUN: Unity compilation, PlayMode tests, rendering and Docker execution")


if __name__ == "__main__":
    arguments = argparse.ArgumentParser(description=__doc__)
    arguments.add_argument("--verbose", action="store_true", help="List unresolved SDK settings references")
    try:
        check_assets(arguments.parse_args().verbose)
    except (ValueError, UnicodeError, yaml.YAMLError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        sys.exit(1)
