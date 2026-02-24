# ?? UI Rewrite Progress - Current Status

## ? Completed Features

### 1. Infrastructure & Setup
- ? .NET upgraded from 3.1 ? .NET 8
- ? React + TypeScript + Vite frontend scaffolded
- ? Build pipeline working (frontend ? `wwwroot`)
- ? SPA fallback routing configured
- ? CORS enabled for development
- ? Visual Studio F5 launches correctly on port 5001
- ? Background images rotation system
- ? Azure Blob Storage migrated to new SDK

### 2. Backend API Endpoints ?

| Endpoint | Status | Description |
|----------|--------|-------------|
| `GET /api/config` | ? | App configuration |
| `GET /api/background-images` | ? | Background image paths |
| `GET /api/players` | ? | List all players |
| `GET /api/players/{id}` | ? | Player detail with stats & Elo history |
| `GET /api/leaderboards?season={season}` | ? | Leaderboard data |
| `GET /api/doodle/upcoming?count=5` | ? | Upcoming Tuesdays with attendance |
| `GET /api/doodle/{date}` | ? | Doodle for specific date |
| `PUT /api/doodle/{date}/availability` | ? | Update player availability |
| `GET /api/matches` | ? | Not yet implemented |
| `GET /api/matches/{id}` | ? | Not yet implemented |
| `POST /api/teams/generate` | ? | Not yet implemented |
| `POST /api/admin/players` | ? | Not yet implemented |
| `POST /api/admin/matches` | ? | Not yet implemented |

### 3. Frontend Pages

| Page | Route | Status | Features |
|------|-------|--------|----------|
| **Leaderboard** | `/` | ? Complete | Season selector, regulars/non-regulars split, recent matches, trends, Czech translations, mobile-first |
| **Player Detail** | `/players/:id` | ? Complete | Stats, Elo history chart, seasonal breakdown, attendance |
| **Doodle** | `/doodle` | ? Complete | Next 5 Tuesdays, mark attendance, stats summary, generate teams button |
| **Players List** | `/players` | ? Placeholder | Player directory, searchable |
| **Matches** | `/matches` | ? Placeholder | Match history, filters |
| **Match Detail** | `/matches/:id` | ? Placeholder | Individual match view |
| **Teams Generator** | `/teams` | ? Placeholder | Team options, load more |
| **Add Player** | `/admin/add-player` | ? Placeholder | Admin: add new player |
| **Add Match** | `/admin/add-match` | ? Placeholder | Admin: enter match results |

---

## ?? Progress Summary

### Overall Completion: ~45%

**Breakdown:**
- Infrastructure: 100% ?
- Backend APIs: 50% (5/10 endpoints)
- Frontend Pages: 33% (3/9 pages)
- Domain Logic: 0% changed (as required) ?

---

## ?? Next Recommended Steps

Following the specification priorities and logical flow:

### Phase 1: Complete Public Pages (High Priority)

#### 1. **Teams Generator** (`/teams` + API)
**Why:** Critical user flow - Doodle ? Teams ? Match Entry

**Backend:**
- `POST /api/teams/generate`
  - Accept: date, season, (optional) playerIds
  - Call existing `ITeamGenerator.GenerateTeams()`
  - Return: team options sorted by balance

**Frontend:**
- Query params: `?date=yyyy-MM-dd&season=Summer|Winter`
- Display team options as cards
- Show first 10, "Load more" for rest
- Team A vs Team B with player lists + Elo
- Admin button: "Use this setup to enter result"

#### 2. **Matches Page** (`/matches` + API)
**Why:** Users want to see match history, check past results

**Backend:**
- `GET /api/matches`
  - Filters: season, dateFrom, dateTo, matchType
  - Pagination: skip, take
  - Return: match list with basic info

- `GET /api/matches/{id}`
  - Return: full match details, teams, players, scores

**Frontend:**
- Mobile-friendly list of matches
- Filters: season, date range, match type
- Click match ? navigate to detail page
- Match detail: teams, score, Jirka Lu?ák, date, season

#### 3. **Players List** (`/players`)
**Why:** Complete the player journey (leaderboard ? list ? detail)

**Backend:**
- Endpoint already exists (`GET /api/players`)

**Frontend:**
- Searchable player directory
- Filter: regulars vs non-regulars
- Card-based layout
- Click player ? detail page
- Sort by: name, Elo, attendance

---

### Phase 2: Admin Features (Medium Priority)

#### 4. **Add Match Page** (`/admin/add-match` + API)
**Why:** Core admin workflow, can be prefilled from teams

**Backend:**
- `POST /api/admin/matches`
  - Require Basic Auth
  - Accept: all match fields
  - Call existing match creation logic
  - Calculate Elo changes
  - Save to blob storage

**Frontend:**
- Date/time picker (default: now)
- Season selector (if enabled)
- Match type: Big/Small
- Team A/B player multi-select
- Score inputs
- Winner/loser radio buttons
- Optional: Jirka Lu?ák selector
- Can be prefilled from `/teams?setupId=...`

#### 5. **Add Player Page** (`/admin/add-player` + API)
**Why:** Admin needs to add new players

**Backend:**
- `POST /api/admin/players`
  - Require Basic Auth
  - Accept: name, (optional) initialElo
  - Create player with default values
  - Save to blob storage

**Frontend:**
- Simple form: name input
- Optional: initial Elo (default 1000)
- Success message
- Redirect to player detail or players list

---

### Phase 3: Polish & Deploy (Low Priority)

#### 6. **Admin Mode Toggle**
- Toggle in navigation
- Password prompt (Basic Auth)
- Session persistence (cookie-based)
- Show/hide admin menu items

#### 7. **Error Handling**
- Global error boundary
- Toast notifications for actions
- Retry logic for failed requests
- Offline detection

#### 8. **Performance**
- Code splitting (lazy load pages)
- Optimize bundle size
- Image optimization
- Service worker (optional)

---

## ?? Feature Parity Checklist

Compared to old Razor views:

| Feature | Old UI | New UI | Status |
|---------|--------|--------|--------|
| Leaderboard | ? | ? | Complete |
| Player Stats | ? | ? | Complete |
| Doodle | ? | ? | Complete |
| Team Generator | ? | ? | TODO |
| Match History | ? | ? | TODO |
| Match Detail | ? | ? | TODO |
| Add Match | ? | ? | TODO |
| Add Player | ? | ? | TODO |
| Players List | ? | ? | New feature |

---

## ?? Specification Compliance

### ? Following Rules:
- ? NOT changing Elo calculation logic
- ? NOT changing team generation rules
- ? NOT changing season behavior
- ? NOT changing leaderboard rules
- ? NOT introducing user accounts
- ? NOT reimplementing domain logic in client
- ? Upgraded .NET to 8 (LTS)
- ? Introduced JSON REST APIs
- ? Built React + TypeScript frontend
- ? Improved UX, layout, navigation
- ? Reusing existing logic/services

### Backend = Source of Truth ?
- All calculations server-side
- Frontend is presentational only
- No domain logic in React
- APIs wrap existing services
- Blob storage format unchanged

---

## ?? Mobile-First Design Achieved

### UI Patterns Used:
- ? Card-based layouts (no dense tables)
- ? Touch-friendly buttons (min 44px)
- ? Responsive grid (1/2/3 columns)
- ? Stacked on mobile, side-by-side on desktop
- ? Large font sizes for readability
- ? Color-coded status (green/orange/red)
- ? Icons + labels for clarity
- ? Glassmorphism (semi-transparent cards)
- ? Smooth transitions and hover effects

### Accessibility:
- ? Semantic HTML
- ? ARIA labels where needed
- ? Keyboard navigation support
- ? High contrast colors
- ? Loading states
- ? Error messages

---

## ?? Testing Status

### Tested & Working:
- ? Frontend builds successfully
- ? Backend builds successfully
- ? F5 launches app on port 5001
- ? React UI loads correctly
- ? API endpoints respond
- ? Leaderboard page functional
- ? Player detail page functional
- ? Doodle page functional
- ? Background images rotate
- ? Mobile responsive design
- ? Czech translations display

### Not Yet Tested:
- ? Admin Basic Auth flow
- ? Teams generator
- ? Match creation
- ? Player creation
- ? End-to-end user flows
- ? Production deployment

---

## ?? Documentation Created

| Document | Purpose |
|----------|---------|
| `SETUP_COMPLETION_SUMMARY.md` | Initial setup overview |
| `QUICK_START.md` | How to run the app |
| `LEADERBOARD_IMPLEMENTATION.md` | Leaderboard feature details |
| `PLAYER_DETAIL_PAGE.md` | Player detail feature details |
| `DOODLE_IMPLEMENTATION.md` | Doodle feature details |
| `UI_Rewrite_Specification.md` | Original requirements |
| Various fix/update docs | Issue resolutions |

---

## ?? Recommended Next Task

**Implement Teams Generator (`/teams` + API)**

**Why this next:**
1. Completes critical user flow: Doodle ? Teams ? Match
2. Doodle already has "Generate Teams" button
3. Reuses existing `ITeamGenerator` logic
4. Enables testing full workflow
5. Relatively straightforward (just wraps existing code)

**Effort Estimate:**
- Backend API: ~30 min
- Frontend Page: ~1 hour
- Testing: ~30 min
- **Total: ~2 hours**

Would you like me to implement the Teams Generator next, or would you prefer a different feature?
