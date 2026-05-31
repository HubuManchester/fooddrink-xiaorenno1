using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoodDrinkApp.Models;

public sealed class MenuEntry : INotifyPropertyChanged
{
    private int _quantity;

    public FoodItem Item { get; }

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

    public int SubtotalCalories => Item.Calories * _quantity;

    public string DisplayText => $"{Item.Name} × {_quantity}  —  {SubtotalCalories} kcal";

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
