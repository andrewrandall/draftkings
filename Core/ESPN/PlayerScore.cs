using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings.ESPN
{
    public class PlayerScore
    {
        private Player player;
        private PlayerResult result;

        public PlayerScore(Player player, PlayerResult result)
        {
            this.player = player;
            this.result = result;
        }

        public string Name => player.Name;
        public string Team => player.Team;
        public double Score => CalcScore(result);
        public double Projection => player.Projection;
        public double Salary => player.Salary;
        public double Difference => CalcScore(result) - Projection;

        public static double CalcScore(PlayerResult result)
        {
            double total = 0;
            total += result.PassYards / 25d;
            total += result.PassTds * 4d;
            total -= result.PassInts * 2d;
            total += result.RushYards / 10d;
            total += result.RushTds * 6d;
            total += result.RecYards / 10d;
            total += result.RecTds * 6d;
            total += result.Rec;
            return total;
        }
    }
}
