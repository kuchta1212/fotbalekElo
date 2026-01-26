import { ReactNode } from 'react';

interface ErrorDisplayProps {
  error: Error | null;
  message?: string;
  children?: ReactNode;
}

export function ErrorDisplay({ error, message, children }: ErrorDisplayProps) {
  if (!error && !message) return null;

  return (
    <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4">
      <div className="flex items-start gap-3">
        <div className="flex-shrink-0">
          <svg
            className="h-5 w-5 text-destructive"
            fill="none"
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth="2"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <div className="flex-1">
          <h3 className="text-sm font-medium text-destructive">
            {message || 'An error occurred'}
          </h3>
          {error && (
            <p className="mt-2 text-sm text-destructive/80">
              {error.message}
            </p>
          )}
          {children && <div className="mt-3">{children}</div>}
        </div>
      </div>
    </div>
  );
}
