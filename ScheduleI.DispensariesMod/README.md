# Schedule I - Legal Dispensaries Mod

## File placement
Place `ScheduleI.DispensariesMod` in your mod solution and wire `DispensariesModPlugin` from your loader entrypoint.

- `Core/DispensariesModPlugin.cs`: bootstraps systems and game hooks.
- `Integration/IGameApi.cs`: adapter interfaces to connect this module to Schedule I internals.
- `Data/DispensaryModels.cs`: dispensary, listing, employee, and report models.
- `Systems/DispensaryManager.cs`: property catalog, purchase flow, product listing.
- `Systems/DispensaryEmployeeManager.cs`: hiring, assignment, wage + efficiency calculations.
- `Systems/DispensaryEconomySystem.cs`: daily simulation and profit generation.
- `Systems/DispensarySaveData.cs`: save/load state (`cannabisLegalized`, owned stores, staff).
- `UI/DispensaryUiController.cs`: menu registration points for buy/manage/hire/reports.

## Required integration points
Implement `IGameApi` against game classes:
1. **Progression flags**: map `TryGetFlag("MayorPubliclySmokingWeed", out bool)` to the story state where mayor smoking event is complete.
2. **Real estate NPC**: route dynamic listings into the existing property purchase dialogue.
3. **Player inventory**: return only cannabis items for listing; implement `TryRemove`.
4. **Employee pool source**: either use this mod's hiring flow or map to existing worker NPC templates.
5. **Save system**: register `dispensaries_mod_state` in your save slot serialization.
6. **UI**: replace notification placeholders with full UI panels using base game widgets/styles.

## Test plan
1. **Legalization unlock**
   - Start before mayor event; verify real estate has no dispensary listings.
   - Set the mayor-smoking flag true and advance one daily tick.
   - Expect notification: `Cannabis has been legalized. Dispensaries are now available to purchase.`
2. **Purchase flow**
   - Open real estate NPC menu after legalization.
   - Purchase one dispensary and verify player cash decreases and ownership is saved.
3. **Product listing**
   - Add cannabis item to player inventory.
   - List item with price and demand modifier; verify display slot limits and inventory removal.
4. **Employee hiring/assignment**
   - Hire one or more staff.
   - Assign to store and verify wage total + effective sales multiplier change.
5. **Economy simulation**
   - Advance daily ticks and confirm quantities decrement, revenue posts, daily report added, and profit is applied.
6. **Save/load**
   - Save, restart, load; validate legalization flag, owned dispensaries, listings, employees, and reports persist.
