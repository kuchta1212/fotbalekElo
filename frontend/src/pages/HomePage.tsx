import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { leaderboardService, configService } from '@/services/apiService';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Loading } from '@/components/ui/Loading';
import { ErrorDisplay } from '@/components/ui/ErrorDisplay';
import type { Season } from '@/types/domain';

export function HomePage() {
  const [selectedSeason, setSelectedSeason] = useState<Season>('Overall');

  const { data: configResponse } = useQuery({
    queryKey: ['config'],
    queryFn: configService.get,
  });

  const { data: leaderboardResponse, isLoading, error } = useQuery({
    queryKey: ['leaderboard', selectedSeason],
    queryFn: () => leaderboardService.get(selectedSeason),
  });

  if (isLoading) {
    return <Loading message="Načítání pořadí..." />;
  }

  if (error) {
    return (
      <ErrorDisplay error={error as Error} message="Nepodařilo se načíst pořadí">
        <button
          onClick={() => window.location.reload()}
          className="mt-2 px-4 py-2 bg-primary text-primary-foreground rounded-md"
        >
          Zkusit znovu
        </button>
      </ErrorDisplay>
    );
  }

  // Debug: Log the actual response structure
  console.log('Config Response:', configResponse);
  console.log('Leaderboard Response:', leaderboardResponse);

  // Unwrap the API response (BaseApiController wraps with {success, data})
  const config = (configResponse as any)?.data;
  const leaderboard = (leaderboardResponse as any)?.data;

  console.log('Unwrapped Config:', config);
  console.log('Unwrapped Leaderboard:', leaderboard);

  // Add safety checks
  if (!leaderboard) {
    console.error('Leaderboard data is missing:', leaderboardResponse);
    return (
      <ErrorDisplay 
        error={new Error('No data received')} 
        message="Nepodařilo se načíst data pořadí"
      >
        <button
          onClick={() => window.location.reload()}
          className="mt-2 px-4 py-2 bg-primary text-primary-foreground rounded-md"
        >
          Zkusit znovu
        </button>
      </ErrorDisplay>
    );
  }

  const isSeasoningSupported = config?.isSeasoningSupported ?? false;
  const regulars = leaderboard.regulars || [];
  const nonRegulars = leaderboard.nonRegulars || [];
  const recentMatches = leaderboard.recentMatches || [];

  console.log('Regulars:', regulars);
  console.log('NonRegulars:', nonRegulars);

  return (
    <div className="space-y-6">
      {/* Header with Season Selector */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white/80 backdrop-blur-sm p-4 rounded-lg shadow-sm">
        <h1 className="text-3xl font-bold">Pořadí</h1>
        
        {isSeasoningSupported && (
          <div className="flex gap-2">
            <button
              onClick={() => setSelectedSeason('Overall')}
              className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                selectedSeason === 'Overall'
                  ? 'bg-primary text-primary-foreground'
                  : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
              }`}
            >
              Celkové
            </button>
            <button
              onClick={() => setSelectedSeason('Summer')}
              className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                selectedSeason === 'Summer'
                  ? 'bg-orange-600 text-white'
                  : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
              }`}
            >
              Letní
            </button>
            <button
              onClick={() => setSelectedSeason('Winter')}
              className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                selectedSeason === 'Winter'
                  ? 'bg-blue-600 text-white'
                  : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
              }`}
            >
              Zimní
            </button>
          </div>
        )}
      </div>

      {/* Regular Players Leaderboard */}
      <Card className="bg-white/85 backdrop-blur-sm transition-all duration-300 hover:bg-white/98 shadow-lg">
        <CardHeader>
          <CardTitle>Pravidelní hráči</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            {regulars.map((player: any) => (
              <Link
                key={player.id}
                to={`/players/${player.id}`}
                className="block p-4 rounded-lg border border-gray-200 bg-white/60 hover:bg-white/95 hover:shadow-md backdrop-blur-sm transition-all duration-200"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3 flex-1">
                    <span className="text-2xl font-bold text-muted-foreground w-8">
                      {player.rank}
                    </span>
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold">{player.name}</span>
                        {player.trend === 'up' && (
                          <span className="text-green-600 text-lg font-bold" title="Vzestupný trend">↑</span>
                        )}
                        {player.trend === 'down' && (
                          <span className="text-red-600 text-lg font-bold" title="Sestupný trend">↓</span>
                        )}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        <span className="text-green-600">{player.wins}V</span>
                        {' · '}
                        <span className="text-red-600">{player.losses}P</span>
                        {player.ties > 0 && (
                          <>
                            {' · '}
                            <span className="text-gray-600">{player.ties}R</span>
                          </>
                        )}
                        {' · '}
                        {player.percentage}% účast
                      </div>
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="text-2xl font-bold">{player.elo}</div>
                    {isSeasoningSupported && selectedSeason === 'Overall' && (
                      <div className="text-xs text-muted-foreground">
                        L:{player.summerElo} Z:{player.winterElo}
                      </div>
                    )}
                  </div>
                </div>
              </Link>
            ))}
            {regulars.length === 0 && (
              <p className="text-center text-muted-foreground py-8">
                Zatím žádní pravidelní hráči
              </p>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Non-Regular Players (Lazy Bitches) */}
      {nonRegulars.length > 0 && (
        <Card className="bg-white/85 backdrop-blur-sm transition-all duration-300 hover:bg-white/98 shadow-lg">
          <CardHeader>
            <CardTitle>
              {leaderboard.nonRegularsTitle || 'Lemry líný'} (méně než 30% zápasů)
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {nonRegulars.map((player: any) => (
                <Link
                  key={player.id}
                  to={`/players/${player.id}`}
                  className="block p-4 rounded-lg border border-gray-200 bg-white/60 hover:bg-white/95 hover:shadow-md backdrop-blur-sm transition-all duration-200"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3 flex-1">
                      <span className="text-xl text-muted-foreground w-8">
                        {player.rank}
                      </span>
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <span className="font-medium">{player.name}</span>
                          {player.trend === 'up' && (
                            <span className="text-green-600 text-base font-bold" title="Vzestupný trend">↑</span>
                          )}
                          {player.trend === 'down' && (
                            <span className="text-red-600 text-base font-bold" title="Sestupný trend">↓</span>
                          )}
                        </div>
                        <div className="text-sm text-muted-foreground">
                          <span className="text-green-600">{player.wins}V</span>
                          {' · '}
                          <span className="text-red-600">{player.losses}P</span>
                          {' · '}
                          {player.percentage}% účast
                        </div>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-xl font-bold">{player.elo}</div>
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Recent Matches */}
      {recentMatches.length > 0 && (
        <Card id="matches" className="bg-white/85 backdrop-blur-sm transition-all duration-300 hover:bg-white/98 shadow-lg scroll-mt-20">
          <CardHeader>
            <div className="flex justify-between items-center">
              <CardTitle>Seznam zápasů</CardTitle>
              <Link
                to="/matches"
                className="text-sm text-primary hover:underline"
              >
                Zobrazit vše
              </Link>
            </div>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentMatches.slice(0, 5).map((match: any) => (
                <div
                  key={match.id}
                  className="p-4 rounded-lg border border-gray-200 bg-white/60 hover:bg-white/95 hover:shadow-md backdrop-blur-sm transition-all duration-200"
                >
                  <div className="flex justify-between items-start mb-2">
                    <div>
                      <div className="font-bold text-lg">
                        {match.score}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        {new Date(match.date).toLocaleDateString('cs-CZ')}
                        {match.isSmallMatch && ' · Malý zápas'}
                        {isSeasoningSupported && ` · ${match.season === 'Summer' ? 'Letní' : 'Zimní'}`}
                      </div>
                    </div>
                  </div>
                  
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <div className="font-medium text-green-600 mb-1">
                        Vítězové (Elo: {match.winner.teamElo})
                      </div>
                      <div className="space-y-1">
                        {match.winner.players.map((p: any) => (
                          <div key={p.id} className="text-muted-foreground">
                            {p.name} ({p.elo})
                          </div>
                        ))}
                      </div>
                    </div>
                    
                    <div>
                      <div className="font-medium text-red-600 mb-1">
                        Poražení (Elo: {match.loser.teamElo})
                      </div>
                      <div className="space-y-1">
                        {match.loser.players.map((p: any) => (
                          <div key={p.id} className="text-muted-foreground">
                            {p.name} ({p.elo})
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>

                  {match.jirkaLunak && (
                    <div className="mt-2 pt-2 border-t text-sm text-muted-foreground">
                      <span className="font-medium">Jirka Luňák:</span> {match.jirkaLunak}
                    </div>
                  )}
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
