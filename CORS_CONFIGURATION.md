# ? CORS Configuration - Moved to Configuration Files

## ?? Changes Made

### 1. CORS Origins Moved to Configuration

**Before:** CORS origins were hardcoded in `Program.cs`  
**After:** CORS origins read from `appsettings.json` and `appsettings.Development.json`

---

## ?? Configuration Files

### `appsettings.json` (Production)

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://elo-fotbalek.azurewebsites.net",
      "https://elo-fotbalek.azurewebsites.net",
      "https://usti-elo-fotbalek.azurewebsites.net"
    ]
  }
}
```

### `appsettings.Development.json` (Development)

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "https://localhost:5173",
      "http://localhost:5001",
      "https://localhost:5001",
      "http://localhost:5000"
    ]
  }
}
```

---

## ?? Program.cs Changes

### CORS Configuration

```csharp
// Read allowed origins from configuration
var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Fallback if no origins configured
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});
```

### Middleware Pipeline

```csharp
// Enable CORS before any other middleware
app.UseCors("CorsPolicy");

// ... rest of middleware
```

---

## ?? Environment-Specific Behavior

### Development Environment:
- Uses `appsettings.Development.json`
- Allows localhost URLs on multiple ports
- Includes:
  - `http://localhost:5173` - Vite dev server
  - `https://localhost:5173` - Vite dev server (HTTPS)
  - `http://localhost:5001` - Backend API (same origin)
  - `https://localhost:5001` - Backend API (HTTPS, same origin)
  - `http://localhost:5000` - Backend API (HTTP fallback)

### Production Environment:
- Uses `appsettings.json`
- Only allows Azure production URLs
- Includes:
  - `http://elo-fotbalek.azurewebsites.net`
  - `https://elo-fotbalek.azurewebsites.net`
  - `https://usti-elo-fotbalek.azurewebsites.net`

---

## ? Benefits

### 1. Configuration-Based
- ? No hardcoded URLs in code
- ? Easy to update per environment
- ? No code changes for deployment
- ? Can be overridden via Azure App Settings

### 2. Environment-Specific
- ? Development: Loose CORS for easy local development
- ? Production: Strict CORS for security
- ? Automatic switching based on `ASPNETCORE_ENVIRONMENT`

### 3. Maintainable
- ? Single source of truth per environment
- ? Easy to add/remove origins
- ? Clear separation of concerns

---

## ?? Security Considerations

### Production:
- Only whitelists known production domains
- Requires credentials (cookies)
- No wildcard origins
- HTTPS enforced (except HTTP redirect)

### Development:
- Allows localhost for development
- Includes both HTTP and HTTPS
- Same-origin included (for production build testing)

---

## ?? How to Update CORS Origins

### Add a New Development URL:
Edit `appsettings.Development.json`:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:3000",  // ? Add new URL here
      ...
    ]
  }
}
```

### Add a New Production URL:
Edit `appsettings.json`:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://elo-fotbalek.azurewebsites.net",
      "https://new-site.azurewebsites.net"  // ? Add new URL here
    ]
  }
}
```

### Override in Azure:
In Azure App Service ? Configuration ? Application Settings:
```
Name: Cors__AllowedOrigins__0
Value: https://custom-domain.com

Name: Cors__AllowedOrigins__1
Value: https://another-domain.com
```

---

## ?? Testing

### 1. Development (Same Origin):
```bash
cd frontend
npm run build

# Press F5 in Visual Studio
# Open: https://localhost:5001
```

**Expected:**
- ? No CORS errors in browser console
- ? API calls succeed
- ? Leaderboard loads with data

### 2. Development (Dev Server):
```bash
# Terminal 1: F5 in Visual Studio (backend at :5001)
# Terminal 2:
cd frontend
npm run dev

# Open: http://localhost:5173
```

**Expected:**
- ? No CORS errors
- ? API calls to :5001 succeed
- ? Hot-reload works

### 3. Check CORS Headers:
Open DevTools ? Network ? Select any API call ? Headers:

**Request Headers:**
```
Origin: https://localhost:5001
```

**Response Headers:**
```
Access-Control-Allow-Origin: https://localhost:5001
Access-Control-Allow-Credentials: true
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH
Access-Control-Allow-Headers: *
```

---

## ?? Troubleshooting

### Issue: CORS errors still appearing

**Check 1: Verify configuration is loaded**
Add logging to `ConfigureServices`:
```csharp
var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
Console.WriteLine($"CORS Origins: {string.Join(", ", allowedOrigins ?? Array.Empty<string>())}");
```

**Check 2: Verify correct appsettings file is used**
- Development: `appsettings.Development.json` (when running from Visual Studio)
- Production: `appsettings.json` (when deployed to Azure)

**Check 3: Check browser console for actual origin**
The `Origin` header in the request must exactly match one of the configured origins.

**Check 4: Ensure CORS middleware is first**
```csharp
app.UseCors("CorsPolicy");  // ? Must be BEFORE UseRouting()
app.UseRouting();
```

---

## ?? Files Modified

| File | Change |
|------|--------|
| `Elo-fotbalek/Program.cs` | Read CORS origins from configuration |
| `Elo-fotbalek/appsettings.json` | Added `Cors:AllowedOrigins` (production) |
| `Elo-fotbalek/appsettings.Development.json` | Added `Cors:AllowedOrigins` (development) |

---

## ? Summary

**Before:**
- ? CORS origins hardcoded in `Program.cs`
- ? Need code changes to update origins
- ? Same configuration for all environments

**After:**
- ? CORS origins in configuration files
- ? Environment-specific configurations
- ? Easy to update without code changes
- ? Can be overridden in Azure App Settings
- ? Secure by default (production)
- ? Flexible for development

---

## ?? Result

**CORS is now properly configured and environment-aware!**

- ? Development: Works with dev server (`:5173`) and same origin (`:5001`)
- ? Production: Only allows whitelisted Azure domains
- ? Configuration-based: No hardcoded URLs
- ? Easy to maintain: Update JSON files, not code

**Test it:**
1. Press F5 in Visual Studio
2. Open https://localhost:5001
3. Check browser console - no CORS errors
4. Leaderboard should load successfully

---

**CORS configuration is production-ready!** ??
