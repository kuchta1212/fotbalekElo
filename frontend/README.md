# Elo-fotbalek Frontend

Modern, mobile-first React + TypeScript frontend for the Elo-fotbalek application.

## Tech Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **React Router** - Client-side routing
- **TanStack Query** - Server state management
- **Tailwind CSS** - Utility-first styling
- **Recharts** - Data visualization

## Getting Started

### Prerequisites

- Node.js 18+ and npm

### Installation

```bash
# Install dependencies
npm install
```

### Development

```bash
# Start dev server (runs on http://localhost:5173)
npm run dev
```

The dev server will proxy API requests to the backend at `https://localhost:5001`.

### Build for Production

```bash
# Build for production
npm run build
```

This will output the production build to `../Elo-fotbalek/wwwroot`, ready to be served by the ASP.NET Core backend.

### Preview Production Build

```bash
npm run preview
```

## Project Structure

```
frontend/
├── src/
│   ├── components/       # Reusable UI components
│   │   ├── Layout.tsx
│   │   ├── Navigation.tsx
│   │   └── BackgroundImages.tsx
│   ├── pages/            # Page components (route handlers)
│   │   ├── HomePage.tsx
│   │   ├── DoodlePage.tsx
│   │   ├── TeamsPage.tsx
│   │   ├── PlayersPage.tsx
│   │   ├── MatchesPage.tsx
│   │   └── admin/
│   │       ├── AddPlayerPage.tsx
│   │       └── AddMatchPage.tsx
│   ├── services/         # API client and service layer
│   │   ├── api.ts        # Base fetch utilities
│   │   └── apiService.ts # API endpoint methods
│   ├── types/            # TypeScript type definitions
│   │   ├── domain.ts     # Domain models
│   │   └── api.ts        # API DTOs
│   ├── lib/              # Utility functions
│   │   └── utils.ts
│   ├── App.tsx           # Root component with routing
│   ├── main.tsx          # Entry point
│   └── index.css         # Global styles
├── public/               # Static assets
├── index.html            # HTML template
├── vite.config.ts        # Vite configuration
├── tailwind.config.js    # Tailwind CSS configuration
├── tsconfig.json         # TypeScript configuration
└── package.json          # Dependencies and scripts
```

## Key Features

### Mobile-First Design
- Responsive layouts optimized for mobile devices
- Touch-friendly UI components
- Safe area insets for notched devices

### Background Images
- Rotating background images from backend configuration
- Smooth transitions between images
- Configurable rotation interval

### API Integration
- Type-safe API client with TypeScript
- Automatic request/response handling
- Error handling and retry logic
- Support for Basic Auth (admin endpoints)

### State Management
- TanStack Query for server state
- Automatic caching and refetching
- Optimistic updates support

## Environment Variables

- `VITE_API_BASE_URL` - Base URL for API requests
  - Development: `https://localhost:5001`
  - Production: `/` (same origin)

## Development Notes

### API Endpoints

All API endpoints must be implemented in the backend. The frontend expects:

**Public endpoints:**
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

**Admin endpoints (Basic Auth required):**
- `POST /api/admin/players`
- `POST /api/admin/matches`

### Adding New Components

When creating new components:
1. Use TypeScript for type safety
2. Follow mobile-first responsive design
3. Use Tailwind CSS for styling
4. Leverage TanStack Query for data fetching
5. Handle loading and error states

### Code Style

- Use functional components with hooks
- Prefer named exports over default exports (except for pages)
- Use TypeScript strict mode
- Follow existing patterns for consistency

## Deployment

The frontend is designed to be served by the ASP.NET Core backend:

1. Build the frontend: `npm run build`
2. Output goes to `../Elo-fotbalek/wwwroot`
3. Backend serves static files and handles SPA fallback routing
4. API endpoints are served from the same origin

## Contributing

When implementing new features:
1. Follow the UI_Rewrite_Specification.md
2. Do NOT change domain logic (stays in backend)
3. Maintain mobile-first approach
4. Keep existing behavior intact
5. Test on mobile devices
