# Running Elo-fotbalek from Visual Studio

## ? Setup Complete

The project is now properly configured to run from Visual Studio.

## ?? How to Run

### Option 1: Run Backend Only (Recommended for Testing APIs)

1. **In Visual Studio, press F5 or click the "Play" button** (Elo-fotbalek)
2. Backend starts at: `https://localhost:5001`
3. You can test API endpoints:
   - `https://localhost:5001/api/config`
   - `https://localhost:5001/api/players`
   - `https://localhost:5001/api/background-images`

### Option 2: Run Backend + Frontend Dev Server (Full Development)

**Terminal 1 - Visual Studio (Backend):**
1. Press F5 in Visual Studio
2. Backend runs at `https://localhost:5001`

**Terminal 2 - External Command Prompt (Frontend):**
```cmd
cd frontend
npm run dev
```
3. Frontend dev server runs at `http://localhost:5173`
4. Frontend proxies API calls to backend automatically

### Option 3: Run Production Build (Testing Deployment)

1. **Build Frontend:**
   ```cmd
   cd frontend
   npm run build
   ```
   This outputs to `Elo-fotbalek/wwwroot`

2. **Run Backend from Visual Studio:**
   - Press F5
   - Open browser to `https://localhost:5001`
   - Both frontend and backend served from single origin

## ?? Solution Explorer

The frontend files are now visible in Visual Studio under the `frontend` folder in the project. This includes:
- `package.json`
- `vite.config.ts`
- `tsconfig.json`
- `tailwind.config.js`
- All source files in `src/`

**Note:** The frontend TypeScript files are excluded from C# compilation (`TypeScriptCompileBlocked=true`).

## ?? Launch Configuration

The project uses these launch profiles (configured in `Properties/launchSettings.json`):

1. **Elo-fotbalek** (Default - Kestrel)
   - Direct run: `dotnet run`
   - URLs: `https://localhost:5001` and `http://localhost:5000`
   - **This is the default profile** - runs when you press F5

2. **https** (Alternative Kestrel)
   - Alternative ports: `https://localhost:7001` and `http://localhost:7000`
   - Use if port 5001 is already in use

**Note:** IIS Express has been removed to prevent launching the old MVC app on port 44300.

## ? Verification Steps

### Test Backend is Running:

1. Start the application (F5)
2. Open browser to: `https://localhost:5001/api/config`
3. You should see JSON response with app configuration

### Test API Endpoints:

```powershell
# From PowerShell
Invoke-RestMethod -Uri "https://localhost:5001/api/config" -SkipCertificateCheck
Invoke-RestMethod -Uri "https://localhost:5001/api/players" -SkipCertificateCheck
```

### Test Frontend (if running dev server):

1. Start backend (F5 in Visual Studio)
2. In separate terminal:
   ```cmd
   cd frontend
   npm run dev
   ```
3. Open browser to: `http://localhost:5173`
4. Frontend should load with navigation

## ?? Troubleshooting

### Backend won't start:
- Check `appsettings.Development.json` for correct configuration
- Verify Azure Blob Storage connection string (or set `UseOffline: true`)
- Check Output window in Visual Studio for errors

### Port already in use:
- Change port in `Properties/launchSettings.json`
- Or stop other processes using port 5001

### Frontend can't connect to backend:
- Ensure backend is running
- Check CORS policy in `Program.cs` includes your frontend URL
- Frontend dev server proxy is configured in `vite.config.ts`

### "Module not found" errors in frontend:
- Run `npm install` in the `frontend` directory
- Frontend dependencies are separate from backend

## ?? Configuration Files

### Backend Configuration:
- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development settings (used by F5 debugging)

### Frontend Configuration:
- `.env.development` - Development API base URL
- `.env.production` - Production API base URL (relative)

## ?? Next Steps

1. **To develop API endpoints:** Use Visual Studio debugger (F5)
2. **To develop frontend pages:** Run backend + frontend dev server
3. **To test production build:** Build frontend, then F5 in Visual Studio

The application is ready for development! ??
