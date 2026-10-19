# Player evolution atlases

`Player_Evolution_P2.png` and `Player_Evolution_P3.png` are transparent 3×3 atlases with 256×256 cells. Each atlas depicts the whole character, so movement and attack poses do not fall back to a sealed suit or a face overlay.

Frame order, left to right and top to bottom:

1. Idle A
2. Idle B
3. Walk A
4. Walk B
5. Charge brace
6. Straight punch
7. Jump
8. Hit recoil
9. Recovery

The uncropped generated sheets are kept under `source/`. Rebuild the runtime atlases with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File game/tools/build_player_evolution_atlas.ps1
```
