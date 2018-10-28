using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class StatCategory
    {
        public string Name { get; set; }
        public double Value { get; set; }

        public static StatCategory PassYards => new StatCategory { Name = "Pass Yds", Value = 1d / 25d };
        public static StatCategory PassTds => new StatCategory { Name = "Pass Tds", Value = 4 };
        public static StatCategory PassInts => new StatCategory { Name = "Pass Ints", Value = -1 };

        public static StatCategory RushYards => new StatCategory { Name = "Rush Yds", Value = 1d / 10d };
        public static StatCategory RushTds => new StatCategory { Name = "Rush Tds", Value = 6 };

        public static StatCategory RecYards => new StatCategory { Name = "Rec Yds", Value = 1d / 10d };
        public static StatCategory RecTds => new StatCategory { Name = "Rec Tds", Value = 6 };
        public static StatCategory Rec => new StatCategory { Name = "Rec", Value = 1 };
    }
}
