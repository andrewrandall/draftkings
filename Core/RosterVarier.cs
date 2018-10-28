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
            foreach (var player3 in roster)
            {
                foreach (var player in roster)
                {
                    foreach (var player2 in roster)
                    {
                        if (player == player2 || player == player3 || player2 == player3)
                            continue;

                        var newRoster = roster.Clone();
                        newRoster.Remove(player);
                        newRoster.Remove(player2);
                        newRoster.Remove(player3);

                        var skips = new List<Player>();
                        skips.Add(player);
                        skips.Add(player2);
                        skips.Add(player3);
                        var irreplaceablePositions = new List<string>();

                        double goal = 50000;

                        var newPlayer1 = players
                            .Where(p => p.Position == player.Position)
                            .Where(p => !newRoster.Contains(p))
                            .Except(skips)
                            .OrderByDescending(p => p.Projection)
                            .First();

                        newRoster.Add(newPlayer1);
                        skips.Add(newPlayer1);

                        var newPlayer2 = players
                                .Where(p => p.Position == player2.Position)
                                .Where(p => !newRoster.Contains(p))
                                .Except(skips)
                                .OrderByDescending(p => p.Projection)
                                .First();

                        newRoster.Add(newPlayer2);
                        skips.Add(newPlayer2);

                        var newPlayer3 = players
                                .Where(p => p.Position == player3.Position)
                                .Where(p => !newRoster.Contains(p))
                                .Except(skips)
                                .OrderByDescending(p => p.Projection)
                                .First();

                        newRoster.Add(newPlayer3);
                        skips.Add(newPlayer3);

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
    }
}
