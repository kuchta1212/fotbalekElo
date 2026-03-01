# Launch Settings Fix - Port 44300 Issue Resolved

## ?? Issue
When pressing F5 in Visual Studio, the old MVC app was being launched on port 44300 (IIS Express) instead of the new .NET 8 application.

## ? Fix Applied

Updated `Elo-fotbalek/Properties/launchSettings.json` to:

### Changes Made:
1. **Removed IIS Express profile** - This was causing the old app to launch
2. **Made Kestrel the default profile** - Ensures new .NET 8 app runs
3. **Added modern .NET 8 profile settings** - Including `dotnetRunMessages` and JSON schema

### New Launch Profiles:

#### 1. **Elo-fotbalek** (Default)
- Command: `dotnet run` (Kestrel)
- URLs: 
  - HTTPS: `https://localhost:5001`
  - HTTP: `http://localhost:5000`
- **This runs when you press F5**

#### 2. **https** (Alternative)
- Command: `dotnet run` (Kestrel)
- URLs:
  - HTTPS: `https://localhost:7001`
  - HTTP: `http://localhost:7000`
- Use if port 5001 is already in use

## ?? How to Use

### From Visual Studio:
1. Open `Elo-fotbalek.sln`
2. Press **F5** (or click Play button)
3. Application now starts on: **`https://localhost:5001`** ?
4. New .NET 8 app with React frontend support runs

### Select Different Profile:
1. Click the dropdown next to the Play button
2. Choose either:
   - **Elo-fotbalek** (default, port 5001)
   - **https** (alternative, port 7001)

## ?? Verification

Test that the new app is running:

```powershell
# Test API endpoints
Invoke-RestMethod -Uri "https://localhost:5001/api/config" -SkipCertificateCheck
Invoke-RestMethod -Uri "https://localhost:5001/api/players" -SkipCertificateCheck
Invoke-RestMethod -Uri "https://localhost:5001/api/background-images" -SkipCertificateCheck
```

Expected responses:
- `/api/config` - Returns app configuration JSON
- `/api/players` - Returns players list JSON
- `/api/background-images` - Returns background images array

## ?? Configuration Details

### Before (? Old):
```json
{
  "iisSettings": {
    "iisExpress": {
      "sslPort": 44300  ? Old MVC app port
    }
  },
  "profiles": {
    "IIS Express": {  ? Listed first, became default
      "commandName": "IISExpress"
    }
  }
}
```

### After (? New):
```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "Elo_fotbalek": {  ? Listed first, is default
      "commandName": "Project",
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }
}
```

## ? Result

- ? Pressing F5 now launches the **new .NET 8 application**
- ? Runs on **port 5001** (not 44300)
- ? Serves both **API endpoints** and **React frontend**
- ? No more old MVC app confusion

## ?? Development Workflow

### Backend Only (API Testing):
```
1. Press F5 in Visual Studio
2. Backend at: https://localhost:5001
3. Test APIs directly
```

### Full Stack (Backend + Frontend):
```
Terminal 1 (Visual Studio):
  - Press F5 ? Backend runs

Terminal 2 (Command Prompt):
  - cd frontend
  - npm run dev ? Frontend at http://localhost:5173
```

### Production Mode:
```
1. cd frontend && npm run build
2. Press F5 in Visual Studio
3. Full app at: https://localhost:5001
```

## ?? Summary

**Problem:** Old MVC app launching on port 44300  
**Solution:** Removed IIS Express, made Kestrel default  
**Result:** New .NET 8 app launches correctly on port 5001  

**Status:** ? **FIXED** - Ready for development!
