# ?? Doodle "Další kolo" Admin Button

## Feature Added

**Admin-only button** to advance the doodle poll to the next week - removes the oldest date and adds a new one.

---

## Backend API

### New Endpoint: `POST /api/doodle/advance-poll`

**Authorization:** Requires `MyPolicy` (Basic Auth - Admin only)

**Functionality:**
1. Gets all doodles from storage
2. Finds the **oldest date** (min date)
3. Validates the date has passed (prevents advancing too early)
4. Finds the **newest date** (max date)
5. Calculates **new date** = max date + 7 days (next Tuesday)
6. For each player:
   - Removes oldest date
   - Adds new date with `NoAnswer` status
7. Saves updated doodles to blob storage

**Response:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Poll advanced successfully",
    "removedDate": "2024-01-16",
    "addedDate": "2024-02-20"
  }
}
```

**Error Cases:**
- No doodle data found ? 404
- Oldest date hasn't passed yet ? 400
- Invalid credentials ? 401

**Code:**
```csharp
[HttpPost("advance-poll")]
[Authorize(policy: "MyPolicy")]
public async Task<IActionResult> AdvancePoll()
{
    var doodles = await this.blobClient.GetDoodle();
    
    if (doodles.Count == 0)
        return NotFound("No doodle data found");

    var minDate = doodles[0].PlayersPoll.Keys.Min<DateTime>();
    
    // Don't advance if oldest date hasn't passed
    if (minDate.AddDays(1) > DateTime.UtcNow)
        return BadRequest("The oldest date has not passed yet");

    var maxDate = doodles[0].PlayersPoll.Keys.Max<DateTime>();
    var newDate = maxDate.AddDays(7); // Next Tuesday

    // Remove oldest, add newest for all players
    foreach (var doodle in doodles)
    {
        doodle.PlayersPoll.Remove(minDate);
        doodle.PlayersPoll.Add(newDate, DoodleValue.NoAnswer);
    }

    await this.blobClient.SaveDoodle(doodles);

    return Ok(new {
        success = true,
        message = "Poll advanced successfully",
        removedDate = minDate.ToString("yyyy-MM-dd"),
        addedDate = newDate.ToString("yyyy-MM-dd")
    });
}
```

---

## Frontend Implementation

### Button Location
**Date Selector Row** - Right side (opposite of date buttons)

```
????????????????????????????????????????????????????
? [23.01] [30.01] [06.02]      [Další kolo ?]    ?
????????????????????????????????????????????????????
```

### Button States

#### 1. Initial State (Normal)
```tsx
<button onClick={handleAdvancePoll}>
  Další kolo ?
</button>
```
- Orange button (`bg-orange-600`)
- Shows on right side of dates
- Click to open admin prompt

#### 2. Admin Prompt State
```tsx
<>
  <input 
    type="password" 
    placeholder="Admin heslo"
    value={adminPassword}
  />
  <button onClick={handleAdvancePoll}>
    Potvrdit
  </button>
  <button onClick={handleCancelAdvance}>
    Zrušit
  </button>
</>
```
- Password input field
- Green "Potvrdit" button
- Gray "Zrušit" button
- Press Enter to submit

#### 3. Loading State
```tsx
<button disabled>...</button>
```
- Shows "..." while processing
- Button disabled

---

## User Flow

### Admin Workflow:
```
1. Admin clicks "Další kolo ?"
   ?
2. Password prompt appears
   ?
3. Admin enters password
   ?
4. Admin clicks "Potvrdit" (or presses Enter)
   ?
5. Request sent to API with Basic Auth
   ?
6a. SUCCESS:
    - Alert: "Doodle posunuto na další týden"
    - Page refreshes with new dates
    - Password field cleared
    ?
6b. ERROR:
    - Alert: Error message
    - Password field remains
    - Can retry
```

### Cancel Action:
```
Admin clicks "Zrušit"
   ?
Password prompt closes
Password field cleared
Back to normal state
```

---

## Visual Design

### Layout (Desktop):
```
??????????????????????????????????????????????????????????????
? Date Selector                                              ?
?                                                            ?
? ???????????????????????????????  ???????????????????????? ?
? ? [23.01] [30.01] [06.02]     ?  ?  [Další kolo ?]      ? ?
? ???????????????????????????????  ???????????????????????? ?
?   ? Date buttons (left)              ? Admin button (right) ?
??????????????????????????????????????????????????????????????
```

### Layout (Admin Prompt Active):
```
??????????????????????????????????????????????????????????????
? ???????????????????????????????  ???????????????????????? ?
? ? [23.01] [30.01] [06.02]     ?  ? [____] [?] [?]      ? ?
? ???????????????????????????????  ???????????????????????? ?
?   ? Dates                            ? Password, Confirm, Cancel ?
??????????????????????????????????????????????????????????????
```

### Responsive (Mobile):
```
???????????????????????
? [23.01] [30.01]    ?
? [06.02]            ?
?                    ?
? [Další kolo ?]     ?
? (stacks below)     ?
???????????????????????
```

---

## Code Structure

### State Management:
```typescript
const [showAdminPrompt, setShowAdminPrompt] = useState(false);
const [adminPassword, setAdminPassword] = useState('');

const advancePollMutation = useMutation({
  mutationFn: (password: string) => 
    doodleService.advancePoll({ username: 'admin', password }),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['doodle'] });
    setShowAdminPrompt(false);
    setAdminPassword('');
    alert('Doodle posunuto na další týden');
  },
  onError: (error) => {
    alert(error.message || 'Nepoda?ilo se posunout doodle');
  },
});
```

### Event Handlers:
```typescript
const handleAdvancePoll = () => {
  if (!showAdminPrompt) {
    setShowAdminPrompt(true); // Show password prompt
    return;
  }

  if (!adminPassword) {
    alert('Zadejte heslo');
    return;
  }

  advancePollMutation.mutate(adminPassword);
};

const handleCancelAdvance = () => {
  setShowAdminPrompt(false);
  setAdminPassword('');
};
```

---

## Security

### Authorization:
- ? Backend protected by `[Authorize(policy: "MyPolicy")]`
- ? Requires Basic Auth credentials
- ? Frontend sends username + password
- ? No client-side bypass possible

### User Experience:
- ?? Password field (type="password")
- ?? Admin credentials required
- ?? Failed auth shows error
- ? Can retry with different password
- ? Cancel button to abort

---

## Behavior

### What Happens:
1. **Removes oldest Tuesday** from all players' polls
2. **Adds new Tuesday** (max + 7 days) for all players
3. **New players** automatically get `NoAnswer` for new date
4. **All attendance data** for removed date is lost (archived)

### Example:
**Before:**
```
Dates: [16.01, 23.01, 30.01, 06.02, 13.02]
```

**After "Další kolo":**
```
Dates: [23.01, 30.01, 06.02, 13.02, 20.02]
         ? First date now        ? New date added
```

**Date 16.01 removed** (all attendance data gone)  
**Date 20.02 added** (all players set to NoAnswer)

---

## Validation

### Backend Checks:
1. ? Doodle data exists
2. ? Oldest date has passed (can't advance future dates)
3. ? User is authenticated (Basic Auth)
4. ? User has admin role

### Frontend UX:
- ? Password required
- ? Enter key submits
- ? Loading indicator while processing
- ? Success/error feedback
- ? Auto-refresh after success

---

## Files Modified

### Backend:
- ? `Elo-fotbalek/Controllers/Api/DoodleApiController.cs`
  - Added `[Authorize]` import
  - Added `AdvancePoll()` endpoint with admin auth

### Frontend:
- ? `frontend/src/services/apiService.ts`
  - Added `advancePoll()` method with Basic Auth

- ? `frontend/src/pages/DoodlePage.tsx`
  - Added state: `showAdminPrompt`, `adminPassword`
  - Added mutation: `advancePollMutation`
  - Added handlers: `handleAdvancePoll`, `handleCancelAdvance`
  - Added button UI in date selector section
  - Added password prompt UI

- ? `frontend/wwwroot/` - Rebuilt

---

## Testing

### Test Steps:
1. Navigate to `/doodle`
2. **Check button presence:**
   - ? "Další kolo ?" button on right side of dates
3. **Test admin flow:**
   - Click "Další kolo ?"
   - Password prompt appears
   - Enter admin password
   - Click "Potvrdit" (or press Enter)
   - Should show success alert
   - Dates should update (oldest removed, newest added)
4. **Test cancel:**
   - Click "Další kolo ?"
   - Click "Zrušit"
   - Prompt should close
5. **Test error:**
   - Click "Další kolo ?"
   - Enter wrong password
   - Should show error alert

---

## Czech Translations

| English | Czech |
|---------|-------|
| Next Round | Další kolo |
| Admin password | Admin heslo |
| Confirm | Potvrdit |
| Cancel | Zrušit |
| Poll advanced successfully | Doodle posunuto na další týden |
| Enter password | Zadejte heslo |
| Failed to advance poll | Nepoda?ilo se posunout doodle |

---

**Status:** ? **ADMIN BUTTON IMPLEMENTED**

The Doodle page now has:
- ?? Admin-only "Další kolo ?" button
- ?? Password-protected action
- ?? Removes oldest date, adds new date
- ? Clean UI integration on date row
