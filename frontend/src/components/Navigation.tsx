import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { configService } from '@/services/apiService';

export function Navigation() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isAdminOpen, setIsAdminOpen] = useState(false);
  const location = useLocation();
  
  const { data: configResponse } = useQuery({
    queryKey: ['config'],
    queryFn: configService.get,
  });

  const config = (configResponse as any)?.data;

  const isActive = (path: string) => location.pathname === path;

  const scrollToMatches = () => {
    setIsMenuOpen(false);
    if (location.pathname !== '/') {
      // If not on home page, navigate home first
      window.location.href = '/#matches';
    } else {
      // Already on home page, just scroll
      const matchesSection = document.getElementById('matches');
      if (matchesSection) {
        matchesSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }
  };

  // Navigation links - removed Matches as a route
  const navLinks = [
    ...(config?.isDoodleEnabled ? [{ path: '/doodle', label: 'Doodle' }] : []),
  ];

  return (
    <nav className="backdrop-blur-md bg-white/90 shadow-md sticky top-0 z-50">
      <div className="container mx-auto px-4 max-w-7xl">
        <div className="flex items-center justify-between h-16">
          {/* Logo/Brand - links to home */}
          <Link to="/" className="text-xl font-bold text-gray-900">
            {config?.appName || 'Elo-fotbalek'}
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center space-x-1">
            <button
              onClick={scrollToMatches}
              className="px-4 py-2 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-100 transition-colors"
            >
              Zápasy
            </button>
            
            {navLinks.map((link) => (
              <Link
                key={link.path}
                to={link.path}
                className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                  isActive(link.path)
                    ? 'bg-primary text-primary-foreground'
                    : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                {link.label}
              </Link>
            ))}
            
            {/* Admin dropdown */}
            <div className="relative ml-4 pl-4 border-l border-gray-300">
              <button
                onClick={() => setIsAdminOpen(!isAdminOpen)}
                className="px-4 py-2 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-100 flex items-center gap-1"
              >
                Admin
                <svg
                  className={`w-4 h-4 transition-transform ${isAdminOpen ? 'rotate-180' : ''}`}
                  fill="none"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth="2"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path d="M19 9l-7 7-7-7" />
                </svg>
              </button>
              
              {isAdminOpen && (
                <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-50 border border-gray-200">
                  <Link
                    to="/admin/add-match"
                    onClick={() => setIsAdminOpen(false)}
                    className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    Přidat zápas
                  </Link>
                  <Link
                    to="/admin/add-player"
                    onClick={() => setIsAdminOpen(false)}
                    className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    Přidat hráče
                  </Link>
                </div>
              )}
            </div>
          </div>

          {/* Mobile menu button */}
          <button
            className="md:hidden p-2 rounded-md text-gray-700 hover:bg-gray-100"
            onClick={() => setIsMenuOpen(!isMenuOpen)}
            aria-label="Toggle menu"
          >
            <svg
              className="h-6 w-6"
              fill="none"
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              {isMenuOpen ? (
                <path d="M6 18L18 6M6 6l12 12" />
              ) : (
                <path d="M4 6h16M4 12h16M4 18h16" />
              )}
            </svg>
          </button>
        </div>

        {/* Mobile Navigation */}
        {isMenuOpen && (
          <div className="md:hidden pb-4">
            <div className="flex flex-col space-y-2">
              <button
                onClick={scrollToMatches}
                className="text-left px-4 py-2 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-100 transition-colors"
              >
                Zápasy
              </button>
              
              {navLinks.map((link) => (
                <Link
                  key={link.path}
                  to={link.path}
                  onClick={() => setIsMenuOpen(false)}
                  className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                    isActive(link.path)
                      ? 'bg-primary text-primary-foreground'
                      : 'text-gray-700 hover:bg-gray-100'
                  }`}
                >
                  {link.label}
                </Link>
              ))}
              
              {/* Admin section in mobile */}
              <div className="pt-2 border-t border-gray-300">
                <button
                  onClick={() => setIsAdminOpen(!isAdminOpen)}
                  className="w-full text-left px-4 py-2 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-100 flex items-center justify-between"
                >
                  Admin
                  <svg
                    className={`w-4 h-4 transition-transform ${isAdminOpen ? 'rotate-180' : ''}`}
                    fill="none"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="2"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                
                {isAdminOpen && (
                  <div className="ml-4 mt-2 space-y-2">
                    <Link
                      to="/admin/add-match"
                      onClick={() => {
                        setIsMenuOpen(false);
                        setIsAdminOpen(false);
                      }}
                      className="block px-4 py-2 rounded-md text-sm text-gray-700 hover:bg-gray-100"
                    >
                      Přidat zápas
                    </Link>
                    <Link
                      to="/admin/add-player"
                      onClick={() => {
                        setIsMenuOpen(false);
                        setIsAdminOpen(false);
                      }}
                      className="block px-4 py-2 rounded-md text-sm text-gray-700 hover:bg-gray-100"
                    >
                      Přidat hráče
                    </Link>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </nav>
  );
}
