import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { billingService } from '@/services/apiService';
import { Button } from '@/components/ui/Button';
import { Loading } from '@/components/ui/Loading';
import { ErrorDisplay } from '@/components/ui/ErrorDisplay';
import type { FinanceReport, FinanceReportPeriod } from '@/types/api';

const CZECH_MONTHS_SHORT = [
  'leden', 'únor', 'březen', 'duben', 'květen', 'červen',
  'červenec', 'srpen', 'září', 'říjen', 'listopad', 'prosinec',
];

function formatKc(value: number): string {
  return `${value.toLocaleString('cs-CZ')} Kč`;
}

function formatPeriodLabel(period: FinanceReportPeriod): string {
  const sameMonth = period.fromYear === period.toYear && period.fromMonth === period.toMonth;
  if (sameMonth) {
    return `${CZECH_MONTHS_SHORT[period.fromMonth - 1]} ${period.fromYear}`;
  }
  if (period.fromYear === period.toYear) {
    return `${CZECH_MONTHS_SHORT[period.fromMonth - 1]}–${CZECH_MONTHS_SHORT[period.toMonth - 1]} ${period.fromYear}`;
  }
  return `${CZECH_MONTHS_SHORT[period.fromMonth - 1]} ${period.fromYear} – ${CZECH_MONTHS_SHORT[period.toMonth - 1]} ${period.toYear}`;
}

export function VyuctovaniReportPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [confirmingPlayerId, setConfirmingPlayerId] = useState<string | null>(null);

  const { data: reportResponse, isLoading, error: loadError } = useQuery({
    queryKey: ['finance-report'],
    queryFn: billingService.getReport,
  });

  const report: FinanceReport | undefined = (reportResponse as any)?.data || (reportResponse as any);

  const settleMutation = useMutation({
    mutationFn: (playerId: string) =>
      billingService.settlePlayer(playerId, { username: 'admin', password }),
    onSuccess: () => {
      setConfirmingPlayerId(null);
      setError('');
      queryClient.invalidateQueries({ queryKey: ['finance-report'] });
    },
    onError: (err: any) => {
      setConfirmingPlayerId(null);
      if (err.status === 401) {
        setError('Neplatné heslo');
      } else {
        setError(err.data?.error || err.message || 'Vyrovnání selhalo');
      }
    },
  });

  if (isLoading) return <Loading message="Načítání přehledu..." />;
  if (loadError) return <ErrorDisplay error={loadError as Error} message="Nepodařilo se načíst přehled" />;

  const periods = report?.periods ?? [];
  const entries = report?.entries ?? [];
  const adminMode = password.trim().length > 0;

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 sm:p-6 shadow-lg">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 mb-4">
          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Vyúčtování – dlužníci</h1>
          <Button onClick={() => navigate('/')} className="w-full sm:w-auto bg-gray-500 hover:bg-gray-600">
            ← Zpět
          </Button>
        </div>

        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Admin heslo <span className="text-gray-400 font-normal">(pro tlačítko „Vyrovnáno")</span>
          </label>
          <input
            type="password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            placeholder="Nech prázdné pro pouze čtení"
            className="w-full sm:w-72 px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
          />
        </div>

        {error && (
          <div className="mb-4 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg">
            {error}
          </div>
        )}

        {entries.length === 0 ? (
          <div className="bg-gray-50 border border-gray-200 text-gray-700 px-4 py-6 rounded-lg text-center">
            Žádné dluhy.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Jméno</th>
                  <th className="px-4 py-2 text-right text-xs font-medium text-gray-700 uppercase tracking-wider">Celkem</th>
                  {periods.map(p => (
                    <th key={p.id} className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase tracking-wider whitespace-nowrap">
                      {formatPeriodLabel(p)}
                    </th>
                  ))}
                  {adminMode && (
                    <th className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Akce</th>
                  )}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {entries.map(entry => (
                  <tr key={entry.playerId}>
                    <td className="px-4 py-2 text-sm text-gray-900">{entry.playerName}</td>
                    <td className="px-4 py-2 text-sm font-bold text-gray-900 text-right whitespace-nowrap">
                      {formatKc(entry.total)}
                    </td>
                    {periods.map(p => {
                      const amount = entry.amountsPerPeriod[p.id];
                      return (
                        <td key={p.id} className="px-4 py-2 text-sm text-gray-700 text-right whitespace-nowrap">
                          {amount ? formatKc(amount) : '—'}
                        </td>
                      );
                    })}
                    {adminMode && (
                      <td className="px-4 py-2 text-right">
                        {confirmingPlayerId === entry.playerId ? (
                          <div className="flex items-center gap-2 justify-end">
                            <Button
                              type="button"
                              onClick={() => settleMutation.mutate(entry.playerId)}
                              disabled={settleMutation.isPending}
                              className="bg-red-600 hover:bg-red-700 text-xs px-2 py-1"
                            >
                              {settleMutation.isPending ? '...' : 'Potvrdit'}
                            </Button>
                            <Button
                              type="button"
                              onClick={() => setConfirmingPlayerId(null)}
                              className="bg-gray-400 hover:bg-gray-500 text-xs px-2 py-1"
                            >
                              Zrušit
                            </Button>
                          </div>
                        ) : (
                          <Button
                            type="button"
                            onClick={() => setConfirmingPlayerId(entry.playerId)}
                            className="bg-green-600 hover:bg-green-700 text-xs px-2 py-1"
                          >
                            Vyrovnáno
                          </Button>
                        )}
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
