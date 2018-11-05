using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ESPNStatImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = DoWork();
            task.Wait();
        }

        static async Task DoWork()
        {
            var data = new List<PlayerStat>();
            var c = new HttpClient();

            foreach (var url in UrlConfig.All(8))
            {
                var response = await c.GetAsync(url.Url);
                var html = await response.Content.ReadAsStringAsync();
                var tableStart = html.IndexOf("playerTableTable");
                var tableEnd = html.IndexOf("</table>", tableStart);
                var index = tableStart;

                var goodRowParts = new[] { 6, 7, 8, 10, 11, 12 };

                while (index < tableEnd)
                {
                    var player = new PlayerStat()
                    {
                        Position = url.Position
                    };
                    data.Add(player);

                    var trClassIndex = html.IndexOf("pncPlayerRow", index);
                    var trStart = html.Substring(0, trClassIndex).LastIndexOf("<tr ");
                    var trEnd = html.IndexOf("</tr>", trClassIndex) + "</tr>".Length;
                    var trHtml = html.Substring(trStart, trEnd - trStart);

                    var trParts = trHtml.Split(new[] { "</td>" }, StringSplitOptions.None).Select(p => p.Substring(p.IndexOf(">") + 1)).ToArray();

                    var nameParts = trParts[0].Split(new[] { "</a>" }, StringSplitOptions.None);
                    var name = nameParts[0].Substring(nameParts[0].LastIndexOf(">") + 1);
                    player.Name = name;
                    var team = nameParts[1].Substring(2);
                    team = team.Substring(0, team.IndexOf("&nbsp"));
                    player.Team = team;

                    var aOverC = trParts[5].Split('/');

                    player.PassYards  = int.Parse(trParts[6]);
                    player.PassTds  = int.Parse(trParts[7]);
                    player.PassInts = int.Parse(trParts[8]);
                    player.RushYards  = int.Parse(trParts[11]);
                    player.RushTds  = int.Parse(trParts[12]);
                    player.Rec  = int.Parse(trParts[14]);
                    player.RecYards  = int.Parse(trParts[15]);
                    player.RecTds  = int.Parse(trParts[16]);

                    index = trEnd;
                }
            }
        }
    }
}
