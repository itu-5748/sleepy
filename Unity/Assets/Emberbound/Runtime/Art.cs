using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Emberbound
{
    // Shared, untextured materials and reusable low-poly meshes keep assets small.
    public sealed class Art
    {
        public readonly Material steel, darkSteel, bone, green, poison, wood, gold, cape, stone, moss, leaf, bark, spell;
        readonly Dictionary<string, Material> colors = new Dictionary<string, Material>();
        readonly List<Mesh> ownedMeshes = new List<Mesh>();
        readonly Mesh cone, cylinder;
        public Art()
        {
            steel = Mat("Armor", "AEBAC1", .5f); darkSteel = Mat("Dark armor", "374553", .35f);
            bone = Mat("Bone", "D9CBB0"); green = Mat("Spider", "344B30"); poison = Mat("Poison", "91D74F", 0, .45f);
            wood = Mat("Weapon grip", "624B36"); gold = Mat("Gold", "D4A65A", .3f); cape = Mat("Crimson", "883C42");
            stone = Mat("Stone", "75745F"); moss = Mat("Moss", "536347"); leaf = Mat("Pine", "244334");
            bark = Mat("Bark", "443C30"); spell = Mat("Spell", "F2CB89", 0, .55f);
            cone = Radial(true); cylinder = Radial(false);
        }
        public Material Mat(string name, string hex, float metallic = 0, float glow = 0)
        {
            if (colors.ContainsKey(name)) return colors[name];
            var m = new Material(Resources.Load<Shader>("EmberboundLowPoly")) { name = name, enableInstancing = true };
            Color c; ColorUtility.TryParseHtmlString("#" + hex, out c); m.color = c; m.SetFloat("_Metallic", metallic); m.SetFloat("_Glossiness", .22f);
            if (glow > 0) { m.SetColor("_EmissionColor", c * glow); }
            colors.Add(name, m); return m;
        }
        Mesh Radial(bool pointed)
        {
            var verts = new List<Vector3>(); var tris = new List<int>();
            for (int i = 0; i < 8; i++) {
                float a = i * Mathf.PI / 4, b = (i + 1) * Mathf.PI / 4;
                Vector3 p = new Vector3(Mathf.Cos(a) * .5f, 0, Mathf.Sin(a) * .5f), q = new Vector3(Mathf.Cos(b) * .5f, 0, Mathf.Sin(b) * .5f);
                Face(verts, tris, p, pointed ? Vector3.up : p + Vector3.up, pointed ? Vector3.up : q + Vector3.up, q);
                Face(verts, tris, Vector3.zero, p, q, Vector3.zero);
                if (!pointed) Face(verts, tris, Vector3.up, q + Vector3.up, p + Vector3.up, Vector3.up);
            }
            var mesh = new Mesh { name = pointed ? "8-sided cone" : "8-sided cylinder" }; mesh.SetVertices(verts); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); ownedMeshes.Add(mesh); return mesh;
        }
        static void Face(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int n = v.Count; v.Add(a); v.Add(b); v.Add(c); v.Add(d); t.Add(n); t.Add(n + 1); t.Add(n + 2); t.Add(n); t.Add(n + 2); t.Add(n + 3);
        }
        public Transform Part(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, PrimitiveType shape = PrimitiveType.Cube)
        {
            var g = GameObject.CreatePrimitive(shape); g.name = name; var collider = g.GetComponent<Collider>(); Object.Destroy(collider);
            g.transform.SetParent(parent, false); g.transform.localPosition = pos; g.transform.localScale = scale;
            var r = g.GetComponent<MeshRenderer>(); r.sharedMaterial = mat; r.receiveShadows = true; return g.transform;
        }
        public Transform RadialPart(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, bool pointed)
        {
            var g = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); g.transform.SetParent(parent, false); g.transform.localPosition = pos; g.transform.localScale = scale;
            g.GetComponent<MeshFilter>().sharedMesh = pointed ? cone : cylinder; g.GetComponent<MeshRenderer>().sharedMaterial = mat; return g.transform;
        }
        void Limb(Transform root, string name, Vector3 a, Vector3 b, float width, Material mat)
        {
            var part = Part(root, name, (a + b) / 2, new Vector3(width, (b - a).magnitude, width), mat);
            part.rotation = root.rotation * Quaternion.FromToRotation(Vector3.up, b - a);
        }
        public Transform Knight(Transform parent)
        {
            var root = new GameObject("Knight").transform; root.SetParent(parent, false);
            Part(root, "Breastplate", new Vector3(0, .78f, 0), new Vector3(.47f, .53f, .28f), steel);
            Part(root, "Belt", new Vector3(0, .53f, 0), new Vector3(.49f, .07f, .3f), gold);
            Part(root, "Helm", new Vector3(0, 1.22f, 0), new Vector3(.39f, .39f, .36f), steel);
            Part(root, "Visor", new Vector3(0, 1.23f, .19f), new Vector3(.3f, .065f, .02f), darkSteel);
            Part(root, "Plume", new Vector3(0, 1.49f, -.06f), new Vector3(.11f, .2f, .4f), cape);
            Part(root, "Cape", new Vector3(0, .72f, -.19f), new Vector3(.44f, .68f, .05f), cape);
            foreach (float side in new[] { -1f, 1f }) {
                Part(root, side < 0 ? "Left leg" : "Right leg", new Vector3(side * .13f, .25f, 0), new Vector3(.18f, .45f, .23f), darkSteel);
                Part(root, "Pauldron", new Vector3(side * .33f, .96f, 0), new Vector3(.24f, .22f, .31f), steel);
                Part(root, "Arm", new Vector3(side * .33f, .72f, .03f), new Vector3(.16f, .32f, .2f), steel);
            }
            Part(root, "Shield", new Vector3(-.47f, .73f, .17f), new Vector3(.1f, .53f, .37f), gold);
            Part(root, "Shield inset", new Vector3(-.53f, .74f, .17f), new Vector3(.02f, .4f, .27f), Mat("Heraldry", "365B79"));
            Part(root, "Shield cross", new Vector3(-.55f, .74f, .17f), new Vector3(.02f, .29f, .055f), steel);
            return root;
        }
        public Transform Weapon(Transform parent, int type)
        {
            var root = new GameObject("Weapon").transform; root.SetParent(parent, false); root.localPosition = new Vector3(.39f, .69f, .12f);
            Part(root, "Grip", Vector3.zero, new Vector3(.07f, .3f, .07f), wood);
            if (type == 0) { Part(root, "Guard", new Vector3(0, .15f, 0), new Vector3(.3f, .06f, .1f), gold); Part(root, "Blade", new Vector3(0, .56f, 0), new Vector3(.1f, .75f, .055f), steel); }
            else if (type == 1) { Part(root, "Shaft", new Vector3(0, .1f, 0), new Vector3(.065f, 1.8f, .065f), wood); RadialPart(root, "Spearhead", new Vector3(0, 1, 0), new Vector3(.22f, .4f, .15f), steel, true); }
            else { Part(root, "Staff", new Vector3(0, .1f, 0), new Vector3(.08f, 1.3f, .08f), wood); Part(root, "Cinder", new Vector3(0, .85f, 0), new Vector3(.22f, .3f, .22f), spell, PrimitiveType.Sphere); }
            return root;
        }
        public Transform Enemy(Transform parent, bool spider, bool boss)
        {
            var root = new GameObject(boss ? "Ash Warden" : spider ? "Poison spider" : "Skeleton").transform; root.SetParent(parent, false);
            if (spider) {
                Part(root, "Abdomen", new Vector3(0, .35f, -.16f), new Vector3(.6f, .4f, .7f), green, PrimitiveType.Sphere);
                Part(root, "Head", new Vector3(0, .27f, .3f), new Vector3(.4f, .29f, .32f), green, PrimitiveType.Sphere);
                for (int i = 0; i < 4; i++) foreach (float side in new[] { -1f, 1f }) {
                    Vector3 a = new Vector3(side * .2f, .27f, -.35f + i * .19f), b = new Vector3(side * .53f, .42f, -.55f + i * .33f), c = new Vector3(side * .8f, .06f, -.63f + i * .39f);
                    Limb(root, "Leg upper", a, b, .055f, darkSteel); Limb(root, "Leg lower", b, c, .055f, darkSteel);
                }
                foreach (float side in new[] { -1f, 1f }) Part(root, "Poison eye", new Vector3(side * .1f, .34f, .46f), Vector3.one * .08f, poison);
            } else {
                Part(root, "Spine", new Vector3(0, .7f, 0), new Vector3(.075f, .49f, .08f), bone);
                for (int i = 0; i < 3; i++) Part(root, "Ribs", new Vector3(0, .68f + i * .1f, 0), new Vector3(.37f, .055f, .17f), bone);
                Part(root, "Skull", new Vector3(0, 1.12f, 0), new Vector3(.3f, .33f, .29f), bone);
                foreach (float side in new[] { -1f, 1f }) {
                    Part(root, "Eye socket", new Vector3(side * .075f, 1.16f, .15f), new Vector3(.07f, .08f, .02f), darkSteel);
                    Part(root, side < 0 ? "Left leg" : "Right leg", new Vector3(side * .13f, .25f, 0), new Vector3(.07f, .48f, .08f), bone);
                    Part(root, "Arm", new Vector3(side * .26f, .7f, .02f), new Vector3(.07f, .39f, .08f), bone);
                }
                Part(root, "Pelvis", new Vector3(0, .48f, 0), new Vector3(.3f, .09f, .15f), bone);
                Weapon(root, 0);
                if (boss) { root.localScale = Vector3.one * 1.6f; Part(root, "Crown", new Vector3(0, 1.32f, 0), new Vector3(.39f, .12f, .33f), gold); for (int i = -1; i <= 1; i++) Part(root, "Crown point", new Vector3(i * .13f, 1.43f, .12f), new Vector3(.06f, .18f, .07f), gold); }
            }
            return root;
        }
        public void Terrain(Transform parent)
        {
            const float cell = .4f; int w = Mathf.CeilToInt(Map.Width / cell), h = Mathf.CeilToInt(Map.Depth / cell);
            var cells = new bool[w, h]; for (int x = 0; x < w; x++) for (int z = 0; z < h; z++) cells[x, z] = Map.Ground(new Point((x + .5f) * cell, (z + .5f) * cell));
            var v = new List<Vector3>(); var lists = new List<int>[6]; for (int i = 0; i < 6; i++) lists[i] = new List<int>();
            for (int x = 0; x < w; x++) for (int z = 0; z < h; z++) if (cells[x, z]) {
                float a = x * cell, b = z * cell; int color = (x * 17 + z * 31) % 4;
                Face(v, lists[color], new Vector3(a, 0, b), new Vector3(a, 0, b + cell), new Vector3(a + cell, 0, b + cell), new Vector3(a + cell, 0, b));
                if (x == 0 || !cells[x - 1, z]) Face(v, lists[4], new Vector3(a, -2.2f, b), new Vector3(a, -2.2f, b + cell), new Vector3(a, 0, b + cell), new Vector3(a, 0, b));
                if (x == w - 1 || !cells[x + 1, z]) Face(v, lists[5], new Vector3(a + cell, -2.2f, b + cell), new Vector3(a + cell, -2.2f, b), new Vector3(a + cell, 0, b), new Vector3(a + cell, 0, b + cell));
                if (z == 0 || !cells[x, z - 1]) Face(v, lists[5], new Vector3(a + cell, -2.2f, b), new Vector3(a, -2.2f, b), new Vector3(a, 0, b), new Vector3(a + cell, 0, b));
                if (z == h - 1 || !cells[x, z + 1]) Face(v, lists[4], new Vector3(a, -2.2f, b + cell), new Vector3(a + cell, -2.2f, b + cell), new Vector3(a + cell, 0, b + cell), new Vector3(a, 0, b + cell));
            }
            var mesh = new Mesh { name = "Combined low-poly roads", indexFormat = IndexFormat.UInt32 }; mesh.SetVertices(v); mesh.subMeshCount = 6; for (int i = 0; i < 6; i++) mesh.SetTriangles(lists[i], i); mesh.RecalculateNormals(); mesh.RecalculateBounds(); ownedMeshes.Add(mesh);
            var g = new GameObject("Raised stone paths", typeof(MeshFilter), typeof(MeshRenderer)); g.transform.SetParent(parent, false); g.GetComponent<MeshFilter>().sharedMesh = mesh;
            g.GetComponent<MeshRenderer>().sharedMaterials = new[] { stone, Mat("Stone 2", "686F60"), Mat("Stone 3", "83806B"), Mat("Stone 4", "606658"), Mat("Cliff", "344039"), Mat("Cliff dark", "28332F") };
            // Scenery is built once and combined into spatial chunks by material.
            var chunks = new Transform[9]; for (int i = 0; i < 9; i++) { chunks[i] = new GameObject("Scenery chunk " + i).transform; chunks[i].SetParent(parent, false); }
            var random = new System.Random(71);
            for (int i = 0; i < 350; i++) {
                var p = new Point((float)random.NextDouble() * Map.Width, (float)random.NextDouble() * Map.Depth);
                if (Map.Ground(p) || Map.Ground(p + new Point(.65f, 0)) || Map.Ground(p + new Point(-.65f, 0)) || Map.Ground(p + new Point(0, .65f)) || Map.Ground(p + new Point(0, -.65f))) continue;
                var chunk = chunks[Mathf.Min(8, (int)(p.x / 12))]; float height = 1.7f + (float)random.NextDouble();
                var pos = new Vector3(p.x, -1.1f, p.z);
                RadialPart(chunk, "Tree trunk", pos, new Vector3(.2f, height, .2f), bark, false);
                for (int j = 0; j < 3; j++) RadialPart(chunk, "Tree canopy", pos + Vector3.up * (height * .4f + j * .48f), new Vector3(1.65f - j * .38f, height * .62f, 1.65f - j * .38f), leaf, true);
            }
            foreach (var c in Map.Clearings) foreach (float side in new[] { -1f, 1f }) {
                var chunk = chunks[Mathf.Min(8, (int)(c.center.x / 12))]; var pos = new Vector3(c.center.x + side * 2.2f, 0, c.center.z - .5f);
                Part(chunk, "Ruined pillar", pos + Vector3.up * .55f, new Vector3(.32f, 1.1f, .32f), stone);
                Part(chunk, "Torch", pos + Vector3.up * 1.16f, new Vector3(.16f, .19f, .16f), spell);
            }
            foreach (var chunk in chunks) Combine(chunk);
        }
        void Combine(Transform root)
        {
            var grouped = new Dictionary<Material, List<CombineInstance>>(); var filters = root.GetComponentsInChildren<MeshFilter>();
            foreach (var f in filters) { var m = f.GetComponent<MeshRenderer>().sharedMaterial; if (!grouped.ContainsKey(m)) grouped[m] = new List<CombineInstance>(); grouped[m].Add(new CombineInstance { mesh = f.sharedMesh, transform = root.worldToLocalMatrix * f.transform.localToWorldMatrix }); }
            foreach (var pair in grouped) { var mesh = new Mesh { indexFormat = IndexFormat.UInt32 }; mesh.CombineMeshes(pair.Value.ToArray()); ownedMeshes.Add(mesh); var g = new GameObject(pair.Key.name, typeof(MeshFilter), typeof(MeshRenderer)); g.transform.SetParent(root, false); g.GetComponent<MeshFilter>().sharedMesh = mesh; g.GetComponent<MeshRenderer>().sharedMaterial = pair.Key; }
            foreach (var f in filters) Object.Destroy(f.gameObject);
        }
        public Transform Shrine(Transform parent, Point point)
        {
            var root = new GameObject("Shrine").transform; root.SetParent(parent, false); root.position = new Vector3(point.x, 0, point.z);
            for (int i = 0; i < 3; i++) Part(root, "Altar step", new Vector3(0, .08f + i * .12f, 0), new Vector3(1.5f - i * .3f, .16f, 1.2f - i * .2f), stone);
            Part(root, "Shrine cross", new Vector3(0, 1.05f, 0), new Vector3(.14f, 1.4f, .14f), gold); Part(root, "Crossbar", new Vector3(0, 1.3f, 0), new Vector3(.85f, .12f, .12f), gold);
            Part(root, "Blessing", new Vector3(0, 1.83f, 0), Vector3.one * .23f, spell, PrimitiveType.Sphere);
            return root;
        }
        public void Dispose() { foreach (var m in colors.Values) Object.Destroy(m); foreach (var m in ownedMeshes) Object.Destroy(m); }
    }
}
