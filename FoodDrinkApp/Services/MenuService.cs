using System.ComponentModel;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

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

    public void RemoveItem(FoodItem item)
    {
        _entries.RemoveAll(e => e.Item.Id == item.Id);
        NotifyChanged();
    }

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
