using System;
using System.Collections.Generic;
using ScheduleI.DispensariesMod.Integration;

namespace ScheduleI.DispensariesMod.Data
{
    public class DispensaryPropertyTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal OperatingCostPerDay { get; set; }
        public int StorageCapacity { get; set; }
        public int EmployeeSlots { get; set; }
        public int DisplaySlots { get; set; }
        public float LocationDemandModifier { get; set; }

        public DispensaryPropertyTemplate()
        {
            Id = string.Empty;
            Name = string.Empty;
            Location = string.Empty;
        }

        public DispensaryPropertyTemplate(string name, string location, decimal purchasePrice, decimal operatingCostPerDay, int storageCapacity, int employeeSlots, int displaySlots, float locationDemandModifier)
        {
            Name = name;
            Location = location;
            PurchasePrice = purchasePrice;
            OperatingCostPerDay = operatingCostPerDay;
            StorageCapacity = storageCapacity;
            EmployeeSlots = employeeSlots;
            DisplaySlots = displaySlots;
            LocationDemandModifier = locationDemandModifier;
            Id = name.Replace(" ", "_").ToLowerInvariant();
        }
    }

    public class DispensaryProperty
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal OperatingCostPerDay { get; set; }
        public int StorageCapacity { get; set; }
        public int EmployeeSlots { get; set; }
        public int DisplaySlots { get; set; }
        public float LocationDemandModifier { get; set; }
        public DispensaryInventory Inventory { get; set; }
        public List<string> AssignedEmployeeIds { get; set; }
        public List<DispensaryDailyReport> Reports { get; set; }

        public DispensaryProperty()
        {
            Id = string.Empty;
            Name = string.Empty;
            Location = string.Empty;
            Inventory = new DispensaryInventory();
            AssignedEmployeeIds = new List<string>();
            Reports = new List<DispensaryDailyReport>();
        }
    }

    public class DispensaryInventory
    {
        public List<DispensaryListing> Listings { get; set; }
        public DispensaryInventory() { Listings = new List<DispensaryListing>(); }
    }

    public class DispensaryListing
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public ProductGrade Grade { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public float DemandModifier { get; set; }

        public DispensaryListing() { ProductId = string.Empty; ProductName = string.Empty; }
    }

    public class DispensaryEmployee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal WagePerDay { get; set; }
        public float SalesSkill { get; set; }
        public float Reliability { get; set; }
        public float CustomerService { get; set; }

        public DispensaryEmployee() { Id = string.Empty; Name = string.Empty; }
    }

    public class DispensaryDailyReport
    {
        public DateTime DayUtc { get; set; }
        public decimal Revenue { get; set; }
        public decimal OperatingCosts { get; set; }
        public decimal Wages { get; set; }
        public decimal Profit { get { return Revenue - OperatingCosts - Wages; } }
    }
}
