using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPNStatImporter
{
    public class PlayerStat
    {
        public string Name { get; set; }
        public string Team { get; set; }
        public string Position { get; set; }
        public int PassYards { get; set; }
        public int PassTds { get; set; }
        public int PassInts { get; set; }
        public int RushYards { get; set; }
        public int RushTds { get; set; }
        public int RecYards { get; set; }
        public int RecTds { get; set; }
        public int Rec { get; set; }
    }
}
