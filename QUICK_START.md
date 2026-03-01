# ?? Quick Start Guide - Elo-fotbalek

## ? You're Seeing the NEW React UI Now!

After pressing F5, you should see:
- ? Modern navigation bar
- ? "Leaderboard page coming soon..."
- ? Menu: Leaderboard, Players, Matches, Admin
- ? Mobile-friendly card-based design

The placeholders show "coming soon" because actual pages need to be implemented next.

---

## ?? Two Ways to Develop

### ????? Quick Start (Production Mode)

**Best for:** Testing how the app will work when deployed

```bash
# 1. Build frontend (do this once, or after frontend changes)
cd frontend
npm run build

# 2. Press F5 in Visual Studio
# 3. Open: https://localhost:5001
```

? Pros: Single URL, tests real deployment  
?? Cons: Need to rebuild frontend after every change

---

### ?? Development Mode (Hot Reload)

**Best for:** Actively developing frontend pages

```bash
# Terminal 1: Press F5 in Visual Studio
# Backend runs at: https://localhost:5001

# Terminal 2: Run frontend dev server
cd frontend
npm run dev
# Frontend runs at: http://localhost:5173
```

? Pros: Instant hot-reload, no rebuild needed  
? API calls automatically proxy to backend  
?? Cons: Two terminals, two URLs

---

## ?? When to Rebuild Frontend

Rebuild the frontend when:
- ? You want to test in production mode
- ? You made changes to frontend code
- ? You want to check the deployed experience

```bash
cd frontend
npm run build
```

Then restart backend (F5 in Visual Studio)

---

## ?? What You Should See

### At `https://localhost:5001` (after building frontend):
- ? React app loads
- ? Modern UI with navigation
- ? Placeholder pages with "coming soon" messages

### At `http://localhost:5173` (when running `npm run dev`):
- ? Same React app
- ? Hot-reload enabled
- ? Changes appear instantly

### API Endpoints Work Regardless:
```powershell
Invoke-RestMethod "https://localhost:5001/api/config" -SkipCertificateCheck
Invoke-RestMethod "https://localhost:5001/api/players" -SkipCertificateCheck
```

---

## ? Troubleshooting

### "I see old UI after F5"
```bash
# Rebuild frontend
cd frontend
npm run build

# Restart Visual Studio (F5)
```

### "404 on /api/config"
- ? Check backend is running (F5 in Visual Studio)
- ? Check URL is correct: `https://localhost:5001/api/config`

### "Module not found" in frontend
```bash
cd frontend
npm install
```

### "Port 5001 already in use"
- Stop other Visual Studio instances
- Or change port in `Properties/launchSettings.json`

---

## ?? Recommended Workflow

### For Backend Development:
```
1. Press F5 in Visual Studio
2. Test API endpoints
3. Make backend changes
4. Press F5 again to restart
```

### For Frontend Development:
```
1. Press F5 in Visual Studio (leave running)
2. Open new terminal: cd frontend && npm run dev
3. Make frontend changes
4. See changes instantly at http://localhost:5173
5. Press Ctrl+C to stop dev server when done
```

### For Full Stack Development:
```
1. Press F5 in Visual Studio (backend)
2. cd frontend && npm run dev (frontend)
3. Edit both backend and frontend
4. Hot-reload for frontend
5. F5 restart for backend changes
```

---

## ? Current Status

- ? Backend upgraded to .NET 8
- ? React frontend setup complete
- ? API endpoints created (/api/config, /api/players, /api/background-images)
- ? Build system working
- ? F5 launches correct app
- ? New UI showing

**Next:** Implement actual page content (Leaderboard, Players, Matches, etc.)

---

## ?? Documentation

- `OLD_UI_SHOWING_FIX.md` - Why old UI was showing and how we fixed it
- `RUNNING_FROM_VISUAL_STUDIO.md` - Detailed running guide
- `SETUP_COMPLETION_SUMMARY.md` - Complete setup overview
- `frontend/README.md` - Frontend-specific documentation

---

**You're all set! Start developing! ??**
