﻿using System;
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

        private Dictionary<string, string> dstMap = new Dictionary<string, string>
        {
            { "Philadelphia Eagles", "PHI" },
            { "Kansas City Chiefs", "KC" },
            { "New York Jets", "NYJ" },
            { "Los Angeles Chargers", "LAC" },
            { "Washington Redskins", "WAS" },
            { "Chicago Bears", "CHI" },
            { "Los Angeles Rams", "LAR" },
            { "Atlanta Falcons", "ATL" },
            { "Green Bay Packers", "GB" },
            { "Indianapolis Colts", "IND" },
            { "New England Patriots", "NE" },
            { "Dallas Cowboys", "DAL" },
            { "New York Giants", "NYG" },
            { "Carolina Panthers", "CAR" },
            { "New Orleans Saints", "NO" },
            { "Detroit Lions", "DET" },
            { "San Francisco 49ers", "SF" },
            { "Jacksonville Jaguars", "JAC" },
            { "Pittsburgh Steelers", "PIT" },
            { "Seattle Seahawks", "SEA" },
            { "Cleveland Browns", "CLE" },
            { "Buffalo Bills", "BUF" },
            { "Miami Dolphins", "MIA" },
            { "Tennessee Titans", "TEN" },
            { "Tampa Bay Buccaneers", "TB" },
            { "Arizona Cardinals", "ARI" },
            { "Cincinnati Bengals", "CIN" },
            { "Oakland Raiders", "OAK" }
        };

        private Dictionary<string, string> fantasyProTeamMap = new Dictionary<string, string>
        {
            { "JAC", "JAX" }
        };

        public IEnumerable<Player> Run()
        {
            players = new List<Player>();
            unks = new List<string>();
            var ass = Assembly.Load("DraftKings.Data");
            var reses = ass.GetManifestResourceNames();
            string salaryRes = string.Empty;
            foreach (var res in reses)
            {
                if (res.Contains("w10"))
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

            return players.Where(p => p.Salary > 0 && p.Projection > 0 && p.Position != "K").ToArray();
        }

        private void ProcessPlayer(string line, string fileName)
        {
            var position = fileName.Split('_').Last().Split('.').First();
            var parts = line.Split(',').Select(s => s.Trim('"')).ToArray();

            if (parts.All(p => string.IsNullOrWhiteSpace(p)))
                return;

            var player = new Player()
            {
                Name = parts[0],
                Projection = double.Parse(parts.Last()),
                Team = MapToDKTeam(parts[1])
            };
            players.Add(player);

            var statMaps = new Dictionary<string, IEnumerable<Tuple<int, StatCategory>>>()
            {
                {
                    "QB",
                    new[]
                    {
                        Tuple.Create(4, StatCategory.PassYards),
                        Tuple.Create(5, StatCategory.PassTds),
                        Tuple.Create(6, StatCategory.PassInts),
                        Tuple.Create(8, StatCategory.RushYards),
                        Tuple.Create(9, StatCategory.RushTds),
                    }
                },
                {
                    "RB",
                    new[]
                    {
                        Tuple.Create(3, StatCategory.RushYards),
                        Tuple.Create(4, StatCategory.RushTds),
                        Tuple.Create(5, StatCategory.Rec),
                        Tuple.Create(6, StatCategory.RecYards),
                        Tuple.Create(7, StatCategory.RecTds),
                    }
                },
                {
                    "WR",
                    new[]
                    {
                        Tuple.Create(2, StatCategory.Rec),
                        Tuple.Create(3, StatCategory.RecYards),
                        Tuple.Create(4, StatCategory.RecTds),
                        Tuple.Create(6, StatCategory.RushYards),
                        Tuple.Create(7, StatCategory.RushTds),
                    }
                },
                {
                    "TE",
                    new[]
                    {
                        Tuple.Create(2, StatCategory.Rec),
                        Tuple.Create(3, StatCategory.RecYards),
                        Tuple.Create(4, StatCategory.RecTds),
                    }
                }
            };

            switch(position)
            {
                case "DST":
                    player.Position = "DST";
                    player.Team = dstMap[parts[0]];
                    break;

                case "QB":
                case "RB":
                case "WR":
                case "TE":
                    player.Position = position;
                    player.Stats = new StatCollection( statMaps[position].Select(m => new Stat { Category = m.Item2, Projection = double.Parse(parts[m.Item1]) }).ToArray());
                    break;
            }
        }

        private void ProcessSalary(string line)
        {
            var parts = line.Split(',').Select(s => s.Trim('"').Trim()).ToArray();
            var pos = parts[0];
            var name = parts[2];
            var matchup = parts[6];
            var team = parts[7];
            var dkId = int.Parse(parts[3]);
            var salary = double.Parse(parts[5]);
            var averagePpg = double.Parse(parts[8]);
            
            if (pos == "DST")
            {
                var hits = players.Where(p => 
                    (p.Name.Contains(name) || name.Contains(p.Name)) 
                    && p.Position == pos)
                    .ToArray();

                if (hits.Length == 1)
                {
                    hits[0].Salary = salary;
                    hits[0].Matchup = matchup;
                    hits[0].DKId = dkId;
                    hits[0].AveragePpg = averagePpg;
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

                var best = hits.FirstOrDefault();

                if (best == null || best.Distance > 4)
                {
                    unks.Add(line);
                }
                else
                {
                    best.Player.Salary = salary;
                    hits[0].Player.Matchup = matchup;
                    hits[0].Player.DKId = dkId;
                    hits[0].Player.AveragePpg = averagePpg;
                }
            }
        }

        private string MapToDKTeam(string team)
        {
            if (fantasyProTeamMap.TryGetValue(team.ToUpper(), out string hit))
            {
                return hit;
            }
            return team;
        }
    }
}
