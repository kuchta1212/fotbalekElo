# ? Images Reorganized

## ?? What Was Done

Reorganized images in the `wwwroot` folder into a better structure and updated configuration to use proper paths.

---

## ?? New Folder Structure

### Before:
```
Elo-fotbalek/wwwroot/
??? images/
?   ??? 2018.jpg
?   ??? 2019.jpg
?   ??? 2021.jpg
?   ??? 2022.jpg
?   ??? 2023.jpg
?   ??? 2024-leto.jpg
?   ??? 2024.jpg
?   ??? fotbalek-saman.jpg
?   ??? usti.jpg
?   ??? banner1.svg
?   ??? banner2.svg
?   ??? banner3.svg
?   ??? icons/
?       ??? up.png
?       ??? down.png
?       ??? accept.png
?       ??? maybe.png
?       ??? refused.png
?       ??? ... (other icon files)
```

### After:
```
Elo-fotbalek/wwwroot/
??? images/
?   ??? backgrounds/              ? NEW: Background photos
?   ?   ??? 2018.jpg
?   ?   ??? 2019.jpg
?   ?   ??? 2021.jpg
?   ?   ??? 2022.jpg
?   ?   ??? 2023.jpg
?   ?   ??? 2024-leto.jpg
?   ?   ??? 2024.jpg
?   ?   ??? fotbalek-saman.jpg
?   ?   ??? usti.jpg
?   ??? icons/                    ? Trend & doodle icons
?   ?   ??? up.png
?   ?   ??? down.png
?   ?   ??? accept.png
?   ?   ??? accept_small.png
?   ?   ??? maybe.png
?   ?   ??? maybe_small.png
?   ?   ??? refused.png
?   ?   ??? refused_small.png
?   ?   ??? no_answer.png
?   ?   ??? no_answer_small.png
?   ??? banner1.svg              ? Banners (if used)
?   ??? banner2.svg
?   ??? banner3.svg
```

---

## ?? Configuration Updates

### `appsettings.Development.json`

**Before:**
```json
"BackgroundImages": [
  "../images/2024-leto.jpg",
  "../images/2023.jpg",
  ...
]
```

**After:**
```json
"BackgroundImages": [
  "/images/backgrounds/2024-leto.jpg",
  "/images/backgrounds/2023.jpg",
  "/images/backgrounds/2022.jpg",
  "/images/backgrounds/2021.jpg",
  "/images/backgrounds/2019.jpg",
  "/images/backgrounds/2018.jpg",
  "/images/backgrounds/fotbalek-saman.jpg"
]
```

---

## ?? Image Inventory

### Background Images (Year Photos):
| File | Purpose |
|------|---------|
| `2024-leto.jpg` | Summer 2024 team photo |
| `2024.jpg` | 2024 team photo |
| `2023.jpg` | 2023 team photo |
| `2022.jpg` | 2022 team photo |
| `2021.jpg` | 2021 team photo |
| `2019.jpg` | 2019 team photo |
| `2018.jpg` | 2018 team photo |
| `fotbalek-saman.jpg` | Special/fun team photo |
| `usti.jpg` | Ústí team photo |

### Icons:
| File | Purpose |
|------|---------|
| `up.png` | Trend indicator (?) - player improving |
| `down.png` | Trend indicator (?) - player declining |
| `accept.png` | Doodle: Player coming (Yes) |
| `accept_small.png` | Doodle: Small version |
| `maybe.png` | Doodle: Player maybe coming |
| `maybe_small.png` | Doodle: Small version |
| `refused.png` | Doodle: Player not coming (No) |
| `refused_small.png` | Doodle: Small version |
| `no_answer.png` | Doodle: Player hasn't responded |
| `no_answer_small.png` | Doodle: Small version |

### Banners (if used):
- `banner1.svg`, `banner2.svg`, `banner3.svg`

---

## ?? How Images Are Used

### 1. Background Images (Rotating)

**Component:** `BackgroundImages.tsx`

```typescript
// Fetches from: /api/background-images
// Returns paths like: /images/backgrounds/2024-leto.jpg

<div style={{ backgroundImage: `url(${image})` }} />
```

**Configuration:** `appsettings.Development.json` ? `BackgroundImages` array

**Rotation:** Changes every 30 seconds automatically

---

### 2. Trend Icons (Currently Using Unicode)

**Component:** `HomePage.tsx`

**Current:** Using Unicode arrows (? ?)
```tsx
<span className="text-green-600">?</span>
<span className="text-red-600">?</span>
```

**Available Icons:** `/images/icons/up.png`, `/images/icons/down.png`

**If you want to use icon files instead:**
```tsx
<img src="/images/icons/up.png" alt="?" className="w-4 h-4" />
<img src="/images/icons/down.png" alt="?" className="w-4 h-4" />
```

---

### 3. Doodle Status Icons (Future Use)

**Will be used in:** `DoodlePage.tsx` (when implemented)

**Usage Example:**
```tsx
// Accept (Yes)
<img src="/images/icons/accept.png" alt="P?ijde" />

// Maybe
<img src="/images/icons/maybe.png" alt="Možná" />

// Refused (No)
<img src="/images/icons/refused.png" alt="Nep?ijde" />
```

---

## ?? URL Paths

All images in `wwwroot` are accessible via web:

| File Path | URL |
|-----------|-----|
| `wwwroot/images/backgrounds/2024-leto.jpg` | `/images/backgrounds/2024-leto.jpg` |
| `wwwroot/images/icons/up.png` | `/images/icons/up.png` |
| `wwwroot/images/banner1.svg` | `/images/banner1.svg` |

**Note:** The `wwwroot` part is automatically removed in URLs.

---

## ? Benefits of This Organization

### 1. **Clear Structure**
- Background photos grouped together
- Icons grouped together
- Easy to find specific images

### 2. **Maintainability**
- Add new background: just drop in `/backgrounds/`
- Add new icon: just drop in `/icons/`
- Update config to include them

### 3. **Standard Convention**
- `wwwroot` is the standard static files folder in ASP.NET Core
- Browser can cache files efficiently
- Easy to deploy

### 4. **Scalability**
- Can add more subfolders as needed:
  - `/images/player-avatars/`
  - `/images/team-logos/`
  - `/images/match-photos/`

---

## ?? How to Add New Images

### Adding a New Background Image:

1. **Add the file:**
   ```
   Place image in: Elo-fotbalek/wwwroot/images/backgrounds/2025.jpg
   ```

2. **Update config:**
   ```json
   // In appsettings.Development.json
   "BackgroundImages": [
     "/images/backgrounds/2025.jpg",  ? Add this
     "/images/backgrounds/2024-leto.jpg",
     ...
   ]
   ```

3. **Restart app** - Changes in appsettings require restart

---

### Adding a New Icon:

1. **Add the file:**
   ```
   Place image in: Elo-fotbalek/wwwroot/images/icons/new-icon.png
   ```

2. **Use in React:**
   ```tsx
   <img src="/images/icons/new-icon.png" alt="Description" />
   ```

---

## ?? Files Modified

- ? **Moved:** All `.jpg` background images to `/images/backgrounds/`
- ? **Updated:** `appsettings.Development.json` - Background image paths
- ? **Kept:** Icons in `/images/icons/` (already organized)
- ? **Kept:** SVG banners in `/images/` root

---

## ?? Why `wwwroot`?

The `wwwroot` folder is the **correct location** for static assets in ASP.NET Core because:

1. **Standard Convention** - All ASP.NET Core apps use this
2. **Automatic Serving** - `app.UseStaticFiles()` serves from here
3. **Performance** - Static files served directly, no controller needed
4. **Caching** - Browser can cache efficiently
5. **CDN Ready** - Easy to move to CDN later if needed

**Don't move files out of `wwwroot`** - this is exactly where they should be!

---

## ? Result

**Images are now well-organized and easy to maintain!**

```
? Backgrounds in /images/backgrounds/
? Icons in /images/icons/
? Configuration updated with correct paths
? All images accessible via clean URLs
? Ready for future additions
```

---

## ?? Quick Reference

| Image Type | Location | Used By |
|------------|----------|---------|
| Background Photos | `/images/backgrounds/*.jpg` | `BackgroundImages.tsx` |
| Trend Icons | `/images/icons/up.png, down.png` | Currently: Unicode arrows |
| Doodle Icons | `/images/icons/accept.png, maybe.png, refused.png` | Future: `DoodlePage.tsx` |
| Banners | `/images/banner*.svg` | Not currently used |

---

**The image organization is now clean, maintainable, and follows ASP.NET Core best practices!** ???
