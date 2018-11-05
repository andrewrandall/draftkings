using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPNStatImporter
{
    class UrlConfig
    {
        public string Position { get; set; }
        public int Week { get; set; }
        public int PositionId { get; set; }
        public string Url => $"http://games.espn.com/ffl/leaders?&scoringPeriodId={Week}&seasonId=2018&slotCategoryId={PositionId}";

        public static UrlConfig[] All(int week) =>
            new[]
            {
                new UrlConfig() { Position = "QB", Week = week, PositionId = 0 },
                new UrlConfig() { Position = "RB", Week = week, PositionId = 2 },
                new UrlConfig() { Position = "WR", Week = week, PositionId = 4 },
                new UrlConfig() { Position = "TE", Week = week, PositionId = 6 }
                //new UrlConfig() { Position = "DST", Week = week, PositionId = 16 }
            };
    }
}
