# Schedule I - Legal Dispensaries Mod

## Current integration status
### Compile-safe integrations
- Core dispensary gameplay loop (unlock gate, listing provider callback, purchase gating, ownership persistence model)
- Purchase failure reason classification (`PurchaseFailReason`)
- Integration diagnostics surface (`IIntegrationDiagnostics`)

### Reflection-based integrations (runtime-dependent)
- Progression flag lookup
- Daily tick hook
- Real estate listing injection hook
- Player cash API bridge
- Inventory product bridge
- Save registration bridge
- UI panel registration/opening
- Notification/toast dispatch

### Manual confirmation needed
Because this repository does not include Schedule I runtime assemblies, confirm concrete class/member names in:
- `Integration/ReflectionLookup.cs`

## Vertical slice implemented
1. Detect legalization via mayor-flag resolver.
2. Inject one test dispensary (`Test Leaf`) when legalized.
3. Buy test dispensary through purchase callback.
4. Deduct player cash.
5. Persist owned dispensary through save payload.
6. Reload owned dispensary via save load callback.
7. Open management panel after successful purchase.

## Debug/dev shortcuts (`debugMode = true` in `DispensariesModPlugin`)
- Force legalization true
- Grant +$1,000,000 test cash (if player cash API is available)
- Attempt to add a test cannabis product (`debug_og`)
- Register direct open menu: `dispensary.debug.open`

## Bootstrap / loader call
```csharp
var api = new ScheduleIGameApi(
    gameRoot: GameObject.Find("GameRoot"),
    infoLog: m => Logger.LogInfo(m),
    warnLog: m => Logger.LogWarning(m),
    errorLog: (m, ex) => Logger.LogError($"{m} :: {ex}")
);
var mod = new DispensariesModPlugin(api, debugMode: true);
mod.Initialize();
```

## Smoke test steps
1. Start game and check logs for `Integration status => ...`.
2. If not in debug mode, progress story until mayor legalization flag is active.
3. Open real estate UI and confirm `Test Leaf` listing appears.
4. Purchase listing and confirm success log + cash deduction.
5. Save game, reload game, verify owned dispensary still exists.
6. Open management panel from dispensary manage menu or debug menu.
