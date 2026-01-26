import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { playersService, configService } from '@/services/apiService';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Loading } from '@/components/ui/Loading';
import { ErrorDisplay } from '@/components/ui/ErrorDisplay';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export function PlayerDetailPage() {
  const { id } = useParams<{ id: string }>();
  
  const { data: configResponse } = useQuery({
    queryKey: ['config'],
    queryFn: configService.get,
  });

  const { data: playerResponse, isLoading, error } = useQuery({
    queryKey: ['player', id],
    queryFn: () => playersService.get(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return <Loading message="Načítání statistik hráče..." />;
  }

  if (error) {
    return (
      <ErrorDisplay error={error as Error} message="Nepodařilo se načíst hráče">
        <Link
          to="/"
          className="mt-2 px-4 py-2 bg-primary text-primary-foreground rounded-md inline-block"
        >
          Zpět na pořadí
        </Link>
      </ErrorDisplay>
    );
  }

  // Unwrap API response
  const config = (configResponse as any)?.data;
  const data = (playerResponse as any)?.data;

  if (!data || !data.player) {
    return (
      <ErrorDisplay 
        error={new Error('Hráč nenalezen')} 
        message="Nepodařilo se načíst data hráče"
      >
        <Link
          to="/"
          className="mt-2 px-4 py-2 bg-primary text-primary-foreground rounded-md inline-block"
        >
          Zpět na pořadí
        </Link>
      </ErrorDisplay>
    );
  }

  const { player, stats } = data;
  const isSeasoningSupported = config?.isSeasoningSupported ?? false;

  // Transform Elo history for chart
  const chartData = stats.eloHistory.map((point: any) => ({
    date: new Date(point.date).toLocaleDateString('cs-CZ', { 
      day: '2-digit', 
      month: '2-digit', 
      year: '2-digit' 
    }),
    elo: point.elo,
  }));

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between bg-white/80 backdrop-blur-sm p-4 rounded-lg shadow-sm">
        <div>
          <Link to="/" className="text-sm text-primary hover:underline mb-2 block">
            ← Zpět na pořadí
          </Link>
          <h1 className="text-3xl font-bold">{player.name}</h1>
        </div>
        <div className="text-right">
          <div className="text-sm text-muted-foreground">Aktuální Elo</div>
          <div className="text-4xl font-bold">{player.elo}</div>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* Overall Stats */}
        <Card className="bg-white/85 backdrop-blur-sm shadow-lg">
          <CardHeader>
            <CardTitle className="text-lg">Celková statistika</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Zápasy:</span>
                <span className="font-semibold">{player.matchesPlayed}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-green-600">Výhry:</span>
                <span className="font-semibold">{player.wins}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-red-600">Prohry:</span>
                <span className="font-semibold">{player.losses}</span>
              </div>
              {player.ties > 0 && (
                <div className="flex justify-between">
                  <span className="text-gray-600">Remízy:</span>
                  <span className="font-semibold">{player.ties}</span>
                </div>
              )}
              <div className="flex justify-between pt-2 border-t">
                <span className="text-muted-foreground">Účast za posledních 6 měsíců:</span>
                <span className="font-semibold">{player.percentage}%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Celková účast:</span>
                <span className="font-semibold">{player.totalPercentage}%</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Elo Stats */}
        <Card className="bg-white/85 backdrop-blur-sm shadow-lg">
          <CardHeader>
            <CardTitle className="text-lg">Elo statistiky</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Aktuální:</span>
                <span className="font-semibold">{player.elo}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-green-600">Maximum:</span>
                <span className="font-semibold">{stats.highestElo}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-red-600">Minimum:</span>
                <span className="font-semibold">{stats.lowestElo}</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Seasonal Elo */}
        {isSeasoningSupported && (
          <Card className="bg-white/85 backdrop-blur-sm shadow-lg">
            <CardHeader>
              <CardTitle className="text-lg">Sezónní Elo</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="text-orange-600">Letní:</span>
                  <span className="font-semibold">{player.summerElo}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-blue-600">Zimní:</span>
                  <span className="font-semibold">{player.winterElo}</span>
                </div>
                <div className="flex justify-between pt-2 border-t">
                  <span className="text-muted-foreground">Celkové:</span>
                  <span className="font-semibold">{player.overallElo}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Elo Chart */}
      {chartData.length > 0 && (
        <Card className="bg-white/85 backdrop-blur-sm shadow-lg">
          <CardHeader>
            <CardTitle>Vývoj Elo</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-64 md:h-80">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={chartData} margin={{ top: 5, right: 20, bottom: 20, left: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis 
                    dataKey="date" 
                    tick={{ fontSize: 10 }}
                    angle={-45}
                    textAnchor="end"
                    height={60}
                    interval="preserveStartEnd"
                  />
                  <YAxis 
                    domain={[stats.lowestElo - 50, stats.highestElo + 50]}
                    tick={{ fontSize: 11 }}
                    width={50}
                  />
                  <Tooltip 
                    contentStyle={{ 
                      backgroundColor: 'rgba(255, 255, 255, 0.95)', 
                      border: '1px solid #e5e7eb',
                      borderRadius: '6px',
                      fontSize: '12px'
                    }}
                  />
                  <Line 
                    type="monotone" 
                    dataKey="elo" 
                    stroke="#3b82f6" 
                    strokeWidth={2}
                    dot={false}
                    activeDot={{ r: 4 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
