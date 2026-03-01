# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Football match tracking app with ELO ratings, player rankings, team generation, and doodle attendance system. Czech language UI.

**Stack:** .NET 8 (ASP.NET Core) backend + React 18 / TypeScript / Vite frontend, styled with TailwindCSS.

## Build & Development Commands

### Frontend (`frontend/`)
```bash
cd frontend
npm install          # Install dependencies
npm run dev          # Dev server on http://localhost:5173 (proxies /api/* to https://localhost:5001)
npm run build        # TypeScript check + Vite build (outputs to ../Elo-fotbalek/wwwroot/)
npm run lint         # ESLint
```

### Backend (`Elo-fotbalek/`)
```bash
dotnet build Elo-fotbalek/Elo-fotbalek.csproj
dotnet run --project Elo-fotbalek/Elo-fotbalek.csproj    # Runs on https://localhost:5001
```

### Solution-level
```bash
dotnet build Elo-fotbalek.sln      # Build everything including tests
dotnet test                         # Run test project (Test/)
```

## Architecture

### Frontend → Backend Integration
- Frontend builds into `Elo-fotbalek/wwwroot/` — ASP.NET Core serves the SPA as static files
- Backend serves SPA fallback via `MapFallbackToFile("index.html")` for client-side routing
- Legacy MVC routes preserved under `/legacy/*`

### API Pattern
- All API controllers inherit from `BaseApiController` which wraps responses as `{ success: bool, data: T }`
- Route prefix: `/api/`
- Admin endpoints use Basic Authentication (`BasicAuthenticationHandler.cs`)
- Public endpoints require no auth

### Frontend State Management
- **TanStack Query** for all server state (caching, refetching, optimistic updates)
- No Redux/Context — Query handles data fetching needs
- API calls centralized in `frontend/src/services/apiService.ts`
- Types split: `frontend/src/types/domain.ts` (domain models) and `frontend/src/types/api.ts` (API DTOs)

### Storage
- Azure Blob Storage as primary data store (not a traditional database)
- `IBlobClient` interface with `BlobClient` (Azure) and `OfflineBlobClient` (local fallback)
- Configured via `BlobStorageOptions` in appsettings

### Key Backend Directories
- `Controllers/Api/` — REST API endpoints
- `Storage/` — Azure Blob abstraction
- `EloCounter/` — ELO calculation logic
- `TeamGenerator/` — Balanced team generation algorithm
- `TrendCalculator/` — Player trend analysis
- `Models/` — Domain models (Player, Match, Doodle, Season, etc.)

### Key Frontend Directories
- `src/pages/` — Route-level page components
- `src/pages/admin/` — Admin-only pages (AddPlayer, AddMatch)
- `src/components/ui/` — Reusable UI primitives (Button, Card, Loading, ErrorDisplay)
- `src/services/` — API layer (`api.ts` for fetch utilities, `apiService.ts` for endpoint methods)

## Configuration

- `appsettings.Development.json` is gitignored (contains Azure connection strings)
- `AppConfigurationOptions` controls feature flags: `IsSeasoningSupported`, `IsSmallMatchesEnabled`, `IsJirkaLunakEnabled`, `IsDoodleEnabled`
- Frontend dev proxy configured in `frontend/vite.config.ts`
- CORS origins configured per environment in appsettings

## Domain Concepts

- **Season**: `'Winter' | 'Summer' | 'Overall'` — ELO tracked separately per season
- **Regular player**: ≥30% attendance threshold, shown separately on leaderboard
- **Doodle**: Attendance polling system where players mark Accept/Maybe/Refused for upcoming dates
- **Small matches**: Configurable match type variant
