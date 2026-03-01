import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

/**
 * Merge Tailwind CSS classes with proper precedence
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Format date to local string
 */
export function formatDate(date: string | Date, options?: Intl.DateTimeFormatOptions): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('cs-CZ', options || {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

/**
 * Format date and time to local string
 */
export function formatDateTime(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleString('cs-CZ', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/**
 * Format date for input fields (YYYY-MM-DD)
 */
export function formatDateInput(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toISOString().split('T')[0];
}

/**
 * Get next N Tuesdays
 */
export function getNextTuesdays(count: number): Date[] {
  const tuesdays: Date[] = [];
  const today = new Date();
  let current = new Date(today);
  
  // Find next Tuesday
  const daysUntilTuesday = (2 - current.getDay() + 7) % 7;
  current.setDate(current.getDate() + (daysUntilTuesday || 7));
  
  for (let i = 0; i < count; i++) {
    tuesdays.push(new Date(current));
    current.setDate(current.getDate() + 7);
  }
  
  return tuesdays;
}

/**
 * Calculate Elo difference display
 */
export function formatEloDifference(diff: number): string {
  if (diff === 0) return '±0';
  return diff > 0 ? `+${diff}` : `${diff}`;
}

/**
 * Get season color classes
 */
export function getSeasonColor(season: string): string {
  switch (season.toLowerCase()) {
    case 'winter':
      return 'text-blue-600 bg-blue-100';
    case 'summer':
      return 'text-orange-600 bg-orange-100';
    default:
      return 'text-gray-600 bg-gray-100';
  }
}
