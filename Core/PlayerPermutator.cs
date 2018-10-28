using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class PlayerPermutator
    {
        public IEnumerable<Roster> Permutations(IEnumerable<Player> players)
        {
            var byPos = players.GroupBy(p => p.Position)
                .ToDictionary(g => g.Key, g => g);

            var flexes = byPos["RB"].Concat(byPos["WR"]).Concat(byPos["TE"]).ToArray();

            var rosters = new HashSet<Roster>();
            int limit = 20;
            Roster lowest = new Roster();

            foreach (var qb in byPos["QB"])
            {
                foreach (var te in byPos["TE"])
                {
                    foreach (var flex in flexes)
                    {
                        foreach (var dst in byPos["DST"])
                        {
                            var rbs = byPos["RB"].ToArray();
                            var wrs = byPos["WR"].ToArray();
                            for (int iRb1 = 0; iRb1 < rbs.Length; iRb1++)
                            {
                                for (int iRb2 = iRb1 + 1; iRb2 < rbs.Length; iRb2++)
                                {
                                    for (int iWr1 = 0; iWr1 < wrs.Length; iWr1++)
                                    {
                                        for (int iWr2 = iWr1 + 1; iWr2 < wrs.Length; iWr2++)
                                        {
                                            for (int iWr3 = iWr2 + 1; iWr3 < wrs.Length; iWr3++)
                                            {
                                                var dupeChecker = new HashSet<Player>();
                                                dupeChecker.Add(flex);

                                                if (!dupeChecker.Add(rbs[iRb1]) ||
                                                    !dupeChecker.Add(rbs[iRb2]) ||
                                                    !dupeChecker.Add(wrs[iWr1]) ||
                                                    !dupeChecker.Add(wrs[iWr2]) ||
                                                    !dupeChecker.Add(wrs[iWr3]) ||
                                                    !dupeChecker.Add(te))
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
                                                    rosters.Add(roster);
                                                    lowest = rosters.OrderBy(r => r.Projection).First();
                                                }
                                                else if(roster.Projection > lowest.Projection)
                                                {
                                                    rosters.Add(roster);
                                                    if (rosters.Count > limit)
                                                    {
                                                        rosters.Remove(lowest);
                                                    }
                                                    lowest = rosters.OrderBy(r => r.Projection).First();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return rosters;
        }
    }
}


//foreach (var rb1 in rbs)
//                            {
//                                foreach (var rb2 in rbs)
//                                {
//                                    foreach (var wr1 in wrs)
//                                    {
//                                        foreach (var wr2 in wrs)
//                                        {
//                                            foreach (var wr3 in wrs)
//                                            {
//                                                var dupeCheck = new[]
//                                                {
//                                                    flex, rb1, rb2, wr1, wr2, wr3
//                                                }.GroupBy(x => x);

//                                                if (dupeCheck.Any(x => x.Count() > 1))
//                                                    continue;

//                                                var roster = new Roster();
//roster.Add(qb);
//                                                roster.Add(te);
//                                                roster.Add(flex);
//                                                roster.Add(dst);
//                                                roster.Add(rb1);
//                                                roster.Add(rb2);
//                                                roster.Add(wr1);
//                                                roster.Add(wr2);
//                                                roster.Add(wr3);

//                                                if (roster.Salary > 50000)
//                                                    continue;

//                                                rosters.Add(roster);
//                                            }
//                                        }
//                                    }
//                                }
//                            }