# ? F5 Launch Issue - RESOLVED

## ?? Problem
When pressing **F5** in Visual Studio, the **old MVC app** was launching on **port 44300** (IIS Express) instead of the new .NET 8 application.

## ? Solution Applied

### Updated `launchSettings.json`:
- ? **Removed** IIS Express profile (was launching old app on port 44300)
- ? **Made Kestrel the default** (launches new .NET 8 app on port 5001)
- ? **Added alternative profile** on port 7001 if 5001 is busy

### Result:
```
Press F5 ? New .NET 8 App on https://localhost:5001 ?
```

## ?? Quick Test

**After pressing F5, test these URLs:**

```powershell
# Should return JSON (not HTML from old app)
Invoke-RestMethod -Uri "https://localhost:5001/api/config" -SkipCertificateCheck
Invoke-RestMethod -Uri "https://localhost:5001/api/players" -SkipCertificateCheck
```

**Expected:** JSON responses from new API endpoints

## ?? Launch Profiles Available

When you click the dropdown next to the Play button in Visual Studio, you'll see:

1. **Elo-fotbalek** ? (Default)
   - Port: **5001** (HTTPS) and **5000** (HTTP)
   - This runs when you press F5

2. **https** (Alternative)
   - Port: **7001** (HTTPS) and **7000** (HTTP)
   - Use if port 5001 is already taken

## ? Verification Checklist

After pressing F5:
- [x] Backend starts (no errors in Output window)
- [x] Browser opens to `https://localhost:5001`
- [x] API endpoints work: `/api/config`, `/api/players`, `/api/background-images`
- [x] No port 44300 or old MVC app

## ?? Documentation Updated

- ? `LAUNCH_SETTINGS_FIX.md` - Detailed explanation of the fix
- ? `RUNNING_FROM_VISUAL_STUDIO.md` - Updated with correct profiles
- ? `launchSettings.json` - Cleaned and modernized

## ?? Summary

| Before | After |
|--------|-------|
| ? F5 ? Old app on port 44300 | ? F5 ? New app on port 5001 |
| ? IIS Express launching | ? Kestrel launching |
| ? Old MVC app | ? New .NET 8 + React app |

**Status: FIXED ?**

You can now press F5 in Visual Studio and the correct application will launch!
