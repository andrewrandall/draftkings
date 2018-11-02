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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var players = new Importer().Run();

            var alreadyPlayed = Enumerable.Empty<String>();
            //var alreadyPlayed = new[] { "MIA", "HOU", };// "NO", "MIN", "BOS", "BUF" };
            //var alreadyPlayed = new[] { "MIA", "HOU", "NO", "MIN", "BOS", "BUF" };
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

            var rosters = new List<Roster>();
            rosters.Add(new InchBackByEfficiency2().Run(players));
            rosters.AddRange(new RosterVarier().Vary(rosters.First(), players));

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
                //var top = players
                //    .GroupBy(p => p.Position)
                //    .Select(g =>
                //        g.OrderByDescending(p => p.Projection).First())
                //    .ToDictionary(p => p.Position, p => p);

                //var options = players.Where(p => (p.Projection / top[p.Position].Projection) >= .75d).ToArray();

                permutator.Progress += Permutator_Progress;
                var pr = permutator.Permutations(mostPickedPlayers.Select(p => p.Player).ToArray());
                rosters.AddRange(pr);
            }).ContinueWith(_ =>
            {
                var distinctRosters = rosters.Distinct().OrderByDescending(r => r.Projection).ToArray();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    progress.Text = "Done";
                    items.ItemsSource = distinctRosters.OrderByDescending(r => r.Projection);
                    rosterCount.Text = distinctRosters.Length.ToString();
                });
            });
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
    }
}
