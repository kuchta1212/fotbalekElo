import { useMemo, useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { billingService } from '@/services/apiService';
import { Button } from '@/components/ui/Button';
import type { CalculateBillingRequest, CalculateBillingResponse } from '@/types/api';

const CZECH_MONTHS = [
  'leden', 'únor', 'březen', 'duben', 'květen', 'červen',
  'červenec', 'srpen', 'září', 'říjen', 'listopad', 'prosinec',
];

function formatKc(value: number): string {
  return `${value.toLocaleString('cs-CZ')} Kč`;
}

export function VyuctovaniPage() {
  const navigate = useNavigate();
  const now = new Date();
  const currentYear = now.getFullYear();
  const currentMonth = now.getMonth() + 1;

  const [fromYear, setFromYear] = useState<number>(currentYear);
  const [fromMonth, setFromMonth] = useState<number>(currentMonth);
  const [toYear, setToYear] = useState<number>(currentYear);
  const [toMonth, setToMonth] = useState<number>(currentMonth);
  const [totalAmount, setTotalAmount] = useState<number>(0);
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [result, setResult] = useState<CalculateBillingResponse | null>(null);

  const years = useMemo(() => {
    const arr: number[] = [];
    for (let y = currentYear - 3; y <= currentYear + 1; y++) arr.push(y);
    return arr;
  }, [currentYear]);

  const rangeValid = useMemo(() => {
    return fromYear * 100 + fromMonth <= toYear * 100 + toMonth;
  }, [fromYear, fromMonth, toYear, toMonth]);

  const calculateMutation = useMutation({
    mutationFn: (request: CalculateBillingRequest) =>
      billingService.calculate(request, { username: 'admin', password }),
    onSuccess: (response: any) => {
      const data: CalculateBillingResponse = response?.data || response;
      setResult(data);
      setError('');
    },
    onError: (err: any) => {
      setResult(null);
      if (err.status === 401) {
        setError('Neplatné heslo');
      } else {
        setError(err.data?.error || err.message || 'Vyúčtování selhalo');
      }
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!rangeValid) {
      setError('Počáteční měsíc musí být dříve nebo stejný jako koncový');
      return;
    }
    if (!totalAmount || totalAmount <= 0) {
      setError('Zadejte kladnou částku');
      return;
    }
    if (!password) {
      setError('Zadejte admin heslo');
      return;
    }

    calculateMutation.mutate({
      fromYear,
      fromMonth,
      toYear,
      toMonth,
      totalAmount,
    });
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 sm:p-6 shadow-lg">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 mb-6">
          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Vyúčtování</h1>
          <Button onClick={() => navigate('/')} className="w-full sm:w-auto bg-gray-500 hover:bg-gray-600">
            ← Zpět
          </Button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Od (měsíc / rok)</label>
              <div className="flex gap-2">
                <select
                  value={fromMonth}
                  onChange={e => setFromMonth(parseInt(e.target.value))}
                  className="flex-1 px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
                >
                  {CZECH_MONTHS.map((name, idx) => (
                    <option key={idx + 1} value={idx + 1}>{name}</option>
                  ))}
                </select>
                <select
                  value={fromYear}
                  onChange={e => setFromYear(parseInt(e.target.value))}
                  className="w-28 px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
                >
                  {years.map(y => (
                    <option key={y} value={y}>{y}</option>
                  ))}
                </select>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Do (měsíc / rok)</label>
              <div className="flex gap-2">
                <select
                  value={toMonth}
                  onChange={e => setToMonth(parseInt(e.target.value))}
                  className="flex-1 px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
                >
                  {CZECH_MONTHS.map((name, idx) => (
                    <option key={idx + 1} value={idx + 1}>{name}</option>
                  ))}
                </select>
                <select
                  value={toYear}
                  onChange={e => setToYear(parseInt(e.target.value))}
                  className="w-28 px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
                >
                  {years.map(y => (
                    <option key={y} value={y}>{y}</option>
                  ))}
                </select>
              </div>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Celková zaplacená částka (Kč)</label>
            <input
              type="number"
              min={0}
              step={1}
              value={totalAmount || ''}
              onChange={e => setTotalAmount(parseFloat(e.target.value) || 0)}
              placeholder="např. 5000"
              className="w-full px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
            />
          </div>

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

          {error && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg">
              {error}
            </div>
          )}

          <Button
            type="submit"
            disabled={calculateMutation.isPending || !rangeValid}
            className="w-full py-3 bg-green-600 hover:bg-green-700 disabled:bg-gray-400"
          >
            {calculateMutation.isPending ? 'Počítám...' : 'Spočítat vyúčtování'}
          </Button>
        </form>
      </div>

      {result && (
        <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 sm:p-6 shadow-lg">
          <h2 className="text-xl font-bold text-gray-900 mb-4">Výsledek</h2>

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 mb-4 text-sm">
            <div className="bg-gray-50 rounded-lg p-3">
              <div className="text-gray-500">Cena za hru</div>
              <div className="text-lg font-bold text-gray-900">{formatKc(result.pricePerGame)}</div>
            </div>
            <div className="bg-gray-50 rounded-lg p-3">
              <div className="text-gray-500">Celkem účastí</div>
              <div className="text-lg font-bold text-gray-900">{result.totalAppearances}</div>
            </div>
            <div className="bg-gray-50 rounded-lg p-3">
              <div className="text-gray-500">Vybráno / Potřeba</div>
              <div className="text-lg font-bold text-gray-900">
                {formatKc(result.collectedTotal)} / {formatKc(result.totalAmount)}
              </div>
            </div>
          </div>

          {result.rows.length === 0 ? (
            <div className="bg-yellow-50 border border-yellow-300 text-yellow-800 px-4 py-3 rounded-lg text-sm">
              V tomto období nejsou žádní hráči k vyúčtování.
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Jméno</th>
                    <th className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Účastí</th>
                    <th className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Částka (Kč)</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {result.rows.map(row => (
                    <tr key={row.playerId}>
                      <td className="px-4 py-2 text-sm text-gray-900">{row.playerName}</td>
                      <td className="px-4 py-2 text-sm text-gray-900 text-right">{row.appearances}</td>
                      <td className="px-4 py-2 text-sm font-medium text-gray-900 text-right">
                        {row.amountOwed.toLocaleString('cs-CZ')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {result.matchDays.length > 0 && (
            <div className="mt-4 text-xs text-gray-600">
              <span className="font-medium">Dny zápasů ({result.matchDays.length}):</span>{' '}
              {result.matchDays.map(d => new Date(d).toLocaleDateString('cs-CZ')).join(', ')}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
