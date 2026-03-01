# ?? Debugging Guide - TypeError Issue

## ?? Current Issue

Getting `TypeError: Cannot read properties of undefined (reading 'length')` even after adding null safety.

---

## ?? Debug Steps Added

Added comprehensive console logging to understand the actual data structure:

### In HomePage.tsx:
```typescript
console.log('Config Response:', configResponse);
console.log('Leaderboard Response:', leaderboardResponse);
console.log('Unwrapped Config:', config);
console.log('Unwrapped Leaderboard:', leaderboard);
console.log('Regulars:', regulars);
console.log('NonRegulars:', nonRegulars);
```

---

## ?? How to Debug

### 1. Rebuild and Restart
```bash
# Frontend already rebuilt
# Press F5 in Visual Studio
```

### 2. Open Browser DevTools
```
1. Open: https://localhost:5001
2. Press F12 (or right-click ? Inspect)
3. Go to Console tab
```

### 3. Check Console Logs

You should see these logs in order:

#### Expected Structure:
```javascript
Config Response: { success: true, data: { appName: "...", ... } }
Leaderboard Response: { success: true, data: { regulars: [...], nonRegulars: [...], ... } }
Unwrapped Config: { appName: "...", isSeasoningSupported: true, ... }
Unwrapped Leaderboard: { regulars: [...], nonRegulars: [...], ... }
Regulars: [ { id: "...", name: "...", elo: 1200, ... }, ... ]
NonRegulars: [ ... ]
```

#### If Something is Wrong:
```javascript
// If leaderboardResponse is null or undefined:
Leaderboard Response: undefined
// or
Leaderboard Response: null

// If data property is missing:
Leaderboard Response: { regulars: [...], nonRegulars: [...] }  // No "data" wrapper

// If structure is different:
Leaderboard Response: { /* something else */ }
```

---

## ?? Possible Issues and Solutions

### Issue 1: Response is Not Wrapped
**Symptom:**
```javascript
Leaderboard Response: { regulars: [...], nonRegulars: [...] }
```

**Cause:** API is not using BaseApiController.Ok() properly

**Fix:** Update unwrapping logic:
```typescript
const leaderboard = (leaderboardResponse as any)?.data || leaderboardResponse;
```

### Issue 2: Response is Wrapped Twice
**Symptom:**
```javascript
Leaderboard Response: { data: { data: { regulars: [...] } } }
```

**Cause:** Double wrapping somewhere in the chain

**Fix:** Check BaseApiController and remove duplicate wrapping

### Issue 3: Response is Null
**Symptom:**
```javascript
Leaderboard Response: undefined
```

**Cause:** API call failed, network error, or CORS issue

**Fix:** Check Network tab for:
- 404 errors
- CORS errors
- 500 server errors

### Issue 4: Array Properties are Undefined
**Symptom:**
```javascript
Unwrapped Leaderboard: { season: "overall", isSeasoningSupported: true }
Regulars: undefined
```

**Cause:** API is not returning the arrays

**Fix:** Check LeaderboardApiController - ensure it returns regulars, nonRegulars arrays

---

## ?? Network Tab Debugging

### Open Network Tab:
1. F12 ? Network tab
2. Reload page (Ctrl+R)
3. Find `/api/leaderboards` request
4. Click on it

### Check Request:
```
Request URL: https://localhost:5001/api/leaderboards?season=Overall
Request Method: GET
Status Code: 200 OK
```

### Check Response:
Click "Response" or "Preview" tab:

**Expected:**
```json
{
  "success": true,
  "data": {
    "regulars": [
      {
        "rank": 1,
        "id": "guid-here",
        "name": "Player Name",
        "elo": 1200,
        "wins": 10,
        "losses": 5,
        ...
      }
    ],
    "nonRegulars": [...],
    "recentMatches": [...],
    "season": "overall",
    "isSeasoningSupported": true,
    "nonRegularsTitle": "Nepravidelni"
  }
}
```

### Check Response Headers:
```
Content-Type: application/json; charset=utf-8
Access-Control-Allow-Origin: https://localhost:5001
```

---

## ?? Common Causes

### 1. Frontend Not Rebuilt
**Symptom:** Old code still running

**Fix:**
```bash
cd frontend
npm run build
# Then restart backend (F5)
```

### 2. Browser Cache
**Symptom:** Old JavaScript bundle cached

**Fix:**
```
Ctrl + Shift + Delete ? Clear cache
Or
Ctrl + F5 (hard reload)
```

### 3. Wrong API Endpoint
**Symptom:** 404 error in Network tab

**Fix:**
- Check URL is: `/api/leaderboards` (not `/api/leaderboard`)
- Check backend is running
- Check MapControllers() is in Program.cs

### 4. CORS Blocking Request
**Symptom:** CORS error in console

**Fix:**
- Check `appsettings.Development.json` has correct origins
- Check `UseCors("CorsPolicy")` is before UseRouting()
- Restart backend after CORS changes

### 5. Backend Error
**Symptom:** 500 error in Network tab

**Fix:**
- Check Visual Studio Output window
- Look for exception stack traces
- Check BlobClient is working (UseOffline setting)

---

## ?? Quick Fixes to Try

### Try 1: Remove Data Unwrapping
```typescript
// Instead of:
const leaderboard = (leaderboardResponse as any)?.data;

// Try:
const leaderboard = leaderboardResponse as any;
```

### Try 2: Add Fallback Unwrapping
```typescript
const leaderboard = (leaderboardResponse as any)?.data || (leaderboardResponse as any);
```

### Try 3: Check for Different Response Shape
```typescript
const leaderboard = (leaderboardResponse as any)?.data?.data 
  || (leaderboardResponse as any)?.data 
  || (leaderboardResponse as any);
```

---

## ?? Reporting Back

After checking console logs, please provide:

1. **Console Output:**
   ```
   Config Response: { ... }
   Leaderboard Response: { ... }
   ```

2. **Network Tab:**
   - Request URL
   - Status code
   - Response body (first few lines)

3. **Error Details:**
   - Exact error message
   - Line number
   - Stack trace (if any)

---

## ? Next Steps

1. **Press F5** in Visual Studio
2. **Open** https://localhost:5001
3. **Open DevTools** (F12)
4. **Check Console** for the debug logs
5. **Check Network** tab for the API call
6. **Report back** what you see

This will help identify the exact issue with the data structure!

---

## ?? Expected Result

After these debugging steps, we should see:

? Console shows proper response structure  
? Network tab shows 200 OK with JSON data  
? No CORS errors  
? Data is properly unwrapped  
? Arrays exist and have .length property  

Then we can fix the specific issue based on what we find!
