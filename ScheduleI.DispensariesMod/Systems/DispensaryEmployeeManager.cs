using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleI.DispensariesMod.Data;
using ScheduleI.DispensariesMod.Integration;

namespace ScheduleI.DispensariesMod.Systems;

public sealed class DispensaryEmployeeManager
{
    private readonly IGameApi _gameApi;
    private readonly DispensarySaveData _saveData;

    public DispensaryEmployeeManager(IGameApi gameApi, DispensarySaveData saveData)
    {
        _gameApi = gameApi;
        _saveData = saveData;
    }

    public IReadOnlyCollection<DispensaryEmployee> GetEmployeePool() => _saveData.EmployeePool.Values;

    public DispensaryEmployee HireEmployee(string name, decimal wagePerDay, float salesSkill, float reliability, float customerService)
    {
        var employee = new DispensaryEmployee
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            WagePerDay = wagePerDay,
            SalesSkill = salesSkill,
            Reliability = reliability,
            CustomerService = customerService,
        };

        _saveData.EmployeePool[employee.Id] = employee;
        _gameApi.Notifications.Show($"Hired {employee.Name} for ${employee.WagePerDay}/day.");
        return employee;
    }

    public bool AssignEmployee(DispensaryProperty dispensary, string employeeId)
    {
        if (! _saveData.EmployeePool.ContainsKey(employeeId) || dispensary.AssignedEmployeeIds.Count >= dispensary.EmployeeSlots)
        {
            return false;
        }

        if (!dispensary.AssignedEmployeeIds.Contains(employeeId))
        {
            dispensary.AssignedEmployeeIds.Add(employeeId);
        }

        return true;
    }

    public decimal CalculateDailyWages(DispensaryProperty dispensary)
        => dispensary.AssignedEmployeeIds
            .Select(id => _saveData.EmployeePool.TryGetValue(id, out var employee) ? employee.WagePerDay : 0m)
            .Sum();

    public float GetEffectiveSalesMultiplier(DispensaryProperty dispensary)
    {
        if (dispensary.AssignedEmployeeIds.Count == 0)
        {
            return 0.2f;
        }

        var people = dispensary.AssignedEmployeeIds
            .Select(id => _saveData.EmployeePool.TryGetValue(id, out var employee) ? employee : null)
            .Where(e => e is not null)
            .Cast<DispensaryEmployee>()
            .ToList();

        if (people.Count == 0)
        {
            return 0.2f;
        }

        var avgSkill = people.Average(p => (p.SalesSkill + p.CustomerService + p.Reliability) / 3f);
        return 0.5f + avgSkill;
    }
}
