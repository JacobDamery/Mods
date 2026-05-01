using System.Linq;
using ScheduleI.DispensariesMod.Integration;
using ScheduleI.DispensariesMod.Systems;

namespace ScheduleI.DispensariesMod.UI;

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
        _gameApi.Ui.RegisterMenu("dispensary.buy", () => _gameApi.Ui.OpenDispensaryManagement("market"));

        _gameApi.Ui.RegisterMenu("dispensary.manage", () =>
        {
            var firstOwned = _saveData.OwnedDispensaries.Keys.FirstOrDefault();
            if (firstOwned is null)
            {
                _gameApi.Notifications.Show("You do not own a dispensary yet.");
                return;
            }

            _gameApi.Ui.OpenDispensaryManagement(firstOwned);
        });

        _gameApi.Ui.RegisterMenu("dispensary.listings", () =>
        {
            var firstOwned = _saveData.OwnedDispensaries.Keys.FirstOrDefault();
            if (firstOwned is null)
            {
                _gameApi.Notifications.Show("You do not own a dispensary yet.");
                return;
            }

            _gameApi.Ui.OpenDispensaryListings(firstOwned);
        });

        _gameApi.Ui.RegisterMenu("dispensary.hiring", () =>
        {
            var firstOwned = _saveData.OwnedDispensaries.Keys.FirstOrDefault();
            if (firstOwned is null)
            {
                _gameApi.Notifications.Show("You do not own a dispensary yet.");
                return;
            }

            _gameApi.Ui.OpenDispensaryHiring(firstOwned);
        });

        _gameApi.Ui.RegisterMenu("dispensary.reports", () =>
        {
            var firstOwned = _saveData.OwnedDispensaries.Keys.FirstOrDefault();
            if (firstOwned is null)
            {
                _gameApi.Notifications.Show("You do not own a dispensary yet.");
                return;
            }

            _gameApi.Ui.OpenDispensaryReports(firstOwned);
        });

        _gameApi.Log.Info("DispensariesMod: UI menus registered");
    }
}
