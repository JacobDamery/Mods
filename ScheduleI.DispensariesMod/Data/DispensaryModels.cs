using System;
using System.Collections.Generic;
using ScheduleI.DispensariesMod.Integration;

namespace ScheduleI.DispensariesMod.Data;

public sealed record DispensaryPropertyTemplate(
    string Name,
    string Location,
    decimal PurchasePrice,
    decimal OperatingCostPerDay,
    int StorageCapacity,
    int EmployeeSlots,
    int DisplaySlots,
    float LocationDemandModifier)
{
    public string Id { get; } = Name.Replace(" ", "_").ToLowerInvariant();
}

public sealed class DispensaryProperty
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string Location { get; init; }
    public required decimal PurchasePrice { get; init; }
    public required decimal OperatingCostPerDay { get; init; }
    public required int StorageCapacity { get; init; }
    public required int EmployeeSlots { get; init; }
    public required int DisplaySlots { get; init; }
    public required float LocationDemandModifier { get; init; }

    public DispensaryInventory Inventory { get; } = new();
    public List<string> AssignedEmployeeIds { get; } = new();
    public List<DispensaryDailyReport> Reports { get; } = new();
}

public sealed class DispensaryInventory
{
    public List<DispensaryListing> Listings { get; } = new();
}

public sealed class DispensaryListing
{
    public required string ProductId { get; init; }
    public required string ProductName { get; init; }
    public required ProductGrade Grade { get; init; }
    public required int Quantity { get; set; }
    public required decimal SalePrice { get; set; }
    public required float DemandModifier { get; set; }
}

public sealed class DispensaryEmployee
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required decimal WagePerDay { get; init; }
    public required float SalesSkill { get; init; }
    public required float Reliability { get; init; }
    public required float CustomerService { get; init; }
}

public sealed class DispensaryDailyReport
{
    public required DateTime DayUtc { get; init; }
    public required decimal Revenue { get; init; }
    public required decimal OperatingCosts { get; init; }
    public required decimal Wages { get; init; }
    public decimal Profit => Revenue - OperatingCosts - Wages;
}
