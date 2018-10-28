using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class RosterVarier
    {
        public IEnumerable<Roster> Vary(Roster roster, IEnumerable<Player> players)
        {
            var alreadyPlayed = new[] { "MIA", "HOU", };// "NO", "MIN", "BOS", "BUF" };

            players = players.Where(p => !alreadyPlayed.Contains(p.Team)).ToArray();

            foreach (var player in roster)
            {
                var newRoster = roster.Clone();
                newRoster.Remove(player);

                var skips = new List<Player>();
                skips.Add(player);
                var irreplaceablePositions = new List<string>();

                double goal = 50000;

                newRoster.Add(players
                        .Where(p => p.Position == player.Position)
                        .Except(skips)
                        .OrderByDescending(p => p.Projection)
                        .FirstOrDefault());

                while (newRoster.Salary > goal)
                {
                    var leastEfficentOnRoster = newRoster
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
                    else if (newRoster.Contains(newPlayer))
                    {
                        skips.Add(newPlayer);
                    }
                    else
                    {
                        newRoster.Remove(leastEfficentOnRoster);
                        newRoster.Add(newPlayer);
                    }
                }

                yield return newRoster;
            }
        }
    }
}
