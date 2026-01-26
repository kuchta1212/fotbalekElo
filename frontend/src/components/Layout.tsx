import { ReactNode } from 'react';
import { Navigation } from './Navigation';
import { BackgroundImages } from './BackgroundImages';

interface LayoutProps {
  children: ReactNode;
}

export function Layout({ children }: LayoutProps) {
  return (
    <div className="min-h-screen flex flex-col relative">
      {/* Background images with overlay */}
      <BackgroundImages />
      
      {/* Content wrapper with backdrop */}
      <div className="relative z-10 flex flex-col min-h-screen">
        {/* Navigation */}
        <Navigation />
        
        {/* Main content */}
        <main className="flex-1 container mx-auto px-4 py-6 max-w-7xl">
          {children}
        </main>
        
        {/* Footer */}
        <footer className="py-4 text-center text-sm text-white backdrop-blur-sm bg-black/20">
          <p>© {new Date().getFullYear()} Elo-fotbalek</p>
        </footer>
      </div>
    </div>
  );
}
