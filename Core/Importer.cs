using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class Importer
    {
        private List<Player> players;
        private List<string> unks;

        public IEnumerable<Player> Run()
        {
            players = new List<Player>();
            unks = new List<string>();
            var ass = Assembly.Load("DraftKings.Data");
            var reses = ass.GetManifestResourceNames();
            string salaryRes = string.Empty;
            foreach (var res in reses)
            {
                if (res.Contains("DKSalaries"))
                {
                    salaryRes = res;
                }
                else
                {
                    using (var stream = ass.GetManifestResourceStream(res))
                    using (var reader = new StreamReader(stream))
                    {
                        reader.ReadLine(); // skip 1
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            ProcessPlayer(line, res);
                        }
                    }
                }
            }

            using (var stream = ass.GetManifestResourceStream(salaryRes))
            using (var reader = new StreamReader(stream))
            {
                reader.ReadLine(); // skip 1
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    ProcessSalary(line);
                }
            }

            var x = players.Where(p => p.Salary == 0).ToArray();

            return players.Where(p => p.Salary > 0 && p.Projection > 0 && p.Position != "K").ToArray();
        }

        private void ProcessPlayer(string line, string fileName)
        {
            var position = fileName.Split('_').Last().Split('.').First();
            var parts = line.Split(',').Select(s => s.Trim('"')).ToArray();

            if (parts.All(p => string.IsNullOrWhiteSpace(p)))
                return;

            if (position == "DST")
            {
                var player = new Player
                {
                    Name = parts[0],
                    Position = "DST",
                    Projection = double.Parse(parts.Last()),
                    Team = parts[1]
                };
                players.Add(player);
            }
            else
            {
                var player = new Player
                {
                    Name = parts[0],
                    Position = position,
                    Projection = double.Parse(parts.Last()),
                    Team = parts[1]
                };
                players.Add(player);
            }
        }

        private void ProcessSalary(string line)
        {
            var parts = line.Split(',').Select(s => s.Trim('"').Trim()).ToArray();
            var pos = parts[0];
            var name = parts[2];
            var team = parts[7];
            var salary = double.Parse(parts[5]);
            
            if (pos == "DST")
            {
                var hits = players.Where(p => 
                    (p.Name.Contains(name) || name.Contains(p.Name)) 
                    && p.Position == pos)
                    .ToArray();

                if (hits.Length == 1)
                {
                    hits[0].Salary = salary;
                }
                else
                {
                    throw new Exception("Ambiguous match");
                }
            }
            else
            {
                var hits = players
                   .Where(p => p.Position == pos && p.Team == team)
                   .Select(p =>
                       new
                       {
                           Distance = LevenshteinDistance.Compute(p.Name, name),
                           Player = p
                       })
                   .OrderBy(x => x.Distance)
                   .ToArray();

                var best = hits.First();

                if (best.Distance > 5)
                {
                    unks.Add(line);
                }
                else
                {
                    best.Player.Salary = salary;
                }
            }

            //if (hits.First().Distance > 3)
            //{
            //    var player = new Player
            //    {
            //        Name = "UNK",
            //        Position = "UNK",
            //        Projection = 0.0,
            //        Team = "UNK",
            //        Salary = 0.0
            //    };
            //    unks.Add(line);
            //}

            //var hits = players.Where(p => (p.Name.Contains(name) || name.Contains(p.Name)) && p.Position == pos).ToArray();
            //if (hits.Length == 1)
            //{
            //    hits[0].Salary = salary;
            //}
            //else if (hits.Length == 0)
            //{
            //    var player = new Player
            //    {
            //        Name = "UNK",
            //        Position = "UNK",
            //        Projection = 0.0,
            //        Team = "UNK",
            //        Salary = 0.0
            //    };
            //    players.Add(player);
            //    unks.Add(Tuple.Create(line, player));
            //}
            //else
            //{
            //    throw new Exception("Ambiguous match");
            //}
        }
    }
}
