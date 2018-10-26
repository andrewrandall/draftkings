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
            var orderedPlayers = players.GroupBy(p => p.Position)
                .ToDictionary(p => p.Key, g => g.OrderByDescending(p => p.Projection).ToArray());

            var roster = new Roster();
            roster.Add(orderedPlayers["QB"].First());
            roster.Add(orderedPlayers["RB"].Skip(1).First());
            roster.Add(orderedPlayers["RB"].Skip(2).First());
            roster.Add(orderedPlayers["WR"].Skip(1).First());
            roster.Add(orderedPlayers["WR"].Skip(2).First());
            roster.Add(orderedPlayers["WR"].Skip(3).First());
            roster.Add(orderedPlayers["TE"].First());
            roster.Add(orderedPlayers["DST"].First());

            var flexOptions =
                orderedPlayers["RB"].Union(orderedPlayers["WR"]).Union(orderedPlayers["TE"])
                .OrderByDescending(p => p.Projection);

            foreach (var flex in flexOptions)
            {
                if (roster.CanAdd(flex))
                {
                    roster.Add(flex);
                    break;
                }
            }

            var skips = new List<Player>();
            var irreplaceablePositions = new List<string>();

            double goal = 50000;

            for (int i = 0; i < 5; i++)
            {
                while (roster.Salary > goal)
                {
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

                yield return roster;

                roster = roster.Clone();
                goal = roster.Salary - 1;
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