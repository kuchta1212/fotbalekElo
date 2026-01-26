import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="text-center py-12">
      <h1 className="text-6xl font-bold text-gray-900 dark:text-white mb-4">404</h1>
      <h2 className="text-2xl font-semibold text-gray-700 dark:text-gray-300 mb-6">
        Stránka nenalezena
      </h2>
      <p className="text-muted-foreground mb-8">
        Omlouváme se, stránka kterou hledáte neexistuje.
      </p>
      <Link
        to="/"
        className="inline-block px-6 py-3 bg-primary text-primary-foreground rounded-md hover:bg-primary/90 transition-colors"
      >
        Zpět na hlavní stránku
      </Link>
    </div>
  );
}
