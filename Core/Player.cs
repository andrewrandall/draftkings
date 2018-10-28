using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class Player
    {
        private Guid id = Guid.NewGuid();

        public string Position { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public double Projection { get; set; }
        public double Salary { get; set; }
        public string Matchup { get; set; }

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
            return $"{Position} - {Name} - {Team} - {Projection} for ${Salary}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Player))
                return false;

            return id == ((Player)obj).id;
        }

        public override int GetHashCode()
        {
            return 1877310944 + EqualityComparer<Guid>.Default.GetHashCode(id);
        }
    }
}
