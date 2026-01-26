# fotbalekElo – UI Rewrite Specification (React + .NET)

Repository: **fotbalekElo**  
Project: **Elo-fotbalek**

---

## Purpose

You are working on **Elo-fotbalek**, a football (soccer) game tracker used by a group of friends (~40–50 users), almost exclusively on mobile phones.

The application already exists and is fully functional, but the **UI/UX is outdated and not mobile-friendly**.

The goal of this work is to:
- **Rewrite the UI** into a modern, mobile-first experience
- **Preserve all existing domain logic, rules, and behavior**
- Keep the app public, simple, and personal

This is primarily a **front-end rewrite with a supporting API layer**, not a product redesign.

---

## Core Rules (VERY IMPORTANT)

### DO NOT
- ❌ Change Elo calculation logic
- ❌ Change team generation rules
- ❌ Change season behavior (Winter/Summer)
- ❌ Change leaderboard rules (including “Lazy bitches” logic)
- ❌ Introduce real user accounts or identity
- ❌ Reimplement domain logic in the client

### YOU MAY
- ✅ Upgrade .NET to a supported LTS version (recommended: .NET 8)
- ✅ Introduce JSON REST API endpoints
- ✅ Refactor controllers/views as needed
- ✅ Build a React + TypeScript frontend
- ✅ Improve UX, layout, navigation, and performance
- ✅ Add new endpoints that reuse existing logic/services

**Backend remains the single source of truth.**

---

## Existing System Summary

- Hosting: Azure App Service
- Backend: ASP.NET Core (currently 3.1, can be upgraded)
- Data storage: JSON files stored in Azure Data Lake
- No JSON API exists today (HTML-only)
- No Swagger / OpenAPI
- Auth:
  - App is public
  - Admin actions protected by **Basic Auth**
  - Basic Auth creates a claims-based principal
  - Admin endpoints are protected via authorization policy

---

## Users & Usage Pattern

- 40–50 players
- Almost all usage is on **mobile phones**
- Typical flows:
  - Mark attendance (doodle) before game
  - Generate teams at the pitch or pub
  - Enter results later (usually at home)
  - Check leaderboard and stats socially after games

---

## Domain Concepts (Must Be Preserved)

### Seasons
- Two seasons: **Winter** and **Summer**
- Season is **always selected manually** (never inferred)
- Each player has:
  - Winter Elo
  - Summer Elo
  - Overall Elo
- Season affects:
  - team generation
  - Elo calculations
  - stats and leaderboards

### Matches
A match consists of:
- Date & time (UI must allow manual selection)
- Season (Winter / Summer)
- Match type:
  - **Big match**
  - **Small match**
  (affects Elo weighting)
- Team A players
- Team B players
- Score: Team A score + Team B score
- Explicit winner and loser selection
- Optional special field:
  - **“Jirka Lunak”** (biggest loser of the match)
  - Exactly one player
  - Optional
  - Only shown in UI if set

### Attendance (Doodle)
- Matches are played every **Tuesday**
- Show next ~4–5 Tuesdays
- For each date, players can be marked:
  - Yes / No / Maybe
- There is **no identity or protection**
  - Anyone can set availability for anyone
  - This behavior must stay

### Teams Generator
- Uses:
  - attendance snapshot
  - selected season
  - players’ Elo values
- Generates multiple possible team splits aiming for similar team Elo
- Sometimes generates many options
- UI should:
  - show first 10 options
  - provide “Load more”

---

## Target Architecture

### Backend
- ASP.NET Core (.NET 8 recommended)
- Same Azure App Service
- Same Data Lake JSON storage
- Add REST-style JSON endpoints
- All calculations and logic remain server-side

### Frontend
- React
- TypeScript
- Vite
- Mobile-first responsive design
- Modern card-based UI

Recommended libraries:
- Tailwind CSS + shadcn/ui
- React Router
- TanStack Query
- Recharts (for Elo-over-time charts)

### Deployment
- **Single App Service**
- React build output served as static files by backend
- Backend also serves API
- SPA fallback routing required
- CI/CD must build frontend + backend

---

## Authentication & Admin Mode

- Admin actions are protected by **Basic Auth**
- React UI must support:
  - “Admin mode” toggle
  - Password prompt (Basic Auth challenge)
  - Session persistence via server (cookies)
- No fake or client-side auth
- Public users can browse everything else freely

---

## Required Pages & Routes

### Public

#### `/` – Leaderboard (Home)
- Default landing page
- Two sections:
  - Regular leaderboard
  - “Lazy bitches” leaderboard
- Player rows are clickable → player detail
- Quick access to:
  - Doodle
  - Match history

#### `/doodle`
- Show next 4–5 Tuesdays
- Select active date
- List all players
- Each player has Yes / Maybe / No controls
- CTA: **Generate teams**
  - navigates to `/teams?date=YYYY-MM-DD&season=Winter|Summer`

#### `/teams`
- Requires date + season
- Shows generated team options as cards:
  - Team A list
  - Team B list
  - Team Elo summary
- First 10 options visible
- “Load more” button
- Admin-only action:
  - “Use this setup to enter result”

#### `/players`
- Player directory
- Searchable

#### `/players/:id`
- Player stats page:
  - availability count
  - highest / lowest Elo
  - current Elo (Overall + Winter + Summer)
  - Elo-over-time chart (selectable season)

#### `/matches`
- Match history
- Mobile-friendly list
- Optional filters:
  - season
  - date range
  - match type

#### `/matches/:id` (optional but recommended)
- Match detail view
- Show teams, score, season, type
- Show “Jirka Lunak” only if set

---

### Admin Only

#### `/admin/add-player`
- Add new player form

#### `/admin/add-match`
- Match entry form
- Can be prefilled from Teams Generator
- Optimized for mobile input

---

## Add Match UX (High Priority)

Form fields:
1. Date & time picker (default: now)
2. Season selector (required)
3. Match type: Big / Small
4. Teams:
   - Select players for Team A and Team B
   - OR prefilled from team generator
5. Score:
   - Team A score
   - Team B score
6. Explicit winner / loser selector
7. Optional “Jirka Lunak” player selector

After submit:
- Show success/error clearly
- Option to:
  - Add another match
  - Go to leaderboard

---

## Background Images

- Images are local static files
- Path configured in app settings
- React UI should fetch available images via backend endpoint
- Rotate background every N seconds (configurable)
- Use overlay/blur to preserve readability

---

## API Requirements (New JSON API)

No API exists today. Introduce JSON endpoints that wrap existing logic.

### Public

- `GET /api/leaderboards?season=overall|winter|summer`
- `GET /api/players`
- `GET /api/players/{id}`
- `GET /api/matches`
- `GET /api/matches/{id}`
- `GET /api/doodle/upcoming?count=5`
- `GET /api/doodle/{date}`
- `PUT /api/doodle/{date}/availability`
- `POST /api/teams/generate`
- `GET /api/background-images`

### Admin (auth policy required)

- `POST /api/admin/players`
- `POST /api/admin/matches`

**All APIs must reuse existing domain logic and storage.**

---

## Frontend Guidelines

- Use TypeScript everywhere
- Strongly typed DTOs
- Mobile-first layout
- No dense tables on mobile
- Use cards and lists
- Proper loading and error states
- Accessibility-friendly controls

---

## Acceptance Criteria

- App is fully usable on mobile browsers
- Entering match results is comfortable on phone
- All existing logic behaves exactly as before
- No data incompatibility introduced
- Admin mode works via Basic Auth
- Background images still rotate

---

## Final Notes

- This is a **UI modernization**, not a product rewrite
- Keep behavior boring and predictable
- Prefer clarity over cleverness
- When in doubt: ask before changing logic

