using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraftKings
{
    public class Roster : IEnumerable<Player>
    {
        private List<Player> players = new List<Player>();
        private Player qb;
        private Player rb1;
        private Player rb2;
        private Player wr1;
        private Player wr2;
        private Player wr3;
        private Player te;
        private Player flex;
        private Player dst;

        public bool CanAdd(Player player)
        {
            if (IsFull)
                return false;

            if (players.Contains(player))
                return false;

            switch (player.Position)
            {
                case "QB":
                    return qb == null;
                case "RB":
                    return rb1 == null || rb2 == null || flex == null;
                case "WR":
                    return wr1 == null || wr2 == null || wr3 == null || flex == null;
                case "DST":
                    return dst == null;
                case "TE":
                    return te == null || flex == null;
                default:
                    throw new ArgumentException("player position");
            }
        }

        public void Add(Player player)
        {
            if (!CanAdd(player))
                throw new ArgumentException("player");

            players.Add(player);

            switch (player.Position)
            {
                case "QB":
                    qb = player;
                    break;

                case "RB":
                    if (rb1 == null)
                    {
                        rb1 = player;
                    }
                    else if (rb2 == null)
                    {
                        rb2 = player;
                    }
                    else
                    {
                        flex = player;
                    }
                    break;

                case "WR":
                    if (wr1 == null)
                    {
                        wr1 = player;
                    }
                    else if (wr2 == null)
                    {
                        wr2 = player;
                    }
                    else if (wr3 == null)
                    {
                        wr3 = player;
                    }
                    else
                    {
                        flex = player;
                    }
                    break;

                case "DST":
                    dst = player;
                    break;

                case "TE":
                    if (te == null)
                    {
                        te = player;
                    }
                    break;

                default:
                    throw new ArgumentException("player position");
            }
        }

        public double Salary
        {
            get
            {
                return players.Sum(p => p.Salary);
            }
        }

        public double Projection
        {
            get
            {
                return players.Sum(p => p.Projection);
            }
        }

        public bool IsFull
        {
            get
            {
                return players.Count() == 9;
            }
        }

        public void Remove(Player player)
        {
            players.Remove(player);

            switch (player.Position)
            {
                case "QB":
                    qb = null;
                    break;

                case "RB":
                    if (rb1 == player)
                    {
                        rb1 = null;
                    }
                    else if (rb2 == player)
                    {
                        rb2 = null;
                    }
                    else
                    {
                        flex = null;
                    }
                    break;

                case "WR":
                    if (wr1 == player)
                    {
                        wr1= null;
                    }
                    else if (wr2 == player)
                    {
                        wr2= null;
                    }
                    else if (wr3 == player)
                    {
                        wr3= null;
                    }
                    else
                    {
                        flex= null;
                    }
                    break;

                case "DST":
                    dst= null;
                    break;

                case "TE":
                    if (te == player)
                    {
                        te= null;
                    }
                    break;

                default:
                    throw new ArgumentException("player position");
            }
        }

        public IEnumerator<Player> GetEnumerator()
        {
            return players.OrderByDescending(p => p.Projection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return players.OrderByDescending(p => p.Projection).GetEnumerator();
        }

        public Roster Clone()
        {
            return new Roster()
            {
                players = players.ToList(),
                qb = qb,
                rb1 = rb1,
                rb2 = rb2,
                wr1 = wr1,
                wr2 = wr2,
                wr3 = wr3,
                te = te,
                flex = flex,
                dst = dst
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Roster))
                return false;

            var other = (Roster)obj;

            if (this.Except(other).Any())
                return false;

            if (other.Except(this).Any())
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = -52375223;
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(qb);
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(rb1);
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(rb2);
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(wr1);
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(wr2);
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(wr3);
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(te);
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(flex);
            hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(dst);
            return hashCode;
        }
    }
}
