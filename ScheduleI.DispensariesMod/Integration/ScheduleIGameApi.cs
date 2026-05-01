using System;
using System.Collections.Generic;

namespace ScheduleI.DispensariesMod.Integration
{
    public sealed class ScheduleIGameApi : IGameApi
    {
        public IGameEvents Events { get; private set; }
        public IProgressionApi Progression { get; private set; }
        public INotificationApi Notifications { get; private set; }
        public IPlayerApi Player { get; private set; }
        public IRealEstateApi RealEstate { get; private set; }
        public IInventoryApi Inventory { get; private set; }
        public ISaveSystemApi SaveSystem { get; private set; }
        public IUiApi Ui { get; private set; }
        public ILogApi Log { get; private set; }
        public IIntegrationDiagnostics Diagnostics { get; private set; }

        public ScheduleIGameApi(object gameRoot, Action<string> infoLog, Action<string> warnLog, Action<string, Exception> errorLog)
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
            private readonly Action<string, Exception> _error;
            public DelegateLogApi(Action<string> info, Action<string> warn, Action<string, Exception> error) { _info = info; _warn = warn; _error = error; }
            public void Info(string message) { _info(message); }
            public void Warn(string message) { _warn(message); }
            public void Error(string message, Exception ex) { _error(message, ex); }
        }

        private sealed class ReflectionGameEvents : IGameEvents { public event Action OnDailyTick = delegate { }; public ReflectionGameEvents(object root, ILogApi log) { ReflectionLookup.TryHookDailyTick(root, delegate { OnDailyTick(); }, log); } }
        private sealed class ReflectionProgressionApi : IProgressionApi
        {
            private static readonly string[] CandidateFlags = new[] { "MayorPubliclySmokingWeed", "MayorSmokingWeed", "MayorSmokesWeedPublic", "WeedLegalizedStoryBeat" };
            private readonly object _root; private readonly ILogApi _log;
            public ReflectionProgressionApi(object root, ILogApi log) { _root = root; _log = log; }
            public bool TryGetFlag(string key, out bool value) { return ReflectionLookup.TryGetBooleanFlag(_root, key, out value, _log); }
            public bool TryGetMayorWeedLegalizationFlag(out string resolvedFlag, out bool value)
            {
                foreach (var flag in CandidateFlags)
                {
                    if (TryGetFlag(flag, out value)) { resolvedFlag = flag; return true; }
                }
                resolvedFlag = string.Empty; value = false; _log.Warn("No mayor legalization progression flag found."); return false;
            }
        }

        private sealed class ReflectionNotificationApi : INotificationApi { private readonly object _root; private readonly ILogApi _log; public ReflectionNotificationApi(object root, ILogApi log) { _root = root; _log = log; } public void Show(string message) { ReflectionLookup.TryNotify(_root, message, _log); } }
        private sealed class ReflectionPlayerApi : IPlayerApi { private readonly object _root; private readonly ILogApi _log; public ReflectionPlayerApi(object root, ILogApi log) { _root = root; _log = log; } public bool IsAvailable { get { return ReflectionLookup.CanAccessPlayerCash(_root, _log); } } public decimal Cash { get { return ReflectionLookup.GetPlayerCash(_root, _log); } set { ReflectionLookup.SetPlayerCash(_root, value, _log); } } }
        private sealed class ReflectionRealEstateApi : IRealEstateApi
        {
            private readonly object _root; private readonly ILogApi _log;
            public ReflectionRealEstateApi(object root, ILogApi log) { _root = root; _log = log; }
            public bool IsAvailable { get { return ReflectionLookup.CanHookRealEstate(_root, _log); } }
            public event Func<string, bool> OnPurchaseRequested = delegate { return false; };
            public void RegisterDynamicListingProvider(string categoryId, Func<IList<RealEstateListing>> provider)
            {
                ReflectionLookup.RegisterRealEstateProvider(_root, categoryId, provider, delegate (string id) { return OnPurchaseRequested(id); }, _log);
            }
        }
        private sealed class ReflectionInventoryApi : IInventoryApi
        {
            private readonly object _root; private readonly ILogApi _log;
            public ReflectionInventoryApi(object root, ILogApi log) { _root = root; _log = log; }
            public IList<PlayerProductStack> GetCannabisProducts() { return ReflectionLookup.GetCannabisProducts(_root, _log); }
            public bool TryRemove(string productId, int quantity) { return ReflectionLookup.TryRemoveInventory(_root, productId, quantity, _log); }
            public bool TryAddDebugProduct(string productId, string productName, ProductGrade grade, int quantity, decimal basePrice) { return ReflectionLookup.TryAddInventory(_root, productId, productName, grade, quantity, basePrice, _log); }
        }
        private sealed class ReflectionSaveSystemApi : ISaveSystemApi { private readonly object _root; private readonly ILogApi _log; public ReflectionSaveSystemApi(object root, ILogApi log) { _root = root; _log = log; } public bool IsAvailable { get { return ReflectionLookup.CanRegisterSave(_root, _log); } } public void Register(string key, Func<string> save, Action<string> load) { ReflectionLookup.RegisterSaveBlock(_root, key, save, load, _log); } }
        private sealed class ReflectionUiApi : IUiApi { private readonly object _root; private readonly ILogApi _log; public ReflectionUiApi(object root, ILogApi log) { _root = root; _log = log; } public void RegisterMenu(string menuId, Action openAction) { ReflectionLookup.RegisterMenu(_root, menuId, openAction, _log); } public void OpenDispensaryManagement(string dispensaryId) { ReflectionLookup.OpenPanel(_root, "DispensaryManagement", dispensaryId, _log); } public void OpenDispensaryListings(string dispensaryId) { ReflectionLookup.OpenPanel(_root, "DispensaryListings", dispensaryId, _log); } public void OpenDispensaryHiring(string dispensaryId) { ReflectionLookup.OpenPanel(_root, "DispensaryHiring", dispensaryId, _log); } public void OpenDispensaryReports(string dispensaryId) { ReflectionLookup.OpenPanel(_root, "DispensaryReports", dispensaryId, _log); } }
        private sealed class ReflectionDiagnostics : IIntegrationDiagnostics
        {
            private readonly object _root; private readonly ILogApi _log;
            public ReflectionDiagnostics(object root, ILogApi log) { _root = root; _log = log; }
            public IntegrationStatus GetStatus()
            {
                return new IntegrationStatus
                {
                    LegalizationFlagLookupFound = ReflectionLookup.HasLegalizationFlagAccess(_root, _log),
                    RealEstateHookFound = ReflectionLookup.CanHookRealEstate(_root, _log),
                    UiHookFound = ReflectionLookup.CanOpenUiPanels(_root, _log),
                    SaveHookFound = ReflectionLookup.CanRegisterSave(_root, _log),
                    DailyTickFound = ReflectionLookup.CanHookDailyTick(_root, _log)
                };
            }
            public void DumpStatus() { var s = GetStatus(); _log.Info("Integration status => Flag:" + s.LegalizationFlagLookupFound + " RealEstate:" + s.RealEstateHookFound + " UI:" + s.UiHookFound + " Save:" + s.SaveHookFound + " DailyTick:" + s.DailyTickFound); }
        }
    }
}
