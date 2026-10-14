using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Emberbound.Tests
{
    public class RulesTests
    {
        [Test] public void LegacyRewardsMatchBrowserAndSettleOnce()
        {
            var legacy = new Legacy(); var run = new RunRecord { kills = 25, damageTaken = 170, dodges = 12 };
            for (int i = 0; i < 4; i++) run.visited.Add(i);
            var r = Progression.Settle(legacy, run); Assert.AreEqual(2, r.health); Assert.AreEqual(3, r.power); Assert.AreEqual(2, r.haste); Assert.AreEqual(11, r.embers);
            Progression.Settle(legacy, run); Assert.AreEqual(1, legacy.runs); Assert.AreEqual(11, legacy.embers);
        }
        [Test] public void PurchasesCannotOverspendOrExceedCap()
        {
            var legacy = new Legacy { embers = 3 }; Assert.IsTrue(legacy.Buy(1)); Assert.IsFalse(legacy.Buy(1)); Assert.AreEqual(1, legacy.power);
            legacy.power = 200; legacy.embers = 3; Assert.IsFalse(legacy.Buy(1)); Assert.AreEqual(3, legacy.embers); Assert.IsFalse(legacy.Buy(99));
        }
        [Test] public void PoisonExpiresAndDoesNotStack()
        {
            var p = new Poison(); p.Apply(); float sum = 0; for (int i = 0; i < 90; i++) sum += p.Advance(1f / 30);
            Assert.AreEqual(12, sum); Assert.AreEqual(0, p.Advance(1)); p.Apply(); p.Advance(.3f); p.Apply(); Assert.AreEqual(0, p.Advance(.1f)); Assert.AreEqual(12, p.Advance(2.9f));
        }
        [Test] public void SkeletonStrikeCanBeEvadedBeforeWindupFinishes()
        {
            var melee = new Melee(); Assert.IsFalse(melee.Advance(.8f, .01f)); Assert.Greater(melee.windup, 0); Assert.IsFalse(melee.Advance(2, .5f));
            melee = new Melee(); melee.Advance(.8f, .01f); Assert.IsTrue(melee.Advance(.8f, .5f)); Assert.IsFalse(melee.Advance(.8f, .1f));
        }
        [Test] public void DashCannotEscapeThePath()
        {
            Point p = Map.Move(new Point(3, 11), new Point(0, -20)); Assert.IsTrue(Map.Walkable(p)); Assert.Greater(p.z, 8); Assert.IsFalse(Map.Ground(new Point(3, 1)));
        }
        [Test] public void EveryShrineCampAndBossIsReachable()
        {
            const float step = .4f; var seen = new HashSet<string>(); var queue = new Queue<Point>(); queue.Enqueue(new Point(8, 28)); seen.Add("8,28");
            while (queue.Count > 0) {
                Point p = queue.Dequeue(); foreach (var d in new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) }) {
                    var q = p + d; string key = q.x + "," + q.z; if (!seen.Contains(key) && Map.Walkable(q * step)) { seen.Add(key); queue.Enqueue(q); }
                }
            }
            var targets = new List<Point>(Map.Shrines); targets.AddRange(Map.Camps); targets.Add(Map.Boss);
            foreach (var p in targets) Assert.IsTrue(seen.Contains(Math.Round(p.x / step) + "," + Math.Round(p.z / step)), "Unreachable: " + p.x + "," + p.z);
        }
    }
}
