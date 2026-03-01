import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { adminService } from '@/services/apiService';
import { Button } from '@/components/ui/Button';

export function AddPlayerPage() {
  const navigate = useNavigate();

  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const addPlayerMutation = useMutation({
    mutationFn: () =>
      adminService.addPlayer({ name: name.trim() }, { username: 'admin', password }),
    onSuccess: (response: any) => {
      const data = response?.data || response;
      alert(data.message || `Hráč '${data.name}' úspěšně přidán`);
      navigate('/');
    },
    onError: (err: any) => {
      if (err.status === 401) {
        setError('Neplatné heslo');
      } else {
        setError(err.data?.error || err.message || 'Nepodařilo se přidat hráče');
      }
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!name.trim()) {
      setError('Zadejte jméno hráče');
      return;
    }
    if (!password) {
      setError('Zadejte admin heslo');
      return;
    }

    addPlayerMutation.mutate();
  };

  return (
    <div className="max-w-md mx-auto space-y-6">
      <div className="bg-white/90 backdrop-blur-md rounded-lg p-4 sm:p-6 shadow-lg">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 mb-6">
          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Přidat hráče</h1>
          <Button onClick={() => navigate('/')} className="w-full sm:w-auto bg-gray-500 hover:bg-gray-600">
            ← Zpět
          </Button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Jméno</label>
            <input
              type="text"
              value={name}
              onChange={e => setName(e.target.value)}
              placeholder="Jméno hráče"
              className="w-full px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-900"
              autoFocus
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
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg text-sm">
              {error}
            </div>
          )}

          <Button
            type="submit"
            disabled={addPlayerMutation.isPending}
            className="w-full py-3 bg-green-600 hover:bg-green-700 disabled:bg-gray-400"
          >
            {addPlayerMutation.isPending ? 'Ukládám...' : 'Přidej hráče'}
          </Button>
        </form>

        <p className="text-xs text-gray-500 mt-4">
          Nový hráč bude vytvořen s ELO 1000.
        </p>
      </div>
    </div>
  );
}
