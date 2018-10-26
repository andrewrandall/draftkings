using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class InchBackByCost
    {
        public Roster Run(IEnumerable<Player> players)
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

            var allTopPlayersOrdered = players
                .GroupBy(p => p.Position)
                .SelectMany(g => g.Where(p => p.Projection > Percentile(g, 50)))
                .OrderByDescending(p => p.Salary).ToArray();

            int index = 0;
            while (roster.Salary > 50000)
            {
                var newPlayer = allTopPlayersOrdered[index];

                if (!roster.Contains(newPlayer))
                {
                    var mostExpensiveAtPosition = roster
                        .Where(p => p.Position == newPlayer.Position)
                        .OrderByDescending(p => p.Salary)
                        .First();

                    if (mostExpensiveAtPosition.Salary > newPlayer.Salary)
                    {
                        roster.Remove(mostExpensiveAtPosition);
                        roster.Add(newPlayer);
                    }
                }

                index++;
            }

            return roster;
        }

        private static double Percentile(IEnumerable<Player> players, double percentile)
        {
            double count = players.Count();
            int boundary = (int)Math.Floor((percentile / 100) * count);
            return players.OrderByDescending(p => p.Projection).Skip(boundary).First().Projection;
        }
    }
}