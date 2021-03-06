﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class PlayerPermutator
    {
        public event EventHandler<double> Progress;

        public IEnumerable<Roster> Permutations(IEnumerable<Player> players)
        {
            var byPos = players
                .GroupBy(p => p.Position)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        if (new[] { "QB" }.Contains(g.Key))
                        {
                            return g.ToArray();
                        }
                        else if (g.Key == "DST")
                        {
                            return g.OrderByDescending(p => p.Projection).Take(3).ToArray();
                        }
                        else
                        {
                            return g.Where(p => p.Projection > 10d).ToArray();
                        }
                    });

            var flexes = byPos["RB"].Concat(byPos["WR"]).ToArray();

            var rosters = new ConcurrentDictionary<Roster, byte>();
            int limit = 20;
            Roster lowest = new Roster();

            double index = 0;
            double count = flexes.Length 
                * byPos["QB"].Length 
                * byPos["TE"].Length 
                * byPos["DST"].Length 
                * (byPos["RB"].Length - 1)
                * (byPos["RB"].Length - 1)
                * (byPos["WR"].Length - 2)
                * (byPos["WR"].Length - 2)
                * (byPos["WR"].Length - 2);

            Parallel.ForEach(flexes, new ParallelOptions { MaxDegreeOfParallelism = 7 }, flex =>
            {
                foreach (var qb in byPos["QB"])
                {
                    foreach (var te in byPos["TE"])
                    {
                        foreach (var dst in byPos["DST"])
                        {
                            var rbs = byPos["RB"];
                            var wrs = byPos["WR"];
                            for (int iRb1 = 0; iRb1 < rbs.Length - 1; iRb1++)
                            {
                                for (int iRb2 = 1; iRb2 < rbs.Length; iRb2++)
                                {
                                    for (int iWr1 = 0; iWr1 < wrs.Length - 2; iWr1++)
                                    {
                                        for (int iWr2 = 1; iWr2 < wrs.Length - 1; iWr2++)
                                        {
                                            for (int iWr3 = 2; iWr3 < wrs.Length; iWr3++)
                                            {
                                                ++index;

                                                if (Progress != null && index % 100000d == 0)
                                                {
                                                    Progress(this, index / count);
                                                }

                                                var dupeChecker = new HashSet<Player>();
                                                dupeChecker.Add(flex);

                                                if (!dupeChecker.Add(rbs[iRb1]) ||
                                                    !dupeChecker.Add(rbs[iRb2]) ||
                                                    !dupeChecker.Add(wrs[iWr1]) ||
                                                    !dupeChecker.Add(wrs[iWr2]) ||
                                                    !dupeChecker.Add(wrs[iWr3]))
                                                {
                                                    continue;
                                                }

                                                var roster = new Roster();
                                                roster.Add(qb);
                                                roster.Add(te);
                                                roster.Add(flex);
                                                roster.Add(dst);
                                                roster.Add(rbs[iRb1]);
                                                roster.Add(rbs[iRb2]);
                                                roster.Add(wrs[iWr1]);
                                                roster.Add(wrs[iWr2]);
                                                roster.Add(wrs[iWr3]);

                                                if (roster.Salary > 50000)
                                                    continue;

                                                if (rosters.Count < limit)
                                                {
                                                    if (rosters.TryAdd(roster, 0))
                                                    {
                                                        lowest = rosters.Keys.OrderBy(r => r.Projection).First();
                                                    }
                                                }
                                                else if (roster.Projection > lowest.Projection)
                                                {
                                                    rosters.TryAdd(roster, 0);
                                                    if (rosters.Count > limit)
                                                    {
                                                        rosters.TryRemove(lowest, out byte z);
                                                    }
                                                    lowest = rosters.Keys.OrderBy(r => r.Projection).First();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return rosters.Keys;
        }
    }
}