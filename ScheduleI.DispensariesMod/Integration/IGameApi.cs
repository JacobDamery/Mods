using System;
using System.Collections.Generic;

namespace ScheduleI.DispensariesMod.Integration;

/// <summary>
/// Thin adapter for Schedule I systems. Implement this against the game's actual APIs.
/// </summary>
public interface IGameApi
{
    IGameEvents Events { get; }
    IProgressionApi Progression { get; }
    INotificationApi Notifications { get; }
    IPlayerApi Player { get; }
    IRealEstateApi RealEstate { get; }
    IInventoryApi Inventory { get; }
    ISaveSystemApi SaveSystem { get; }
    IUiApi Ui { get; }
}

public interface IGameEvents { event Action OnDailyTick; }
public interface IProgressionApi { bool TryGetFlag(string key, out bool value); }
public interface INotificationApi { void Show(string message); }
public interface IPlayerApi { decimal Cash { get; set; } }
public interface IInventoryApi
{
    IReadOnlyList<PlayerProductStack> GetCannabisProducts();
    bool TryRemove(string productId, int quantity);
}
public interface IUiApi
{
    void RegisterMenu(string menuId, Action openAction);
}
public interface ISaveSystemApi
{
    void Register(string key, Func<string> save, Action<string> load);
}
public interface IRealEstateApi
{
    void RegisterDynamicListingProvider(string categoryId, Func<IReadOnlyList<RealEstateListing>> provider);
    event Func<string, bool> OnPurchaseRequested;
}

public readonly record struct RealEstateListing(string Id, string Name, string Location, decimal Price, string Category);
public readonly record struct PlayerProductStack(string ProductId, string ProductName, ProductGrade Grade, int Quantity, decimal BaseStreetPrice);
public enum ProductGrade { Low, Mid, High, Exotic }
