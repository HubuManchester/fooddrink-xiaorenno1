using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoodDrinkApp.Models;

// Wraps a FoodItem with a mutable quantity so the menu page can track how many
// of each dish the user wants. Property changes flow up to MenuService for UI updates.
public sealed class MenuEntry : INotifyPropertyChanged
{
    private int _quantity;

    public FoodItem Item { get; }

    // Clamped to at least 1 so the decrement button never goes below zero.
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = Math.Max(1, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SubtotalCalories));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    // Calories contributed by this entry based on how many portions were added.
    public int SubtotalCalories => Item.Calories * _quantity;

    public string DisplayText => $"{Item.Name} × {_quantity}  —  {SubtotalCalories} kcal";

    // Used by SemanticProperties.Hint so screen readers describe what the remove button does.
    public string RemoveHint => $"Remove {Item.Name} from menu";

    public MenuEntry(FoodItem item, int quantity = 1)
    {
        Item = item;
        _quantity = Math.Max(1, quantity);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
