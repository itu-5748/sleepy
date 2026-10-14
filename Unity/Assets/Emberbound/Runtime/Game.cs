using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emberbound
{
    public sealed class Game : MonoBehaviour
    {
        const string SaveKey = "EmberboundUnity_v1";
        enum Phase { Sanctuary, Playing, Shrine, Paused, End }
        sealed class Enemy
        {
            public Point pos; public float hp, max, fire, speed; public bool spider, boss, counted;
            public Transform model, weapon; public Melee melee = new Melee(); public float flash;
        }
        sealed class Bolt
        {
            public Point pos, velocity; public float damage, life; public bool friendly, poison; public Transform model;
        }
        Art art;
        Transform scenery, actors, knight, knightWeapon;
        Camera view;
        Light sun;
        readonly List<Enemy> enemies = new List<Enemy>();
        readonly List<Bolt> bolts = new List<Bolt>();
        readonly Transform[] shrines = new Transform[4];
        readonly bool[] shrineUsed = new bool[4];
        readonly bool[] campsActive = new bool[15];
        readonly Poison poison = new Poison();
        Legacy legacy;
        RunRecord run;
        Phase phase;
        Point player, aim, dashDirection;
        float hp, maxHp, attackCooldown, dashCooldown, dashRemaining, invulnerable, damageMultiplier = 1, attackRate = 1, playTime;
        int nearbyShrine = -1;
        bool bossSpawned, won, sound;
        string result = "", storageMessage = "";
        AudioSource audioSource;
        AudioClip hitSound;
        GUIStyle title, body, small, button, heading;
        readonly string[] weaponNames = { "Ashblade", "Thorn spear", "Cinder staff" };
        readonly float[] weaponDamage = { 24, 32, 19 }, weaponDelay = { .32f, .5f, .38f }, weaponReach = { 2.3f, 3.8f, 12 };
        static Vector3 V(Point p, float y = 0) { return new Vector3(p.x, y, p.z); }
        static Point P(Vector3 v) { return new Point(v.x, v.z); }

        void Awake()
        {
            legacy = LoadLegacy(); art = new Art();
            scenery = new GameObject("Static world").transform; scenery.SetParent(transform, false); art.Terrain(scenery);
            actors = new GameObject("Run actors").transform; actors.SetParent(transform, false);
            for (int i = 0; i < 4; i++) shrines[i] = art.Shrine(scenery, Map.Shrines[i]);
            var cameraObject = new GameObject("Top-down camera", typeof(Camera), typeof(AudioListener)); cameraObject.transform.SetParent(transform, false);
            view = cameraObject.GetComponent<Camera>(); view.orthographic = true; view.orthographicSize = 8.2f; view.nearClipPlane = .1f; view.farClipPlane = 85; view.clearFlags = CameraClearFlags.SolidColor; view.backgroundColor = new Color(.055f, .09f, .10f);
            var lightObject = new GameObject("Sun", typeof(Light)); lightObject.transform.SetParent(transform, false); sun = lightObject.GetComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.15f; sun.color = new Color(1, .91f, .76f); sun.transform.rotation = Quaternion.Euler(48, -35, 0); sun.shadows = LightShadows.Hard;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(.42f, .48f, .46f); RenderSettings.fog = true; RenderSettings.fogColor = view.backgroundColor; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 24; RenderSettings.fogEndDistance = 70;
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 2; QualitySettings.shadowDistance = 22; QualitySettings.shadowResolution = ShadowResolution.Low; QualitySettings.shadowCascades = 0;
            audioSource = gameObject.AddComponent<AudioSource>(); float[] wave = new float[2205]; for (int i = 0; i < wave.Length; i++) wave[i] = Mathf.Sin(i * 2 * Mathf.PI * 260 / 22050) * .06f * (1 - (float)i / wave.Length); hitSound = AudioClip.Create("Combat click", wave.Length, 1, 22050, false); hitSound.SetData(wave, 0);
            player = new Point(3, 11); MakeKnight(); PositionCamera(); phase = Phase.Sanctuary;
        }
        public static Legacy LoadLegacy()
        {
            try { var value = JsonUtility.FromJson<Legacy>(PlayerPrefs.GetString(SaveKey, "")); if (value != null && value.version == 1) { value.Sanitize(); return value; } } catch (Exception) { }
            return new Legacy();
        }
        public static void StoreLegacy(Legacy value) { value.Sanitize(); PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(value)); PlayerPrefs.Save(); }
        void Save()
        {
            try { StoreLegacy(legacy); } catch (Exception ex) { storageMessage = "Save failed; progress lasts this session. " + ex.GetType().Name; }
        }
        void MakeKnight()
        {
            if (knight != null) { knight.gameObject.SetActive(false); Destroy(knight.gameObject); }
            knight = art.Knight(actors); knightWeapon = art.Weapon(knight, legacy.weapon); knight.position = V(player);
        }
        void StartRun()
        {
            foreach (var e in enemies) { e.model.gameObject.SetActive(false); Destroy(e.model.gameObject); } enemies.Clear();
            foreach (var b in bolts) { b.life = 0; b.model.gameObject.SetActive(false); }
            Array.Clear(campsActive, 0, campsActive.Length); Array.Clear(shrineUsed, 0, shrineUsed.Length);
            for (int i = 0; i < 4; i++) shrines[i].Find("Blessing").gameObject.SetActive(true);
            run = new RunRecord(); run.visited.Add(0); player = new Point(3, 11); maxHp = hp = 100 + legacy.health * 2; poison.remaining = poison.tick = 0;
            attackCooldown = dashCooldown = dashRemaining = invulnerable = playTime = 0; damageMultiplier = attackRate = 1; bossSpawned = won = false; nearbyShrine = -1;
            MakeKnight(); PositionCamera(); phase = Phase.Playing;
        }
        void PositionCamera()
        {
            var target = V(player, .35f); view.transform.position = target + new Vector3(-9, 14, -9); view.transform.LookAt(target);
        }
        Point InputDirection()
        {
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            float z = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            Vector3 forward = view.transform.forward; forward.y = 0; forward.Normalize(); Vector3 right = view.transform.right; right.y = 0; right.Normalize();
            return P(right * x + forward * z).Normalized;
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { if (phase == Phase.Playing) phase = Phase.Paused; else if (phase == Phase.Paused) phase = Phase.Playing; }
            if (phase != Phase.Playing) return;
            float dt = Mathf.Min(Time.deltaTime, .05f); playTime += dt;
            if (hp <= 0) { Finish(false); return; }
            Damage(poison.Advance(dt), false); if (hp <= 0) { Finish(false); return; }
            attackCooldown = Mathf.Max(0, attackCooldown - dt); dashCooldown = Mathf.Max(0, dashCooldown - dt); invulnerable = Mathf.Max(0, invulnerable - dt);
            var ray = view.ScreenPointToRay(Input.mousePosition); float distance; if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out distance)) aim = P(ray.GetPoint(distance));
            Point direction = InputDirection();
            if (Input.GetKeyDown(KeyCode.Space) && dashCooldown == 0) { dashDirection = direction.Length > 0 ? direction : (aim - player).Normalized; dashRemaining = .16f; dashCooldown = 1.1f; invulnerable = .24f; }
            bool dashing = dashRemaining > 0; if (dashing) { direction = dashDirection; dashRemaining = Mathf.Max(0, dashRemaining - dt); }
            float speed = dashing ? 17 : 4.2f * (1 + legacy.haste * .0015f); player = Map.Move(player, direction * speed * dt);
            knight.position = V(player); Point facing = aim - player; if (facing.Length > .05f) knight.rotation = Quaternion.LookRotation(V(facing));
            Animate(knight, direction.Length > 0, playTime); knightWeapon.localRotation = Quaternion.Euler(attackCooldown > weaponDelay[legacy.weapon] / attackRate - .13f ? 70 : 0, 0, attackCooldown > 0 ? -20 : 0);
            PositionCamera();
            if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.J)) && attackCooldown == 0) Attack();
            Explore(); if (phase != Phase.Playing) return;
            UpdateEnemies(dt, dashing); UpdateBolts(dt, dashing);
            if (hp <= 0) Finish(false);
        }
        void Animate(Transform model, bool moving, float time)
        {
            var left = model.Find("Left leg"); var right = model.Find("Right leg"); float a = moving ? Mathf.Sin(time * 14) * 22 : 0;
            if (left != null) left.localRotation = Quaternion.Euler(a, 0, 0); if (right != null) right.localRotation = Quaternion.Euler(-a, 0, 0);
        }
        void Explore()
        {
            run.visited.Add(Map.Region(player.x));
            for (int i = 0; i < Map.Camps.Count; i++) if (!campsActive[i] && (player - Map.Camps[i]).Length < 10.4f) {
                campsActive[i] = true; int region = i / 3; int count = region == 4 ? 3 : 3 + region;
                for (int j = 0; j < count; j++) { Point p = Map.Camps[i] + new Point(UnityEngine.Random.Range(-1.15f, 1.15f), UnityEngine.Random.Range(-1.15f, 1.15f)); if (!Map.Walkable(p, .4f)) p = Map.Camps[i]; SpawnEnemy(p, region, j % 3 == 2, false); }
            }
            if (!bossSpawned && (player - Map.Boss).Length < 12.4f) { bossSpawned = true; SpawnEnemy(Map.Boss, 4, false, true); }
            nearbyShrine = -1; for (int i = 0; i < 4; i++) if (!shrineUsed[i] && (player - Map.Shrines[i]).Length < 1.8f) nearbyShrine = i;
            if (nearbyShrine >= 0 && Input.GetKeyDown(KeyCode.E)) phase = Phase.Shrine;
        }
        void SpawnEnemy(Point pos, int region, bool spider, bool boss)
        {
            var e = new Enemy { pos = pos, hp = boss ? 800 : 45 + (region + 1) * 12, max = boss ? 800 : 45 + (region + 1) * 12, fire = 1.5f, speed = boss ? 1.5f : spider ? 1.3f : 1.94f + region * .14f, spider = spider, boss = boss };
            e.model = art.Enemy(actors, spider, boss); e.model.position = V(pos); e.weapon = e.model.Find("Weapon"); enemies.Add(e);
        }
        void Attack()
        {
            int w = legacy.weapon; attackCooldown = weaponDelay[w] / attackRate; if (sound) audioSource.PlayOneShot(hitSound);
            Point direction = (aim - player).Normalized; float damage = weaponDamage[w] * (1 + legacy.power * .005f) * damageMultiplier;
            if (w == 2) { SpawnBolt(player, direction * 10.4f, damage, true, false); return; }
            foreach (var e in enemies) if (e.hp > 0) {
                Point to = e.pos - player; float dot = to.Length > 0 ? to.Normalized.x * direction.x + to.Normalized.z * direction.z : 1;
                if (to.Length < weaponReach[w] + (e.boss ? .68f : .4f) && dot > (w == 1 ? .92f : .49f) && Map.Line(player, e.pos)) Hit(e, damage);
            }
        }
        void Hit(Enemy e, float damage)
        {
            if (e.hp <= 0) return; e.hp -= damage; e.flash = .1f;
            if (e.hp <= 0 && !e.counted) { e.counted = true; run.kills++; hp = Mathf.Min(maxHp, hp + 2); e.model.gameObject.SetActive(false); if (e.boss) won = true; }
        }
        void Damage(float amount, bool impact)
        {
            if (amount <= 0) return; hp -= amount; run.damageTaken += amount; if (impact) invulnerable = .7f;
        }
        void UpdateEnemies(float dt, bool dashing)
        {
            foreach (var e in enemies) {
                if (e.hp <= 0) continue; float range = (e.pos - player).Length; bool active = range < 22; if (e.model.gameObject.activeSelf != active) e.model.gameObject.SetActive(active); if (!active || range > 14) continue;
                Point target = player;
                if (!Map.Line(e.pos, player)) target = Mathf.Abs(e.pos.z - Map.RoadZ(e.pos.x)) > .5f ? new Point(e.pos.x, Map.RoadZ(e.pos.x)) : new Point(e.pos.x + Mathf.Sign(player.x - e.pos.x) * 1.4f, Map.RoadZ(e.pos.x + Mathf.Sign(player.x - e.pos.x) * 1.4f));
                Point direction = (target - e.pos).Normalized; float speed = e.melee.windup > 0 ? 0 : e.speed * (e.spider && range < 4.8f ? -.3f : 1);
                e.pos = Map.Move(e.pos, direction * speed * dt, e.boss ? .68f : e.spider ? .32f : .4f); e.model.position = V(e.pos); if ((player - e.pos).Length > .01f) e.model.rotation = Quaternion.LookRotation(V(player - e.pos));
                Animate(e.model, speed != 0, playTime + e.pos.x);
                if (e.weapon != null) e.weapon.localRotation = Quaternion.Euler(e.melee.windup > 0 ? -30 : e.melee.swing > 0 ? 90 : 0, 0, 0);
                if (e.spider) e.model.localScale = new Vector3(1, 1 + Mathf.Sin(playTime * 12 + e.pos.x) * .025f, 1);
                e.fire -= dt;
                if ((e.spider || e.boss) && e.fire <= 0 && range < 10.4f && Map.Line(e.pos, player)) {
                    e.fire = e.boss ? 1 : 2; Point to = (player - e.pos).Normalized;
                    for (int i = 0; i < (e.boss ? 5 : 1); i++) { float a = e.boss ? (i - 2) * .25f : 0; Point v = new Point(to.x * Mathf.Cos(a) - to.z * Mathf.Sin(a), to.x * Mathf.Sin(a) + to.z * Mathf.Cos(a)); SpawnBolt(e.pos, v * 3.8f, e.boss ? 20 : 8, false, !e.boss); }
                }
                bool strike = !e.spider && !e.boss && e.melee.Advance((e.pos - player).Length, dt);
                if ((e.boss && range < .96f) || strike) { if (dashing) run.dodges++; else if (invulnerable <= 0 && Map.Line(e.pos, player)) Damage(e.boss ? 24 : 14, true); }
            }
        }
        void SpawnBolt(Point pos, Point velocity, float damage, bool friendly, bool toxic)
        {
            Bolt b = null; foreach (var existing in bolts) if (existing.life <= 0) { b = existing; break; }
            if (b == null) { if (bolts.Count >= 96) return; b = new Bolt { model = art.Part(actors, "Pooled bolt", Vector3.zero, Vector3.one * .18f, art.spell, PrimitiveType.Sphere) }; bolts.Add(b); }
            b.pos = pos; b.velocity = velocity; b.damage = damage; b.friendly = friendly; b.poison = toxic; b.life = friendly ? 1.2f : 5; b.model.gameObject.SetActive(true); b.model.GetComponent<MeshRenderer>().sharedMaterial = toxic ? art.poison : friendly ? art.spell : art.cape; b.model.position = V(pos, .45f);
        }
        void UpdateBolts(float dt, bool dashing)
        {
            foreach (var b in bolts) {
                if (b.life <= 0) continue; Point previous = b.pos; b.pos += b.velocity * dt; b.life -= dt;
                if (!Map.Line(previous, b.pos)) b.life = 0;
                if (b.life > 0 && b.friendly) { foreach (var e in enemies) if (e.hp > 0 && (e.pos - b.pos).Length < (e.boss ? .8f : .52f)) { Hit(e, b.damage); b.life = 0; break; } }
                else if (b.life > 0 && !b.friendly && (player - b.pos).Length < .4f) {
                    if (dashing) { run.dodges++; b.life = 0; }
                    else if (invulnerable <= 0) { Damage(b.damage, true); if (b.poison) poison.Apply(); b.life = 0; }
                }
                b.model.position = V(b.pos, .45f); if (b.life <= 0) b.model.gameObject.SetActive(false);
            }
            if (won && hp > 0) Finish(true);
        }
        void Finish(bool victory)
        {
            if (phase != Phase.Playing) return; phase = Phase.End; won = victory; Rewards r = Progression.Settle(legacy, run); Save();
            result = run.kills + " enemies defeated · " + run.visited.Count + " regions reached\n\n+" + r.health + " vitality   +" + r.power + " might   +" + r.haste + " agility\n" + r.embers + " embers earned";
        }
        void Bless(int choice)
        {
            if (nearbyShrine < 0 || shrineUsed[nearbyShrine]) return;
            shrineUsed[nearbyShrine] = true; shrines[nearbyShrine].Find("Blessing").gameObject.SetActive(false);
            if (choice == 0) damageMultiplier *= 1.25f; else if (choice == 1) { maxHp += 20; hp = Mathf.Min(maxHp, hp + 40); } else attackRate *= 1.2f;
            phase = Phase.Playing;
        }
        void OnApplicationFocus(bool focused) { if (!focused && phase == Phase.Playing) phase = Phase.Paused; }
        void OnDestroy() { if (art != null) art.Dispose(); if (hitSound != null) Destroy(hitSound); }
        void Styles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true }; title.normal.textColor = new Color(.88f, .75f, .5f);
            body = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, alignment = TextAnchor.UpperCenter };
            small = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            heading = new GUIStyle(small) { fontSize = 19, fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { fontSize = 16, wordWrap = true };
        }
        bool Button(Rect r, string text) { return GUI.Button(r, text, button); }
        void OnGUI()
        {
            Styles(); float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f); GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale); float width = Screen.width / scale, height = Screen.height / scale;
            GUI.Box(new Rect(15, 15, width - 30, 70), ""); GUI.Label(new Rect(30, 26, 380, 26), "EMBERBOUND / " + Map.Names[Map.Region(player.x)].ToUpperInvariant(), heading);
            GUI.Label(new Rect(30, 57, 650, 22), "WASD Move · Mouse Aim · Click/J Attack · Space Dash · E Shrine · Esc Pause", small);
            if (phase != Phase.Sanctuary) GUI.Label(new Rect(width - 390, 29, 345, 32), Mathf.CeilToInt(Mathf.Max(0, hp)) + " / " + maxHp + " HP   " + (poison.remaining > 0 ? "POISONED" : "") + "   Dash " + (dashCooldown > 0 ? "charging" : "ready"), small);
            if (phase == Phase.Playing) {
                if (nearbyShrine >= 0) GUI.Label(new Rect(width / 2 - 160, height - 60, 320, 32), "E · Commune with the shrine", body);
                GUI.Label(new Rect(30, height - 45, 500, 22), "Eastward journey: " + Mathf.RoundToInt(player.x / Map.Width * 100) + "% · Follow the raised roads", small);
                foreach (var e in enemies) if (e.hp > 0 && e.model.gameObject.activeSelf) {
                    Vector3 p = view.WorldToScreenPoint(e.model.position + Vector3.up * (e.boss ? 2.6f : 1.6f)); if (p.z < 0 || p.x < 0 || p.x > Screen.width || p.y < 0 || p.y > Screen.height) continue;
                    float x = p.x / scale, y = (Screen.height - p.y) / scale;
                    GUI.color = e.spider ? new Color(.65f, .9f, .35f) : new Color(.9f, .75f, .53f); GUI.DrawTexture(new Rect(x - 20, y, 40 * Mathf.Clamp01(e.hp / e.max), 4), Texture2D.whiteTexture); GUI.color = Color.white;
                    if (e.melee.windup > 0) GUI.Label(new Rect(x - 35, y - 20, 70, 22), "STRIKE!", small);
                }
                DrawMap(new Rect(width - 255, 100, 235, 85)); return;
            }
            Rect panel = new Rect(width / 2 - 300, height / 2 - 220, 600, 440); GUI.Box(panel, ""); float px = panel.x, py = panel.y;
            string text = phase == Phase.Sanctuary ? "The crucible remembers." : phase == Phase.Shrine ? "Take a blessing." : phase == Phase.Paused ? "Take a breath." : won ? "The crucible yields." : "An ember remains.";
            GUI.Label(new Rect(px + 20, py + 15, 560, 65), text, title);
            if (phase == Phase.Sanctuary) {
                GUI.Label(new Rect(px + 25, py + 84, 550, 50), "Explore five connected regions. Fight, fall, return stronger.\nYour legacy survives every life.", body);
                for (int i = 0; i < 3; i++) if (Button(new Rect(px + 20 + i * 190, py + 145, 180, 42), (legacy.weapon == i ? "✓ " : "") + weaponNames[i])) { legacy.weapon = i; Save(); MakeKnight(); }
                string[] names = { "Vitality", "Might", "Agility" }; int[] values = { legacy.health, legacy.power, legacy.haste };
                for (int i = 0; i < 3; i++) { GUI.Label(new Rect(px + 25, py + 203 + i * 38, 380, 30), names[i] + " +" + values[i] + (i == 0 ? "  (+2 max HP)" : i == 1 ? "  (+0.5% damage)" : "  (+0.15% speed)"), small); GUI.enabled = legacy.embers >= 3 && values[i] < 200; if (Button(new Rect(px + 445, py + 200 + i * 38, 125, 32), "3 embers +")) { legacy.Buy(i); Save(); } GUI.enabled = true; }
                GUI.Label(new Rect(px + 25, py + 326, 330, 28), legacy.embers + " embers · " + legacy.runs + " lives lived", small);
                if (Button(new Rect(px + 170, py + 365, 260, 48), "ENTER THE CRUCIBLE")) StartRun();
            } else if (phase == Phase.Shrine) {
                GUI.Label(new Rect(px + 35, py + 104, 530, 50), "A gift for this life. Choose your path.", body);
                if (Button(new Rect(px + 70, py + 175, 460, 55), "Fury · Deal 25% more damage")) Bless(0);
                if (Button(new Rect(px + 70, py + 240, 460, 55), "Renewal · Heal 40 and gain 20 max health")) Bless(1);
                if (Button(new Rect(px + 70, py + 305, 460, 55), "Quickening · Attack 20% faster")) Bless(2);
            } else if (phase == Phase.Paused) {
                GUI.Label(new Rect(px + 35, py + 120, 530, 70), "Your journey waits here.\nProgress is awarded when the run ends.", body);
                if (Button(new Rect(px + 170, py + 260, 260, 50), "RESUME")) phase = Phase.Playing;
            } else {
                GUI.Label(new Rect(px + 35, py + 105, 530, 140), result, body);
                if (Button(new Rect(px + 125, py + 300, 350, 55), "RETURN TO THE SANCTUARY")) { phase = Phase.Sanctuary; player = new Point(3, 11); knight.position = V(player); PositionCamera(); foreach (var e in enemies) e.model.gameObject.SetActive(false); foreach (var b in bolts) b.model.gameObject.SetActive(false); }
            }
            if (Button(new Rect(20, height - 48, 130, 30), sound ? "Sound ON" : "Sound OFF")) sound = !sound;
            if (Button(new Rect(160, height - 48, 180, 30), sun.shadows == LightShadows.None ? "Quality: Low" : "Quality: Balanced")) { bool low = sun.shadows != LightShadows.None; sun.shadows = low ? LightShadows.None : LightShadows.Hard; QualitySettings.antiAliasing = low ? 0 : 2; }
            if (!string.IsNullOrEmpty(storageMessage)) GUI.Label(new Rect(350, height - 48, width - 380, 45), storageMessage, small);
        }
        void DrawMap(Rect rect)
        {
            GUI.Box(rect, ""); GUI.color = new Color(.68f, .67f, .51f);
            for (float x = 1; x < 107; x += 1) GUI.DrawTexture(new Rect(rect.x + x / Map.Width * rect.width, rect.y + Map.RoadZ(x) / Map.Depth * rect.height, 4, 3), Texture2D.whiteTexture);
            for (int i = 0; i < 4; i++) { GUI.color = shrineUsed[i] ? Color.gray : new Color(.95f, .78f, .42f); GUI.DrawTexture(new Rect(rect.x + Map.Shrines[i].x / Map.Width * rect.width, rect.y + Map.Shrines[i].z / Map.Depth * rect.height, 4, 4), Texture2D.whiteTexture); }
            GUI.color = Color.white; GUI.DrawTexture(new Rect(rect.x + player.x / Map.Width * rect.width, rect.y + player.z / Map.Depth * rect.height, 5, 5), Texture2D.whiteTexture);
        }
    }
}
