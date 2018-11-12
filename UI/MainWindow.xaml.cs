//using DraftKings.Backup;
using DraftKings.Backup;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DraftKings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEnumerable<Player> allPlayers;
        private IEnumerable<Roster> allRosters;

        public MainWindow()
        {
            InitializeComponent();

            TryRestoreBackup();
        }

        private void Permutator_Progress(object sender, double e)
        {
            Application.Current.Dispatcher.Invoke(() => progress.Text = e.ToString());
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var roster = (Roster)((Button)sender).DataContext;
            var fileBuilder = new StringBuilder();
            fileBuilder.AppendLine("QB,RB,RB,WR,WR,WR,TE,FLEX,DST");
            fileBuilder.AppendLine(roster.ToCsv());
            File.WriteAllText("roster.csv", fileBuilder.ToString());
        }

        private void TryRestoreBackup()
        {
            if (File.Exists("bak.json"))
            {
                var json = File.ReadAllText("bak.json");
                var backup = JsonConvert.DeserializeObject<DataBackup>(json);

                var players = backup.Players.Select(p => p.FromBackup());

                var byPos = players.GroupBy(p => p.Position);
                foreach (var group in byPos)
                {
                    switch (group.Key)
                    {
                        case "QB":
                            qbGrid.ItemsSource = group.ToArray();
                            break;

                        case "RB":
                            rbGrid.ItemsSource = group.ToArray();
                            break;

                        case "WR":
                            wrGrid.ItemsSource = group.ToArray();
                            break;

                        case "TE":
                            teGrid.ItemsSource = group.ToArray();
                            break;

                        case "DST":
                            dstGrid.ItemsSource = group.ToArray();
                            break;
                    }
                }

                var mostPickedPlayers = backup.MostPlayedPlayers.Select(
                    x =>
                    new
                    {
                        Count = x.PlayedCount,
                        x.Player,
                        x.Player.Position,
                        x.Player.Name,
                        x.Player.Team,
                        x.Player.Projection,
                        x.Player.Salary,
                        x.Player.Matchup,
                        PointPerCost = x.Player.Projection / x.Player.Salary
                    })
                    .OrderByDescending(x => x.Count)
                    .ToArray();

                pickGrid.ItemsSource = mostPickedPlayers;

                var rosters = backup.TopRosters.Select(r => r.FromBackup()).ToArray();
                allRosters = rosters.ToArray();
                items.ItemsSource = rosters.OrderByDescending(r => r.Projection);
                rosterCount.Text = rosters.Length.ToString();
                progress.Text = "Restored Backup";
            }
        }

        private void Filter_Changed(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(filter.Text))
            {
                items.ItemsSource = allRosters;
            }
            else
            {
                var teams = filter.Text.Split(' ');

                items.ItemsSource =
                    allRosters.Where(r => !r.Any(p => teams.Contains(p.Team))).ToArray();
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var players = new Importer().Run();

            var alreadyPlayed = new[] { "CAR", "PIT", "NYG", "SF", "DAL", "PHI" };
            //var alreadyPlayed = Enumerable.Empty<string>();
            players = players.Where(p => !alreadyPlayed.Contains(p.Team)).ToArray();

            var byPos = players.GroupBy(p => p.Position);
            foreach (var group in byPos)
            {
                switch (group.Key)
                {
                    case "QB":
                        qbGrid.ItemsSource = group.ToArray();
                        break;

                    case "RB":
                        rbGrid.ItemsSource = group.ToArray();
                        break;

                    case "WR":
                        wrGrid.ItemsSource = group.ToArray();
                        break;

                    case "TE":
                        teGrid.ItemsSource = group.ToArray();
                        break;

                    case "DST":
                        dstGrid.ItemsSource = group.ToArray();
                        break;
                }
            }

            var results = new ESPN.Importer().Run(players, alreadyPlayed).ToArray();
            resultsGrid.ItemsSource = results;
            var avgDiff = results.Sum(r => r.Difference) / results.Length;

            var sb = new StringBuilder();
            sb.AppendLine($"Avg Diff: {avgDiff}");

            var resByPos = results.GroupBy(r => r.Position)
                .Select(g => new
                {
                    Position = g.Key,
                    AvgDiff = g.Sum(p => p.Difference) / g.Count(),
                    AvgDiffVsAvg = g.Sum(p => p.DifferenceVsSeason) / g.Count()
                });

            foreach (var x in resByPos)
            {
                sb.AppendLine($"Avg Diff at {x.Position} = {x.AvgDiff}");
            }

            var resByPos2 = results.GroupBy(r => r.Position)
                .Select(g => new
                {
                    Position = g.Key,
                    Count = (double)g.Count(),
                    Group = g.OrderByDescending(p => p.Score).ToArray()
                })
                .Select(x => new
                {
                    Position = x.Position,
                    TopAvg = x.Group.Select(z => z.Score).Take((int)Math.Ceiling(x.Count * .25d)).Average()
                });

            foreach (var x in resByPos2)
            {
                sb.AppendLine($"Top 25% Avg at {x.Position} = {x.TopAvg}");
            }

            resultsInfo.Text = sb.ToString();
        }

        private void RosterizeButton_Click(object sender, RoutedEventArgs e)
        {
            var rosters = new List<Roster>();
            rosters.Add(new InchBackByEfficiency2().Run(allPlayers));
            rosters.AddRange(new RosterVarier().Vary(rosters.First(), allPlayers));

            mostPickedRosters.ItemsSource = rosters
                .GroupBy(r => r)
                .Select(g =>
                    new
                    {
                        Count = g.Count(),
                        Roster = g.Key
                    })
                .OrderByDescending(g => g.Count)
                .ToArray();

            var mostPickedPlayers = rosters.SelectMany(r => r).GroupBy(p => p).Select(g =>
                    new
                    {
                        Count = g.Count(),
                        g.Key.Position,
                        g.Key.Name,
                        g.Key.Team,
                        g.Key.Projection,
                        g.Key.Salary,
                        g.Key.Matchup,
                        g.Key.PointPerCost,
                        Player = g.Key
                    })
                    .OrderByDescending(x => x.Count);

            pickGrid.ItemsSource = mostPickedPlayers;

            var permutator = new PlayerPermutator();

            Task.Run(() =>
            {
                permutator.Progress += Permutator_Progress;
                var pr = permutator.Permutations(mostPickedPlayers.Select(p => p.Player).ToArray());
                return pr;
            }).ContinueWith(t =>
            {
                rosters.AddRange(t.Result);
                var distinctRosters = rosters.Distinct().OrderByDescending(r => r.Projection).ToArray();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    progress.Text = "Done";
                    items.ItemsSource = distinctRosters.OrderByDescending(r => r.Projection);
                    rosterCount.Text = distinctRosters.Length.ToString();
                    allRosters = distinctRosters.ToArray();
                });

                var backup = new DataBackup
                {
                    TopRosters = distinctRosters.Select(
                        r =>
                        new RosterBackup()
                        {
                            Players = r.Select(p => p.ToBackup())
                        }),
                    MostPlayedPlayers = mostPickedPlayers.Select(
                        x =>
                        new MostPlayedPlayerBackup
                        {
                            PlayedCount = x.Count,
                            Player = x.Player.ToBackup()
                        }),
                    Players = allPlayers.Select(p => p.ToBackup())
                };
                var json = JsonConvert.SerializeObject(backup);
                File.WriteAllText("bak.json", json);
            });
        }
    }
}
