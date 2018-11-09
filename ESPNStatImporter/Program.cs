using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

            int week = 9;
            try
            {
                foreach (var url in UrlConfig.All(week))
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

                        //var aOverC = trParts[5].Split('/');

                        player.PassYards = ScrubStatCol(trParts[6]);
                        player.PassTds = ScrubStatCol(trParts[7]);
                        player.PassInts = ScrubStatCol(trParts[8]);
                        player.RushYards = ScrubStatCol(trParts[11]);
                        player.RushTds = ScrubStatCol(trParts[12]);
                        player.Rec = ScrubStatCol(trParts[14]);
                        player.RecYards = ScrubStatCol(trParts[15]);
                        player.RecTds = ScrubStatCol(trParts[16]);

                        index = trEnd;
                    }
                }

                var json = JsonConvert.SerializeObject(data);
                File.WriteAllText($"week{week}.json", json);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private static int ScrubStatCol(string col)
        {
            if (col.Contains("<span"))
            {
                var parts = col.Split('>');
                var s = parts[1].Substring(0, parts[1].IndexOf("<"));
                return int.Parse(s);
            }
            return int.Parse(col);
        }
    }
}
