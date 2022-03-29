using System;
using System.Collections.Generic;
using Elo_fotbalek.EloCounter;
using Elo_fotbalek.Models;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestCalculateElo_EqualTeam_BrutalWin_BigWieght()
        {
            var match = new Match()
            {
                Winner = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = (1234 + 1120 + 980 + 970) / 4
                },
                Looser = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = (1234 + 1120 + 980 + 970) / 4
                },
                Date = DateTime.Now,
                WinnerAmount = 12,
                LooserAmount = 5,
                Weight = 30
            };

            var eloCalultor = new FifaEloCounter();
            var fifaEloResult = eloCalultor.CalculateFifaElo(match);
        }

        [Test]
        public void TestCalculateElo_EqualTeam_BrutalWin_SmallWeight()
        {
            var match = new Match()
            {
                Winner = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = (1234 + 1120 + 980 + 970) / 4
                },
                Looser = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = (1234 + 1120 + 980 + 970) / 4
                },
                Date = DateTime.Now,
                WinnerAmount = 12,
                LooserAmount = 5,
                Weight = 10
            };

            var eloCalultor = new FifaEloCounter();
            var fifaEloResult = eloCalultor.CalculateFifaElo(match);
        }

        [Test]
        public void TestCalculateElo_EqualTeam_SmallWin_BigWieght()
        {
            var match = new Match()
            {
                Winner = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = (1234 + 1120 + 980 + 970) / 4
                },
                Looser = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = (1234 + 1120 + 980 + 970) / 4
                },
                Date = DateTime.Now,
                WinnerAmount = 1,
                LooserAmount = 0,
                Weight = 30
            };

            var eloCalultor = new FifaEloCounter();
            var fifaEloResult = eloCalultor.CalculateFifaElo(match);
        }

        [Test]
        public void TestCalculateElo_NotTeam_BiggerWin_BetterWins_BigWieght()
        {
            var match = new Match()
            {
                Winner = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1052
                },
                Looser = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1011
                },
                Date = DateTime.Now,
                WinnerAmount = 12,
                LooserAmount = 7,
                Weight = 30
            };

            var eloCalultor = new FifaEloCounter();
            var fifaEloResult = eloCalultor.CalculateFifaElo(match);
        }

        [Test]
        public void TestCalculateElo_NotEqualTeam_BiggerWin_WorseWins_BigWieght()
        {
            var match = new Match()
            {
                Winner = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1002
                },
                Looser = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1011
                },
                Date = DateTime.Now,
                WinnerAmount = 12,
                LooserAmount = 7,
                Weight = 30
            };

            var eloCalultor = new FifaEloCounter();
            var fifaEloResult = eloCalultor.CalculateFifaElo(match);
        }

        [Test]
        public void TestCalculateElo_EqualTeam_Tie_BigWieght()
        {
            var match = new Match()
            {
                Winner = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1052
                },
                Looser = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1052
                },
                Date = DateTime.Now,
                WinnerAmount = 7,
                LooserAmount = 7,
                Weight = 30
            };

            var eloCalultor = new FifaEloCounter();
            var fifaEloResult = eloCalultor.CalculateFifaElo(match);
        }

        [Test]
        public void TestCalculateElo_NotEqualTeam_Tie_BigWieght()
        {
            var match = new Match()
            {
                Winner = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1052
                },
                Looser = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1011
                },
                Date = DateTime.Now,
                WinnerAmount = 7,
                LooserAmount = 7,
                Weight = 30
            };

            var eloCalultor = new FifaEloCounter();
            var fifaEloResult = eloCalultor.CalculateFifaElo(match);
        }

        [Test]
        public void TestCalculateElo_NotEqualTeam_WorseSetAsWinner_Tie_BigWieght()
        {
            var match = new Match()
            {
                Winner = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1011
                },
                Looser = new Team()
                {
                    Players = new List<Player>()
                    {
                        new Player()
                        {
                            Name = "A",
                            Elo = 1234
                        },

                        new Player()
                        {
                            Name = "B",
                            Elo = 1120
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 980
                        },

                        new Player()
                        {
                            Name = "C",
                            Elo = 970
                        },
                    },
                    TeamElo = 1052
                },
                Date = DateTime.Now,
                WinnerAmount = 7,
                LooserAmount = 7,
                Weight = 30
            };

            var eloCalultor = new FifaEloCounter();
            var fifaEloResult = eloCalultor.CalculateFifaElo(match);
        }
    }
}