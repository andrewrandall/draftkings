using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class InchBackByEfficiency2
    {
        public IEnumerable<Roster> Run(IEnumerable<Player> players)
        {
            var alreadyPlayed = new[] { "MIA", "HOU" };

            players = players.Where(p => !alreadyPlayed.Contains(p.Team)).ToArray();

            var orderedPlayers = players.GroupBy(p => p.Position)
                .ToDictionary(p => p.Key, g => g.OrderByDescending(p => p.Projection).ToArray());

            var perfectRoster = new Roster();
            perfectRoster.Add(orderedPlayers["QB"].First());
            perfectRoster.Add(orderedPlayers["RB"].Skip(1).First());
            perfectRoster.Add(orderedPlayers["RB"].Skip(2).First());
            perfectRoster.Add(orderedPlayers["WR"].Skip(1).First());
            perfectRoster.Add(orderedPlayers["WR"].Skip(2).First());
            perfectRoster.Add(orderedPlayers["WR"].Skip(3).First());
            perfectRoster.Add(orderedPlayers["TE"].First());
            perfectRoster.Add(orderedPlayers["DST"].First());

            var flexOptions =
                orderedPlayers["RB"].Union(orderedPlayers["WR"]).Union(orderedPlayers["TE"])
                .OrderByDescending(p => p.Projection);

            foreach (var flex in flexOptions)
            {
                if (perfectRoster.CanAdd(flex))
                {
                    perfectRoster.Add(flex);
                    break;
                }
            }

            var roster = perfectRoster.Clone();
            var skips = new List<Player>();
            var irreplaceablePositions = new List<string>();

            double goal = 50000;

            foreach (var index in Enumerable.Range(0, 5))
            {
                foreach (var index2 in Enumerable.Range(0, 5))
                {
                    bool impossible = false;

                    while (roster.Salary > goal || !roster.IsFull)
                    {
                        if (irreplaceablePositions.Count == 5)
                        {
                            impossible = true;
                            break;
                        }

                        var leastEfficentOnRoster = roster
                            .Where(p => !irreplaceablePositions.Contains(p.Position))
                            .OrderBy(p => p.PointPerCost)
                            .First();

                        var newPlayer = players
                            .Where(p => p.Position == leastEfficentOnRoster.Position)
                            .Where(p => p.Salary < leastEfficentOnRoster.Salary)
                            .Except(skips)
                            .OrderByDescending(p => p.Projection)
                            .FirstOrDefault();

                        if (newPlayer == null)
                        {
                            irreplaceablePositions.Add(leastEfficentOnRoster.Position);
                        }
                        else if (roster.Contains(newPlayer))
                        {
                            skips.Add(newPlayer);
                        }
                        else
                        {
                            roster.Remove(leastEfficentOnRoster);
                            roster.Add(newPlayer);
                        }
                    }

                    if (!impossible)
                    {
                        yield return roster;
                    }
                    else
                    {
                        break;
                    }

                    roster = roster.Clone();
                    goal = roster.Salary - 1;
                }

                skips = new List<Player>();
                irreplaceablePositions = new List<string>();
                goal = 50000;
                roster = perfectRoster.Clone();
                roster.Remove(roster.OrderByDescending(p => p.Salary).First());
            }
        }

        private static double Percentile(IEnumerable<Player> players, double percentile)
        {
            double count = players.Count();
            int boundary = (int)Math.Floor((percentile / 100) * count);
            return players.OrderByDescending(p => p.Projection).Skip(boundary).First().Projection;
        }
    }
}