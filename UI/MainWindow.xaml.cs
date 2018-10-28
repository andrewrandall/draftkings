using System;
using System.Collections.Generic;
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

            var alreadyPlayed = new[] { "MIA", "HOU", };// "NO", "MIN", "BOS", "BUF" };
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

            pickGrid.ItemsSource =
                rosters.SelectMany(r => r).GroupBy(p => p).Select(g =>
                    new
                    {
                        Count = g.Count(),
                        Player = g.Key
                    })
                    .OrderByDescending(x => x.Count);

            var distinctRosters = rosters.Distinct().OrderByDescending(r => r.Projection).ToArray();
            items.ItemsSource = distinctRosters;

            rosterCount.Text = distinctRosters.Length.ToString();
        }
    }
}
