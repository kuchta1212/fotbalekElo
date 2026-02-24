import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { doodleService } from '@/services/apiService';
import { Loading } from '@/components/ui/Loading';
import { ErrorDisplay } from '@/components/ui/ErrorDisplay';
import { Button } from '@/components/ui/Button';
import type { UpdateAvailabilityRequest, DoodleDateDto, DoodlePlayerDto } from '@/types/api';

type DoodleStatus = 'Accept' | 'Maybe' | 'Refused' | 'NoAnswer';

const statusIcons: Record<DoodleStatus, string> = {
  Accept: '✓',
  Maybe: '?',
  Refused: '✗',
  NoAnswer: '—',
};

const statusColors: Record<DoodleStatus, string> = {
  Accept: 'bg-green-500/80 hover:bg-green-600/80',
  Maybe: 'bg-orange-500/80 hover:bg-orange-600/80',
  Refused: 'bg-red-500/80 hover:bg-red-600/80',
  NoAnswer: 'bg-gray-500/80 hover:bg-gray-600/80',
};

const statusLabels: Record<DoodleStatus, string> = {
  Accept: 'Přijde',
  Maybe: 'Možná',
  Refused: 'Nepřijde',
  NoAnswer: 'Bez odpovědi',
};

export function DoodlePage() {
const navigate = useNavigate();
const queryClient = useQueryClient();
const [selectedDate, setSelectedDate] = useState<string | null>(null);
const [selectedSeason, setSelectedSeason] = useState<'Summer' | 'Winter'>('Summer');
const [showAdminPrompt, setShowAdminPrompt] = useState(false);
const [adminPassword, setAdminPassword] = useState('');

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['doodle', 'upcoming'],
    queryFn: () => doodleService.upcoming(5),
    staleTime: 0, // Always fetch fresh data
    refetchOnMount: true, // Refetch when component mounts
  });

  const updateMutation = useMutation({
    mutationFn: ({ date, request }: { date: string; request: UpdateAvailabilityRequest }) =>
      doodleService.updateAvailability(date, request),
    onMutate: async ({ date, request }) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['doodle'] });

      // Snapshot previous value
      const previousData = queryClient.getQueryData(['doodle', 'upcoming']);

      // Optimistically update
      queryClient.setQueryData(['doodle', 'upcoming'], (old: any) => {
        if (!old?.data) return old;
        
        const newData = { ...old };
        const dates = [...newData.data.dates];
        const dateIndex = dates.findIndex((d: DoodleDateDto) => d.date === date);
        
        if (dateIndex !== -1) {
          const players = [...dates[dateIndex].players];
          const playerIndex = players.findIndex((p: DoodlePlayerDto) => p.name === request.playerName);
          
          if (playerIndex !== -1) {
            players[playerIndex] = { ...players[playerIndex], status: request.status };
            dates[dateIndex] = { ...dates[dateIndex], players };
            newData.data = { ...newData.data, dates };
            
            // Update stats
            const stats = { coming: 0, maybe: 0, refused: 0 };
            players.forEach((p: DoodlePlayerDto) => {
              if (p.status === 'Accept') stats.coming++;
              else if (p.status === 'Maybe') stats.maybe++;
              else if (p.status === 'Refused') stats.refused++;
            });
            newData.data.stats = stats;
          }
        }
        
        return newData;
      });

      return { previousData };
    },
    onError: (_err, _variables, context) => {
      // Rollback on error
      if (context?.previousData) {
        queryClient.setQueryData(['doodle', 'upcoming'], context.previousData);
      }
    },
    onSettled: () => {
      // Refetch to ensure consistency
      queryClient.invalidateQueries({ queryKey: ['doodle'] });
    },
  });

  const advancePollMutation = useMutation({
    mutationFn: (password: string) => 
      doodleService.advancePoll({ username: 'admin', password }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['doodle'] });
      setShowAdminPrompt(false);
      setAdminPassword('');
      alert('Doodle posunuto na další týden');
    },
    onError: (error: any) => {
      alert(error.message || 'Nepodařilo se posunout doodle. Zkontrolujte heslo.');
    },
  });

  if (isLoading) return <Loading message="Načítání doodle..." />;
  if (error) return (
    <ErrorDisplay error={error} message="Nepodařilo se načíst doodle">
      <Button onClick={() => refetch()}>Zkusit znovu</Button>
    </ErrorDisplay>
  );

  // Unwrap the API response (BaseApiController wraps with {success, data})
  const doodleData = (data as any)?.data;

  if (!doodleData || doodleData.dates.length === 0) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold mb-4">Žádné nadcházející termíny</h2>
        <p className="text-muted-foreground">Zatím nejsou naplánované žádné zápasy.</p>
      </div>
    );
  }

  const currentDate = selectedDate || doodleData.dates[0].date;
  const currentDateData = doodleData.dates.find((d: DoodleDateDto) => d.date === currentDate);

  if (!currentDateData) return null;

  const handleStatusClick = (playerName: string, currentStatus: DoodleStatus) => {
    // Cycle: Accept → Maybe → Refused → Accept
    // First click (from NoAnswer): Accept
    let nextStatus: DoodleStatus;
    
    if (currentStatus === 'NoAnswer') {
      nextStatus = 'Accept'; // First click = Accept
    } else {
      const statusCycle: DoodleStatus[] = ['Accept', 'Maybe', 'Refused'];
      const currentIndex = statusCycle.indexOf(currentStatus);
      nextStatus = statusCycle[(currentIndex + 1) % statusCycle.length];
    }

    // Check player limit when trying to accept
    if (nextStatus === 'Accept' && doodleData.playerLimit > 0) {
      if (doodleData.stats.coming >= doodleData.playerLimit) {
        alert(doodleData.overLimitMessage || `Kapacita naplněna (${doodleData.playerLimit} hráčů)`);
        return;
      }
    }

    updateMutation.mutate({
      date: currentDate,
      request: {
        playerName,
        status: nextStatus,
      },
    });
  };

  const handleGenerateTeams = () => {
    navigate(`/teams?date=${currentDate}&season=${selectedSeason}`);
  };

  const handleAdvancePoll = () => {
    if (!showAdminPrompt) {
      setShowAdminPrompt(true);
      return;
    }

    if (!adminPassword) {
      alert('Zadejte heslo');
      return;
    }

    advancePollMutation.mutate(adminPassword);
  };

  const handleCancelAdvance = () => {
    setShowAdminPrompt(false);
    setAdminPassword('');
  };

  return (
    <div className="space-y-6">
      {/* 1. STATS SUMMARY - Who's coming */}
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-6 shadow-lg">
        <h1 className="text-3xl font-bold mb-4 text-gray-900">Doodle - Docházka</h1>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="bg-green-500/30 backdrop-blur-sm rounded-lg p-4 border border-green-500/40">
            <div className="text-sm text-green-800 font-medium">Přihlášených</div>
            <div className="text-3xl font-bold text-green-900">{doodleData.stats.coming}</div>
          </div>
          <div className="bg-orange-500/30 backdrop-blur-sm rounded-lg p-4 border border-orange-500/40">
            <div className="text-sm text-orange-800 font-medium">Možná</div>
            <div className="text-3xl font-bold text-orange-900">{doodleData.stats.maybe}</div>
          </div>
          <div className="bg-red-500/30 backdrop-blur-sm rounded-lg p-4 border border-red-500/40">
            <div className="text-sm text-red-800 font-medium">Odmítlo</div>
            <div className="text-3xl font-bold text-red-900">{doodleData.stats.refused}</div>
          </div>
        </div>

        {doodleData.playerLimit > 0 && doodleData.stats.coming >= doodleData.playerLimit && (
          <div className="bg-yellow-500/30 backdrop-blur-sm rounded-lg p-4 border border-yellow-500/50 mt-4">
            <p className="text-yellow-900 font-medium">
              {doodleData.overLimitMessage || `Kapacita naplněna (${doodleData.playerLimit} hráčů)`}
            </p>
          </div>
        )}
      </div>

      {/* 2. GENERATE TEAMS - Compact one-line layout */}
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 shadow-lg">
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div className="flex-1">
            <h2 className="text-lg font-bold text-gray-900">Vygenerovat týmy pro {currentDateData.displayDate}</h2>
          </div>
          
          <div className="flex items-center gap-3">
            {doodleData.isSeasoningSupported && (
              <div className="flex gap-2">
                <button
                  onClick={() => setSelectedSeason('Summer')}
                  className={`px-3 py-2 rounded-lg font-medium transition-colors text-sm ${
                    selectedSeason === 'Summer'
                      ? 'bg-orange-500 text-white'
                      : 'bg-white/60 hover:bg-white/80 text-gray-900 border border-gray-300'
                  }`}
                >
                  Léto
                </button>
                <button
                  onClick={() => setSelectedSeason('Winter')}
                  className={`px-3 py-2 rounded-lg font-medium transition-colors text-sm ${
                    selectedSeason === 'Winter'
                      ? 'bg-blue-500 text-white'
                      : 'bg-white/60 hover:bg-white/80 text-gray-900 border border-gray-300'
                  }`}
                >
                  Zima
                </button>
              </div>
            )}

            <button
              onClick={handleGenerateTeams}
              disabled={doodleData.stats.coming === 0}
              className="px-6 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-600 disabled:cursor-not-allowed text-white font-bold rounded-lg transition-colors shadow-md whitespace-nowrap"
            >
              Vygenerovat týmy
            </button>
          </div>
        </div>
        
        {doodleData.stats.coming === 0 && (
          <p className="text-sm text-gray-600 mt-2">
            Nejprve musí alespoň jeden hráč potvrdit účast
          </p>
        )}
      </div>

      {/* 3. DATE SELECTOR */}
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 shadow-lg">
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          {/* Date buttons */}
          <div className="flex flex-wrap gap-2 flex-1">
            {doodleData.dates.map((dateData: DoodleDateDto, index: number) => (
              <button
                key={dateData.date}
                onClick={() => setSelectedDate(dateData.date)}
                className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                  dateData.date === currentDate
                    ? 'bg-blue-600 text-white shadow-md'
                    : 'bg-white/60 hover:bg-white/80 text-gray-900 border border-gray-300'
                } ${index === 0 ? 'ring-2 ring-blue-400' : ''}`}
              >
                {dateData.displayDate}
                {index === 0 && ' (Nejbližší)'}
              </button>
            ))}
          </div>

          {/* Admin: Advance Poll Button */}
          <div className="flex items-center gap-2 flex-shrink-0 w-full sm:w-auto">
            {showAdminPrompt ? (
              <>
                <input
                  type="password"
                  value={adminPassword}
                  onChange={(e) => setAdminPassword(e.target.value)}
                  placeholder="Admin heslo"
                  className="px-3 py-2 rounded-lg border border-gray-300 text-gray-900 text-sm flex-1 sm:flex-initial"
                  onKeyDown={(e) => e.key === 'Enter' && handleAdvancePoll()}
                />
                <button
                  onClick={handleAdvancePoll}
                  disabled={advancePollMutation.isPending}
                  className="px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-600 text-white font-medium rounded-lg transition-colors text-sm whitespace-nowrap"
                >
                  {advancePollMutation.isPending ? '...' : 'Potvrdit'}
                </button>
                <button
                  onClick={handleCancelAdvance}
                  className="px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white font-medium rounded-lg transition-colors text-sm whitespace-nowrap"
                >
                  Zrušit
                </button>
              </>
            ) : (
              <button
                onClick={handleAdvancePoll}
                className="w-full sm:w-auto px-4 py-2 bg-orange-600 hover:bg-orange-700 text-white font-medium rounded-lg transition-colors shadow-md text-sm whitespace-nowrap"
                title="Admin: Posunout na další týden"
              >
                Další kolo →
              </button>
            )}
          </div>
        </div>
      </div>

      {/* 4. PLAYERS LIST */}
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-6 shadow-lg">
        <h2 className="text-xl font-bold mb-4 text-gray-900">
          Hráči - {currentDateData.displayDate}
        </h2>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
          {currentDateData.players.map((player: DoodlePlayerDto) => (
            <div
              key={player.name}
              className="flex items-center justify-between bg-white/70 rounded-lg p-3 hover:bg-white/90 transition-colors border border-gray-200 shadow-sm"
            >
              <span className="font-medium text-gray-900">{player.name}</span>
              <button
                onClick={() => handleStatusClick(player.name, player.status as DoodleStatus)}
                disabled={updateMutation.isPending}
                className={`px-3 py-1 rounded-md font-medium transition-all text-white shadow-sm ${
                  statusColors[player.status as DoodleStatus]
                } ${updateMutation.isPending ? 'opacity-50 cursor-not-allowed' : ''}`}
                title={statusLabels[player.status as DoodleStatus]}
              >
                <span className="text-lg">{statusIcons[player.status as DoodleStatus]}</span>
                <span className="ml-2 text-sm hidden sm:inline">
                  {statusLabels[player.status as DoodleStatus]}
                </span>
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
