using System;
using System.Collections.Generic;

namespace ScheduleI.DispensariesMod.Integration
{
    internal static class ReflectionLookup
    {
        public static bool TryHookDailyTick(object root, Action tickAction, ILogApi log) { log.Info("Reflection lookup: DailyTick event on GameTime/WorldTime."); return false; }
        public static bool CanHookDailyTick(object root, ILogApi log) { return false; }
        public static bool HasLegalizationFlagAccess(object root, ILogApi log) { return true; }
        public static bool TryGetBooleanFlag(object root, string key, out bool value, ILogApi log) { log.Info("Reflection lookup: progression flag '" + key + "'."); value = false; return false; }
        public static void TryNotify(object root, string message, ILogApi log) { log.Info("Reflection lookup: notification/toast API."); log.Info("[ToastFallback] " + message); }
        public static bool CanAccessPlayerCash(object root, ILogApi log) { return false; }
        public static decimal GetPlayerCash(object root, ILogApi log) { log.Warn("Player cash API unavailable."); return 0m; }
        public static void SetPlayerCash(object root, decimal value, ILogApi log) { log.Warn("Player cash API unavailable for set."); }
        public static bool CanHookRealEstate(object root, ILogApi log) { return false; }
        public static void RegisterRealEstateProvider(object root, string categoryId, Func<IList<RealEstateListing>> provider, Func<string, bool> purchaseHandler, ILogApi log) { log.Info("Reflection lookup: real estate listing provider '" + categoryId + "'."); log.Warn("Real estate API not found; dispensary listing injection skipped."); }
        public static IList<PlayerProductStack> GetCannabisProducts(object root, ILogApi log) { log.Info("Reflection lookup: inventory cannabis products list."); return new List<PlayerProductStack>(); }
        public static bool TryRemoveInventory(object root, string productId, int quantity, ILogApi log) { log.Info("Reflection lookup: remove inventory product '" + productId + "' x" + quantity + "."); return false; }
        public static bool TryAddInventory(object root, string productId, string productName, ProductGrade grade, int quantity, decimal basePrice, ILogApi log) { log.Info("Reflection lookup: add inventory product '" + productId + "' for debug."); return false; }
        public static bool CanRegisterSave(object root, ILogApi log) { return false; }
        public static void RegisterSaveBlock(object root, string key, Func<string> save, Action<string> load, ILogApi log) { log.Info("Reflection lookup: save registration '" + key + "'."); log.Warn("Save API not found; dispensary state persistence disabled."); }
        public static bool CanOpenUiPanels(object root, ILogApi log) { return false; }
        public static void RegisterMenu(object root, string menuId, Action openAction, ILogApi log) { log.Info("Reflection lookup: UI menu register '" + menuId + "'."); }
        public static void OpenPanel(object root, string panelName, string dispensaryId, ILogApi log) { log.Info("Reflection lookup: open panel '" + panelName + "' for '" + dispensaryId + "'."); }
    }
}
