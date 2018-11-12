using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings.ESPN
{
    public class PlayerResult
    {
        public string Name { get; set; }
        public string Team { get; set; }
        public string Position { get; set; }
        public double PassYards { get; set; }
        public double PassTds { get; set; }
        public double PassInts { get; set; }
        public double RushYards { get; set; }
        public double RushTds { get; set; }
        public double RecYards { get; set; }
        public double RecTds { get; set; }
        public double Rec { get; set; }
    }
}
