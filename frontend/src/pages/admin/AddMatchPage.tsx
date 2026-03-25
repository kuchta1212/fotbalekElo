import { useState, useEffect } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useNavigate, useLocation } from 'react-router-dom';
import { matchesService, adminService, configService } from '@/services/apiService';
import { Loading } from '@/components/ui/Loading';
import { ErrorDisplay } from '@/components/ui/ErrorDisplay';
import { Button } from '@/components/ui/Button';
import type { MatchPlayerOptionDto, AddMatchRequest } from '@/types/api';

const EMPTY_ID = '00000000-0000-0000-0000-000000000000';

interface PrefilledState {
  teamOneIds?: string[];
  teamTwoIds?: string[];
  season?: string;
}

export function AddMatchPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const prefilled = location.state as PrefilledState | null;

  const [winnerScore, setWinnerScore] = useState(0);
  const [loserScore, setLoserScore] = useState(0);
  const [weight, setWeight] = useState<'BigMatch' | 'SmallMatch'>('BigMatch');
  const [season, setSeason] = useState(prefilled?.season || 'Summer');
  const [heroId, setHeroId] = useState('');
  const [winnerIds, setWinnerIds] = useState<string[]>(prefilled?.teamOneIds ?? [EMPTY_ID]);
  const [loserIds, setLoserIds] = useState<string[]>(prefilled?.teamTwoIds ?? [EMPTY_ID]);
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [initialized, setInitialized] = useState(!prefilled);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [showScoreConfirm, setShowScoreConfirm] = useState(false);

  // Ensure prefilled IDs are applied after players load
  useEffect(() => {
    if (prefilled && !initialized) {
      if (prefilled.teamOneIds?.length) setWinnerIds(prefilled.teamOneIds);
      if (prefilled.teamTwoIds?.length) setLoserIds(prefilled.teamTwoIds);
      if (prefilled.season) setSeason(prefilled.season);
      setInitialized(true);
    }
  }, [prefilled, initialized]);

  const { data: playersData, isLoading: playersLoading, error: playersError } = useQuery({
    queryKey: ['match-players'],
    queryFn: matchesService.getPlayersForMatch,
  });

  const { data: configResponse } = useQuery({
    queryKey: ['config'],
    queryFn: configService.get,
  });

  const config = (configResponse as any)?.data;

  const addMatchMutation = useMutation({
    mutationFn: (request: AddMatchRequest) =>
      adminService.addMatch(request, { username: 'admin', password }),
    onSuccess: (response: any) => {
      const data = response?.data || response;
      alert(`Zápas přidán! Skóre: ${data.score}\nZměna ELO vítězů: +${data.winnerEloChange}\nZměna ELO poražených: ${data.loserEloChange}`);
      navigate('/');
    },
    onError: (err: any) => {
      if (err.status === 401) {
        setError('Neplatné heslo');
      } else {
        setError(err.data?.error || err.message || 'Nepodařilo se přidat zápas');
      }
    },
  });

  const deleteLastMatchMutation = useMutation({
    mutationFn: () =>
      adminService.deleteLastMatch({ username: 'admin', password }),
    onSuccess: (response: any) => {
      const data = response?.data || response;
      alert(`Poslední zápas smazán! Skóre: ${data.deletedMatchScore}`);
      setShowDeleteConfirm(false);
      navigate('/');
    },
    onError: (err: any) => {
      if (err.status === 401) {
        setError('Neplatné heslo');
      } else {
        setError(err.data?.error || err.message || 'Nepodařilo se smazat zápas');
      }
      setShowDeleteConfirm(false);
    },
  });

  if (playersLoading) return <Loading message="Načítání hráčů..." />;
  if (playersError) return <ErrorDisplay error={playersError as Error} message="Nepodařilo se načíst hráče" />;

  const players: MatchPlayerOptionDto[] = (playersData as any)?.data?.players || playersData?.players || [];

  const updatePlayerSlot = (
    team: 'winner' | 'loser',
    index: number,
    value: string
  ) => {
    if (team === 'winner') {
      const updated = [...winnerIds];
      updated[index] = value;
      setWinnerIds(updated);
    } else {
      const updated = [...loserIds];
      updated[index] = value;
      setLoserIds(updated);
    }
  };

  const addSlot = (team: 'winner' | 'loser') => {
    if (team === 'winner') setWinnerIds([...winnerIds, EMPTY_ID]);
    else setLoserIds([...loserIds, EMPTY_ID]);
  };

  const removeSlot = (team: 'winner' | 'loser', index: number) => {
    if (team === 'winner' && winnerIds.length > 1) {
      setWinnerIds(winnerIds.filter((_, i) => i !== index));
    } else if (team === 'loser' && loserIds.length > 1) {
      setLoserIds(loserIds.filter((_, i) => i !== index));
    }
  };

  const submitMatch = () => {
    const validWinners = winnerIds.filter(id => id !== EMPTY_ID);
    const validLosers = loserIds.filter(id => id !== EMPTY_ID);

    addMatchMutation.mutate({
      winnerPlayerIds: validWinners,
      loserPlayerIds: validLosers,
      winnerScore,
      loserScore,
      weight,
      season,
      heroId: heroId || undefined,
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setShowScoreConfirm(false);

    const validWinners = winnerIds.filter(id => id !== EMPTY_ID);
    const validLosers = loserIds.filter(id => id !== EMPTY_ID);

    if (validWinners.length === 0) {
      setError('Vyberte alespoň jednoho vítěze');
      return;
    }
    if (validLosers.length === 0) {
      setError('Vyberte alespoň jednoho poraženého');
      return;
    }
    if (!password) {
      setError('Zadejte admin heslo');
      return;
    }

    if (winnerScore === 0 && loserScore === 0) {
      setShowScoreConfirm(true);
      return;
    }

    submitMatch();
  };

  const renderPlayerSlots = (
    team: 'winner' | 'loser',
    ids: string[],
    label: string,
    colorClass: string
  ) => (
    <div>
      <h3 className={`text-lg font-bold mb-3 ${colorClass}`}>{label}</h3>
      <div className="space-y-2">
        {ids.map((selectedId, index) => (
          <div key={index} className="flex items-center gap-2">
            <select
              value={selectedId}
              onChange={e => updatePlayerSlot(team, index, e.target.value)}
              className="flex-1 px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900 text-sm"
            >
              <option value={EMPTY_ID}>---</option>
              {players.map(p => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
            {ids.length > 1 && (
              <button
                type="button"
                onClick={() => removeSlot(team, index)}
                className="px-2 py-2 text-red-600 hover:bg-red-50 rounded-lg text-sm"
                title="Odebrat"
              >
                ✗
              </button>
            )}
          </div>
        ))}
        <button
          type="button"
          onClick={() => addSlot(team)}
          className="text-sm text-blue-600 hover:text-blue-800 font-medium"
        >
          + Přidat hráče
        </button>
      </div>
    </div>
  );

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 sm:p-6 shadow-lg">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 mb-6">
          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Přidat zápas</h1>
          <Button onClick={() => navigate('/')} className="w-full sm:w-auto bg-gray-500 hover:bg-gray-600">
            ← Zpět
          </Button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Score */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Skóre vítězů</label>
              <input
                type="number"
                min={0}
                value={winnerScore}
                onChange={e => setWinnerScore(parseInt(e.target.value) || 0)}
                className="w-full px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Skóre poražených</label>
              <input
                type="number"
                min={0}
                value={loserScore}
                onChange={e => setLoserScore(parseInt(e.target.value) || 0)}
                className="w-full px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
              />
            </div>
          </div>

          {/* Options row */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {config?.isSmallMatchesEnabled && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Váha zápasu</label>
                <select
                  value={weight}
                  onChange={e => setWeight(e.target.value as 'BigMatch' | 'SmallMatch')}
                  className="w-full px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
                >
                  <option value="BigMatch">Velký zápas</option>
                  <option value="SmallMatch">Malý zápas</option>
                </select>
              </div>
            )}

            {config?.isSeasoningSupported && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Sezóna</label>
                <select
                  value={season}
                  onChange={e => setSeason(e.target.value)}
                  className="w-full px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
                >
                  <option value="Summer">Léto</option>
                  <option value="Winter">Zima</option>
                </select>
              </div>
            )}
          </div>

          {/* Jirka Luňák */}
          {config?.isJirkaLunakEnabled && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Jirka Luňák, kapitán a trenér
              </label>
              <select
                value={heroId}
                onChange={e => setHeroId(e.target.value)}
                className="w-full px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
              >
                <option value="">---</option>
                {players.map(p => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
            </div>
          )}

          {/* Swap teams button */}
          <div className="flex justify-center">
            <button
              type="button"
              onClick={() => {
                const prevWinners = [...winnerIds];
                const prevLosers = [...loserIds];
                const prevWinnerScore = winnerScore;
                const prevLoserScore = loserScore;
                setWinnerIds(prevLosers);
                setLoserIds(prevWinners);
                setWinnerScore(prevLoserScore);
                setLoserScore(prevWinnerScore);
              }}
              className="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white font-medium rounded-lg transition-colors shadow-md text-sm flex items-center gap-2"
            >
              ⇄ Prohodit týmy
            </button>
          </div>

          {/* Teams */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            {renderPlayerSlots('winner', winnerIds, 'Vítězové', 'text-green-700')}
            {renderPlayerSlots('loser', loserIds, 'Poražení', 'text-red-700')}
          </div>

          {/* Admin password */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Admin heslo</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="Zadejte heslo"
              className="w-full px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
            />
          </div>

          {/* Error */}
          {error && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg">
              {error}
            </div>
          )}

          {/* Score confirmation */}
          {showScoreConfirm && (
            <div className="bg-yellow-100 border border-yellow-400 text-yellow-800 px-4 py-3 rounded-lg flex flex-col sm:flex-row sm:items-center gap-3">
              <span className="text-sm font-medium">Skóre je 0:0 — opravdu chcete odeslat?</span>
              <div className="flex gap-2">
                <Button
                  type="button"
                  onClick={() => { setShowScoreConfirm(false); submitMatch(); }}
                  disabled={addMatchMutation.isPending}
                  className="bg-green-600 hover:bg-green-700 text-sm px-3 py-1"
                >
                  {addMatchMutation.isPending ? 'Ukládám...' : 'Ano, odeslat'}
                </Button>
                <Button
                  type="button"
                  onClick={() => setShowScoreConfirm(false)}
                  className="bg-gray-500 hover:bg-gray-600 text-sm px-3 py-1"
                >
                  Zrušit
                </Button>
              </div>
            </div>
          )}

          {/* Submit */}
          <Button
            type="submit"
            disabled={addMatchMutation.isPending}
            className="w-full py-3 bg-green-600 hover:bg-green-700 disabled:bg-gray-400"
          >
            {addMatchMutation.isPending ? 'Ukládám...' : 'Vytvoř a vypočti ELO'}
          </Button>
        </form>
      </div>
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 sm:p-6 shadow-lg border border-red-200">
        <h2 className="text-xl font-bold text-red-700 mb-2">Smazat poslední zápas</h2>
        <p className="text-sm text-gray-600 mb-4">
          Smaže poslední přidaný zápas a vrátí všechny změny ELO, výher/proher a trendů.
        </p>
        {!showDeleteConfirm ? (
          <Button
            type="button"
            onClick={() => {
              if (!password) {
                setError('Zadejte admin heslo');
                return;
              }
              setShowDeleteConfirm(true);
            }}
            className="bg-red-600 hover:bg-red-700"
          >
            Smazat poslední zápas
          </Button>
        ) : (
          <div className="flex items-center gap-3">
            <span className="text-sm font-medium text-red-700">Opravdu smazat?</span>
            <Button
              type="button"
              onClick={() => deleteLastMatchMutation.mutate()}
              disabled={deleteLastMatchMutation.isPending}
              className="bg-red-600 hover:bg-red-700"
            >
              {deleteLastMatchMutation.isPending ? 'Mažu...' : 'Ano, smazat'}
            </Button>
            <Button
              type="button"
              onClick={() => setShowDeleteConfirm(false)}
              className="bg-gray-500 hover:bg-gray-600"
            >
              Zrušit
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
