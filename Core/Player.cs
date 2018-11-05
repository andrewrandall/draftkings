using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class Player
    {
        public string Position { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public double Projection { get; set; }
        public double Salary { get; set; }
        public string Matchup { get; set; }
        public int DKId { get; set; }
        public double AveragePpg { get; set; }

        public StatCollection Stats { get; set; }

        public double PointPerCost
        {
            get
            {
                return Projection / Salary;
            }
        }

        public override string ToString()
        {
            return $"{Position} - {Name} - {Team} - {Projection} for ${Salary} - {AveragePpg}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Player))
                return false;

            return Position == ((Player)obj).Position
                && Team == ((Player)obj).Team
                && Name == ((Player)obj).Name;
        }

        public override int GetHashCode()
        {
            var hashCode = -944881940;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Position);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Team);
            return hashCode;
        }
    }
}
