using System.ComponentModel;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

// Singleton that holds the user's current menu selection across pages.
// Raises PropertyChanged so MenuPage can react to quantity changes without manual refresh.
public sealed class MenuService : INotifyPropertyChanged
{
    private static readonly Lazy<MenuService> _instance = new(() => new MenuService());
    public static MenuService Instance => _instance.Value;

    private readonly List<MenuEntry> _entries = [];

    public IReadOnlyList<MenuEntry> Entries => _entries.AsReadOnly();

    public int TotalCalories => _entries.Sum(e => e.SubtotalCalories);

    public int ItemCount => _entries.Sum(e => e.Quantity);

    public bool HasItems => _entries.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    private MenuService() { }

    // Adds a dish or increments its quantity if it already exists in the menu.
    public void AddItem(FoodItem item, int quantity = 1)
    {
        var existing = _entries.FirstOrDefault(e => e.Item.Id == item.Id);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _entries.Add(new MenuEntry(item, quantity));
        }

        NotifyChanged();
    }

    // Removes every occurrence of a dish regardless of quantity.
    public void RemoveItem(FoodItem item)
    {
        _entries.RemoveAll(e => e.Item.Id == item.Id);
        NotifyChanged();
    }

    // Resets the entire menu back to empty.
    public void Clear()
    {
        _entries.Clear();
        NotifyChanged();
    }

    public void IncrementQuantity(FoodItem item)
    {
        var entry = _entries.FirstOrDefault(e => e.Item.Id == item.Id);
        if (entry is not null)
        {
            entry.Quantity++;
            NotifyChanged();
        }
    }

    // Lowers the count by one and removes the entry entirely if it would reach zero.
    public void DecrementQuantity(FoodItem item)
    {
        var entry = _entries.FirstOrDefault(e => e.Item.Id == item.Id);
        if (entry is not null)
        {
            if (entry.Quantity <= 1)
            {
                RemoveItem(item);
            }
            else
            {
                entry.Quantity--;
                NotifyChanged();
            }
        }
    }

    private void NotifyChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Entries)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalCalories)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemCount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasItems)));
    }
}
