using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class StatCollection : IEnumerable<Stat>
    {
        private List<Stat> stats = new List<Stat>();

        public StatCollection(IEnumerable<Stat> stats)
        {
            this.stats = stats.ToList();
        }

        public double Total => stats.Sum(p => p.ProjectedPoints);

        public IEnumerator<Stat> GetEnumerator()
        {
            return stats.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return stats.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(", ", stats.Select(s => s.ToString()));
        }
    }
}
