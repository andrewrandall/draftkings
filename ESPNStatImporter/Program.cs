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
            var c = new HttpClient();
            var qbUrl = @"http://games.espn.com/ffl/leaders?&scoringPeriodId=8&seasonId=2018&slotCategoryId=0";
            var qbResponse = await c.GetAsync(qbUrl);
            var html = await qbResponse.Content.ReadAsStringAsync();
            var tableStart = html.IndexOf("playerTableTable");
            var tableEnd = html.IndexOf("</table>", tableStart);
            var index = tableStart;
            var data = new List<List<string>>();

            var goodRowParts = new[] { 6, 7, 8, 10, 11, 12 };

            while (index < tableEnd)
            {
                var rowData = new List<string>();
                data.Add(rowData);
                var trClassIndex = html.IndexOf("pncPlayerRow", index);
                var trStart = html.Substring(0, trClassIndex).LastIndexOf("<tr ");
                var trEnd = html.IndexOf("</tr>", trClassIndex) + "</tr>".Length;
                var trHtml = html.Substring(trStart, trEnd - trStart);

                var trParts = trHtml.Split(new[] { "</td>" }, StringSplitOptions.None).Select(p => p.Substring(p.IndexOf(">") + 1)).ToArray();

                var nameParts = trParts[0].Split(new[] { "</a>" }, StringSplitOptions.None);
                var name = nameParts[0].Substring(nameParts[0].LastIndexOf(">") + 1);
                rowData.Add(name);
                var team = nameParts[1].Substring(2);
                team = team.Substring(0, team.IndexOf("&nbsp"));
                rowData.Add(team);

                rowData.AddRange(trParts[5].Split('/'));
                foreach (var goodIndex in goodRowParts)
                {
                    rowData.Add(trParts[goodIndex]);
                }

                index = trEnd;
            }
        }
    }
}
