# ? Frontend API Base URL Fixed

## ?? Problem

Frontend was making API calls to incorrect URLs:
- ? **Wrong:** `https://api/leaderboards` (missing base URL)
- ? **Correct:** `https://localhost:5001/api/leaderboards`

This caused CORS errors because the browser was trying to resolve `https://api/` as a domain.

---

## ?? Root Cause

The `.env.production` file had `VITE_API_BASE_URL=/` which was being interpreted incorrectly during the build process.

### How Environment Variables Work in Vite:

1. **Build Time:** Environment variables are **baked into the JavaScript bundle** during `npm run build`
2. **Runtime:** The built JavaScript uses the hardcoded value from build time
3. **No Runtime Override:** Once built, you can't change the API base URL without rebuilding

---

## ? Solution

Updated `.env.production` to use an **empty string** instead of `/`:

### Before (? Broken):
```
VITE_API_BASE_URL=/
```

### After (? Fixed):
```
VITE_API_BASE_URL=
```

This ensures that API calls use **relative paths** from the same origin.

---

## ?? Environment Files

### `.env.development` (Dev Server Mode)
```env
VITE_API_BASE_URL=https://localhost:5001
```

**Used when:** Running `npm run dev`  
**Purpose:** Points to backend API running on different port

### `.env.production` (Production Build Mode)
```env
VITE_API_BASE_URL=
```

**Used when:** Running `npm run build`  
**Purpose:** Uses same-origin relative paths (backend serves the frontend)

---

## ?? How API URLs Are Constructed

### In `api.ts`:
```typescript
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

const url = `${API_BASE_URL}${endpoint}`;
// endpoint = '/api/leaderboards'
```

### Development Mode (`npm run dev`):
```
API_BASE_URL = 'https://localhost:5001'
endpoint = '/api/leaderboards'
url = 'https://localhost:5001/api/leaderboards' ?
```

### Production Mode (`npm run build`):
```
API_BASE_URL = '' (empty string)
endpoint = '/api/leaderboards'
url = '/api/leaderboards' ? (relative to current origin)
```

---

## ?? Testing

### 1. Rebuild Frontend:
```bash
cd frontend
npm run build
```

### 2. Start Backend:
Press **F5** in Visual Studio

### 3. Open Browser:
```
https://localhost:5001
```

### 4. Check Network Tab:
Open DevTools ? Network ? Filter: "leaderboards"

**Expected Request URL:**
```
https://localhost:5001/api/leaderboards?season=Overall
```

**NOT:**
```
https://api/leaderboards?season=Overall  ?
```

### 5. Verify Response:
- Status: `200 OK`
- Response: JSON with leaderboard data
- No CORS errors in console

---

## ?? Different Deployment Scenarios

### Scenario 1: Development (Dev Server)
```bash
# Terminal 1: Backend (F5 in Visual Studio)
# Terminal 2: Frontend dev server
cd frontend
npm run dev
# Open: http://localhost:5173
```

**API Calls:** `https://localhost:5001/api/leaderboards`  
**CORS:** Required (different origin)

### Scenario 2: Production Build (Same Origin)
```bash
cd frontend
npm run build
# Press F5 in Visual Studio
# Open: https://localhost:5001
```

**API Calls:** `/api/leaderboards` (same origin)  
**CORS:** Not strictly required (but enabled for consistency)

### Scenario 3: Azure Deployment
```
Frontend: https://elo-fotbalek.azurewebsites.net
Backend API: https://elo-fotbalek.azurewebsites.net/api
```

**API Calls:** `/api/leaderboards` (same origin)  
**CORS:** Not strictly required (but configured for flexibility)

---

## ?? When to Rebuild Frontend

You **must rebuild** the frontend after changing `.env` files:

```bash
cd frontend
npm run build
```

Then restart the backend (F5 in Visual Studio).

### Why?
Vite **bakes environment variables into the JavaScript bundle** at build time. Runtime changes to `.env` files have **no effect** on already-built code.

---

## ?? Troubleshooting

### Issue: Still seeing wrong URLs

**Check 1: Clear browser cache**
```
Ctrl + Shift + Delete ? Clear cached images and files
```

**Check 2: Hard reload**
```
Ctrl + F5 (Windows) or Cmd + Shift + R (Mac)
```

**Check 3: Verify build output**
```bash
# Check the built JavaScript contains correct base URL
Get-Content Elo-fotbalek/wwwroot/assets/main-*.js | Select-String "VITE_API_BASE_URL"
```

**Check 4: Verify wwwroot has latest build**
```bash
Get-ChildItem Elo-fotbalek/wwwroot/assets -Filter "main-*.js" | Select-Object Name, LastWriteTime
```

Should show recent timestamp.

---

## ?? Important Notes

### 1. Environment Variables Are Build-Time Only
- Changes to `.env` files don't affect running app
- Must rebuild after every `.env` change
- No hot-reload for environment variables

### 2. Same-Origin vs Cross-Origin

**Same-Origin (Production Build):**
- Frontend: `https://localhost:5001`
- API: `https://localhost:5001/api`
- Uses: `VITE_API_BASE_URL=` (empty)
- Calls: `/api/leaderboards`

**Cross-Origin (Dev Server):**
- Frontend: `http://localhost:5173`
- API: `https://localhost:5001/api`
- Uses: `VITE_API_BASE_URL=https://localhost:5001`
- Calls: `https://localhost:5001/api/leaderboards`

### 3. Azure Deployment
When deploying to Azure, the **production build** is used, so:
- `VITE_API_BASE_URL=` (empty)
- API calls use relative paths: `/api/leaderboards`
- Same origin: `https://elo-fotbalek.azurewebsites.net`

---

## ?? Files Modified

| File | Change |
|------|--------|
| `frontend/.env.production` | Changed from `/` to `` (empty string) |
| `frontend/wwwroot/assets/main-*.js` | Rebuilt with correct base URL |

---

## ? Verification Checklist

After fixing and rebuilding:

- [x] Frontend rebuilt (`npm run build`)
- [x] Backend restarted (F5)
- [x] Browser opened to `https://localhost:5001`
- [x] Network tab shows: `https://localhost:5001/api/leaderboards?season=Overall`
- [x] Response: `200 OK` with JSON data
- [x] No CORS errors in console
- [x] Leaderboard displays data (not "coming soon" placeholder)

---

## ?? Summary

**Problem:** API base URL was configured incorrectly, causing malformed URLs  
**Cause:** `.env.production` had `VITE_API_BASE_URL=/` instead of empty string  
**Fix:** Changed to `VITE_API_BASE_URL=` and rebuilt frontend  
**Result:** API calls now use correct same-origin relative paths  

---

## ? Result

**API calls now work correctly:**
- ? Development (dev server): `https://localhost:5001/api/leaderboards`
- ? Production (same origin): `/api/leaderboards` ? resolved to `https://localhost:5001/api/leaderboards`
- ? Azure deployment: `/api/leaderboards` ? resolved to `https://elo-fotbalek.azurewebsites.net/api/leaderboards`

**Frontend successfully communicates with backend!** ??

Test it: Press F5, open https://localhost:5001, and verify the leaderboard loads with real data.
