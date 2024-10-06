namespace Elo_fotbalek.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Elo_fotbalek.Configuration;
    using Elo_fotbalek.EloCounter;
    using Elo_fotbalek.Models;
    using Elo_fotbalek.Storage;
    using Elo_fotbalek.TrendCalculator;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    public class AdminController : Controller
    {
        private readonly IBlobClient blobClient;
        private readonly IEloCalulator eloCalulator;
        private readonly ITrendCalculator trendCalculator;
        private readonly IOptions<AppConfigurationOptions> appConfiguration;


        public AdminController(IBlobClient blobClient, IEloCalulator eloCalulator, ITrendCalculator trendCalculator, IOptions<AppConfigurationOptions> appConfiguration)
        {
            this.blobClient = blobClient;
            this.eloCalulator = eloCalulator;
            this.trendCalculator = trendCalculator;
            this.appConfiguration = appConfiguration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginModel() { AppConfiguration = this.appConfiguration.Value});
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel user)
        {
            var passwd = (await this.blobClient.GetUsers()).FirstOrDefault(u => u.Name == "Admin")?.Password;

            if (!(passwd is null) && user.Password == passwd)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim("FullName", "Admin"),
                    new Claim(ClaimTypes.Role, "Administrator")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {

                    RedirectUri = "/Home/Index"

                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);


                return RedirectToAction("Index", "Home");
            }

            return Redirect("/Accout/Error");
        }


        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public bool IsLogedIn()
        {
           return HttpContext.User?.Identity?.Name != null;
        }

        public async Task<IActionResult> RecalculateToNewElo()
        {
            //reset to default value
            var players = await blobClient.GetPlayers();

            foreach (var player in players)
            {
                player.Elo = 1000;
            }

            await blobClient.UpdatePlayers(players);

            var matches = await blobClient.GetMatches();

            foreach (var match in matches.OrderBy(m => m.Date))
            {
                await blobClient.RemoveMatch(match);

                var newPlayers = await blobClient.GetPlayers();

                var teamElo = 0;
                foreach (var player in match.Winner.Players)
                {
                    player.Elo = newPlayers.First(p => p.Id == player.Id).Elo;
                    teamElo += player.Elo;
                }

                match.Winner.TeamElo = teamElo / match.Winner.Players.Count;

                teamElo = 0;
                foreach (var player in match.Looser.Players)
                {
                    player.Elo = newPlayers.First(p => p.Id == player.Id).Elo;
                    teamElo += player.Elo;
                }

                match.Looser.TeamElo = teamElo / match.Looser.Players.Count;

                await blobClient.AddMatch(match);

                var eloResult = eloCalulator.CalculateFifaElo(match);

                foreach (var player in match.Winner.Players)
                {
                    var newPlayerWinner = newPlayers.First(np => np.Id == player.Id);
                    newPlayerWinner.Elo += (int)eloResult.WinnerPointChange;
                }

                foreach (var player in match.Looser.Players)
                {
                    var newPlayerLooser = newPlayers.First(np => np.Id == player.Id);
                    newPlayerLooser.Elo += (int)eloResult.LooserPointChange;
                }

                await blobClient.UpdatePlayers(newPlayers);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CountWictoriesAndLooses()
        {
            var players = await blobClient.GetPlayers();

            var matches = await blobClient.GetMatches();

            foreach (var match in matches)
            {
                foreach (var player in players)
                {
                    if (match.Winner.Players.Contains(player))
                    {
                        if (player.AmountOfWins == null)
                        {
                            player.AmountOfWins = new MatchCounter();
                        }
                        if (match.Weight == 30)
                        {
                            player.AmountOfWins.BigMatches++;
                        }
                        else
                        {
                            player.AmountOfWins.SmallMatches++;
                        }
                        
                        
                    }
                    else if (match.Looser.Players.Contains(player))
                    {
                        if (player.AmountOfLooses == null)
                        {
                            player.AmountOfLooses = new MatchCounter();
                        }
                        if (match.Weight == 30)
                        {
                            player.AmountOfLooses.BigMatches++;
                        }
                        else
                        {
                            player.AmountOfLooses.SmallMatches++;
                        }
                    }
                }
                
            }

            await blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> AddTrendData()
        {
            var matches = await blobClient.GetMatches();
            var matchesOrdered = matches.OrderByDescending(m => m.Date);

            var players = await blobClient.GetPlayers();

            foreach (var player in players)
            {
                player.Trend = new TrendData
                {
                    Data = new Dictionary<DateTime, int>(),
                    Trend = Trend.STAY
                };

                var matchCount = 0;

                foreach (var match in matchesOrdered)
                {
                    if (matchCount < 5)
                    {
                        if (match.Winner.Players.Contains(player))
                        {
                            player.Trend = trendCalculator.CalculateTrend(player.Trend, match.Date, 1);
                            matchCount++;
                        }
                        else if (match.Looser.Players.Contains(player))
                        {
                            player.Trend = trendCalculator.CalculateTrend(player.Trend, match.Date, 0);
                            matchCount++;
                        }
                    }                   
                }
            }

            await blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> IntroduceSeasonalElo()
        {
            var players = await blobClient.GetPlayers();

            foreach (var player in players)
            {
                player.Elos = new SeasonalElos
                {
                    SummerElo = player.Elo,
                    WinterElo = 1000
                };
            }

            await blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CalculateMissingMatches()
        {
            var players = await blobClient.GetPlayers();
            var matches = await blobClient.GetMatches();

            var sortedMatches = matches.OrderBy(x => x.Date);
            foreach (var match in sortedMatches)
            {
                foreach (var player in players)
                {
                    if (!match.Winner.Players.Contains(player) && !match.Looser.Players.Contains(player))
                    {
                        player.AmountOfMissedGames++;
                    }
                    else
                    {
                        player.AmountOfMissedGames = 0;
                    }
                }    
            }

            await blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ReCalculateTrend()
        {
            var players = await blobClient.GetPlayers();

            foreach (var player in players)
            {
                if (player.AmountOfMissedGames >= 5)
                {
                    player.Trend.Trend = Trend.STAY;
                }
            }

            await blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> IntroducePercentage()
        {
            var players = await this.blobClient.GetPlayers();
            var matches = await this.blobClient.GetMatches();

            var countedMatches = matches.Where(m => m.Date >= DateTime.Now.AddMonths(-this.appConfiguration.Value.AmountOfMonthsToBeCounted));

            var playersPercInSelectedPeriod = new Dictionary<string, int>();
            foreach (var countedMatch in countedMatches)
            {
                foreach (var player in countedMatch.GetAllPlayers())
                {
                    if (playersPercInSelectedPeriod.ContainsKey(player.Id.ToString()))
                    {
                        playersPercInSelectedPeriod[player.Id.ToString()]++;
                    }
                    else
                    {
                        playersPercInSelectedPeriod.Add(player.Id.ToString(), 1);
                    }
                }
            }


            foreach (var player in players)
            {
                var totalAmountOfPlayedMatches = (player.AmountOfLooses?.TotalAmount() ?? 0) + (player.AmountOfWins?.TotalAmount() ?? 0);
                player.TotalPercentage = (int)(((double)totalAmountOfPlayedMatches / (double)matches.Count) * 100);
                player.Percentage =
                        playersPercInSelectedPeriod.ContainsKey(player.Id.ToString())
                        ? (int)(((double)playersPercInSelectedPeriod[player.Id.ToString()] / (double)countedMatches.Count()) * 100)
                        : 0;
            }

            await this.blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }
    }
}