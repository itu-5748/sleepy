# Emberbound — Unity 3D

A lightweight, fully 3D port of the browser prototype. The terrain, knight, spiders, skeletons, weapon models, and shrine models are real meshes, with an orthographic angled camera. The models are deliberately simple low-poly shapes rather than production character assets. No external art, animation packs, paid assets, or backend required.

## Open and play

1. Install **Unity Hub**, then **Unity 6 LTS, 6000.0.38f1**, with your operating system's build support. A newer **6000.0 LTS** patch can upgrade this project; that editor upgrade has not been validated here. Add **Web Build Support** only if you want a browser build.
2. Download the repository and unzip it. In Unity Hub choose **Add → Add project from disk**, selecting the **Unity** folder containing `Assets`, `Packages`, and `ProjectSettings`. Do not select the outer browser repository.
3. Open with the selected editor. Let Unity import packages and compile scripts.
4. Open **Assets/Scenes/Emberbound.unity** (or use **Emberbound → Open Game Scene**).
5. Press **Play**, choose your weapon, and click **Enter the Crucible**. Click the Game view to give it keyboard focus.

The scene's root component generates the 3D world on entering Play. An empty-looking scene before Play is expected. You can inspect the generated models and meshes in the Hierarchy during Play. They are discarded on leaving Play; edit the generator code to make lasting changes.

The project uses Unity's **Built-in Render Pipeline** and a small custom lit shader in Resources. Do not install URP or HDRP as a prerequisite. First import sets the legacy input backend and a scene in the build list. If input reports a backend error, set **Edit → Project Settings → Player → Other Settings → Active Input Handling → Input Manager (Old)**, then restart the editor.

## Controls and gameplay

- **WASD / arrows:** move relative to the camera.
- **Mouse:** aim; **left click / J:** attack.
- **Space:** dash (short invulnerability, 1.1-second cooldown).
- **E:** interact with a nearby shrine; **Escape:** pause.

Explore five connected regions on raised stone paths. The surrounding forest and ravine are impassable; movement and dashes are constrained by the same map rules as the browser version. Spiders spit poison bolts: eight impact damage plus twelve damage over three seconds. Skeletons telegraph a 0.4-second sword swing. The crowned Ash Warden at the eastern end has 800 HP and a projectile fan.

Four optional shrines grant Fury (+25% damage), Renewal (+20 max HP and heal 40), or Quickening (+20% attack rate). These blessings expire with the run. Weapons retain the blade, spear, and ranged cinder attacks. Defeating an enemy heals two HP, matching the prototype; lifesteal was discussed but has not been added.

At death or victory, small permanent stat gains follow the original rules. Vitality comes from damage endured, might from kills, and agility from attacks evaded with a dash. Each stat earns at most three points per run and caps at 200. Embers can buy a chosen stat for three each. New regions count once; rewards settle only once per run. Closing an unfinished run abandons its rewards.

## Saves and transferring browser progress

Unity stores progression using **PlayerPrefs** under `EmberboundUnity_v1`. It does not automatically read your browser's localStorage. Different builds, machines, and browsers can have separate saves.

1. In the browser version, click **Export legacy to Unity** in the sidebar.
2. In Unity, stop Play mode and select **Emberbound → Import Browser Legacy JSON**.
3. Select `emberbound-legacy.json`, review the overwrite confirmation, and start a new Play session.

Import copies permanent stats, embers, completed runs, and weapon selection. It does not copy an unfinished run. Back up the JSON before replacing a save.

## Performance design

- No runtime textures, skeletal rig imports, realtime point lights, postprocessing, physics simulation, or NavMesh baking.
- Terrain is one combined mesh with six material slots. Static trees and ruins are combined into nine spatial chunks by material.
- Low-poly models share materials and primitive meshes; GPU instancing is enabled.
- One hard-shadow directional light; 22-unit shadow distance, low-resolution shadow map, and no shadow cascades.
- Distant enemies stop simulating; model roots are hidden past 22 units.
- Projectiles reuse a pool capped at 96 rather than allocating every shot.
- Target is 60 FPS, not a measured guarantee. **Quality: Low** in the sanctuary or pause screen disables shadows and anti-aliasing. Start with a desktop build before evaluating Web performance.

Performance and visual appearance still require checking in Unity on your actual hardware. This cloud environment has no Unity editor or license, so the Unity scene, shader, and player build have not been executed here.

## Test and build

Open **Window → General → Test Runner**. Run **EditMode** tests for progression, poison, melee windup, purchase limits, map collision, and connected destinations. Run **PlayMode** for startup/model/camera smoke validation. See `VALIDATION.md` for what was actually run in this cloud task.

Desktop: choose a desktop platform in **File → Build Profiles**, install its build module, then use **Emberbound → Build Desktop (current platform)** or the normal Unity build UI. For Web, select Web in Build Profiles and ensure the scene is listed. The editor helper also supports a licensed CLI build:

```sh
Unity -batchmode -projectPath /absolute/path/to/Unity \
  -executeMethod Emberbound.Editor.ProjectSetup.BuildWeb -quit \
  -logFile /absolute/path/to/build.log
```

This outputs to `Builds/WebGL` by default, overridable with `EMBERBOUND_BUILD_PATH`. Serve that output over HTTP; opening a Web player using `file://` is unsupported. The existing GitHub Pages workflow still deploys the JavaScript game. It does not build or deploy Unity until an editor, build module, license, and a separate deployment workflow are configured.
