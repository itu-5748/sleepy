using System;
using System.Collections.Generic;

namespace Emberbound
{
    [Serializable]
    public struct Point
    {
        public float x, z;
        public Point(float x, float z) { this.x = x; this.z = z; }
        public static Point operator +(Point a, Point b) { return new Point(a.x + b.x, a.z + b.z); }
        public static Point operator -(Point a, Point b) { return new Point(a.x - b.x, a.z - b.z); }
        public static Point operator *(Point a, float b) { return new Point(a.x * b, a.z * b); }
        public float Length { get { return (float)Math.Sqrt(x * x + z * z); } }
        public Point Normalized { get { return Length > .0001f ? this * (1f / Length) : new Point(); } }
    }

    [Serializable]
    public class Legacy
    {
        public int version = 1, runs, embers, health, power, haste, weapon;
        public void Sanitize()
        {
            runs = Math.Max(0, runs); embers = Math.Max(0, embers);
            health = Math.Max(0, Math.Min(200, health)); power = Math.Max(0, Math.Min(200, power));
            haste = Math.Max(0, Math.Min(200, haste)); weapon = Math.Max(0, Math.Min(2, weapon));
        }
        public bool Buy(int stat)
        {
            if (embers < 3 || stat < 0 || stat > 2) return false;
            if ((stat == 0 ? health : stat == 1 ? power : haste) >= 200) return false;
            embers -= 3; if (stat == 0) health++; else if (stat == 1) power++; else haste++;
            return true;
        }
    }

    public class RunRecord
    {
        public int kills, dodges;
        public float damageTaken;
        public HashSet<int> visited = new HashSet<int>();
        public bool settled;
    }
    public struct Rewards { public int health, power, haste, embers; }
    public static class Progression
    {
        public static Rewards Settle(Legacy legacy, RunRecord run)
        {
            if (run.settled) return new Rewards();
            run.settled = true;
            var reward = new Rewards {
                health = Math.Min(3, (int)(run.damageTaken / 80)), power = Math.Min(3, run.kills / 8),
                haste = Math.Min(3, run.dodges / 6), embers = Math.Max(1, run.kills / 5 + Math.Max(0, run.visited.Count - 1) * 2)
            };
            legacy.runs++; legacy.embers += reward.embers;
            legacy.health = Math.Min(200, legacy.health + reward.health);
            legacy.power = Math.Min(200, legacy.power + reward.power);
            legacy.haste = Math.Min(200, legacy.haste + reward.haste);
            return reward;
        }
    }
    public class Poison
    {
        public float remaining, tick;
        public void Apply() { remaining = 3; tick = 0; }
        public float Advance(float dt)
        {
            tick += Math.Min(dt, remaining); remaining = Math.Max(0, remaining - dt);
            int count = (int)Math.Floor((tick + .00001f) / .5f); tick -= count * .5f;
            return count * 2;
        }
    }
    public class Melee
    {
        public float windup, cooldown, swing;
        public bool Advance(float distance, float dt)
        {
            cooldown = Math.Max(0, cooldown - dt); swing = Math.Max(0, swing - dt);
            if (windup > 0) {
                windup = Math.Max(0, windup - dt);
                if (windup == 0) { cooldown = .9f; swing = .18f; return distance < 1.16f; }
                return false;
            }
            if (distance < 1.04f && cooldown == 0) windup = .4f;
            return false;
        }
    }
    public struct Road { public Point a, b; public float radius; }
    public struct Clearing { public Point center; public float radius; }
    public static class Map
    {
        public const float Width = 108, Depth = 22;
        public static readonly string[] Names = { "Ashen Approach", "Whispering Grove", "Broken Causeway", "Cinder Wilds", "Warden's Sanctum" };
        public static readonly float[] Starts = { 0, 20, 42, 64, 86 };
        public static readonly Point[] Shrines = { new Point(18, 6.6f), new Point(39, 15.2f), new Point(60, 6.6f), new Point(82, 15.2f) };
        public static readonly Point Boss = new Point(101, 11);
        public static readonly List<Road> Roads = new List<Road>();
        public static readonly List<Clearing> Clearings = new List<Clearing>();
        public static readonly List<Point> Camps = new List<Point>();
        static Map()
        {
            for (float x = 1.2f; x < 106.79f; x += 1.2f) {
                float end = Math.Min(x + 1.2f, 106.8f);
                Roads.Add(new Road { a = new Point(x, RoadZ(x)), b = new Point(end, RoadZ(end)), radius = 1.6f });
            }
            Clearings.Add(new Clearing { center = new Point(3, 11), radius = 2.6f });
            Clearings.Add(new Clearing { center = Boss, radius = 4.2f });
            foreach (var s in Shrines) { Roads.Add(new Road { a = new Point(s.x, RoadZ(s.x)), b = s, radius = 1.1f }); Clearings.Add(new Clearing { center = s, radius = 1.8f }); }
            foreach (float start in Starts) for (int i = 0; i < 3; i++) {
                float x = start + 8.4f + i * 4.8f;
                var p = new Point(x, RoadZ(x)); Camps.Add(p); Clearings.Add(new Clearing { center = p, radius = 2.8f });
            }
        }
        public static float RoadZ(float x) { return 11 + (float)Math.Sin(x / 10) * 1.8f; }
        public static int Region(float x) { for (int i = 4; i >= 0; i--) if (x >= Starts[i]) return i; return 0; }
        public static bool Ground(Point p)
        {
            if (p.x < .48f || p.x > Width - .48f || p.z < .48f || p.z > Depth - .48f) return false;
            foreach (var c in Clearings) if ((p - c.center).Length <= c.radius) return true;
            foreach (var r in Roads) {
                Point d = r.b - r.a; float l = d.x * d.x + d.z * d.z;
                float t = Math.Max(0, Math.Min(1, ((p.x - r.a.x) * d.x + (p.z - r.a.z) * d.z) / l));
                if ((p - (r.a + d * t)).Length <= r.radius) return true;
            }
            return false;
        }
        public static bool Walkable(Point p, float radius = .28f)
        {
            if (!Ground(p)) return false;
            for (int i = 0; i < 12; i++) {
                float a = i * (float)Math.PI / 6;
                if (!Ground(p + new Point((float)Math.Cos(a), (float)Math.Sin(a)) * radius)) return false;
            }
            return true;
        }
        public static Point Move(Point body, Point delta, float radius = .28f)
        {
            int steps = Math.Max(1, (int)Math.Ceiling(delta.Length / .08f)); Point d = delta * (1f / steps);
            for (int i = 0; i < steps; i++) {
                if (Walkable(body + d, radius)) body += d;
                else {
                    Point x = new Point(d.x, 0), z = new Point(0, d.z);
                    if (Walkable(body + x, radius)) body += x;
                    if (Walkable(body + z, radius)) body += z;
                }
            }
            return body;
        }
        public static bool Line(Point a, Point b)
        {
            int n = Math.Max(1, (int)Math.Ceiling((b - a).Length / .16f));
            for (int i = 0; i <= n; i++) if (!Ground(a + (b - a) * ((float)i / n))) return false;
            return true;
        }
    }
}
