using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScheduleI.DispensariesMod.Integration
{
    internal static class ReflectionLookup
    {
        public static bool TryHookDailyTick(object root, Action tickAction, ILogApi log) { log.Info("Reflection lookup: DailyTick event on Schedule I Il2Cpp time systems."); return false; }
        public static bool CanHookDailyTick(object root, ILogApi log) { return false; }
        public static bool HasLegalizationFlagAccess(object root, ILogApi log) { return true; }
        public static bool TryGetBooleanFlag(object root, string key, out bool value, ILogApi log) { log.Info("Reflection lookup: progression flag '" + key + "'."); value = false; return false; }
        public static void TryNotify(object root, string message, ILogApi log) { log.Info("Reflection lookup: notification/toast API."); log.Info("[ToastFallback] " + message); }
        public static bool CanAccessPlayerCash(object root, ILogApi log) { return false; }
        public static decimal GetPlayerCash(object root, ILogApi log) { log.Warn("Player cash API unavailable."); return 0m; }
        public static void SetPlayerCash(object root, decimal value, ILogApi log) { log.Warn("Player cash API unavailable for set."); }
        public static bool CanHookRealEstate(object root, ILogApi log) { return false; }
        public static void RegisterRealEstateProvider(object root, string categoryId, Func<IList<RealEstateListing>> provider, Func<string, bool> purchaseHandler, ILogApi log) { log.Info("Reflection lookup: real estate listing provider '" + categoryId + "'."); log.Warn("Real estate API not found; dispensary listing injection skipped."); LogCandidates(log, "RealEstate", new[] { "Property", "RealEstate", "Estate", "Business", "NPC", "Interaction" }); }
        public static IList<PlayerProductStack> GetCannabisProducts(object root, ILogApi log) { log.Info("Reflection lookup: inventory cannabis products list."); return new List<PlayerProductStack>(); }
        public static bool TryRemoveInventory(object root, string productId, int quantity, ILogApi log) { log.Info("Reflection lookup: remove inventory product '" + productId + "' x" + quantity + "."); return false; }
        public static bool TryAddInventory(object root, string productId, string productName, ProductGrade grade, int quantity, decimal basePrice, ILogApi log) { log.Info("Reflection lookup: add inventory product '" + productId + "' for debug."); return false; }
        public static bool CanRegisterSave(object root, ILogApi log) { return false; }
        public static void RegisterSaveBlock(object root, string key, Func<string> save, Action<string> load, ILogApi log) { log.Info("Reflection lookup: save registration '" + key + "'."); log.Warn("Save API not found; dispensary state persistence disabled."); LogCandidates(log, "SaveLoad", new[] { "Save", "Load", "Profile", "Data" }); }
        public static bool CanOpenUiPanels(object root, ILogApi log) { return false; }
        public static void RegisterMenu(object root, string menuId, Action openAction, ILogApi log) { log.Info("Reflection lookup: UI menu register '" + menuId + "'."); }
        public static void OpenPanel(object root, string panelName, string dispensaryId, ILogApi log) { log.Info("Reflection lookup: open panel '" + panelName + "' for '" + dispensaryId + "'."); }

        public static bool IsHotkeyPressed(object root, string keyName, ILogApi log)
        {
            try
            {
                var inputType = Type.GetType("UnityEngine.Input, UnityEngine.InputLegacyModule") ?? Type.GetType("UnityEngine.Input, UnityEngine.CoreModule");
                var keyCodeType = Type.GetType("UnityEngine.KeyCode, UnityEngine.InputLegacyModule") ?? Type.GetType("UnityEngine.KeyCode, UnityEngine.CoreModule");
                if (inputType == null || keyCodeType == null) return false;
                var key = Enum.Parse(keyCodeType, keyName);
                var method = inputType.GetMethod("GetKeyDown", new[] { keyCodeType });
                if (method == null) return false;
                return (bool)method.Invoke(null, new[] { key });
            }
            catch { return false; }
        }

        public static void ShowMayorLegalizationMessage(object root, ILogApi log)
        {
            var text = "The Mayor has publicly endorsed cannabis and was seen smoking it. Weed is now legalized, and dispensaries are unlocked.";
            TryNotify(root, text, log);
            log.Info(text);
            LogCandidates(log, "MayorDialogue", new[] { "Mayor", "Dialogue", "Dialog", "Interaction", "NPC", "UI", "Menu" });
        }

        public static void DumpIl2CppTypeCandidates(object root, ILogApi log)
        {
            LogCandidates(log, "Global", new[] { "Property", "RealEstate", "Estate", "Business", "Save", "Load", "Time", "Day", "UI", "Menu", "Player", "Money", "Cash", "Inventory", "Mayor", "Dialogue", "Dialog", "Interaction", "NPC" });
        }

        private static void LogCandidates(ILogApi log, string category, string[] patterns)
        {
            try
            {
                var asm = AppDomain.CurrentDomain.GetAssemblies();
                var matches = new List<string>();
                foreach (var a in asm)
                {
                    Type[] types;
                    try { types = a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
                    foreach (var t in types)
                    {
                        var full = t.FullName ?? t.Name;
                        if (patterns.Any(p => full.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            matches.Add(a.GetName().Name + ":" + full);
                        }
                    }
                }

                log.Warn("[" + category + "] Top candidate types:");
                foreach (var m in matches.Distinct().Take(20)) log.Warn("  - " + m);
            }
            catch (Exception ex)
            {
                log.Error("Failed type candidate dump for " + category, ex);
            }
        }
    }
}
