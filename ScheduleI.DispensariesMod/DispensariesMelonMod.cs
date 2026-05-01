using MelonLoader;
using ScheduleI.DispensariesMod.Core;
using ScheduleI.DispensariesMod.Integration;

[assembly: MelonInfo(typeof(ScheduleI.DispensariesMod.DispensariesMelonMod), "Schedule I Dispensaries Mod", "0.1.0", "JacobDamery")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ScheduleI.DispensariesMod
{
    public class DispensariesMelonMod : MelonMod
    {
        private DispensariesModPlugin _plugin;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Schedule I Dispensaries Mod initializing...");

            var gameApi = new ScheduleIGameApi(
                null,
                delegate (string msg) { LoggerInstance.Msg(msg); },
                delegate (string msg) { LoggerInstance.Warning(msg); },
                delegate (string msg, System.Exception ex) { LoggerInstance.Error(msg + (ex == null ? string.Empty : " :: " + ex)); });

            _plugin = new DispensariesModPlugin(gameApi, true);
            _plugin.Initialize();

            LoggerInstance.Msg("Schedule I Dispensaries Mod initialized.");
        }

        public override void OnUpdate()
        {
            if (_plugin != null)
            {
                _plugin.OnUpdate();
            }
        }
    }
}
