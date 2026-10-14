# Validation record

## Executed in this cloud environment

- All Unity C# files parsed without syntax errors by Roslyn using C# 9 language rules.
- `Rules.cs` compiled using the .NET 8 C# compiler and executed with the six existing EditMode rule-test methods using a minimal assertion harness. All six passed: reward parity/single settlement, purchase bounds, poison expiration/refresh, melee timing/escape, dash terrain collision, and map connectivity.
- These checks validate portable game rules. They do **not** establish Unity API compilation, import success, shader compilation, or visual correctness.
- Browser tests and a browser save-export check were run separately; see the task's report.

## Included but not executed here

- Unity EditMode execution through the Unity Test Framework.
- Unity PlayMode startup smoke test for terrain, knight meshes, and the orthographic camera.
- Unity editor compilation/import, scene loading, shader compilation, desktop/Web builds, and FPS/GPU/memory measurements.

Unity is not installed or licensed in this cloud machine. Open the project locally and run the Test Runner and Play before relying on a release build. If an editor error occurs, provide its full first Console error and your exact editor version.
