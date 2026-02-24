# ?? Doodle Route Conflict Fix

## Issue
```
InvalidOperationException: No service for type 'Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory' has been registered.
```

When navigating to `/doodle` in the browser.

## Root Cause

The **old MVC `DoodleController`** (Razor views) had the route `[Route("doodle")]`, which conflicted with the React app's `/doodle` route.

When the user navigated to `/doodle`:
1. ASP.NET routing matched the old MVC controller first
2. The old controller tried to return a Razor `View()`
3. MVC views require `ITempDataDictionaryFactory`
4. But `Program.cs` only has `AddControllers()` (API-only), not `AddControllersWithViews()` (MVC)
5. ? Exception thrown

### Architecture Context

After the UI rewrite:
- ? **New:** `DoodleApiController` at `/api/doodle/*` (JSON API)
- ? **New:** React `DoodlePage` component (UI)
- ? **Old:** `DoodleController` at `/doodle` (Razor MVC - no longer needed)

The React SPA should handle the `/doodle` route, not the old MVC controller.

## Solution

**Renamed the old MVC controller route** to avoid conflicts while preserving it for reference:

```csharp
// Before (caused conflict)
[Route("doodle")]
public class DoodleController : Controller

// After (no conflict)
[Route("legacy/doodle")]
public class DoodleController : Controller
```

Now:
- `/doodle` ? React SPA handles it ?
- `/api/doodle/*` ? API controller handles it ?
- `/legacy/doodle` ? Old MVC controller (if needed for reference)

## Why Not Delete?

The old `DoodleController` contains:
- Original business logic implementations
- Useful reference for domain behavior
- Can be deleted later after full verification

## Same Issue Likely Exists For:

Check these old MVC controllers for similar route conflicts:

| Old Controller | Route | New API | Status |
|----------------|-------|---------|--------|
| `HomeController` | `/` | `/api/leaderboards` | ?? Might conflict |
| `DoodleController` | `/doodle` | `/api/doodle` | ? Fixed |
| Other admin routes | Various | TBD | ?? Check later |

### Recommended Action

After verifying the new React UI works, rename or remove other old MVC controllers:
- Keep only API controllers (`/api/*`)
- Keep admin controllers that still use Razor views (if any need Basic Auth forms)
- Remove or rename the rest

## Testing

1. Navigate to `/doodle` in browser
2. Should see React Doodle page (not MVC error) ?
3. API calls to `/api/doodle/upcoming` should work ?

## Files Modified

- ? `Elo-fotbalek/Controllers/DoodleController.cs`
  - Changed route from `[Route("doodle")]` to `[Route("legacy/doodle")]`
  - Added comment explaining it's replaced

---

**Status:** ? **FIXED**

The `/doodle` route now correctly serves the React SPA instead of trying to invoke the old MVC controller.
