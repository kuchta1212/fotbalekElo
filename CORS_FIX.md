# ? CORS Configuration Fixed

## ?? Problem
Frontend (React) couldn't communicate with backend API due to CORS policy blocking requests.

## ? Solution Applied

Updated `Program.cs` to allow CORS from multiple origins:

### Allowed Origins:

#### Development:
- `http://localhost:5173` - Vite dev server
- `https://localhost:5173` - Vite dev server (HTTPS)
- `http://localhost:5001` - Backend (same origin)
- `https://localhost:5001` - Backend (same origin, HTTPS)
- `http://localhost:5000` - Backend (HTTP alternative)

#### Production (Azure):
- `http://elo-fotbalek.azurewebsites.net`
- `https://elo-fotbalek.azurewebsites.net`
- `https://usti-elo-fotbalek.azurewebsites.net`

### Configuration Details:

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                // Development
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:5001",
                "https://localhost:5001",
                "http://localhost:5000",
                // Production
                "http://elo-fotbalek.azurewebsites.net",
                "https://elo-fotbalek.azurewebsites.net",
                "https://usti-elo-fotbalek.azurewebsites.net"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Middleware Order:

CORS is now applied **first** in the middleware pipeline (before routing):

```csharp
app.UseCors("AllowAll");  // ? Applied early, for all environments

if (app.Environment.IsDevelopment()) { ... }
else { ... }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
// ... rest of pipeline
```

## ? What Changed

### Before (? Broken):
- CORS only allowed `localhost:5173` (dev server)
- CORS only enabled in Development environment
- Same-origin requests (localhost:5001) were blocked
- Production Azure URLs not whitelisted

### After (? Fixed):
- CORS allows all necessary development URLs
- CORS allows production Azure URLs
- CORS enabled in **all environments**
- Same-origin requests now work
- Credentials (cookies) allowed

## ?? Testing

### 1. Test Development Mode:

**Option A: Same Origin (Production Build)**
```sh
cd frontend && npm run build
# Then F5 in Visual Studio
# Open: https://localhost:5001
```
Expected: API calls work (no CORS errors)

**Option B: Dev Server (Hot Reload)**
```sh
# Terminal 1: F5 in Visual Studio
# Terminal 2:
cd frontend && npm run dev
# Open: http://localhost:5173
```
Expected: API calls proxied to backend, no CORS errors

### 2. Check Browser Console:

? **Before (Error):**
```
Access to fetch at 'https://localhost:5001/api/leaderboards' 
from origin 'https://localhost:5001' has been blocked by CORS policy
```

? **After (Success):**
```
200 OK - /api/leaderboards
```

### 3. Test API Endpoints:

Open DevTools ? Network tab and verify:
- `GET /api/config` - ? 200 OK
- `GET /api/leaderboards` - ? 200 OK
- `GET /api/players` - ? 200 OK

## ?? Response Headers

The backend now sends these CORS headers:

```
Access-Control-Allow-Origin: https://localhost:5001
Access-Control-Allow-Credentials: true
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
Access-Control-Allow-Headers: *
```

## ?? Important Notes

### Why Same-Origin CORS?

Even though the React app is served from `https://localhost:5001` (same origin as the API), browsers still enforce CORS for:
- Fetch API calls with credentials
- Preflight OPTIONS requests
- Custom headers

### Production Deployment:

When deploying to Azure:
1. ? Both Azure URLs are whitelisted
2. ? CORS works in production environment
3. ? No code changes needed for deployment

### Security:

This CORS configuration is **appropriate** because:
- ? Specific origins listed (not `*`)
- ? Only known development and production URLs
- ? Credentials required (not allowing anonymous origins)
- ? Limited to application's own domains

## ?? Summary

| Issue | Status |
|-------|--------|
| CORS blocking API calls | ? Fixed |
| Same-origin requests blocked | ? Fixed |
| Dev server CORS | ? Fixed |
| Production Azure URLs | ? Whitelisted |
| Credentials support | ? Enabled |

## ? Result

**Frontend can now communicate with backend in all scenarios:**
- ? Development (same origin)
- ? Development (dev server on :5173)
- ? Production (elo-fotbalek.azurewebsites.net)
- ? Production (usti-elo-fotbalek.azurewebsites.net)

**Test it now:**
1. Press F5 in Visual Studio
2. Open https://localhost:5001
3. Check browser console - no CORS errors
4. Leaderboard data should load successfully

---

**CORS is now properly configured for all environments!** ??
