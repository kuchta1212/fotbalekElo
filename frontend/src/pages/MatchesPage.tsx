import { useInfiniteQuery, useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { matchesService, configService } from '@/services/apiService';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Loading } from '@/components/ui/Loading';
import { ErrorDisplay } from '@/components/ui/ErrorDisplay';
import { Button } from '@/components/ui/Button';
import type { MatchDto } from '@/types/api';

const CZECH_MONTHS = [
  'Leden', 'Únor', 'Březen', 'Duben', 'Květen', 'Červen',
  'Červenec', 'Srpen', 'Září', 'Říjen', 'Listopad', 'Prosinec',
];

function getPreviousMonth(year: number, month: number) {
  if (month === 1) return { year: year - 1, month: 12 };
  return { year, month: month - 1 };
}

export function MatchesPage() {
  const { data: configResponse } = useQuery({
    queryKey: ['config'],
    queryFn: configService.get,
  });
  const isSeasoningSupported = (configResponse as any)?.data?.isSeasoningSupported ?? false;

  const {
    data,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery({
    queryKey: ['matches'],
    queryFn: async ({ pageParam }) => {
      const res = await matchesService.list(pageParam?.year, pageParam?.month);
      return (res as any)?.data as { matches: MatchDto[]; year: number; month: number; hasMore: boolean };
    },
    initialPageParam: undefined as { year: number; month: number } | undefined,
    getNextPageParam: (lastPage) => {
      if (!lastPage?.hasMore) return undefined;
      return getPreviousMonth(lastPage.year, lastPage.month);
    },
  });

  if (isLoading) return <Loading />;
  if (error) return <ErrorDisplay error={error} message="Nepodařilo se načíst zápasy" />;

  const pages = data?.pages ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Zápasy</h1>
        <Link to="/" className="text-sm text-primary hover:underline">
          Zpět na přehled
        </Link>
      </div>

      {pages.map((page) => (
        <Card key={`${page.year}-${page.month}`}>
          <CardHeader>
            <CardTitle>
              {CZECH_MONTHS[page.month - 1]} {page.year}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {page.matches.length === 0 ? (
              <p className="text-muted-foreground text-sm">Žádné zápasy v tomto měsíci</p>
            ) : (
              <div className="space-y-4">
                {page.matches.map((match) => (
                  <MatchCard
                    key={match.id}
                    match={match}
                    isSeasoningSupported={isSeasoningSupported}
                  />
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      ))}

      {hasNextPage && (
        <div className="flex justify-center">
          <Button
            variant="outline"
            onClick={() => fetchNextPage()}
            disabled={isFetchingNextPage}
          >
            {isFetchingNextPage ? 'Načítání...' : 'Načíst další'}
          </Button>
        </div>
      )}

      {!hasNextPage && pages.length > 0 && (
        <p className="text-center text-sm text-muted-foreground">Žádné starší zápasy</p>
      )}
    </div>
  );
}

function MatchCard({ match, isSeasoningSupported }: { match: MatchDto; isSeasoningSupported: boolean }) {
  return (
    <div className="p-4 rounded-lg border border-gray-200 bg-white/60 hover:bg-white/95 hover:shadow-md backdrop-blur-sm transition-all duration-200">
      <div className="flex justify-between items-start mb-2">
        <div>
          <div className="font-bold text-lg">{match.score}</div>
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
            {match.winner.players.map((p) => (
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
            {match.loser.players.map((p) => (
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
  );
}
