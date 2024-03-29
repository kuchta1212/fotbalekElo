﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.TeamGenerator
{
    public class TeamGenerator : ITeamGenerator
    {
        private List<Team> result = new List<Team>();
        private List<Player> allPlayers;
        private int amountOfPlayers;
        private int totalAmountOfPlayers;
        private Season season;
        private IModelCreator modelCreator;

        public TeamGenerator(IModelCreator modelCreator)
        {
            this.modelCreator = modelCreator;
        }

        public Task<List<GeneratorResult>> GenerateTeams(List<string> playerIds, Season season)
        {
            return this.GenerateTeams(playerIds, new List<string>(), season);
        }

        public async Task<List<GeneratorResult>> GenerateTeams(List<string> playerIds, List<string> substitudeIds, Season season)
        {
            var dummyTeamPlayers = await this.modelCreator.CreateTeam(playerIds, season);
            var dummyTeamSubstitudes = await this.modelCreator.CreateTeam(substitudeIds, season);

            dummyTeamSubstitudes.Players.ForEach(s => s.Name += "-Náhradník");
            dummyTeamPlayers.Players.AddRange(dummyTeamSubstitudes.Players);

            this.allPlayers = dummyTeamPlayers.Players;
            this.totalAmountOfPlayers = this.allPlayers.Count;
            this.amountOfPlayers = this.allPlayers.Count / 2;
            this.season = season;

            this.Generate(0, 0, new List<Player>());

            var result = this.SelectBestOption();
            return result.First().Value;
        }

        public SortedList<int, List<GeneratorResult>> SelectBestOption()
        {
            var orderedResultList = new SortedList<int, List<GeneratorResult>>();
            foreach (var firstTeam in this.result)
            {
                var secondTeam = this.GetSecondTeam(firstTeam);
                var eloDiff = Math.Abs(firstTeam.TeamElo - secondTeam.TeamElo);
                if (orderedResultList.ContainsKey(eloDiff))
                {
                    orderedResultList.TryGetValue(eloDiff, out var value);
                    value.Add(new GeneratorResult() {TeamOne = firstTeam, TeamTwo = secondTeam, EloDiff = eloDiff, Season = this.season});
                }
                else
                {
                    var list = new List<GeneratorResult>
                    {
                        new GeneratorResult() {TeamOne = firstTeam, TeamTwo = secondTeam, EloDiff = eloDiff, Season = this.season}
                    };
                    orderedResultList.Add(eloDiff, list);
                }
            }

            return orderedResultList;
        }

        private Team GetSecondTeam(Team firstTeam)
        {
            var players = allPlayers.Where(p => !firstTeam.Players.Contains(p)).ToList();
            return new Team()
            {
                Players = players,
                TeamElo = Team.CalculateTeamElo(players, this.season)
            };
        }


        public bool Generate(int index, int depth, List<Player> temp)
        {
            if (depth == this.amountOfPlayers)
            {
                return true;
            }

            for (var i = index; i < totalAmountOfPlayers; i++)
            {
                temp.Add(this.allPlayers[i]);
                if (this.Generate(i + 1, depth + 1, temp))
                {
                    if (this.IsNotComplement(temp))
                    {
                        var tmp = new Player[amountOfPlayers];
                        temp.CopyTo(tmp);
                        this.result.Add(new Team()
                        {
                            Players = tmp.ToList(),
                            TeamElo = Team.CalculateTeamElo(tmp.ToList(), this.season)
                        });
                    }
                }

                temp.RemoveAt(depth);
            }

            return false;
        }

        /// <summary>
        /// In the end I will need two teams
        /// The generate method will generate every possibility of team
        /// But when I have team (1,2,3) of total 6 players, I do not need to generate another team (4,5,6), its natural opossite
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        private bool IsNotComplement(List<Player> players)
        {
            var tempTeam = new Team()
            {
                Players = players
            };

            var firstTeam = this.GetSecondTeam(tempTeam);

            return !this.result.Contains(firstTeam);
        }
    }
}
