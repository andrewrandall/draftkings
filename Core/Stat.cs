using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class Stat
    {
        public StatCategory Category { get; set; }
        public double Projection { get; set; }

        public double ProjectedPoints => Projection * Category.Value;

        public override string ToString()
        {
            return $"{Category.Name}: {ProjectedPoints} = {Projection}@{Category.Value}";
        }
    }
}
