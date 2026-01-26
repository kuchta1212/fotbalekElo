# Frontend Setup Summary

## What Has Been Created

### Project Structure ✅

```
frontend/
├── src/
│   ├── components/
│   │   ├── ui/                    # Reusable UI components
│   │   │   ├── Button.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── Loading.tsx
│   │   │   └── ErrorDisplay.tsx
│   │   ├── Layout.tsx             # Main layout wrapper
│   │   ├── Navigation.tsx         # Navigation bar with mobile menu
│   │   └── BackgroundImages.tsx   # Rotating background images
│   ├── pages/                     # Page components (one per route)
│   │   ├── HomePage.tsx           # Leaderboard (/)
│   │   ├── DoodlePage.tsx         # Attendance (/doodle)
│   │   ├── TeamsPage.tsx          # Team generator (/teams)
│   │   ├── PlayersPage.tsx        # Player list (/players)
│   │   ├── PlayerDetailPage.tsx   # Player stats (/players/:id)
│   │   ├── MatchesPage.tsx        # Match history (/matches)
│   │   ├── MatchDetailPage.tsx    # Match detail (/matches/:id)
│   │   ├── NotFoundPage.tsx       # 404 page
│   │   └── admin/
│   │       ├── AddPlayerPage.tsx  # Add player form
│   │       └── AddMatchPage.tsx   # Add match form
│   ├── services/
│   │   ├── api.ts                 # Base fetch utilities with auth
│   │   └── apiService.ts          # All API endpoint methods
│   ├── types/
│   │   ├── domain.ts              # Domain models (Player, Match, etc.)
│   │   └── api.ts                 # API request/response DTOs
│   ├── lib/
│   │   └── utils.ts               # Helper functions
│   ├── App.tsx                    # Root component with routing
│   ├── main.tsx                   # Entry point
│   ├── index.css                  # Global styles (Tailwind)
│   └── vite-env.d.ts              # TypeScript environment definitions
├── public/                        # Static assets (empty initially)
├── index.html                     # HTML template
├── vite.config.ts                 # Vite configuration
├── tailwind.config.js             # Tailwind CSS config
├── postcss.config.js              # PostCSS config
├── tsconfig.json                  # TypeScript config
├── tsconfig.node.json             # TypeScript config for Node
├── .eslintrc.cjs                  # ESLint config
├── .editorconfig                  # Editor config
├── .gitignore                     # Git ignore patterns
├── .env.development               # Development environment variables
├── .env.production                # Production environment variables
├── package.json                   # Dependencies and scripts
└── README.md                      # Frontend documentation
```

## Key Features Implemented

### 1. Modern Tech Stack
- ✅ React 18 with TypeScript
- ✅ Vite for fast development and optimized builds
- ✅ Tailwind CSS for styling
- ✅ TanStack Query for server state management
- ✅ React Router for client-side routing

### 2. Mobile-First Design
- ✅ Responsive layout components
- ✅ Mobile-friendly navigation with hamburger menu
- ✅ Touch-optimized UI components
- ✅ Safe area insets support

### 3. Background Images
- ✅ Component that rotates background images
- ✅ Fetches image list from backend API
- ✅ Configurable rotation interval
- ✅ Smooth transitions with overlay for readability

### 4. API Integration
- ✅ Type-safe API client with fetch utilities
- ✅ Support for Basic Auth (admin endpoints)
- ✅ Error handling with custom ApiError class
- ✅ Request/response type definitions
- ✅ Service methods for all planned endpoints:
  - Leaderboards
  - Players (list, detail)
  - Matches (list, detail, filters)
  - Doodle (upcoming, get, update availability)
  - Teams generator
  - Background images
  - App configuration
  - Admin (add player, add match)

### 5. TypeScript Types
- ✅ Complete domain models (Player, Match, Season, etc.)
- ✅ API request/response DTOs
- ✅ Strongly typed throughout

### 6. Build Configuration
- ✅ Vite configured to output to `../Elo-fotbalek/wwwroot`
- ✅ Dev server proxy to backend at `https://localhost:5001`
- ✅ Environment-specific configuration
- ✅ TypeScript path aliases (`@/*` → `./src/*`)

### 7. Base UI Components
- ✅ Button (multiple variants and sizes)
- ✅ Card (with Header, Title, Content, Footer)
- ✅ Loading spinner and page loader
- ✅ Error display component
- ✅ Layout with navigation and footer
- ✅ Background image rotator

### 8. Routing Structure
- ✅ All required routes defined
- ✅ Public routes (leaderboard, doodle, teams, players, matches)
- ✅ Admin routes (add player, add match)
- ✅ 404 page
- ✅ Placeholder pages ready for implementation

## Next Steps

### 1. Install Dependencies
```bash
cd frontend
npm install
```

### 2. Backend API Implementation
The backend needs to implement these JSON API endpoints:
- `GET /api/leaderboards?season={season}`
- `GET /api/players`
- `GET /api/players/{id}`
- `GET /api/matches`
- `GET /api/matches/{id}`
- `GET /api/doodle/upcoming?count=5`
- `GET /api/doodle/{date}`
- `PUT /api/doodle/{date}/availability`
- `POST /api/teams/generate`
- `GET /api/background-images`
- `GET /api/config`
- `POST /api/admin/players` (Basic Auth)
- `POST /api/admin/matches` (Basic Auth)

### 3. Backend Configuration
- Configure static file serving from `wwwroot`
- Add SPA fallback routing (serve index.html for client routes)
- Enable CORS if needed during development

### 4. Implement Pages
Each page component is currently a placeholder. Implement them one by one:
1. **HomePage** - Leaderboard with regular and "lazy bitches" sections
2. **DoodlePage** - Attendance marking for upcoming Tuesdays
3. **TeamsPage** - Team generator with load more functionality
4. **PlayersPage** - Searchable player list
5. **PlayerDetailPage** - Player stats with Elo chart
6. **MatchesPage** - Match history with filters
7. **MatchDetailPage** - Match details view
8. **AddPlayerPage** - Admin form to add player
9. **AddMatchPage** - Admin form to add match (mobile-optimized)

### 5. Additional Features to Build
- Auth context for admin mode management
- Form validation utilities
- More UI components as needed (Input, Select, etc.)
- Elo chart component using Recharts
- Team card component for team generator results
- Player card component for lists

## Development Workflow

1. **Start backend** (ASP.NET Core on port 5001)
2. **Start frontend** dev server:
   ```bash
   cd frontend
   npm run dev
   ```
3. **Access app** at http://localhost:5173
4. **API requests** automatically proxy to backend

## Build for Production

```bash
cd frontend
npm run build
```

Output goes to `../Elo-fotbalek/wwwroot` and is ready to be served by the backend.

## Notes

- All pages are currently placeholders showing "coming soon" messages
- API endpoints are defined but backend implementation is needed
- Mobile-first responsive design is in place
- TypeScript ensures type safety throughout
- Follow UI_Rewrite_Specification.md for implementation details
- DO NOT change domain logic - it stays in the backend

## Aligned with Specification ✅

This setup follows all requirements from `UI_Rewrite_Specification.md`:
- ✅ React + TypeScript frontend
- ✅ Vite build tool
- ✅ Mobile-first design
- ✅ All required pages and routes defined
- ✅ API service layer ready
- ✅ Basic Auth support for admin endpoints
- ✅ Background image rotation
- ✅ Single App Service deployment (backend serves frontend)
- ✅ No domain logic in frontend
- ✅ Backend remains source of truth
