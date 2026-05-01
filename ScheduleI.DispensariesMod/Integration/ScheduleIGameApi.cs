using System;
using System.Collections.Generic;

namespace ScheduleI.DispensariesMod.Integration;

public sealed class ScheduleIGameApi : IGameApi
{
    public IGameEvents Events { get; }
    public IProgressionApi Progression { get; }
    public INotificationApi Notifications { get; }
    public IPlayerApi Player { get; }
    public IRealEstateApi RealEstate { get; }
    public IInventoryApi Inventory { get; }
    public ISaveSystemApi SaveSystem { get; }
    public IUiApi Ui { get; }
    public ILogApi Log { get; }
    public IIntegrationDiagnostics Diagnostics { get; }

    public ScheduleIGameApi(object gameRoot, Action<string> infoLog, Action<string> warnLog, Action<string, Exception?> errorLog)
    {
        Log = new DelegateLogApi(infoLog, warnLog, errorLog);

        Events = new ReflectionGameEvents(gameRoot, Log);
        Progression = new ReflectionProgressionApi(gameRoot, Log);
        Notifications = new ReflectionNotificationApi(gameRoot, Log);
        Player = new ReflectionPlayerApi(gameRoot, Log);
        RealEstate = new ReflectionRealEstateApi(gameRoot, Log);
        Inventory = new ReflectionInventoryApi(gameRoot, Log);
        SaveSystem = new ReflectionSaveSystemApi(gameRoot, Log);
        Ui = new ReflectionUiApi(gameRoot, Log);
        Diagnostics = new ReflectionDiagnostics(gameRoot, Log);
    }

    private sealed class DelegateLogApi : ILogApi
    {
        private readonly Action<string> _info;
        private readonly Action<string> _warn;
        private readonly Action<string, Exception?> _error;
        public DelegateLogApi(Action<string> info, Action<string> warn, Action<string, Exception?> error) { _info = info; _warn = warn; _error = error; }
        public void Info(string message) => _info(message);
        public void Warn(string message) => _warn(message);
        public void Error(string message, Exception? ex = null) => _error(message, ex);
    }

    private sealed class ReflectionGameEvents : IGameEvents
    {
        public event Action OnDailyTick = delegate { };
        public ReflectionGameEvents(object root, ILogApi log)
        {
            ReflectionLookup.TryHookDailyTick(root, () => OnDailyTick(), log);
        }
    }

    private sealed class ReflectionProgressionApi : IProgressionApi
    {
        private static readonly string[] CandidateFlags = ["MayorPubliclySmokingWeed", "MayorSmokingWeed", "MayorSmokesWeedPublic", "WeedLegalizedStoryBeat"];
        private readonly object _root; private readonly ILogApi _log;
        public ReflectionProgressionApi(object root, ILogApi log) { _root = root; _log = log; }
        public bool TryGetFlag(string key, out bool value) => ReflectionLookup.TryGetBooleanFlag(_root, key, out value, _log);
        public bool TryGetMayorWeedLegalizationFlag(out string resolvedFlag, out bool value)
        {
            foreach (var flag in CandidateFlags)
            {
                if (TryGetFlag(flag, out value)) { resolvedFlag = flag; return true; }
            }

            resolvedFlag = string.Empty;
            value = false;
            _log.Warn("No mayor legalization progression flag found.");
            return false;
        }
    }

    private sealed class ReflectionNotificationApi(object root, ILogApi log) : INotificationApi { public void Show(string message) => ReflectionLookup.TryNotify(root, message, log); }
    private sealed class ReflectionPlayerApi(object root, ILogApi log) : IPlayerApi { public bool IsAvailable => ReflectionLookup.CanAccessPlayerCash(root, log); public decimal Cash { get => ReflectionLookup.GetPlayerCash(root, log); set => ReflectionLookup.SetPlayerCash(root, value, log); } }
    private sealed class ReflectionRealEstateApi(object root, ILogApi log) : IRealEstateApi
    {
        public bool IsAvailable => ReflectionLookup.CanHookRealEstate(root, log);
        public event Func<string, bool> OnPurchaseRequested = _ => false;
        public void RegisterDynamicListingProvider(string categoryId, Func<IReadOnlyList<RealEstateListing>> provider)
            => ReflectionLookup.RegisterRealEstateProvider(root, categoryId, provider, id => OnPurchaseRequested(id), log);
    }
    private sealed class ReflectionInventoryApi(object root, ILogApi log) : IInventoryApi
    {
        public IReadOnlyList<PlayerProductStack> GetCannabisProducts() => ReflectionLookup.GetCannabisProducts(root, log);
        public bool TryRemove(string productId, int quantity) => ReflectionLookup.TryRemoveInventory(root, productId, quantity, log);
        public bool TryAddDebugProduct(string productId, string productName, ProductGrade grade, int quantity, decimal basePrice)
            => ReflectionLookup.TryAddInventory(root, productId, productName, grade, quantity, basePrice, log);
    }
    private sealed class ReflectionSaveSystemApi(object root, ILogApi log) : ISaveSystemApi
    {
        public bool IsAvailable => ReflectionLookup.CanRegisterSave(root, log);
        public void Register(string key, Func<string> save, Action<string> load) => ReflectionLookup.RegisterSaveBlock(root, key, save, load, log);
    }
    private sealed class ReflectionUiApi(object root, ILogApi log) : IUiApi
    {
        public void RegisterMenu(string menuId, Action openAction) => ReflectionLookup.RegisterMenu(root, menuId, openAction, log);
        public void OpenDispensaryManagement(string dispensaryId) => ReflectionLookup.OpenPanel(root, "DispensaryManagement", dispensaryId, log);
        public void OpenDispensaryListings(string dispensaryId) => ReflectionLookup.OpenPanel(root, "DispensaryListings", dispensaryId, log);
        public void OpenDispensaryHiring(string dispensaryId) => ReflectionLookup.OpenPanel(root, "DispensaryHiring", dispensaryId, log);
        public void OpenDispensaryReports(string dispensaryId) => ReflectionLookup.OpenPanel(root, "DispensaryReports", dispensaryId, log);
    }

    private sealed class ReflectionDiagnostics(object root, ILogApi log) : IIntegrationDiagnostics
    {
        public IntegrationStatus GetStatus() => new(
            ReflectionLookup.HasLegalizationFlagAccess(root, log),
            ReflectionLookup.CanHookRealEstate(root, log),
            ReflectionLookup.CanOpenUiPanels(root, log),
            ReflectionLookup.CanRegisterSave(root, log),
            ReflectionLookup.CanHookDailyTick(root, log));

        public void DumpStatus()
        {
            var s = GetStatus();
            log.Info($"Integration status => Flag:{s.LegalizationFlagLookupFound} RealEstate:{s.RealEstateHookFound} UI:{s.UiHookFound} Save:{s.SaveHookFound} DailyTick:{s.DailyTickFound}");
        }
    }
}
