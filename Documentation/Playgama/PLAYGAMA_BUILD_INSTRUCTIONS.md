# Cube Invasion — Playgama WebGL build instructions

Branch: `playgama`. Editor: **Unity 6000.4.8f1** with **WebGL Build Support** installed
(Unity Hub → Installs → 6000.4.8f1 → Add modules → WebGL Build Support).

## What is in the project
| Piece | Location |
|---|---|
| Playgama Bridge SDK | `Packages/manifest.json` → `com.playgama.bridge` = `https://github.com/playgama/bridge-unity.git#v2.4.0` (pinned) |
| Bridge WebGL template | `Assets/WebGLTemplates/Bridge/` (official files + title, touch/long-press guards, hidden-tab audio backstop) |
| Bridge config | `Assets/WebGLTemplates/Bridge/playgama-bridge-config.json` (copied next to `index.html` by the build) |
| Platform layer | `Assets/CubeInvasion/Runtime/Platform/` — `PlaygamaService`, `GamePrefs`, `AdManager` |
| Web glue in game | `Assets/CubeInvasion/Runtime/InvasionPrototype.Playgama.cs` |
| Build script | `Assets/CubeInvasion/Editor/PlaygamaWebBuild.cs` |
| Edit-mode checks | `Assets/CubeInvasion/Editor/PlaygamaPlatformCheck.cs` |

All Bridge calls are inside `#if UNITY_WEBGL`. Android/iOS builds compile the pass-through branch:
PlayerPrefs settings, the signed local profile file and no ads — exactly as before.

## Build from the editor
1. `File → Build Profiles` → select **Web** → **Switch Platform** (first switch re-imports assets).
2. Menu **Cube Invasion → Playgama → Build WebGL + ZIP**.
   This runs `Configure` (WebGL-only player settings), builds `Assets/CubeInvasion/CubeInvasionPrototype.unity`
   to `Builds/Playgama/WebGL/`, verifies `index.html` + config + Bridge scripts, and zips the folder contents to
   `Builds/Playgama/CubeInvasion_Playgama_v1.0.zip` with `index.html` at the archive root.
3. Switch back to Android for device work (`Cube Invasion → Configure Android Portrait`). The repository is left on Android.

## Build from the command line (editor closed)
```sh
"/Applications/Unity/Hub/Editor/6000.4.8f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -buildTarget WebGL \
  -projectPath "$PWD" -executeMethod PlaygamaWebBuild.Build -quit -logFile /tmp/cube-playgama-build.log
```
Success markers in the log: `CUBE_PLAYGAMA_CONFIGURE_OK`, `CUBE_PLAYGAMA_BUILD Succeeded`, `CUBE_PLAYGAMA_ZIP_OK`.

## WebGL player settings applied by `PlaygamaWebBuild.Configure`
- Template `PROJECT:Bridge`
- Compression **Gzip**, **Decompression Fallback on** (Playgama 5.1.1), **data caching off** (Unity's asset cache writes IndexedDB; Playgama 3.2.1)
- Exceptions: explicitly thrown only; no diagnostics overlay
- Memory: 64 MB initial, 1024 MB max, geometric growth (well below the 2 GB start-up limit)
- Managed stripping **Medium**, IL2CPP code generation *optimize size*, compiler configuration *Master*
Quality level for WebGL is *Medium* → `MobilePipeline` URP asset (no realtime shadows, no HDR, MSAA 2x).

## Local test
```sh
cd Builds/Playgama/WebGL && python3 -m http.server 8080
```
Open `http://localhost:8080/`. Bridge runs its mock platform on localhost. Append `?platform_id=playgama`
to exercise the Playgama platform adapter. Bridge DevTools / the Playgama Developer Console "Test game" flow is the
authoritative QA for ads and storage.

## Playgama console checklist (manual, by the developer)
1. Upload `CubeInvasion_Playgama_v1.0.zip` as a new game draft.
2. Orientation: **Portrait**. Languages: **English** only. Title: **Cube Invasion**.
3. Run **Test Game / QA** in the console before *Submit*. Do not submit until the audio blocker in the release report is resolved.
