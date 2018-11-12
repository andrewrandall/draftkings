using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings.ESPN
{
    public class Importer
    {
        public IEnumerable<PlayerScore> Run(IEnumerable<Player> players, IEnumerable<string> alreadyPlayed)
        {
            var path = @"..\..\..\ESPNStatImporter\bin\Debug";
            var files = Directory.GetFiles(path, "*.json");
            var newestWeek = files.Select(f => int.Parse(Path.GetFileNameWithoutExtension(f).Substring(4))).OrderByDescending(f => f).First();
            var file = File.ReadAllText(Path.Combine(path, $"week{newestWeek}.json"));
            var results = JsonConvert.DeserializeObject<PlayerResult[]>(file);

            var misses = new List<PlayerResult>();

            foreach (var result in results)
            {
                if (alreadyPlayed.Contains(result.Team, StringComparer.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var team = MapTeam(result.Team);

                var hits = players
                    .Where(p => string.Equals(p.Team, team, StringComparison.CurrentCultureIgnoreCase)
                        && string.Equals(p.Position, result.Position, StringComparison.CurrentCultureIgnoreCase))
                    .Select(p =>
                    new
                    {
                        Player = p,
                        Distance = LevenshteinDistance.Compute(p.Name, result.Name),
                    })
                    .OrderBy(x => x.Distance)
                    .ToArray();

                if (hits.Any(h => h.Distance <= 4 && h.Player.Name == "Matt Ryan"))
                {
                    var x = 1;
                }

                if (!hits.Any() || hits.First().Distance > 4)
                {
                    misses.Add(result);
                    var score = PlayerScore.CalcScore(result);
                    if (result.Team == "FA" || score == 0)
                    {
                        continue;
                    }
                    //yield return new PlayerScore(new Player { Name = $"MISS - {result.Name}", Team = team }, result);
                }
                else
                {
                    var hit = hits.First().Player;
                    if (result.Team == "FA" || hit.Salary == 0)
                    {
                        continue;
                    }
                    yield return new PlayerScore(hit, result);
                }
            }
        }

        private static Dictionary<string, string> teamMap = new Dictionary<string, string>()
        {
            { "WSH", "WAS" },
            { "JAC", "JAX" },
        };

        private static string MapTeam(string team)
        {
            if (teamMap.TryGetValue(team.ToUpper().Trim(), out string hit))
            {
                return hit;
            }
            return team.ToUpper().Trim();
        }
    }
}
