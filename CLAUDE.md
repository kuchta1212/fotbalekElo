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

### Tests (`Elo-Fotbalek-Test/`)
```bash
dotnet test                                                          # Run all tests
dotnet test Elo-Fotbalek-Test/Elo-Fotbalek-Test.csproj               # Run test project only
dotnet test --filter "TestName"                                      # Run single test by name
```
Tests use NUnit 4.1 and cover ELO calculation scenarios. The `Test/` directory at root is **not** a test project — it contains JSON data files used by `OfflineBlobClient` for local development.

### Solution-level
```bash
dotnet build Elo-fotbalek.sln      # Build everything including tests
```

## Architecture

### Frontend → Backend Integration
- Frontend builds into `Elo-fotbalek/wwwroot/` — ASP.NET Core serves the SPA as static files
- Backend serves SPA fallback via `MapFallbackToFile("index.html")` for client-side routing
- Legacy MVC routes preserved under `/legacy/*`
- Frontend uses `@/` path alias mapping to `frontend/src/`

### API Pattern
- All API controllers inherit from `BaseApiController` which wraps responses as `{ success: bool, data: T }`
- Route prefix: `/api/`
- Admin endpoints use Basic Authentication (`BasicAuthenticationHandler.cs`)
- Public endpoints require no auth
- Pages unwrap the response with `(response as any)?.data` pattern

### Frontend State Management
- **TanStack Query v5** for all server state (caching, refetching, optimistic updates)
- No Redux/Context — Query handles data fetching needs
- API calls centralized in `frontend/src/services/apiService.ts`
- Types split: `frontend/src/types/domain.ts` (domain models) and `frontend/src/types/api.ts` (API DTOs)
- QueryClient configured with 5-minute staleTime and 1 retry

### Storage
- Azure Blob Storage as primary data store (not a traditional database) — all data stored as JSON blobs (`players`, `matches`, `users`, `doodle`)
- `IBlobClient` interface with `BlobClient` (Azure) and `OfflineBlobClient` (local fallback)
- Configured via `BlobStorageOptions` in appsettings; set `BlobStorage:UseOffline = true` for local dev without Azure

### Key Backend Directories
- `Controllers/Api/` — REST API endpoints
- `Storage/` — Azure Blob abstraction
- `EloCounter/` — ELO calculation logic (`EloCalculator` is active; `FifaEloCounter` is deprecated)
- `TeamGenerator/` — Balanced team generation algorithm
- `TrendCalculator/` — Player trend analysis
- `Models/` — Domain models (Player, Match, Doodle, Season, etc.)

### Key Frontend Directories
- `src/pages/` — Route-level page components
- `src/pages/admin/` — Admin-only pages (AddPlayer, AddMatch)
- `src/components/ui/` — Reusable UI primitives (Button, Card, Loading, ErrorDisplay)
- `src/services/` — API layer (`api.ts` for fetch utilities, `apiService.ts` for endpoint methods)

## CI/CD

GitHub Actions (`.github/workflows/azure-webapps-dotnet-core.yml`): triggered on push to `master`.
1. Build: installs Node 20, runs `npm install && npm run build` in `frontend/` (populates `wwwroot/`), then `dotnet build` + `dotnet publish`
2. Deploy: publishes to Azure Web App `elo-fotbalek`

Frontend must build before .NET publish so static files are included in the artifact.

## Configuration

- `appsettings.Development.json` is gitignored (contains Azure connection strings)
- `AppConfigurationOptions` controls feature flags: `IsSeasoningSupported`, `IsSmallMatchesEnabled`, `IsJirkaLunakEnabled`, `IsDoodleEnabled`
- Frontend dev proxy configured in `frontend/vite.config.ts`
- CORS origins configured per environment in appsettings

## Domain Concepts

- **Season**: `'Winter' | 'Summer' | 'Overall'` — ELO tracked separately per season
- **Overall ELO**: `1000 + (SummerElo - 1000) + (WinterElo - 1000)` — combined deviation from base
- **Regular player**: ≥30% attendance in recent months (configurable window, default 6 months)
- **Doodle**: Attendance polling system where players mark Accept/Maybe/Refused for upcoming dates
- **Small matches**: Configurable match type variant (weight=10 vs weight=30 for big matches)

### ELO Calculation
FIFA-style formula: `PointChange = Weight × GoalDiffIndex × (MatchResult - ExpectedResult)`
- GoalDiffIndex: 1 for diff 0-1, 1.5 for diff 2, `(11 + diff) / 8` for diff 3+
- ExpectedResult uses standard ELO probability formula with 400 rating scale

### Team ELO & Balance
Team ELO is an attendance-weighted average of player ELOs:
- Players with <30% attendance are treated as 1000 (new) or dampened (70% of deviation from 1000)
- Team generator does exhaustive C(n, n/2) search, sorted by ELO difference, showing best-balanced options
