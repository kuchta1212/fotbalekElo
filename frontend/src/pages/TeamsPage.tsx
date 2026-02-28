import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { teamsService } from '@/services/apiService';
import { Loading } from '@/components/ui/Loading';
import { ErrorDisplay } from '@/components/ui/ErrorDisplay';
import { Button } from '@/components/ui/Button';
import type { GeneratorResultDto } from '@/types/api';

export function TeamsPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  
  const dateParam = searchParams.get('date') || 'first';
  const seasonParam = searchParams.get('season') || 'Summer';
  
  const [selectedResultIndex, setSelectedResultIndex] = useState(0);

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['generate-teams', dateParam, seasonParam],
    queryFn: () => teamsService.generate(dateParam, seasonParam),
  });

  if (isLoading) return <Loading message="Generuji týmy..." />;
  if (error) return (
    <ErrorDisplay error={error as Error} message="Nepodařilo se vygenerovat týmy">
      <div className="flex gap-2">
        <Button onClick={() => refetch()}>Zkusit znovu</Button>
        <Button onClick={() => navigate('/doodle')} className="bg-gray-500 hover:bg-gray-600">Zpět na Doodle</Button>
      </div>
    </ErrorDisplay>
  );

  const teamsData = (data as any)?.data || data;
  const results: GeneratorResultDto[] = teamsData?.results || [];
  const currentResult = results[selectedResultIndex];

  if (results.length === 0) {
    return (
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-6 shadow-lg text-center">
        <h2 className="text-2xl font-bold mb-4 text-gray-900">Žádné výsledky</h2>
        <p className="text-gray-700 mb-4">Nepodařilo se vygenerovat žádné kombinace týmů.</p>
        <Button onClick={() => navigate('/doodle')}>Zpět na Doodle</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 sm:p-6 shadow-lg">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 mb-2">
          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Vygenerované týmy</h1>
          <Button onClick={() => navigate('/doodle')} className="w-full sm:w-auto bg-gray-500 hover:bg-gray-600">
            ← Zpět na Doodle
          </Button>
        </div>
        <p className="text-sm sm:text-base text-gray-700">
          Datum: <strong>{teamsData.displayDate}</strong> | 
          Sezóna: <strong>{seasonParam === 'Summer' ? 'Léto' : 'Zima'}</strong> | 
          Hráčů: <strong>{teamsData.playerCount}</strong> | 
          Variant: <strong>{results.length}</strong>
        </p>
      </div>

      {/* Variant navigation */}
      {results.length > 1 && (
        <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 shadow-lg">
          <div className="flex flex-wrap items-center gap-2">
            <Button
              onClick={() => setSelectedResultIndex(0)}
              disabled={selectedResultIndex === 0}
              className="bg-blue-800 hover:bg-blue-900 disabled:bg-gray-400 text-sm"
            >
              ⟨⟨ První
            </Button>
            <Button
              onClick={() => setSelectedResultIndex(prev => Math.max(0, prev - 1))}
              disabled={selectedResultIndex === 0}
              className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-sm"
            >
              ← Předchozí
            </Button>
            <span className="px-3 py-2 bg-gray-100 rounded-lg text-gray-900 font-medium text-sm">
              {selectedResultIndex + 1} / {results.length}
            </span>
            <Button
              onClick={() => setSelectedResultIndex(prev => Math.min(results.length - 1, prev + 1))}
              disabled={selectedResultIndex === results.length - 1}
              className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-sm"
            >
              Další →
            </Button>
            <Button
              onClick={() => setSelectedResultIndex(results.length - 1)}
              disabled={selectedResultIndex === results.length - 1}
              className="bg-blue-800 hover:bg-blue-900 disabled:bg-gray-400 text-sm"
            >
              Poslední ⟩⟩
            </Button>
            <span className={`px-3 py-2 rounded-lg font-bold text-sm ${
              currentResult.eloDiff <= 50 ? 'bg-green-100 text-green-800' : 
              currentResult.eloDiff <= 100 ? 'bg-orange-100 text-orange-800' : 
              'bg-red-100 text-red-800'
            }`}>
              {currentResult.eloDiff <= 50 ? '✓' : currentResult.eloDiff <= 100 ? '⚠' : '✗'} ELO: ±{currentResult.eloDiff}
            </span>
          </div>
        </div>
      )}

      {/* Teams display */}
      {currentResult && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Team One */}
          <div className="bg-blue-500/20 backdrop-blur-md rounded-lg p-6 border-2 border-blue-500/40 shadow-lg">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-2xl font-bold text-blue-900">Tým 1</h2>
              <span className="text-2xl font-bold text-blue-900">ELO: {currentResult.teamOne.teamElo}</span>
            </div>
            <div className="space-y-2">
              {currentResult.teamOne.players.map(player => (
                <div
                  key={player.id}
                  className="bg-white/70 rounded-lg p-3 flex justify-between items-center"
                >
                  <span className="font-medium text-gray-900">{player.name}</span>
                  <span className="text-gray-600 font-mono">{player.elo}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Team Two */}
          <div className="bg-red-500/20 backdrop-blur-md rounded-lg p-6 border-2 border-red-500/40 shadow-lg">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-2xl font-bold text-red-900">Tým 2</h2>
              <span className="text-2xl font-bold text-red-900">ELO: {currentResult.teamTwo.teamElo}</span>
            </div>
            <div className="space-y-2">
              {currentResult.teamTwo.players.map(player => (
                <div
                  key={player.id}
                  className="bg-white/70 rounded-lg p-3 flex justify-between items-center"
                >
                  <span className="font-medium text-gray-900">{player.name}</span>
                  <span className="text-gray-600 font-mono">{player.elo}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Add match button */}
      {currentResult && (
        <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 shadow-lg">
          <Button
            onClick={() => {
              navigate('/admin/add-match', {
                state: {
                  teamOneIds: currentResult.teamOne.players.map(p => p.id),
                  teamTwoIds: currentResult.teamTwo.players.map(p => p.id),
                  season: seasonParam,
                },
              });
            }}
            className="w-full py-3 bg-green-600 hover:bg-green-700"
          >
            Přidat zápas s těmito týmy
          </Button>
        </div>
      )}

      {/* Balance info - only show for single result or first result */}
      {currentResult && results.length === 1 && (
        <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 shadow-lg">
          <div className="flex items-center gap-2">
            <span className={`text-2xl ${currentResult.eloDiff <= 50 ? 'text-green-600' : currentResult.eloDiff <= 100 ? 'text-orange-600' : 'text-red-600'}`}>
              {currentResult.eloDiff <= 50 ? '✓' : currentResult.eloDiff <= 100 ? '⚠' : '✗'}
            </span>
            <div>
              <strong>Rozdíl ELO: {currentResult.eloDiff}</strong>
              <p className="text-sm text-gray-600">
                {currentResult.eloDiff <= 50 && 'Vynikající vyrovnanost!'}
                {currentResult.eloDiff > 50 && currentResult.eloDiff <= 100 && 'Dobré vyrovnání'}
                {currentResult.eloDiff > 100 && 'Týmy nejsou úplně vyrovnané'}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
