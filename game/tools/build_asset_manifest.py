"""Build the local browser game's sprite-sheet index from the supplied metadata."""

from __future__ import annotations

import json
import struct
from pathlib import Path


GAME_DIR = Path(__file__).resolve().parents[1]
ASSET_DIR = GAME_DIR / "assets"
OUTPUT = GAME_DIR / "asset-manifest.js"

sprites: dict[str, dict[str, object]] = {}
for metadata_path in sorted(ASSET_DIR.rglob("*.sprite.json")):
    metadata = json.loads(metadata_path.read_text(encoding="utf-8"))
    image_path = metadata_path.with_suffix("").with_suffix(".png")
    if not image_path.is_file():
        raise FileNotFoundError(f"Missing sprite sheet: {image_path}")

    name = metadata.get("name")
    if not name or name in sprites:
        raise ValueError(f"Missing or duplicate sprite name: {name!r}")

    with image_path.open("rb") as image_file:
        header = image_file.read(24)
    if header[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(f"Not a PNG file: {image_path}")
    image_width, image_height = struct.unpack(">II", header[16:24])

    cell_width = int(metadata["cellWidth"])
    cell_height = int(metadata["cellHeight"])
    columns = int(metadata["columns"])
    rows = int(metadata["rows"])
    if columns * cell_width > image_width or rows * cell_height > image_height:
        raise ValueError(f"Frame grid exceeds image bounds: {image_path}")

    sprites[name] = {
        "src": image_path.relative_to(GAME_DIR).as_posix(),
        "frames": int(metadata["frameCount"]),
        "width": cell_width,
        "height": cell_height,
        "columns": columns,
        "rows": rows,
        "fps": float(metadata.get("fps", 12)),
        "loop": bool(metadata.get("loop", True)),
        "pivot": metadata.get("pivot", [0.5, 0.125]),
        "scale": float(metadata.get("sourceScale", 0.4)),
    }

OUTPUT.write_text(
    "window.LAST_SPARK_SPRITES = "
    + json.dumps(sprites, ensure_ascii=False, separators=(",", ":"))
    + ";\n",
    encoding="utf-8",
)
print(f"Indexed {len(sprites)} sprite sheets in {OUTPUT.relative_to(GAME_DIR)}")
