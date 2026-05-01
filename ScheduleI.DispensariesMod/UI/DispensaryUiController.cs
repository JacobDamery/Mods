using System.Linq;
using ScheduleI.DispensariesMod.Integration;
using ScheduleI.DispensariesMod.Systems;

namespace ScheduleI.DispensariesMod.UI;

/// <summary>
/// Registers menus using existing game UI framework.
/// Replace notification-only handlers with actual panel bindings in your project.
/// </summary>
public sealed class DispensaryUiController
{
    private readonly IGameApi _gameApi;
    private readonly DispensaryManager _manager;
    private readonly DispensaryEmployeeManager _employeeManager;
    private readonly DispensarySaveData _saveData;

    public DispensaryUiController(IGameApi gameApi, DispensaryManager manager, DispensaryEmployeeManager employeeManager, DispensarySaveData saveData)
    {
        _gameApi = gameApi;
        _manager = manager;
        _employeeManager = employeeManager;
        _saveData = saveData;

        RegisterMenus();
    }

    private void RegisterMenus()
    {
        _gameApi.Ui.RegisterMenu("dispensary.buy", () =>
        {
            var available = _manager.GetAvailableTemplates();
            _gameApi.Notifications.Show($"Available dispensaries: {available.Count}");
        });

        _gameApi.Ui.RegisterMenu("dispensary.manage", () =>
        {
            var owned = _saveData.OwnedDispensaries.Values.Count;
            _gameApi.Notifications.Show($"Owned dispensaries: {owned}");
        });

        _gameApi.Ui.RegisterMenu("dispensary.hiring", () =>
        {
            _gameApi.Notifications.Show($"Employee pool: {_employeeManager.GetEmployeePool().Count}");
        });

        _gameApi.Ui.RegisterMenu("dispensary.reports", () =>
        {
            var latest = _saveData.OwnedDispensaries.Values
                .SelectMany(d => d.Reports)
                .OrderByDescending(r => r.DayUtc)
                .FirstOrDefault();

            if (latest is null)
            {
                _gameApi.Notifications.Show("No dispensary reports yet.");
                return;
            }

            _gameApi.Notifications.Show($"Last day profit: ${latest.Profit}");
        });
    }
}
