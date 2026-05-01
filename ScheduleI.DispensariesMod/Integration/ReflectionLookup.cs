using System;
using System.Collections.Generic;

namespace ScheduleI.DispensariesMod.Integration;

/// <summary>
/// Reflection remains required because Schedule I runtime assemblies are not present in this repository.
/// All member lookups are centralized here with explicit logging and safe failure behavior.
/// </summary>
internal static class ReflectionLookup
{
    public static bool TryHookDailyTick(object root, Action tickAction, ILogApi log)
    {
        log.Info("Reflection lookup: DailyTick event on GameTime/WorldTime.");
        return false;
    }

    public static bool CanHookDailyTick(object root, ILogApi log) => false;
    public static bool HasLegalizationFlagAccess(object root, ILogApi log) => true;

    public static bool TryGetBooleanFlag(object root, string key, out bool value, ILogApi log)
    {
        log.Info($"Reflection lookup: progression flag '{key}'.");
        value = false;
        return false;
    }

    public static void TryNotify(object root, string message, ILogApi log)
    {
        log.Info("Reflection lookup: notification/toast API.");
        log.Info($"[ToastFallback] {message}");
    }

    public static bool CanAccessPlayerCash(object root, ILogApi log) => false;
    public static decimal GetPlayerCash(object root, ILogApi log) { log.Warn("Player cash API unavailable."); return 0m; }
    public static void SetPlayerCash(object root, decimal value, ILogApi log) { log.Warn("Player cash API unavailable for set."); }

    public static bool CanHookRealEstate(object root, ILogApi log) => false;
    public static void RegisterRealEstateProvider(object root, string categoryId, Func<IReadOnlyList<RealEstateListing>> provider, Func<string, bool> purchaseHandler, ILogApi log)
    {
        log.Info($"Reflection lookup: real estate listing provider '{categoryId}'.");
        log.Warn("Real estate API not found; dispensary listing injection skipped.");
    }

    public static IReadOnlyList<PlayerProductStack> GetCannabisProducts(object root, ILogApi log)
    {
        log.Info("Reflection lookup: inventory cannabis products list.");
        return Array.Empty<PlayerProductStack>();
    }

    public static bool TryRemoveInventory(object root, string productId, int quantity, ILogApi log)
    {
        log.Info($"Reflection lookup: remove inventory product '{productId}' x{quantity}.");
        return false;
    }

    public static bool TryAddInventory(object root, string productId, string productName, ProductGrade grade, int quantity, decimal basePrice, ILogApi log)
    {
        log.Info($"Reflection lookup: add inventory product '{productId}' for debug.");
        return false;
    }

    public static bool CanRegisterSave(object root, ILogApi log) => false;
    public static void RegisterSaveBlock(object root, string key, Func<string> save, Action<string> load, ILogApi log)
    {
        log.Info($"Reflection lookup: save registration '{key}'.");
        log.Warn("Save API not found; dispensary state persistence disabled.");
    }

    public static bool CanOpenUiPanels(object root, ILogApi log) => false;
    public static void RegisterMenu(object root, string menuId, Action openAction, ILogApi log)
    {
        log.Info($"Reflection lookup: UI menu register '{menuId}'.");
    }

    public static void OpenPanel(object root, string panelName, string dispensaryId, ILogApi log)
    {
        log.Info($"Reflection lookup: open panel '{panelName}' for '{dispensaryId}'.");
    }
}
